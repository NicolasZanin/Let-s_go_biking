package t;
import ws.client.generated.*;

public class Main {
    public static void main(String[] args) {
        System.out.println("Hello World! we are going to test a SOAP client written in Java");
        ServiceRoutingServer serviceRoutingServer = new ServiceRoutingServer();
        IServiceRoutingServer service = serviceRoutingServer.getBasicHttpBindingIServiceRoutingServer();
        System.out.println(service.add(1, 2));
    }
}