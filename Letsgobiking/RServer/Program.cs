using PROXY;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RServer
{
        class Program
        {
            static async Task Main(string[] args)
            {
            // Create an instance of the SOAP client
            ServiceProxy client = new ServiceProxy();

                await client.Get("dublin");

            Console.ReadLine();
            }
        }
    }
