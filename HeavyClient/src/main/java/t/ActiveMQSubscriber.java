package t;

import javax.jms.*;
import org.apache.activemq.ActiveMQConnectionFactory;

/**
 * Classe pouvant lire les message de la queue de ActiveMQ
 */
public class ActiveMQSubscriber {
    private static Connection connectionActiveMQ;
    private static MessageConsumer messageConsumer;

    /**
     * Constructeur par défaut qui ne doit pas être utilisé
     */
    private ActiveMQSubscriber() {
        throw new IllegalArgumentException("Utility Class");
    }

    /**
     * Effectue la connection vers activeMQ
     * @throws JMSException l'exception JMS
     */
    private static void initActiveMQSubscriber() throws JMSException {
        ActiveMQConnectionFactory factory = new ActiveMQConnectionFactory("tcp://localhost:61616");
        connectionActiveMQ = factory.createConnection();
        connectionActiveMQ.start();

        Session session = connectionActiveMQ.createSession(false, Session.AUTO_ACKNOWLEDGE);

        Destination destination = session.createQueue("test");
        // Créer un consommateur de messages
        messageConsumer = session.createConsumer(destination);
    }

    /**
     * Effectue la connection vers activeMQ
     * @throws JMSException l'exception JMS
     * @return <code>true</code> si le message était dans la pile sinon <code>false</code>
     */
    public static String recevoirMessage() throws JMSException {
        if (connectionActiveMQ == null)
            initActiveMQSubscriber();

        // Recevoir le message
        Message receivedMessage = messageConsumer.receive(1000);
        if (receivedMessage == null)
            return null;

        if (receivedMessage instanceof TextMessage textMessage)
            return textMessage.getText();
        else
            return  receivedMessage.toString();
    }

    /**
     * Fermer la connection vers ActiveMQSubscriber
     * @throws JMSException l'exception JMS
     */
    public static void close() throws JMSException {
        // Fermer la connexion
        connectionActiveMQ.close();
    }
}
