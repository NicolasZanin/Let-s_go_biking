using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TPREST;

namespace PROXY
{
    public class Proxy : IServiceProxy
    {
        public Contrat GetContrat(string name)
        {
            Console.WriteLine("BBBB");
            Console.WriteLine(ProxyCacheContrats.Get(name));

            return ProxyCacheContrats.Get(name);

        }

        public List<Station> GetOneStationForAllContrat()
        {
            return ProxyCacheContrats.GetOneStationForAllContrat();
        }

        public Task<int> GetNombreVelo(string stationNumber, string nameContract)
        {
            return ProxyCacheVelos.GetNombreVelos(stationNumber, nameContract);
        }

        public Task<int> GetPlaceVelo(string stationNumber, string nameContract)
        {
            return ProxyCacheVelos.GetPlaceVelos(stationNumber, nameContract);
        }
    }
}