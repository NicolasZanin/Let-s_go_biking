﻿using System;
using System.ServiceModel.Description;
using System.ServiceModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace RoutingServer {
    class HostRoutingServer  {
        static OpenStreetMapManager OM = new OpenStreetMapManager();

        public static async Task<JsonDocument> ComputeItineraire(string start, string end, string locomotion)
        {
            return await OM.ComputeItineraire(start,end,locomotion);
        }
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
                ServiceReference1.IServiceProxy serviceProxy = new ServiceReference1.ServiceProxyClient();
                serviceProxy.Get("dublin");
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("Service is host at " + DateTime.Now.ToString());
            Console.WriteLine("Host is running... Press <Enter> key to stop");
            Console.WriteLine(ComputeItineraire("7.5043,43.7765", "7.2661, 43.7031", "cycling-regular"));
            Console.ReadLine();
            host.Close();
        }
    }
}
