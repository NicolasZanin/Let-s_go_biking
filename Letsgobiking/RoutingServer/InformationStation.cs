namespace RoutingServer
{
    public class InformationStation
    {
        public string nomContract { get; }
        public string numeroStation { get; }
        public string coordonnes { get; }

        public InformationStation(string coordonnes, string nomContract = null, string numeroStation = null)
        {
            this.nomContract = nomContract;
            this.numeroStation = numeroStation;
            this.coordonnes = coordonnes;
        }
    }
}
