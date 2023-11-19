using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading.Tasks;

namespace PROXY{

    [ServiceContract]
    public interface IServiceProxy
    {
        [OperationContract]
        Task Get(string key);
    }
}
