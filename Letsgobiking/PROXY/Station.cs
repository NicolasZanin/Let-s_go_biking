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

    public class Station
    {
        public int number;
        public string contractName;
        public string name;
        public string addres;
        public Position position;

        public int availabilities;

        public Station(
        int number,
        string contractName,
        string name,
        string address,
        Position position,
        int availabilities
    )
        {
            this.number = number;
            this.contractName = contractName;
            this.name = name;
            this.addres = address;
            this.position = position;
            this.availabilities = availabilities;
        }

    }

    public class Position
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
    }
}