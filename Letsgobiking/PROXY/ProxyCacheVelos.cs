﻿using System;
using System.Net.Http;
using System.Runtime.Caching;
using System.Text.Json;
using System.Threading.Tasks;

namespace TPREST
{
    public class ProxyCacheVelos
    {
        private static MemoryCache cache;

        public ProxyCacheVelos()
        {

        }

        public static void Init() {
            cache = MemoryCache.Default;
        }

        public static async Task<int> GetNombreVelos(string stationNumber, string nameContract)
        {
            object objNombreVelo = cache.Get(stationNumber);
            if (objNombreVelo == null)
            {
                int nombreVelo = await setNewElement(stationNumber, nameContract);
                return nombreVelo;
            }

            return (int)objNombreVelo;
        }

        private static async Task<int> setNewElement(string stationNumber, string nameContract)
        {
            using (var client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync("https://api.jcdecaux.com/vls/v3/stations/" + stationNumber + "?contract=" + nameContract + "&apiKey=0d238e8d9993c554ac2e5a7ce158e357f8457dbe");
                response.EnsureSuccessStatusCode();
                Task<string> responseBody = response.Content.ReadAsStringAsync();
                JsonDocument document = JsonDocument.Parse(responseBody.Result);
                JsonElement json = document.RootElement.GetProperty("totalStands").GetProperty("availabilities").GetProperty("bikes");
                int nombreVelo = json.GetInt32();
                DateTime date = DateTime.Now;
                date = date.AddMinutes(2);
                cache.Set(stationNumber, nombreVelo, date);

                return nombreVelo;
            }
        }
    }
}
