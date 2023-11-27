using System.Runtime.Serialization;

namespace PROXY
{

    [DataContract]
    public class Station
    {
        [DataMember]
        public int number;
        [DataMember]
        public string contractName;
        [DataMember]
        public string name;
        [DataMember]
        public string address;
        [DataMember]
        public Position position;
        [DataMember]
        public int availabilities;

        public Station(int number, string contractName, string name, string address, Position position, int availabilities)
        {
            this.number = number;
            this.contractName = contractName;
            this.name = name;
            this.address = address;
            this.position = position;
            this.availabilities = availabilities;
        }
    }

    [DataContract]
    public class Position
    {
        [DataMember]
        private double latitude;
        [DataMember]
        private double longitude;

        public Position(double longitude, double latitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;
        }
    }
}