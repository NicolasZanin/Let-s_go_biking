using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PROXY;

namespace PROXY
{
    public class Proxy : IServiceProxy
    {
        private GenericProxyCache<Contrat> cache;

        public Proxy()
        {
            cache = new GenericProxyCache<Contrat>();
        }

        static async Task Main()
        {
            Proxy proxy = new Proxy();
            await proxy.Get("dublin");
            await proxy.Get("dublin");
            Console.ReadKey();
        }

        public async Task Get(string key)
        {
            Contrat contratInfo = await cache.Get(key);

            if (contratInfo != null)
            {
                List<Station> stations = contratInfo.getStations();
                Console.WriteLine("Stations in " + contratInfo.name);
                Console.WriteLine("Stations in " + contratInfo.stations.Count);

                /*foreach (var station in stations)
                {
                    Console.WriteLine(station.number);
                }*/
            }
            else
            {
                Console.WriteLine($"Contract {key} not found in the cache.");
                Console.ReadKey();
            }
        }
    }
}