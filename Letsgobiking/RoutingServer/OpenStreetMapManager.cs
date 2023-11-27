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

namespace RoutingServer
{
    class OpenStreetMapManager
    {
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

        public static async Task<String> ComputeItineraire(string start, string end, string locomotion)
        {
            HttpClient OSMapi = new HttpClient();
            string apiUrl = "https://api.openrouteservice.org/v2/directions/" + locomotion + "?api_key=5b3ce3597851110001cf6248ef43ce34eac546a6b118b6706bfe11ce&start=" + start + "&end=" + end;
            HttpResponseMessage response = await OSMapi.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
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
