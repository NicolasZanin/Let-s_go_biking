using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace RoutingServer { 
    class ServiceRoutingServer : IServiceRoutingServer
    {
        public int Add(int num1, int num2) {
            return num1 + num2;
        }

        public int Substract(int num1, int num2) {
            return num1 - num2;
        }
    }
}
