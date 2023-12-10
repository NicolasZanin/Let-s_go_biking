using System;
using System.Net.Http;
using System.Runtime.Caching;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace TPREST
{
    public class ProxyCacheVelos
    {
        private static MemoryCache cache;
        private static String apiKeyJCDecaux = "0d238e8d9993c554ac2e5a7ce158e357f8457dbe";

        public ProxyCacheVelos()
        {

        }

        public static void Init() {
            cache = MemoryCache.Default;
        }

        //récupère le nombre de vélo dispo
        public static async Task<int> GetNombreVelos(string stationNumber, string nameContract)
        {
            stationAvailabilities objNombreVelo = (stationAvailabilities)cache.Get(stationNumber);
            if (objNombreVelo == null)
            {
                stationAvailabilities availabilities = await setNewElement(stationNumber, nameContract, 2);
                return availabilities.getVelo();
            }

            return objNombreVelo.getVelo();
        }

        //récupère le nombre de place de dépose dispo
        public static async Task<int> GetPlaceVelos(string stationNumber, string nameContract)
        {

            stationAvailabilities objPlaceVelo = (stationAvailabilities)cache.Get(stationNumber);

            if (objPlaceVelo == null)
            {

                stationAvailabilities availabilities = await setNewElement(stationNumber, nameContract, 2);

                return availabilities.getPlace();
            }

            return objPlaceVelo.getPlace();
        }

        //insère les infos d'une stations dans le cache
        private static async Task<stationAvailabilities> setNewElement(string stationNumber, string nameContract, int minuteTime)
        {
            using (var client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync("https://api.jcdecaux.com/vls/v3/stations/" + stationNumber + "?contract=" + nameContract + "&apiKey=" + apiKeyJCDecaux);
                response.EnsureSuccessStatusCode();
                Task<string> responseBody = response.Content.ReadAsStringAsync();
                JsonDocument document = JsonDocument.Parse(responseBody.Result);
                JsonElement jsonBikes = document.RootElement.GetProperty("totalStands").GetProperty("availabilities").GetProperty("bikes");
                JsonElement jsonPlace = document.RootElement.GetProperty("totalStands").GetProperty("availabilities").GetProperty("stands");
                int nombreVelo = jsonBikes.GetInt32();
                int nombrePlace = jsonPlace.GetInt32();
                stationAvailabilities toAdd = new stationAvailabilities(nombreVelo, nombrePlace);
                DateTime date = DateTime.Now;
                date = date.AddMinutes(minuteTime);
                cache.Set(stationNumber, toAdd, date);

                return toAdd;
            }
        }
    }
    [DataContract]
    public class stationAvailabilities
    {
        [DataMember]
        private int veloDispo;
        [DataMember]
        private int placeDispo;

        public stationAvailabilities(int veloDispo, int placeDispo)
        {
            this.veloDispo = veloDispo;
            this.placeDispo = placeDispo;
        }

        public int getPlace() { return this.placeDispo; }
        public int getVelo() { return this.veloDispo; }

    }
}
