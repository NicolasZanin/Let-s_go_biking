using System;
using Apache.NMS;
using Apache.NMS.ActiveMQ;

namespace ActiveMQProducer {
    /// <summary> Cette classe permet d'utiliser ActiveMQ pour produire des messages </summary>
    public class Producer {
        private static IConnection connectionProducer;
        private static ISession sessionProducer;
        private static IDestination destinationProducer;

        static void Main(String[] args){

        }

        /// <summary> Cette méthode initialise ActiveMQ </summary>
        private static void initProducer() {
            Uri connecturi = new Uri("activemq:tcp://localhost:61616");

            // Create a Connection Factory
            ConnectionFactory connectionFactory = new ConnectionFactory(connecturi);

            // Create a single Connection from the Connection Factory.
            connectionProducer = connectionFactory.CreateConnection();
            connectionProducer.Start();

            // Create a session from the Connection.
            sessionProducer = connectionProducer.CreateSession();

            // Use the session to target a queue.
            destinationProducer = sessionProducer.GetQueue("test");
        }

        /// <summary> Envoie des message à la Queue de ActiveMQ </summary>
        public static void envoyerMessage(params String[] messages) {
            if (connectionProducer == null)
                initProducer();

            // Create a Producer targetting the selected queue.
            IMessageProducer producer = sessionProducer.CreateProducer(destinationProducer);

            // You may configure everything to your needs, for instance:
            producer.DeliveryMode = MsgDeliveryMode.NonPersistent;

            for (int i = 0; i < messages.Length; i++) {
                ITextMessage message = sessionProducer.CreateTextMessage(messages[i]);
                producer.Send(message);
            }
        }

        /// <summary> Cette méthode ferme la connection vers ActiveMQ </summary>
        public static void close() {
            connectionProducer.Close();
            sessionProducer.Close();
        }

        static void Main(string[] args)
        {
        }
    }
}
