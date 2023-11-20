using RouteServer.ServiceProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RouteServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create an instance of the SOAP client
            IServiceProxy client = new ServiceProxyClient();

            // Call methods on the client to perform operations
            Console.WriteLine("Démarrage : ...");
            String res = "test";
            res = client.Get("dublin");
            Console.WriteLine(res);
            Console.WriteLine("FIN : ...");


            Console.ReadLine();
        }
    }
}
