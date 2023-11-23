using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RoutingServer { 
    class ServiceRoutingServer : IServiceRoutingServer
    {
        public int Add(int num1, int num2) {
            return num1 + num2;
        }

        public async Task<String> ComputeItineraire(string start, string end, string locomotion) {
            return await OpenStreetMapManager.ComputeItineraire( start,  end,  locomotion);
        }
    }
}
