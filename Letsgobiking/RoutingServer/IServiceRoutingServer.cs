using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace RoutingServer {

    [ServiceContract()]
    public interface IServiceRoutingServer{
        [OperationContract()]
        int Add(int num1, int num2);

        [OperationContract()]
        Task<List<String>> ComputeItineraireAsync(string start, string end, string locomotion);

    }
}
