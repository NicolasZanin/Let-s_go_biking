using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoutingServer { 
    class ServiceRoutingServer : IServiceRoutingServer
    {

        //renvoie l'itinéraire global
        public async Task<List<string>> ComputeItineraireAsync(string coordinatesStart, string coordinatesEnd)
        {
            return await OpenStreetMapManager.ComputeItineraire(coordinatesStart, coordinatesEnd);
        }

    }
}
