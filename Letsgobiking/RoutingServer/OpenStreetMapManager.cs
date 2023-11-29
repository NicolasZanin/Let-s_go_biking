using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace RoutingServer
{
    class OpenStreetMapManager
    {

        private static List<Itineraire> itineraires = new List<Itineraire>();

        public static async Task<string> convertAddressToPointAsync(string address) {
            string apiURL = "https://api.openrouteservice.org/geocode/search/structured?api_key=5b3ce3597851110001cf6248ef43ce34eac546a6b118b6706bfe11ce&address=";
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
            JsonElement bbox = document.RootElement.GetProperty("bbox");
            return bbox[0] + "," + bbox[1];
        }

        public static async Task<String> ComputeItineraire(string start, string end, string locomotion, int nEtape)
        {
            if (nEtape == 0) {
                itineraires.Clear();
                return await ComputeItineraireEtape0(start, end, locomotion);
            }
            /*HttpClient OSMapi = new HttpClient();
            string apiUrl = "https://api.openrouteservice.org/v2/directions/" + locomotion + "?api_key=5b3ce3597851110001cf6248ef43ce34eac546a6b118b6706bfe11ce&start=" + start + "&end=" + end;
            HttpResponseMessage response = await OSMapi.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;*/
            return "";
        }

        private static async Task<string> ComputeItineraireEtape0(string start, string end, string locomotion) {
            // trouve la station la plus proche avec des vélos Debut et arrivé
            string startNearestContrat = await ServiceRoutingServer.findNearestContratAsync(start);
            string endNearestContrat = await ServiceRoutingServer.findNearestContratAsync(end);
            string startCoordNearestStation = await ServiceRoutingServer.findNearestStationAsync(start, startNearestContrat);
            string endCoordNearestStation = await ServiceRoutingServer.findNearestStationAsync(end, endNearestContrat);

            Itineraire itineraireStartToStation1 = await getItineraireAsync(start, startCoordNearestStation, "foot-walking");
            Itineraire itineraireStation1to2 = await getItineraireAsync(startCoordNearestStation, endCoordNearestStation, "cycling-regular");
            Itineraire itineraireeStation2ToEnd = await getItineraireAsync(endCoordNearestStation, end, "foot-walking");
            double distanceTotaleBike = itineraireStartToStation1.distance + itineraireStation1to2.distance + itineraireeStation2ToEnd.distance;
            Itineraire itineraireAllFoot = await getItineraireAsync(start, end, "foot-walking");

            // call OSM si distance pied debut fin plus petite que à station
            if (distanceTotaleBike > itineraireAllFoot.distance * 1.5) {
                    
                //si ça vaut pas le coup start end de base locomotion walking
                itineraires.Add(itineraireAllFoot);
                return JsonSerializer.Serialize(new List<JsonElement>[]{ itineraireAllFoot.coordonnes });
            }

            itineraires.Add(itineraireStartToStation1);
            itineraires.Add(itineraireStation1to2);
            itineraires.Add(itineraireeStation2ToEnd);

            List<JsonElement>[] listItineraire = { itineraireStartToStation1.coordonnes, itineraireStation1to2.coordonnes, itineraireeStation2ToEnd.coordonnes };
            return JsonSerializer.Serialize(listItineraire);
        }

        internal static async Task<Itineraire> getItineraireAsync(string coordinatesStart, string station1Coord, string locomotion)
        {
            HttpClient OSMapi = new HttpClient();
            string apiUrl = "https://api.openrouteservice.org/v2/directions/" + locomotion + "?api_key=5b3ce3597851110001cf6248ef43ce34eac546a6b118b6706bfe11ce&start=" + coordinatesStart + "&end=" + station1Coord;
            //Console.WriteLine(apiUrl);
            HttpResponseMessage response = await OSMapi.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return new Itineraire(responseBody);
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
