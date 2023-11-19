﻿using System;
using System.ServiceModel.Description;
using System.ServiceModel;

namespace RoutingServer {
    class HostRoutingServer  {
        static void Main(string[] args) { 
            Uri httpUrl = new Uri("http://localhost:8091/IServiceRoutingServer/ServiceRoutingServer");

            //Create ServiceHost
            ServiceHost host = new ServiceHost(typeof(ServiceRoutingServer), httpUrl);

            //Add a service endpoint
            host.AddServiceEndpoint(typeof(IServiceRoutingServer), new WebHttpBinding(), "");

            //Enable metadata exchange
            ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
            smb.HttpGetEnabled = true;
            host.Description.Behaviors.Add(smb);

            //Start the Service
            host.Open();

            Console.WriteLine("Service is host at " + DateTime.Now.ToString());
            Console.WriteLine("Host is running... Press <Enter> key to stop");
            Console.ReadLine();

        }
    }
}
