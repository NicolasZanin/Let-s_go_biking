package client;
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
        while(true) {
            System.out.println("Where do you want to start?");
            Scanner sc = new Scanner(System.in);
            String start = sc.nextLine();
            System.out.println("Where do you want to go?");
            String end = sc.nextLine();
            System.out.println("Recherche d'itinéraire en cours...");
            try {
                ArrayOfstring itineraires = service.computeItineraire(start, end);


            if (itineraires.getString().get(0).equals("Error, check your inputs")) {
                System.out.println("Votre recherche n'a pas abouti, veuillez vérifier vos entrées.");
                continue;
            }

            List<String> itinerairesList = itineraires.getString();
            ItineraireViewer itineraireViewer = new ItineraireViewer();
            itineraireViewer.showItineraire(itinerairesList);


            if (ActiveMQSubscriber.isStart()) {
                boucleLecture(service);
                String message = ActiveMQSubscriber.recevoirMessage();
                while (message != null) {
                    message = ActiveMQSubscriber.recevoirMessage();
                }
            }else {
                ArrayOfstring newItineraires = service.computeItineraire("InProcess", "InProcess");

                for (String itineraire : newItineraires.getString())
                    System.out.println(itineraire);
            }
            }catch (Exception e){
                System.out.println("An error occured,It seems that too many calls have been made to openRouteService, so wait a bit and please try again");
            }
        }
    }

    private static void boucleLecture(IServiceRoutingServer service) throws JMSException, InterruptedException {
        System.out.println("C'est parti !");
        boolean modeAutomatique = false;
        while (true) {
            String message = ActiveMQSubscriber.recevoirMessage();

            if (message == null) {
                service.computeItineraire("InProcess", "InProcess");
                System.out.println();
                if (modeAutomatique) {
                    Thread.sleep(500);
                }else{
                    System.out.println("Appuyer sur Entrée pour continuer...");
                    System.out.println("OU appuyer sur A pour passer en mode Automatique");
                    System.out.println("OU appuyer sur E pour arrêter");

                    Scanner sc = new Scanner(System.in);
                    String next = sc.nextLine();
                    if (next.equals("E") || next.equals("e")) {
                        break;
                    }
                    if (next.equals("A") || next.equals("a")) {
                        modeAutomatique = true;
                    }
                }
                continue;
            }

            else if (message.startsWith("f") || message.startsWith("V")) {
                System.out.println(message);
                continue;
            }

            else if (message.contains("FINI")) {
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
    }
}