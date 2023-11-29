package t;
import ws.client.generated.*;
import javax.swing.JFrame;
import org.jxmapviewer.JXMapViewer;
import org.jxmapviewer.OSMTileFactoryInfo;
import org.jxmapviewer.viewer.DefaultTileFactory;
import org.jxmapviewer.viewer.GeoPosition;
import org.jxmapviewer.viewer.TileFactoryInfo;

import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.io.Console;
import java.util.*;

import javax.swing.JFrame;

import org.jxmapviewer.JXMapViewer;
import org.jxmapviewer.OSMTileFactoryInfo;
import org.jxmapviewer.painter.CompoundPainter;
import org.jxmapviewer.painter.Painter;
import org.jxmapviewer.viewer.DefaultTileFactory;
import org.jxmapviewer.viewer.DefaultWaypoint;
import org.jxmapviewer.viewer.GeoPosition;
import org.jxmapviewer.viewer.TileFactoryInfo;
import org.jxmapviewer.viewer.Waypoint;
import org.jxmapviewer.viewer.WaypointPainter;
import java.awt.BorderLayout;
import java.beans.PropertyChangeEvent;
import java.beans.PropertyChangeListener;
import java.io.File;

import javax.swing.JFrame;
import javax.swing.JLabel;
import javax.swing.WindowConstants;
import javax.swing.event.MouseInputListener;

import org.jxmapviewer.JXMapViewer;
import org.jxmapviewer.OSMTileFactoryInfo;
import org.jxmapviewer.cache.FileBasedLocalCache;
import org.jxmapviewer.input.CenterMapListener;
import org.jxmapviewer.input.PanKeyListener;
import org.jxmapviewer.input.PanMouseInputListener;
import org.jxmapviewer.input.ZoomMouseWheelListenerCursor;
import org.jxmapviewer.viewer.DefaultTileFactory;
import org.jxmapviewer.viewer.GeoPosition;
import org.jxmapviewer.viewer.TileFactoryInfo;
import java.awt.Color;
import java.io.File;
import java.util.Arrays;
import java.util.HashSet;
import java.util.Set;

import javax.swing.JFrame;
import javax.swing.event.MouseInputListener;

import org.jxmapviewer.JXMapViewer;
import org.jxmapviewer.VirtualEarthTileFactoryInfo;
import org.jxmapviewer.cache.FileBasedLocalCache;
import org.jxmapviewer.input.CenterMapListener;
import org.jxmapviewer.input.PanKeyListener;
import org.jxmapviewer.input.PanMouseInputListener;
import org.jxmapviewer.input.ZoomMouseWheelListenerCenter;
import org.jxmapviewer.viewer.DefaultTileFactory;
import org.jxmapviewer.viewer.GeoPosition;
import org.jxmapviewer.viewer.TileFactoryInfo;
import org.jxmapviewer.viewer.WaypointPainter;
import java.awt.Point;
import java.awt.Rectangle;
import java.awt.event.MouseEvent;
import java.awt.event.MouseMotionListener;
import java.awt.geom.Point2D;

import ws.client.generated.*;

import javax.jms.JMSException;
import javax.xml.bind.JAXBElement;
import java.util.List;

public class Main {
    public static void main(String[] args) throws JMSException, InterruptedException {
        /*while(ActiveMQSubscriber.recevoirMessage()){ }
        ActiveMQSubscriber.close();
        exempleUtilisation();*/

        System.out.println("Hello World! we are going to test a SOAP client written in Java");
        ServiceRoutingServer serviceRoutingServer = new ServiceRoutingServer();
        IServiceRoutingServer service = serviceRoutingServer.getBasicHttpBindingIServiceRoutingServer();
        System.out.println(service.add(1,2));
        System.out.println("Hello World! Welcome to Let's Go Biking!");
        System.out.println("Where do you want to start?");
        Scanner sc = new Scanner(System.in);
        String start = "2.295753,49.894067";
        String Menton ="7.49754,43.774481";
        System.out.println("Where do you want to go?");
        String end = "2.30082,49.804325";
        String Antibes = "7.125102,43.580418";
        ArrayOfstring itineraires = service.computeItineraire(start, Antibes, "cycling-regular");
        List<String> itinerairesList = itineraires.getString();
        int i = 0;
        for (String itineraire : itinerairesList) {
            if(i == 1){
                ItineraireViewer.showItineraire(itineraire,"BLUE");
            }else{
                ItineraireViewer.showItineraire(itineraire,"RED");
            }
            i++;
        }
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