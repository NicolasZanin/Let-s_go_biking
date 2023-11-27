using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Caching;

namespace PROXY
{
    public class GenericProxyCache<T>
    {
        private MemoryCache cache;
        private DateTimeOffset dt_default = ObjectCache.InfiniteAbsoluteExpiration;
        public GenericProxyCache()
        {
            cache = MemoryCache.Default;
        }

        public Task<T> Get(string cacheItemName)
        {
             return GetAsync(cacheItemName, dt_default);

        }

        public Task<T> Get(string cacheItemName, double dtSeconds)
        {
            var expiration = DateTimeOffset.UtcNow.AddSeconds(dtSeconds);
            return GetAsync(cacheItemName, expiration);
        }

        public async Task<T> GetAsync(string cacheItemName, DateTimeOffset dt)
        {
            if (!cache.Contains(cacheItemName) || cache.Get(cacheItemName) == null)
            {
                T newItem = await CreateNewItemAsync(cacheItemName);
                CacheItemPolicy policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = dt
                };
                cache.Set(cacheItemName, newItem, policy);
                return newItem;
            }
            else
            {
                return (T)cache.Get(cacheItemName);
            }
        }

        private async Task<T> CreateNewItemAsync(string cacheItemName)
        {
            Console.WriteLine("Have to Create");
            try
            {
                Contrat newItem = new Contrat(cacheItemName);
                //newItem.stations = await newItem.LoadStations("0d238e8d9993c554ac2e5a7ce158e357f8457dbe");
                //return (T)(object)newItem;
                return default(T);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating new Contrat: {ex.Message}");
                return default(T);
            }
        }

    }

}
