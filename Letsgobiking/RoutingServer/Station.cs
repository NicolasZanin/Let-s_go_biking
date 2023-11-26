using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Runtime.Serialization;

namespace RoutingServer
{
    [Serializable]
    public class Station : ISerializable
    {
        public int number;
        public string contractName;
        public string name;
        public string address;
        public Position position;

        public int availabilities;

        public Station(
            int number,
            string contractName,
            string name,
            string address,
            Position position,
            int availabilities
        ) {
            this.number = number;
            this.contractName = contractName;
            this.name = name;
            this.address = address;
            this.position = position;
            this.availabilities = availabilities;
        }

        public int Add(int a, int b)
        {
            return a + b;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue( "number", number );
            info.AddValue("contractName", contractName);
            info.AddValue("name", name);
            info.AddValue("address", address);
            info.AddValue("position", position);
            info.AddValue("availabilities", availabilities);
        }
    }

    [Serializable]
    public class Position : ISerializable
    {
        private double v1;
        private double v2;

        public Position(double v1, double v2)
        {
            this.v1 = v1;
            this.v2 = v2;
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("longitude", Longitude);
            info.AddValue("latitude", Latitude);
        }
    }
}