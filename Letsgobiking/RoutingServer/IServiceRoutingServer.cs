using System.Runtime.Serialization;
using System.ServiceModel;

namespace RoutingServer {

    [ServiceContract]
    public interface IServiceRoutingServer{
        [OperationContract]
        int Add(int num1, int num2);

    }
}
