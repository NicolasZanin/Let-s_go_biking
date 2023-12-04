using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace RoutingServer {

    [ServiceContract()]
    public interface IServiceRoutingServer{
        [OperationContract()]
        Task<List<String>> ComputeItineraireAsync(string start, string end);

    }
}
