using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEST
{
    class Program
    {
        private static ServiceReference1.IServiceRoutingServer service = new ServiceReference1.ServiceRoutingServerClient();
        static void Main(string[] args)
        {
            service.ComputeItineraire("7.5043,43.7765", "7.2661,43.7031", "foot-walking");
        }
    }
}
