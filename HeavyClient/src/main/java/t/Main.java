package t;

import ws.client.generated.*;

import javax.jms.JMSException;
import javax.xml.bind.JAXBElement;
import java.util.List;

public class Main {
    public static void main(String[] args) throws JMSException, InterruptedException {
        /*while(ActiveMQSubscriber.recevoirMessage()){ }
        ActiveMQSubscriber.close();
        exempleUtilisation();*/

        /*System.out.println("Hello World! we are going to test a SOAP client written in Java");
        ServiceRoutingServer serviceRoutingServer = new ServiceRoutingServer();
        IServiceRoutingServer service = serviceRoutingServer.getBasicHttpBindingIServiceRoutingServer();
        System.out.println(service.add(1, 2));*/
    }

    /*private static void exempleUtilisation() throws InterruptedException {
        Proxy pro = new Proxy();
        IServiceProxy proxy = pro.getBasicHttpBindingIServiceProxy();

        Contrat contrat = proxy.getContrat("dublin");
        JAXBElement<ArrayOfStation> jax = contrat.getStations();
        ArrayOfStation aOS = jax.getValue();
        List<Station> stations = aOS.getStation();
        Station stationChoisi = stations.get(0);
        String nameContrat = stationChoisi.getContractName().getValue();
        String numeroStation = stationChoisi.getNumber().toString();
        System.out.println(stations.size());
        int i = 0;
        while (i < 5) {
            System.out.println(proxy.getNombreVelo(numeroStation, nameContrat));
            Thread.sleep(30000);
            i++;
        }

        Contrat contrat2 = proxy.getContrat("lyon");
        JAXBElement<ArrayOfStation> jax2 = contrat2.getStations();
        ArrayOfStation aOS2 = jax2.getValue();
        List<Station> stations2 = aOS2.getStation();
        Station stationChoisi2 = stations2.get(0);
        String nameContrat2 = stationChoisi2.getContractName().getValue();
        String numeroStation2 = stationChoisi2.getNumber().toString();
        System.out.println(stations2.size());
    }*/
}