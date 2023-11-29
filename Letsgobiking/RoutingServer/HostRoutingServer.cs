using ActiveMQProducer;
using RoutingServer.ProxyService;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text.Json;
using System.Threading.Tasks;

namespace RoutingServer {
    class HostRoutingServer  {

        static async Task Main(string[] args) {
            Uri httpUrl = new Uri("http://localhost:8091/IServiceRoutingServer/ServiceRoutingServer");
            
            //Create ServiceHost
            ServiceHost host = new ServiceHost(typeof(ServiceRoutingServer), httpUrl);

            //Add a service endpoint
            host.AddServiceEndpoint(typeof(IServiceRoutingServer), new BasicHttpBinding(), "");

            //Enable metadata exchange
            try {
                ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;
                host.Description.Behaviors.Add(smb);
            }
            catch (Exception ex)  {} // Si les métadonnées sont déjà activé

            //Start the Service
            host.Open();
            try {
                Producer.envoyerMessage("Hello", "Bonjour");
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("Service is host at " + DateTime.Now.ToString());
            Console.WriteLine("Host is running... Press <Enter> key to stop");
            Console.ReadLine();
            
            Producer.close();
            host.Close();
        }
    }
}
