using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RoutingServer
{
    class OpenStreetMapManager
    {
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
