using RoutingServer.ProxyService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace RoutingServer { 
    class ServiceRoutingServer : IServiceRoutingServer
    {
        public int Add(int num1, int num2) {
            return num1 + num2;
        }

        private static ProxyService.IServiceProxy proxyService = new ProxyService.ServiceProxyClient();


        public async Task<List<string>> ComputeItineraireAsync(string coordinatesStart, string coordinatesEnd, string locomotion)
        {
            return await OpenStreetMapManager.ComputeItineraire(coordinatesStart, coordinatesEnd, locomotion);
            // call OSM adresse -> coordonnées //ZAZIN
            //Active MQ divisé en étapes //ZAZIN

            //retourner la liste des 3 itinéraire //ZAZIN
            // Essayer de stocker dans OSMManager les Trajets de getDistance au lieu de refaire avec ComputeItinéraire //ZAZIN
        }

        public static async Task<string> findNearestContratAsync(string coordinatesStart)
        {
            double distance = double.MaxValue;
            String res = null;
            Station[] listeStations = proxyService.GetOneStationForAllContrat();
            foreach (Station station in listeStations)
            {
                String station1CoordLongitude = station.position.longitude.ToString().Replace(",",".");
                String station1CoordLatitude = station.position.latitude.ToString().Replace(",", ".");
                String station1Coord = station1CoordLongitude + "," + station1CoordLatitude;
                try
                {
                    double newDistance = CalculateEuclideanDistance(coordinatesStart, station1Coord); //await OpenStreetMapManager.getDistanceAsync(coordinatesStart, station1Coord, "foot-walking");

                    if (newDistance < distance)
                    {
                        distance = newDistance;
                        res = station.contractName;
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine("Bad Request: The request to the OpenRouteService API was not valid.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }
            Console.WriteLine("Resultat : " + res);
            return res;
        }

        public static async Task<InformationStation> findNearestStationAsync(string coordinates, string contrat, Boolean onBike)
        {
            double distance = double.MaxValue;
            InformationStation res = null;
            List<InformationStation> listeStations = await findNearestStationsAsync(coordinates, contrat, onBike);
            Console.WriteLine("findNearestStationsAsync");
            foreach (InformationStation station in listeStations)
            {
                try
                {
                    double newDistance = await OpenStreetMapManager.getDistanceAsync(coordinates, station.coordonnes, "foot-walking");
                    if (newDistance < distance)
                    {
                        distance = newDistance;
                        res = station;
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine("Bad Request: The request to the OpenRouteService API was not valid.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }
            Console.WriteLine("Resultat : " + res);
            return res;
        }

        private static async Task<List<InformationStation>> findNearestStationsAsync(string coordinatesStart, string contrat,bool onBike)
        {
            List<(InformationStation informationStation, double distance)> nearestStations = new List<(InformationStation informationStation, double distance)>();
            Console.WriteLine("AAAA");

            Contrat getContrat = proxyService.GetContrat(contrat);
            Station[] listeStations = getContrat.stations;
            Console.WriteLine("DAIM");
            foreach (Station station in listeStations)
            {
                Console.WriteLine("tac");

                String stationNumber = station.number.ToString();
                if (onBike)
                {
                    int placeDispo = await proxyService.GetPlaceVeloAsync(stationNumber, contrat);
                    if (placeDispo <= 0) { continue; }

                }
                else
                {
                    int nbVeloDispo = await proxyService.GetNombreVeloAsync(stationNumber, contrat);
                    if (nbVeloDispo <= 0) { continue; }
                }
                

                string station1CoordLongitude = station.position.longitude.ToString().Replace(",", ".");
                string station1CoordLatitude = station.position.latitude.ToString().Replace(",", ".");
                string station1Coord = station1CoordLongitude + "," + station1CoordLatitude;
                try
                {
                    double newDistance = CalculateEuclideanDistance(coordinatesStart, station1Coord); //await OpenStreetMapManager.getDistanceAsync(coordinatesStart, station1Coord, "foot-walking");
                    if (nearestStations.Count < 5)
                    {
                        
                        nearestStations.Add((new InformationStation(station1Coord, station.contractName, station.number.ToString()), 
                            newDistance));
                    }
                    else
                    {
                        // Tri des stations par distance croissante
                        nearestStations.Sort((s1, s2) => s1.distance.CompareTo(s2.distance));
                        // Remplacement de la station avec la plus grande distance si la nouvelle distance est plus petite
                        if (newDistance < nearestStations[4].distance)
                        {
                            nearestStations[4] = (new InformationStation(station1Coord, station.contractName, station.number.ToString()),
                                newDistance);
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine("Bad Request: The request to the OpenRouteService API was not valid.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }

            // Trier la liste finale par distance croissante avant de la renvoyer
            Console.WriteLine("HERE : 7");

            nearestStations.Sort((s1, s2) => s1.distance.CompareTo(s2.distance));
            Console.WriteLine("Resultat : " + nearestStations.Select(s => s.informationStation).ToList());
            for(int i = 0; i < 5; i++)
            {
                Console.WriteLine(nearestStations[i].informationStation.nomContract);
                Console.WriteLine(nearestStations[i].informationStation.numeroStation);
                Console.WriteLine(nearestStations[i].informationStation.coordonnes);
            }
            // Retourner une liste des coordonnées des 5 stations les plus proches
            return nearestStations.Select(s => s.informationStation).ToList();
        }


        private static double CalculateEuclideanDistance(string coordinates1, string coordinates2)
        {
            var coord1 = ParseCoordinates(coordinates1);
            var coord2 = ParseCoordinates(coordinates2);

            double latitudeDifference = coord2.latitude - coord1.latitude;
            double longitudeDifference = coord2.longitude - coord1.longitude;

            return Math.Sqrt(latitudeDifference * latitudeDifference + longitudeDifference * longitudeDifference);
        }

        private static (double latitude, double longitude) ParseCoordinates(string coordinates)
        {
            var parts = coordinates.Split(',');
            if (parts.Length == 2 && double.TryParse(parts[0].Replace(".", ","), out var longitude) && double.TryParse(parts[1].Replace(".", ","), out var latitude))
            {
                return (latitude, longitude);
            }

            throw new ArgumentException("Invalid coordinates format");
        }
    }

    public class InformationStation  {
        public string nomContract { get; }
        public string numeroStation { get; }
        public string coordonnes { get; }

        public InformationStation(string coordonnes, string nomContract= null, string numeroStation = null) {
            this.nomContract = nomContract;
            this.numeroStation = numeroStation;
            this.coordonnes = coordonnes;
        }
    }
}
