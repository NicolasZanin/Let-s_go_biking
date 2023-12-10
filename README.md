# Let-s_go_biking
Projet mdwsoc Let's go biking


## Preparation pour le lancement du projet

- Lancer activeMQ avec la commande suivante : `activemq start` sous le port 61616
- Lancer le serveur PROXY avant le serveur Routing Server (Let-s_go_biking\Letsgobiking\PROXY\bin\Debug\TPREST.exe) en mode administrateur
- Ajouter le server proxy dans le connected service du serveur Routing Server avec l'URL suivante: `http://localhost:8090/IProxy/Proxy?wsdl`
- Lancer le serveur Routing Server (Let-s_go_biking\Letsgobiking\RoutingServer\bin\Debug\RoutingServer) en mode administrateur
- Ajouter dans le pom.xml dans la balise wsdlURL l'URL suivante : `http://localhost:8091/IServiceRoutingServer/ServiceRoutingServer?wsdl`
- Lancer le client java

## Lancement du projet

- Lancer le serveur PROXY avant le serveur Routing Server (Let-s_go_biking\Letsgobiking\PROXY\bin\Debug\TPREST.exe) en mode administrateur
- Lancer le serveur Routing Server (Let-s_go_biking\Letsgobiking\RoutingServer\bin\Debug\RoutingServer) en mode administrateur
- Lancer Main de Java

Pour mettre une addresse de départ ou d'arrivé, la forme général est: `2400 routes des dolines, 06560 Valbonne`
Mais fonctionne aussi de cette façon `2400 routes des dolines`
Ou même `Valbonne`