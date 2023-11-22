package t;

import javax.jms.*;
import org.apache.activemq.ActiveMQConnectionFactory;

public class ActiveMQ {
    public static void main(String[] args) throws JMSException {
        ActiveMQConnectionFactory factory = new ActiveMQConnectionFactory("tcp://localhost:61616");
        Connection connection = factory.createConnection();
        connection.start();

        Session session = connection.createSession(false, Session.AUTO_ACKNOWLEDGE);

        Destination destination = session.createQueue("test");
        // Créer un consommateur de messages
        MessageConsumer consumer = session.createConsumer(destination);

        // Recevoir le message
        Message receivedMessage = consumer.receive(1000);

        if (receivedMessage instanceof TextMessage) {
            TextMessage textMessage = (TextMessage) receivedMessage;
            String textReceived = textMessage.getText();
            System.out.println("Received: " + textReceived);
        } else {
            System.out.println("Received: " + receivedMessage);
        }

        // Fermer la connexion
        connection.close();
    }
}
