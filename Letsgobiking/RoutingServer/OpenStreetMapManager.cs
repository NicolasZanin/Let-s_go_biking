using ActiveMQProducer;
using RoutingServer.ProxyService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace RoutingServer
{
    class OpenStreetMapManager
    {

        private static List<Itineraire> itineraires = new List<Itineraire>();
        private static int numeroEtapeActuel = 0;
        private static String apiKey = "5b3ce3597851110001cf6248ef43ce34eac546a6b118b6706bfe11ce";

        //convertit une adresse en coordonnes avec OpenROuteService
        public static async Task<string> convertAddressToPointAsync(string address) {
            string apiURL = "https://api.openrouteservice.org/geocode/search?api_key=" + apiKey + "&text=" + address;
            //fait l'appel est récupère les coordonnées
            HttpClient OSMapi = new HttpClient();
            HttpResponseMessage response = await OSMapi.GetAsync(apiURL);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            JsonDocument document = JsonDocument.Parse(responseBody);
            JsonElement coordinates = document.RootElement.GetProperty("features")[0].GetProperty("geometry").GetProperty("coordinates");
            return coordinates[0] + "," + coordinates[1];
        }
        //trouve les coordonées des points et calcul l'itinéraire global
        public static async Task<List<string>> ComputeItineraire(string start, string end)
        {
            if (!Producer.estConnecter() && start == "" && end == "") {
                List<string> liste = new List<string>();
                
                foreach (Itineraire itineraire in itineraires) {
                    foreach (JsonElement morceauItineraire in itineraire.morceauItineraire)
                        liste.Add(morceauItineraire.GetProperty("instruction").GetString());
                }

                return liste;
            }

            // En train de faire l'itinéraire
            if (numeroEtapeActuel != 0 && start == "" && end == "") {
                ComputeItineraireEtapeAsync(start, end);
                return new List<string>();
            }
            
            // Si le client à arreter le programme avant la fin du trajet
            if (numeroEtapeActuel != 0) {
                ReinitialiseVariable();
            }

            start = await convertAddressToPointAsync(start);
            end = await convertAddressToPointAsync(end);
            List<string> list = await ComputeItineraireGlobal(start, end);

            if (Producer.estConnecter())
                SendSteps(10);

            return list;        
        }

        //calcul l'itinéraire global
        private static async Task<List<string>> ComputeItineraireGlobal(string start, string end) {
            List<string> list = new List<string>();

            // trouve la station la plus proche avec des vélos Debut et arrivé
            string startNearestContrat = await ServiceRoutingServer.findNearestContratAsync(start);
            string endNearestContrat = await ServiceRoutingServer.findNearestContratAsync(end);
            bool onBike = false;

            //regarde la station la plus proche du départ à pied
            InformationStation startCoordNearestStation = await ServiceRoutingServer.findNearestStationAsync(start, startNearestContrat, onBike);
            onBike = true;
            //regarde la station la plus proche de l'arrivé à vélo
            InformationStation endCoordNearestStation = await ServiceRoutingServer.findNearestStationAsync(end, endNearestContrat, onBike);
            
            //Calcul les itinéraires
            Itineraire itineraireStartToStation1 = await getItineraireAsync(start, startCoordNearestStation.coordonnes, "foot-walking", startCoordNearestStation);
            Itineraire itineraireStation1to2 = await getItineraireAsync(startCoordNearestStation.coordonnes, endCoordNearestStation.coordonnes, "cycling-road", endCoordNearestStation);
            Itineraire itineraireeStation2ToEnd = await getItineraireAsync(endCoordNearestStation.coordonnes, end, "foot-walking");

            // distance à pied en suivant l'itinéraire en prenant un vélo
            double distanceTotaleByFoot = itineraireStartToStation1.distance + itineraireeStation2ToEnd.distance;

            //distance tout à pied sans prendre de vélo
            Itineraire itineraireAllFoot = await getItineraireAsync(start, end, "foot-walking");

            // Renvoie l'itinéraire tout à pied distance pied debut fin plus petite que en passant par les stations
            if (distanceTotaleByFoot >= itineraireAllFoot.distance) {
                //si ça vaut pas le coup start end de base locomotion walking
                itineraires.Add(itineraireAllFoot);
                list.Add(JsonSerializer.Serialize(itineraireAllFoot.coordonnes));
                return list;
            }

            //sinon revoie l'itinéraire en prennant le vélo
            itineraires.Add(itineraireStartToStation1);
            itineraires.Add(itineraireStation1to2);
            itineraires.Add(itineraireeStation2ToEnd);
            list.Add(JsonSerializer.Serialize(itineraireStartToStation1.coordonnes));
            list.Add(JsonSerializer.Serialize(itineraireStation1to2.coordonnes));
            list.Add(JsonSerializer.Serialize(itineraireeStation2ToEnd.coordonnes));

            return list;
        }

        //regarde la validité du trajet et en trouve un nouveau si besoin
        private static async void ComputeItineraireEtapeAsync(string start, string end) {
            int verifVelo = await CheckValidityOfItinerary();

            switch (verifVelo) {
                case 0: SendSteps(10); break; //tout vas bien on peut continuer
                case 1: ReinitialiseVariable(); await ComputeItineraireGlobal(await convertAddressToPointAsync(start), await convertAddressToPointAsync(end)); break; //plus de vélo dispo dans la station 1, recherche d'un nouvel itinéraire
                case 2: FindNewItineraryBike(end); break; //plus de place pour déposer mon vélo, nouvelle recherche d'itinéraire 
            }
        }

        //je suis en vélo et il n'y à plus de place pour poser mon vélo, nouvelle recherche d'itinéraire 
        private static async void FindNewItineraryBike(string end) {
            string endNearestContrat = await ServiceRoutingServer.findNearestContratAsync(end);
            InformationStation endCoordNearestStation = await ServiceRoutingServer.findNearestStationAsync(end, endNearestContrat, true);
            string address = itineraires[1].morceauItineraire[numeroEtapeActuel - itineraires[0].morceauItineraire.Count].GetProperty("name").GetString();
            
            itineraires[1] = await getItineraireAsync(await convertAddressToPointAsync(address), endCoordNearestStation.coordonnes, "cycling-road", endCoordNearestStation);
            itineraires[2] = await getItineraireAsync(endCoordNearestStation.coordonnes,await convertAddressToPointAsync(end), "foot-walking");
            numeroEtapeActuel = itineraires[0].morceauItineraire.Count;
            SendSteps(10);
        }

        //Active MQ envoie des étapes
        private static void SendSteps(int nombreMessage) {
            // taille du trajet à pied
            int tailleItineraire = itineraires[0].morceauItineraire.Count;
            List<string> list = new List<string>(nombreMessage);

            //si je suis dans la première partie du trajet
            if (numeroEtapeActuel < tailleItineraire) {
                // prend le min en mon étape actuelle plus la nombre de'atapes souhaité et la taille du trajet à pied pour aller à la station 1
                int minEtape = Math.Min(numeroEtapeActuel + nombreMessage, tailleItineraire);
                
                //recupére les étapes
                for (int i = numeroEtapeActuel; i < minEtape; i++)
                    list.Add(JsonSerializer.Serialize(itineraires[0].morceauItineraire[i]));

                //si on fait que le trajet à pied et qu'il est fini je le dis
                if (list.Count < nombreMessage && itineraires.Count == 1)
                {
                    list.Add("FINI");
                    ActiveMQProducer.Producer.envoyerMessage(list.ToArray());
                    ReinitialiseVariable();
                    return;
                }
                //sinon c'est qu'il est temps de prendre le vélo
                else if (list.Count < nombreMessage) {
                    numeroEtapeActuel += list.Count;
                    list.Add("Velo");
                }
                //sinon j'envoie juste les étapes
                else if (itineraires.Count == 1)
                {
                    numeroEtapeActuel += nombreMessage;
                    ActiveMQProducer.Producer.envoyerMessage(list.ToArray());
                    return;
                }
            }


            int ancienneTaille = tailleItineraire;
            //taille trajet j'usqua la deuxieme station
            tailleItineraire += itineraires[1].morceauItineraire.Count;
            
            //si je suis à vélo entre la première et la deuxième station
            if (list.Count < nombreMessage && numeroEtapeActuel < tailleItineraire) {
                int nombreMessageRestant = nombreMessage - list.Count;
                int minEtape = Math.Min(numeroEtapeActuel + nombreMessageRestant, tailleItineraire);

                for (int i = numeroEtapeActuel; i < minEtape; i++)
                    list.Add(JsonSerializer.Serialize(itineraires[1].morceauItineraire[i - ancienneTaille]));
                
                // si je suis arrivé à la station 2 ont le signale
                if (list.Count < nombreMessage) {
                    numeroEtapeActuel += nombreMessage - list.Count + nombreMessageRestant;
                    list.Add("fin velo");
                }
            }

            ancienneTaille = tailleItineraire;
            tailleItineraire += itineraires[2].morceauItineraire.Count;
            //troisième partie du trajet, à pied jusqu'a la fin
            if (list.Count < nombreMessage && numeroEtapeActuel < tailleItineraire)
            {
                int nombreMessageRestant = nombreMessage - list.Count;
                int minEtape = Math.Min(numeroEtapeActuel + nombreMessageRestant, tailleItineraire);

                for (int i = numeroEtapeActuel; i < minEtape; i++)
                    list.Add(JsonSerializer.Serialize(itineraires[2].morceauItineraire[i - ancienneTaille]));

                if (list.Count < nombreMessage)
                {
                    list.Add("FINI");
                    ReinitialiseVariable();
                }
            }
            else if (list.Count < nombreMessage) {
                list.Add("FINI");
                ReinitialiseVariable();
            }

            numeroEtapeActuel += nombreMessage;
            ActiveMQProducer.Producer.envoyerMessage(list.ToArray());
        }

        //vérifie la validité du trajet
        private static async Task<int> CheckValidityOfItinerary() {
            if (itineraires.Count == 1) //à pied donc pas de vélo à vérif
                return 0;

            //si je suis à l'étape une je vérifie que il y à des vélo dispo dans la station
            int etapeItineraire1 = itineraires[0].morceauItineraire.Count;
            IServiceProxy proxy = new ServiceProxyClient();
            
            if (numeroEtapeActuel < etapeItineraire1) {
                InformationStation information = itineraires[0].informationStation;

                int nombreVelo = await proxy.GetNombreVeloAsync(information.numeroStation, information.nomContract);

                if (nombreVelo == 0)
                    return 1;
            }

            // si je suis à vélo, je vérifie qu'il y à des place pour le poser à la station d'arrivée
            else if (numeroEtapeActuel < etapeItineraire1 + itineraires[1].morceauItineraire.Count) {
                InformationStation information = itineraires[1].informationStation;

                int nombreVelo = await proxy.GetNombreVeloAsync(information.numeroStation, information.nomContract);
                int nombrePlaceVelo = await proxy.GetPlaceVeloAsync(information.numeroStation, information.nomContract);

                if (nombreVelo == nombrePlaceVelo)
                    return 2;
            }
            
            return 0;

        }

        //call à open route service pour récup les itinéraire
        internal static async Task<Itineraire> getItineraireAsync(string coordinatesStart, string station1Coord, string locomotion, InformationStation info = null)
        {
            HttpClient OSMapi = new HttpClient();
            string apiUrl = "https://api.openrouteservice.org/v2/directions/" + locomotion + "?api_key=" + apiKey + "&start=" + coordinatesStart + "&end=" + station1Coord;
            //Console.WriteLine(apiUrl);
            HttpResponseMessage response = await OSMapi.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return new Itineraire(responseBody, info);
        }

        //call open pour calculer distance d'un itinéraire
        internal static async Task<double> getDistanceAsync(string coordinatesStart, string station1Coord, string locomotion)
        {
            HttpClient OSMapi = new HttpClient();
            string apiUrl = "https://api.openrouteservice.org/v2/directions/" + locomotion + "?api_key=" + apiKey + "&start=" + coordinatesStart + "&end=" + station1Coord;
            //Console.WriteLine(apiUrl);
            HttpResponseMessage response = await OSMapi.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            JsonDocument document = JsonDocument.Parse(responseBody);

            return document.RootElement.GetProperty("features")[0].GetProperty("properties").GetProperty("segments")[0].GetProperty("distance").GetDouble();
        }

        // Réinitialise les variables
        private static void ReinitialiseVariable() {
            itineraires.Clear();
            numeroEtapeActuel = 0;
        }
    }
}
