package t;

import javax.jms.JMSException;

public class Main {
    public static void main(String[] args) throws JMSException {
        while(ActiveMQSubscriber.recevoirMessage()){ }
        ActiveMQSubscriber.close();

        /*System.out.println("Hello World! we are going to test a SOAP client written in Java");
        ServiceRoutingServer serviceRoutingServer = new ServiceRoutingServer();
        IServiceRoutingServer service = serviceRoutingServer.getBasicHttpBindingIServiceRoutingServer();
        System.out.println(service.add(1, 2));*/
    }
}