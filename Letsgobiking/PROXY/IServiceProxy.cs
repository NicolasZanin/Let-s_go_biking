using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading.Tasks;

namespace PROXY{

    [ServiceContract()]
    public interface IServiceProxy
    {
        [OperationContract]
        Contrat GetContrat(string name);

        [OperationContract]
        Task<int> GetNombreVelo(string stationNumber, string nameContract);
    }
}
