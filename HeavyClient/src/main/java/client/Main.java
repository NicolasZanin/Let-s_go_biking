package client;
import org.json.JSONException;
import org.json.JSONObject;
import ws.client.generated.*;
import java.util.*;
import java.util.logging.*;
import javax.jms.JMSException;

public class Main {
    private static final String IN_PROCESS = "InProcess";
    private static final Logger log = Logger.getLogger("Main");

    public static void main(String[] args) throws JMSException {
        ServiceRoutingServer serviceRoutingServer = new ServiceRoutingServer();
        IServiceRoutingServer service = serviceRoutingServer.getBasicHttpBindingIServiceRoutingServer();

        initLogger();
        log.info("Hello World! Welcome to Let's Go Biking!");

        while(true) {
            log.info("Where do you want to start?");
            Scanner sc = new Scanner(System.in);
            String start = sc.nextLine();
            log.info("Where do you want to go?");
            String end = sc.nextLine();
            log.info("Recherche d'itinéraire en cours...");
            ArrayOfstring itineraires = service.computeItineraire(start, end);

            if (itineraires.getString().get(0).equals("Error, check your inputs")) {
                log.severe("Votre recherche n'a pas abouti, veuillez vérifier vos entrées.");
            }

            else {
                try {

                    List<String> itinerairesList = itineraires.getString();
                    ItineraireViewer.showItineraire(itinerairesList);

                    // Si le serveur ActiveMQ est démarré alors on lis sur active MQ
                    if (ActiveMQSubscriber.isStart()) {
                        boucleLecture(service);
                        String message = ActiveMQSubscriber.recevoirMessage();

                        while (message != null) {
                            transformMessage(message);
                            message = ActiveMQSubscriber.recevoirMessage();
                        }
                    }

                    // Sinon on récupère tout l'itinéraire
                    else {
                        ArrayOfstring newItineraires = service.computeItineraire(IN_PROCESS, IN_PROCESS);

                        for (String itineraire : newItineraires.getString())
                            log.info(itineraire);
                    }

                    break;
                }
                catch (Exception e){
                    log.severe("An error occured, please try again");
                }
            }
        }

        ActiveMQSubscriber.close();
    }

    /**
     * Initialise le logger
     */
    private static void initLogger() {
        Handler consoleHandler = new ConsoleHandler();
        consoleHandler.setFormatter(new FormatterLogger());
        log.addHandler(consoleHandler);
        log.setUseParentHandlers(false);
    }

    /**
     * Lis les messages de la queue ActiveMQ
     * @param service le service de routing Serveur
     * @throws JMSException l'exception JMS si erreur dans la pile
     * @throws InterruptedException en cas d'interruption lors du sleep
     */

    private static void boucleLecture(IServiceRoutingServer service) throws JMSException, InterruptedException {
        log.info("C'est parti !");
        boolean modeAutomatique = false;

        while (true) {
            String message = ActiveMQSubscriber.recevoirMessage();

            if (message == null) {
                service.computeItineraire(IN_PROCESS, IN_PROCESS);
                log.info("");

                if (modeAutomatique) {
                    Thread.sleep(500);
                }
                else{
                    log.info("Appuyer sur Entrée pour continuer...");
                    log.info("OU appuyer sur A pour passer en mode Automatique");
                    log.info("OU appuyer sur E pour arrêter");

                    Scanner sc = new Scanner(System.in);
                    String next = sc.nextLine();

                    if (next.equalsIgnoreCase("e")) {
                        break;
                    }

                    if (next.equalsIgnoreCase("a")) {
                        modeAutomatique = true;
                    }
                }
            }

            // Si le message commence par f pour 'fini velo' ou V pour 'Vélo'
            else if (message.startsWith("f") || message.startsWith("V")) {
                log.info(message);
            }

            // Si le message commence par F pour 'Fini'
            else if (message.contains("FINI")) {
                log.info(message);
                break;
            }

            else {
                transformMessage(message);
            }
        }
    }


    /**
     * Transforme le json en string et affiche l'instruction
     * @param message le json à transformer
     */
    private static void transformMessage(String message) {
        try {
            JSONObject jsonObject = new JSONObject(message);
            String instruction = jsonObject.getString("instruction");
            log.info(instruction);
        } catch (JSONException jsonException) {
            log.severe(jsonException.getMessage());
        }
    }
}