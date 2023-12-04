using System.Collections.Generic;
using System.Text.Json;

namespace RoutingServer
{
    public class Itineraire {
        public double distance; //taille du morceau d'itinéraire 
        public InformationStation informationStation; //info sur la station d'arrivé
        public List<JsonElement> morceauItineraire; //les instructions pour ActiveMQ
        public List<JsonElement> coordonnes; //coordonnées pour construction de la Map

        public Itineraire(string itineraire, InformationStation informationStation = null) {
            morceauItineraire = new List<JsonElement>();
            coordonnes = new List<JsonElement>();
            InitVariable(itineraire);
            this.informationStation = informationStation; //les informations de station pour vérifier la validité de l'itinéraire
        }

        //Initialise avec JSON de openRouteService
        private void InitVariable(string itineraire) {
            JsonDocument json = JsonDocument.Parse(itineraire);
            JsonElement jsonFeatures = json.RootElement.GetProperty("features");
            JsonElement jsonItineraire = jsonFeatures[0].GetProperty("properties").GetProperty("segments")[0];
            distance = jsonItineraire.GetProperty("distance").GetDouble(); //distance

            foreach (JsonElement jsonElt in jsonItineraire.GetProperty("steps").EnumerateArray())
            {
                morceauItineraire.Add(jsonElt); //étapes pour activeMQ
            }

            JsonElement jsonCoordonnes = jsonFeatures[0].GetProperty("geometry").GetProperty("coordinates");

            foreach (JsonElement jsonElt in jsonCoordonnes.EnumerateArray())
            {
                coordonnes.Add(jsonElt); //coordonnées pour affichage map
            }
        }
    }
}
