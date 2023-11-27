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
            // regarde la station la plus proche du start qui à des vélo dispo

            // départ stationStart à pied ou direct à Pied ? //OPEN STREET MAP DISTANCE
            //regarde la station la plus proche de end qui à des vélos dispo
            // stationENd to end à pied ou direct à Pied ? //OPEN STREET MAP DISTANCE
            //Itineraire à pied de start à stationStart
            //Itineraire à velo station start à StationEnd
            //Itineraire à pied de stationEnd à end
            //OU itineraire à pied start to end
            return await OpenStreetMapManager.ComputeItineraire( start,  end,  locomotion);
        }
    }
}
