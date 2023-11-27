using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text.Json;
using System.Threading.Tasks;

namespace RoutingServer {

    [ServiceContract()]
    public interface IServiceRoutingServer{
        [OperationContract()]
        int Add(int num1, int num2);

        [OperationContract()]
        Task<String> ComputeItineraireAsync(string start, string end, string locomotion);

    }
}
