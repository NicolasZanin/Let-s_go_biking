using RoutingServer.ProxyService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace RoutingServer
{
    public class CalculStation   {
        private static IServiceProxy proxyService = new ServiceProxyClient();

        //cherche le contrat les plus proche
        public static async Task<string> findNearestContratAsync(string coordinatesStart)
        {
            double distance = double.MaxValue;
            string res = null;
            Station[] listeStations = proxyService.GetOneStationForAllContrat();

            foreach (Station station in listeStations)
            {
                string station1CoordLongitude = station.position.longitude.ToString().Replace(",", ".");
                string station1CoordLatitude = station.position.latitude.ToString().Replace(",", ".");
                string station1Coord = station1CoordLongitude + "," + station1CoordLatitude;
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

        //trouve la station la plus proche à pied
        public static async Task<InformationStation> findNearestStationAsync(string coordinates, string contrat, bool onBike)
        {
            double distance = double.MaxValue;
            InformationStation res = null;
            List<InformationStation> listeStations = await findNearestStationsAsync(coordinates, contrat, onBike);

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

        //trouve les stations les plus proches à vol d'oiseau
        private static async Task<List<InformationStation>> findNearestStationsAsync(string coordinatesStart, string contrat, bool onBike)
        {
            List<(InformationStation informationStation, double distance)> nearestStations = new List<(InformationStation informationStation, double distance)>();

            Contrat getContrat = proxyService.GetContrat(contrat);
            Station[] listeStations = getContrat.stations;
            foreach (Station station in listeStations)
            {

                string stationNumber = station.number.ToString();
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
                catch (HttpRequestException)
                {
                    Console.WriteLine("Bad Request: The request to the OpenRouteService API was not valid.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }

            // Trier la liste finale par distance croissante avant de la renvoyer
            nearestStations.Sort((s1, s2) => s1.distance.CompareTo(s2.distance));
            Console.WriteLine("Resultat : " + nearestStations.Select(s => s.informationStation).ToList());
            for (int i = 0; i < 5; i++)
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
}
