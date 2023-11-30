using RoutingServer.ProxyService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Text.Json;
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
            Console.WriteLine(coordinatesStart);
            // call OSM adresse -> coordonnées //ZAZIN
            //Active MQ divisé en étapes //ZAZIN

            List<string> res = new List<string>();


            // trouve sation la plus proche avec des vélos Debut et arrivé
            String startNearestContrat = await findNearestContratAsync(coordinatesStart);
            String endNearestContrat = await findNearestContratAsync(coordinatesEnd);
            Console.WriteLine("CONTRAT");
            Boolean onBike = false;
            String startCoordNearestStation = await findNearestStationAsync(coordinatesStart,startNearestContrat, onBike);
            Console.WriteLine("NEARESTSTATION");
            onBike = true;
            String endCoordNearestStation = await findNearestStationAsync(coordinatesEnd, endNearestContrat, onBike);
            Console.WriteLine("NEARESTSTATION2");

            Double distanceStartToStation1 = await OpenStreetMapManager.getDistanceAsync(coordinatesStart, startCoordNearestStation, "foot-walking");
            Double distanceStation1to2 = await OpenStreetMapManager.getDistanceAsync(startCoordNearestStation, endCoordNearestStation, "cycling-road");
            Double distanceStation2ToEnd = await OpenStreetMapManager.getDistanceAsync(endCoordNearestStation, coordinatesEnd, "foot-walking");
            
            Double distanceByFoot = distanceStartToStation1 + distanceStation2ToEnd;
            Double distanceAllFoot = await OpenStreetMapManager.getDistanceAsync(coordinatesStart, coordinatesEnd, "foot-walking");

            // call OSM si distance pied debut fin plus petite que à station
            if(distanceAllFoot <= distanceByFoot)
            {
                //si ça vaut pas le coup start end de base locomotion walking
                res.Add(await OpenStreetMapManager.ComputeItineraire(coordinatesStart, coordinatesEnd, "foot-walking"));
                return res;
            }
            // sinon itinéraire OSM deput sation 1 à pied
            // OSM station 1 à station 2 velo
            //OSM station 2 end à pied
            //retourner la liste des 3 itinéraire //ZAZIN
            // Essayer de stocker dans OSMManager les Trajets de getDistance au lieu de refaire avec ComputeItinéraire //ZAZIN
            res.Add(await OpenStreetMapManager.ComputeItineraire(coordinatesStart, startCoordNearestStation, "foot-walking"));
            res.Add(await OpenStreetMapManager.ComputeItineraire(startCoordNearestStation, endCoordNearestStation, locomotion));
            res.Add(await OpenStreetMapManager.ComputeItineraire(endCoordNearestStation, coordinatesEnd, "foot-walking"));

            return res;
        }

        private static async Task<string> findNearestContratAsync(string coordinatesStart)
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

        private static async Task<string> findNearestStationAsync(string coordinates, string contrat, Boolean onBike)
        {
            double distance = double.MaxValue;
            String res = null;
            List<string> listeStations = await findNearestStationsAsync(coordinates, contrat, onBike);
            Console.WriteLine("findNearestStationsAsync");
            foreach (String station in listeStations)
            {
                try
                {
                    double newDistance = await OpenStreetMapManager.getDistanceAsync(coordinates, station, "foot-walking");
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

        private static async Task<List<string>> findNearestStationsAsync(string coordinatesStart, string contrat,Boolean onBike)
        {
            List<(string stationCoord, double distance)> nearestStations = new List<(string stationCoord, double distance)>();
            Contrat getContrat = proxyService.GetContrat(contrat);
            Station[] listeStations = getContrat.stations;
            foreach (Station station in listeStations)
            {

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
                String station1Coord = station1CoordLongitude + "," + station1CoordLatitude;
                try
                {
                    double newDistance = CalculateEuclideanDistance(coordinatesStart, station1Coord); //await OpenStreetMapManager.getDistanceAsync(coordinatesStart, station1Coord, "foot-walking");
                    if (nearestStations.Count < 5)
                    {
                        nearestStations.Add((station1Coord, newDistance));
                    }
                    else
                    {
                        // Tri des stations par distance croissante
                        nearestStations.Sort((s1, s2) => s1.distance.CompareTo(s2.distance));
                        // Remplacement de la station avec la plus grande distance si la nouvelle distance est plus petite
                        if (newDistance < nearestStations[4].distance)
                        {
                            nearestStations[4] = (station1Coord, newDistance);
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
            Console.WriteLine("Resultat : " + nearestStations.Select(s => s.stationCoord).ToList());
            // Retourner une liste des coordonnées des 5 stations les plus proches
            return nearestStations.Select(s => s.stationCoord).ToList();
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
}
