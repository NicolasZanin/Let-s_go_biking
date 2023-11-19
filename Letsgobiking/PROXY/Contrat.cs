using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;

namespace PROXY
{

    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class Contrat
    {
        public string name;
        public List<Station> stations;

        // Constructor making a request to JCDecaux API to get stations for the contract
        public Contrat(string contractName)
        {
            name = contractName;
            stations = new List<Station>();
        }

        public async Task<List<Station>> LoadStations(string apiKey)
        {
            using (var client = new HttpClient())
            {
                string apiUrl = "https://api.jcdecaux.com/vls/v3/stations?contract="+name+"&apiKey=0d238e8d9993c554ac2e5a7ce158e357f8457dbe";
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                JsonDocument document = JsonDocument.Parse(responseBody);
                List<Station> stations = new List<Station>();


                foreach (JsonElement item in document.RootElement.EnumerateArray())
                {
                    Station toAdd = new Station(item.GetProperty("number").GetInt32(),
                        item.GetProperty("contractName").GetString(),
                        item.GetProperty("name").GetString(),
                        item.GetProperty("address").GetString(),
                        new Position(item.GetProperty("position").GetProperty("latitude").GetDouble(), item.GetProperty("position").GetProperty("longitude").GetDouble()),
                        item.GetProperty("totalStands").GetProperty("availabilities").GetProperty("bikes").GetInt32()
                        );
                    stations.Add(toAdd);
                }
                return stations;
            }
        }

        public List<Station> getStations()
        {
            return this.stations;
        }
    }
}
