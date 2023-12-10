using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace PROXY
{
    [DataContract]
    public class Contrat
    {
        [DataMember]
        public string name;
        [DataMember]
        public List<Station> stations;

        [DataMember]
        private static string apiKeyJCDecaux = "0d238e8d9993c554ac2e5a7ce158e357f8457dbe";

        // Constructor making a request to JCDecaux API to get stations for the contract
        public Contrat(string contractName)
        {
            name = contractName;
            stations = LoadStations().Result;
        }

        private async Task<List<Station>> LoadStations()
        {
            using (var client = new HttpClient())
            {
                string apiUrl = "https://api.jcdecaux.com/vls/v3/stations?contract=" + name + "&apiKey=" + apiKeyJCDecaux;
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();
                
                string responseBody = await response.Content.ReadAsStringAsync();
                JsonDocument jsonResponseBody = JsonDocument.Parse(responseBody);
                List<Station> stations = new List<Station>();

                foreach (JsonElement elementJsonStation in jsonResponseBody.RootElement.EnumerateArray())
                {
                    double longitude = elementJsonStation.GetProperty("position").GetProperty("longitude").GetDouble();
                    double latitude = elementJsonStation.GetProperty("position").GetProperty("latitude").GetDouble();
                    int numeroStation = elementJsonStation.GetProperty("number").GetInt32();
                    string nameContract = elementJsonStation.GetProperty("contractName").GetString();
                    string nameStation = elementJsonStation.GetProperty("name").GetString();
                    int numberTotalStands = elementJsonStation.GetProperty("totalStands").GetProperty("availabilities").GetProperty("bikes").GetInt32();
                    string address = elementJsonStation.GetProperty("address").GetString();
                    Position positionStation = new Position(longitude, latitude);

                    stations.Add(new Station(numeroStation, nameContract, nameStation, address, positionStation, numberTotalStands););
                }
                return stations;
            }
        }

        public List<Station> getStations()
        {
            return stations;
        }
    }
}
