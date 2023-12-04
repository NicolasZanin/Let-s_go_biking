package t;
import org.json.JSONObject;
import ws.client.generated.*;
import java.util.*;
import javax.jms.JMSException;
import java.util.List;
public class Main {
    public static void main(String[] args) throws JMSException, InterruptedException {
        ServiceRoutingServer serviceRoutingServer = new ServiceRoutingServer();
        IServiceRoutingServer service = serviceRoutingServer.getBasicHttpBindingIServiceRoutingServer();

        System.out.println("Hello World! Welcome to Let's Go Biking!");
        System.out.println("Where do you want to start?");
        Scanner sc = new Scanner(System.in);
        String start = sc.nextLine();
        System.out.println("Where do you want to go?");
        String end = sc.nextLine();
        System.out.println("Recherche d'itinéraire en cours...");

        ArrayOfstring itineraires = service.computeItineraire(start, end);
        List<String> itinerairesList = itineraires.getString();
        ItineraireViewer itineraireViewer = new ItineraireViewer();
        itineraireViewer.showItineraire(itinerairesList);

        boucleLecture(service);
    }

    private static void boucleLecture(IServiceRoutingServer service) throws JMSException, InterruptedException {
        while (true) {
            String message = ActiveMQSubscriber.recevoirMessage();

            if (message == null) {
                service.computeItineraire("", "");
                System.out.println();
                Thread.sleep(2000);
                continue;
            }

            else if (message.startsWith("f") || message.startsWith("V")) {
                System.out.println(message);
                continue;
            }

            else if (message.startsWith("F")) {
                System.out.println(message);
                break;
            }
            try {
                JSONObject jsonObject = new JSONObject(message);
                String instruction = jsonObject.getString("instruction");
                System.out.println(instruction);
            }
            catch (Exception e) {
                System.err.println(e.getMessage());
                System.err.println(message);
            }
        }
        ActiveMQSubscriber.close();
    }
}