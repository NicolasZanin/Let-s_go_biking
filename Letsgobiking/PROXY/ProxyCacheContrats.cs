﻿using System;
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

        public static void InitAllAsync()
        {
            try
            {
                Task<HttpResponseMessage> response2 = client.GetAsync("https://api.jcdecaux.com/vls/v3/contracts?apiKey=0d238e8d9993c554ac2e5a7ce158e357f8457dbe");
                HttpResponseMessage response = response2.Result;
                response.EnsureSuccessStatusCode();
                Task<string> responseBody = response.Content.ReadAsStringAsync();
                JsonDocument document = JsonDocument.Parse(responseBody.Result);

                foreach (JsonElement item in document.RootElement.EnumerateArray())
                {
                    string nameContrat = item.GetProperty("name").GetString();
                    string country_code = item.GetProperty("country_code").GetString();
                    Contrat contrat = new Contrat(nameContrat);
                    ;
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
            return (Contrat)cache.Get(cacheItemName);

        }
    }
}
