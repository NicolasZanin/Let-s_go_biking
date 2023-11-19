﻿using System.Runtime.Serialization;
using System.ServiceModel;

namespace RoutingServer {

    [ServiceContract(Namespace = "http://tempuri.org/")]
    public interface IServiceRoutingServer{
        [OperationContract]
        int Add(int num1, int num2);

        [OperationContract]
        int Substract(int num1, int num2);

    }
}
