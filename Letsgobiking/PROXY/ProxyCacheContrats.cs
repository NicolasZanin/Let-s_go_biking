using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Caching;
using System.Text.Json;
using System.Threading.Tasks;

namespace PROXY {
    public class ProxyCacheContrats
    {
        static readonly HttpClient client = new HttpClient();
        private static MemoryCache cache = MemoryCache.Default;
        private static DateTimeOffset dt_default = ObjectCache.InfiniteAbsoluteExpiration;
        private static List<string> nomsContrat = new List<string>();
        private static string apiKeyJCDecaux = "0d238e8d9993c554ac2e5a7ce158e357f8457dbe";

        //recupére une station par contrat
        internal static List<Station> GetOneStationForAllContrat()
        {
            List<Station> res = new List<Station>();
            foreach (string contrat in nomsContrat)
            {
                Contrat contratToAdd = Get(contrat);
                if (contratToAdd.getStations().Count > 0)
                {
                    res.Add(contratToAdd.getStations()[0]);
                }
            }
            return res;
        }

        //recupérer toutes les stations
        public static void InitAllAsync()
        {
            try
            {
                Task<HttpResponseMessage> response2 = client.GetAsync("https://api.jcdecaux.com/vls/v3/contracts?apiKey=" + apiKeyJCDecaux );
                HttpResponseMessage response = response2.Result;
                response.EnsureSuccessStatusCode();
                Task<string> responseBody = response.Content.ReadAsStringAsync();
                JsonDocument jsonResponseBody = JsonDocument.Parse(responseBody.Result);

                foreach (JsonElement item in jsonResponseBody.RootElement.EnumerateArray())
                {
                    string nameContrat = item.GetProperty("name").GetString();
                    nomsContrat.Add(nameContrat);
                    
                    string country_code = item.GetProperty("country_code").GetString();
                    Contrat contrat = new Contrat(nameContrat);
                    
                    cache.Set(nameContrat, contrat, dt_default);
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message:{0}", e.Message);
            }
        }

        public static Contrat Get(string cacheItemName)
        {
            Contrat test = (Contrat) cache.Get(cacheItemName);
            return test;

        }
    }
}
