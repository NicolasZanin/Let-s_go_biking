using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RoutingServer
{
    public class Itineraire {
        public double distance;

        public List<JsonElement> morceauItineraire;
        public List<JsonElement> coordonnes;

        public Itineraire(string itineraire) {
            morceauItineraire = new List<JsonElement>();
            coordonnes = new List<JsonElement>();
            InitVariable(itineraire);
        }

        private void InitVariable(string itineraire) {
            JsonDocument json = JsonDocument.Parse(itineraire);
            JsonElement jsonFeatures = json.RootElement.GetProperty("features");
            JsonElement jsonItineraire = jsonFeatures[0].GetProperty("properties").GetProperty("segments")[0];
            distance = jsonItineraire.GetProperty("distance").GetDouble();

            foreach (JsonElement jsonElt in jsonItineraire.GetProperty("steps").EnumerateArray())
            {
                morceauItineraire.Add(jsonElt);
            }

            JsonElement jsonCoordonnes = jsonFeatures[0].GetProperty("geometry").GetProperty("coordinates");

            foreach (JsonElement jsonElt in jsonCoordonnes.EnumerateArray())
            {
                coordonnes.Add(jsonElt);
            }
        }
    }
}
