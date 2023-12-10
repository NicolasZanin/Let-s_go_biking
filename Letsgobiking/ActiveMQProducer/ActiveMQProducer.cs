using System;
using Apache.NMS;
using Apache.NMS.ActiveMQ;

namespace ActiveMQProducer {
    /// <summary> Cette classe permet d'utiliser ActiveMQ pour produire des messages </summary>
    public class Producer {
        private static IConnection connectionProducer;
        private static ISession sessionProducer;
        private static IDestination destinationProducer;
        private static bool isLogin = true;

        static void Main(string[] args){

        }

        /// <summary> Cette méthode initialise ActiveMQ </summary>
        private static void initProducer() {
            Uri uriActiveMQ = new Uri("activemq:tcp://localhost:61616");

            // Create a Connection Factory
            ConnectionFactory connectionFactoryActiveMQ = new ConnectionFactory(uriActiveMQ);

            // Create a single Connection from the Connection Factory.
            try
            {
                connectionProducer = connectionFactoryActiveMQ.CreateConnection();
                connectionProducer.Start();

                // Create a session from the Connection.
                sessionProducer = connectionProducer.CreateSession();

                // Use the session to target a queue.
                destinationProducer = sessionProducer.GetQueue("itineraireQueue");
            }
            // Si la connection n'a pas pu s'établir
            catch (Exception) {
                isLogin = false;
            }
        }

        /// <summary> Envoie des message à la Queue de ActiveMQ </summary>
        public static void envoyerMessage(params string[] messages) {
            // Si l'activeMq n'est pas initialisé
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

        /// <summary> Vérifie si c'est connecter </summary>
        /// <returns><code>true</code> si le serveur activemq est connecter</returns>
        public static bool estConnecter() {
            // Si l'activeMq n'est pas initialisé
            if (connectionProducer == null)
                initProducer();

            return isLogin;
        }

        /// <summary> Cette méthode ferme la connection vers ActiveMQ </summary>
        public static void close() {
            if (estConnecter()) {
                connectionProducer.Close();
                sessionProducer.Close();
            }
        }
    }
}
