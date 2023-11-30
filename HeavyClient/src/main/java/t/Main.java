package t;
import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import org.json.JSONObject;
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
        System.out.println("Hello World! we are going to test a SOAP client written in Java");
        ServiceRoutingServer serviceRoutingServer = new ServiceRoutingServer();
        IServiceRoutingServer service = serviceRoutingServer.getBasicHttpBindingIServiceRoutingServer();
        System.out.println(service.add(1,2));
        System.out.println("Hello World! Welcome to Let's Go Biking!");
        System.out.println("Where do you want to start?");
        Scanner sc = new Scanner(System.in);
        String start = "2.295753,49.894067";
        String Menton ="7.49754,43.774481";
        String Madrid ="-3.7037902,40.4167754";
        System.out.println("Where do you want to go?");
        String end = "2.30082,49.804325";
        String Antibes = "7.125102,43.580418";
        String Paris = "2.333333,48.866667";

        ArrayOfstring itineraires = service.computeItineraire("Menton", "Paris", "cycling-regular");
        List<String> itinerairesList = itineraires.getString();
        ItineraireViewer.showItineraire(itinerairesList);

        boucleLecture(service);
    }

    private static void boucleLecture(IServiceRoutingServer service) throws JMSException, InterruptedException {
        while (true) {
            String message = ActiveMQSubscriber.recevoirMessage();

            if (message == null) {
                service.computeItineraire("", "", "cycling-regular");
                System.out.println();
                Thread.sleep(5000);
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