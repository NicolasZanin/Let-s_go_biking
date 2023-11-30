using RoutingServer.ProxyService;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace RoutingServer
{
    class OpenStreetMapManager
    {

        private static List<Itineraire> itineraires = new List<Itineraire>();
        private static int nombreEtapeActuel = 0;

        public static async Task<string> convertAddressToPointAsync(string address) {
            string apiURL = "https://api.openrouteservice.org/geocode/search?api_key=5b3ce3597851110001cf6248ef43ce34eac546a6b118b6706bfe11ce&text=";
            string[] adressSplit = address.Split(',');
            string nameAdress = adressSplit[0];
            apiURL += nameAdress;

            if (adressSplit.Length > 1) {
                string[] codePostalPays = adressSplit[1].Split(' ');

                if (codePostalPays[0][0] >= '0' && codePostalPays[0][0] <= '9')
                    apiURL += "&postalcode=" + codePostalPays[0];
                
                if (codePostalPays.Length > 1)
                    apiURL += "&locality=" + codePostalPays[1];
                else
                    apiURL += "&locality=" + codePostalPays[0];
            }

            HttpClient OSMapi = new HttpClient();
            HttpResponseMessage response = await OSMapi.GetAsync(apiURL);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            JsonDocument document = JsonDocument.Parse(responseBody);
            JsonElement coordinates = document.RootElement.GetProperty("features")[0].GetProperty("geometry").GetProperty("coordinates");
            return coordinates[0] + "," + coordinates[1];
        }

        public static async Task<List<string>> ComputeItineraire(string start, string end, string locomotion)
        {
            if (nombreEtapeActuel == 0) {
                itineraires.Clear();
                start = await convertAddressToPointAsync(start);
                end = await convertAddressToPointAsync(end);
                Console.WriteLine("ok");
                List<string> list = await ComputeItineraireEtape0(start, end);
                EnvoiMessage(10);
                return list;
            }
            ComputeItineraireEtapeAsync(start, end);
            return new List<string>();
        }

        private static async Task<List<string>> ComputeItineraireEtape0(string start, string end) {
            nombreEtapeActuel = 0;
            List<string> list = new List<string>();

            // trouve la station la plus proche avec des vélos Debut et arrivé
            string startNearestContrat = await ServiceRoutingServer.findNearestContratAsync(start);
            string endNearestContrat = await ServiceRoutingServer.findNearestContratAsync(end);
            Console.WriteLine("CONTRAT");
            bool onBike = false;
            InformationStation startCoordNearestStation = await ServiceRoutingServer.findNearestStationAsync(start, startNearestContrat, onBike);
            Console.WriteLine("NEARESTSTATION");
            onBike = true;
            InformationStation endCoordNearestStation = await ServiceRoutingServer.findNearestStationAsync(end, endNearestContrat, onBike);
            Console.WriteLine("NEARESTSTATION2");

            Itineraire itineraireStartToStation1 = await getItineraireAsync(start, startCoordNearestStation.coordonnes, "foot-walking", startCoordNearestStation);
            Itineraire itineraireStation1to2 = await getItineraireAsync(startCoordNearestStation.coordonnes, endCoordNearestStation.coordonnes, "cycling-regular", endCoordNearestStation);
            Itineraire itineraireeStation2ToEnd = await getItineraireAsync(endCoordNearestStation.coordonnes, end, "foot-walking");
            double distanceTotaleBike = itineraireStartToStation1.distance + itineraireeStation2ToEnd.distance;
            Itineraire itineraireAllFoot = await getItineraireAsync(start, end, "foot-walking");

            // call OSM si distance pied debut fin plus petite que à station
            if (distanceTotaleBike >= itineraireAllFoot.distance) {
                //si ça vaut pas le coup start end de base locomotion walking
                itineraires.Add(itineraireAllFoot);
                list.Add(JsonSerializer.Serialize(itineraireAllFoot.coordonnes));
                return list;
            }

            itineraires.Add(itineraireStartToStation1);
            itineraires.Add(itineraireStation1to2);
            itineraires.Add(itineraireeStation2ToEnd);
            list.Add(JsonSerializer.Serialize(itineraireStartToStation1.coordonnes));
            list.Add(JsonSerializer.Serialize(itineraireStation1to2.coordonnes));
            list.Add(JsonSerializer.Serialize(itineraireeStation2ToEnd.coordonnes));

            return list;
        }

        private static async void ComputeItineraireEtapeAsync(string start, string end) {
            int verifVelo = await VerificationVeloAsync();

            switch (verifVelo) {
                case 0: EnvoiMessage(10); break;
                case 1: await ComputeItineraireEtape0(await convertAddressToPointAsync(start), await convertAddressToPointAsync(end)); break;
                case 2: FindStationBike(end); break;
            }
        }

        private static async void FindStationBike(string end) {
            string endNearestContrat = await ServiceRoutingServer.findNearestContratAsync(end);
            InformationStation endCoordNearestStation = await ServiceRoutingServer.findNearestStationAsync(end, endNearestContrat, true);
            string address = itineraires[1].morceauItineraire[nombreEtapeActuel - itineraires[0].morceauItineraire.Count].GetProperty("name").GetString();
            
            itineraires[1] = await getItineraireAsync(await convertAddressToPointAsync(address), endCoordNearestStation.coordonnes, "cycling-regular", endCoordNearestStation);
            itineraires[2] = await getItineraireAsync(endCoordNearestStation.coordonnes,await convertAddressToPointAsync(end), "foot-walking");
            nombreEtapeActuel = itineraires[0].morceauItineraire.Count;
            EnvoiMessage(10);
        }

        private static void EnvoiMessage(int nombreMessage) {
            int tailleItineraire = itineraires[0].morceauItineraire.Count;
            List<string> list = new List<string>(nombreMessage);

            if (nombreEtapeActuel < tailleItineraire) {
                int minEtape = Math.Min(nombreEtapeActuel + nombreMessage, tailleItineraire);

                for (int i = nombreEtapeActuel; i < minEtape; i++)
                    list.Add(JsonSerializer.Serialize(itineraires[0].morceauItineraire[i]));

                if (list.Count < nombreMessage && itineraires.Count == 1)
                {
                    list.Add("FINI");
                    ActiveMQProducer.Producer.envoyerMessage(list.ToArray());
                    return;
                }
                else if (list.Count < nombreMessage) {
                    nombreEtapeActuel += list.Count;
                    list.Add("Velo");
                }
                else if (itineraires.Count == 1)
                {
                    nombreEtapeActuel += nombreMessage;
                    ActiveMQProducer.Producer.envoyerMessage(list.ToArray());
                    return;
                }
            }

            int ancienneTaille = tailleItineraire;
            tailleItineraire += itineraires[1].morceauItineraire.Count;
            
            if (list.Count < nombreMessage && nombreEtapeActuel < tailleItineraire) {
                int nombreMessageRestant = nombreMessage - list.Count;
                int minEtape = Math.Min(nombreEtapeActuel + nombreMessageRestant, tailleItineraire);

                for (int i = nombreEtapeActuel; i < minEtape; i++)
                    list.Add(JsonSerializer.Serialize(itineraires[0].morceauItineraire[i - ancienneTaille]));

                if (list.Count < nombreMessage) {
                    nombreEtapeActuel += nombreMessage - list.Count + nombreMessageRestant;
                    list.Add("fin velo");
                }
            }

            ancienneTaille = tailleItineraire;
            tailleItineraire += itineraires[2].morceauItineraire.Count;

            if (list.Count < nombreMessage && nombreEtapeActuel < tailleItineraire)
            {
                int nombreMessageRestant = nombreMessage - list.Count;
                int minEtape = Math.Min(nombreEtapeActuel + nombreMessageRestant, tailleItineraire);

                for (int i = nombreEtapeActuel; i < minEtape; i++)
                    list.Add(JsonSerializer.Serialize(itineraires[0].morceauItineraire[i - ancienneTaille]));

                if (list.Count < nombreMessage)
                    list.Add("FINI");
            }
            else if (list.Count < nombreMessage) {
                list.Add("FINI");
            }

            nombreEtapeActuel += nombreMessage;
            ActiveMQProducer.Producer.envoyerMessage(list.ToArray());
        }

        private static async Task<int> VerificationVeloAsync() {
            if (itineraires.Count == 1)
                return 0;

            int etapeItineraire1 = itineraires[0].morceauItineraire.Count;
            IServiceProxy proxy = new ServiceProxyClient();
            
            if (nombreEtapeActuel < etapeItineraire1) {
                InformationStation information = itineraires[0].informationStation;

                int nombreVelo = await proxy.GetNombreVeloAsync(information.numeroStation, information.nomContract);

                if (nombreVelo == 0)
                    return 1;
            }

            else if (nombreEtapeActuel < etapeItineraire1 + itineraires[1].morceauItineraire.Count) {
                InformationStation information = itineraires[1].informationStation;

                int nombreVelo = await proxy.GetNombreVeloAsync(information.numeroStation, information.nomContract);
                int nombrePlaceVelo = await proxy.GetPlaceVeloAsync(information.numeroStation, information.nomContract);

                if (nombreVelo == nombrePlaceVelo)
                    return 2;
            }
            
            return 0;

        }

        internal static async Task<Itineraire> getItineraireAsync(string coordinatesStart, string station1Coord, string locomotion, InformationStation info = null)
        {
            HttpClient OSMapi = new HttpClient();
            string apiUrl = "https://api.openrouteservice.org/v2/directions/" + locomotion + "?api_key=5b3ce3597851110001cf6248ef43ce34eac546a6b118b6706bfe11ce&start=" + coordinatesStart + "&end=" + station1Coord;
            //Console.WriteLine(apiUrl);
            HttpResponseMessage response = await OSMapi.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return new Itineraire(responseBody, info);
        }

        internal static async Task<double> getDistanceAsync(string coordinatesStart, string station1Coord, string locomotion)
        {
            HttpClient OSMapi = new HttpClient();
            string apiUrl = "https://api.openrouteservice.org/v2/directions/" + locomotion + "?api_key=5b3ce3597851110001cf6248ef43ce34eac546a6b118b6706bfe11ce&start=" + coordinatesStart + "&end=" + station1Coord;
            //Console.WriteLine(apiUrl);
            HttpResponseMessage response = await OSMapi.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            JsonDocument document = JsonDocument.Parse(responseBody);

            return document.RootElement.GetProperty("features")[0].GetProperty("properties").GetProperty("segments")[0].GetProperty("distance").GetDouble();
        }
    }
}
