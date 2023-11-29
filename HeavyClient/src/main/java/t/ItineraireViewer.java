package t;

import org.json.JSONArray;
import org.json.JSONObject;
import org.jxmapviewer.JXMapViewer;
import org.jxmapviewer.OSMTileFactoryInfo;
import org.jxmapviewer.input.PanMouseInputListener;
import org.jxmapviewer.painter.CompoundPainter;
import org.jxmapviewer.painter.Painter;
import javax.swing.event.MouseInputListener;
import java.awt.*;
import java.beans.PropertyChangeEvent;
import java.io.File;
import java.util.*;
import javax.swing.JFrame;
import org.jxmapviewer.viewer.DefaultTileFactory;
import org.jxmapviewer.viewer.GeoPosition;
import org.jxmapviewer.viewer.TileFactoryInfo;
import org.jxmapviewer.viewer.DefaultWaypoint;
import org.jxmapviewer.viewer.Waypoint;
import org.jxmapviewer.viewer.WaypointPainter;

import java.beans.PropertyChangeListener;
import javax.swing.JLabel;
import javax.swing.WindowConstants;
import org.jxmapviewer.cache.FileBasedLocalCache;
import org.jxmapviewer.input.CenterMapListener;
import org.jxmapviewer.input.PanKeyListener;
import org.jxmapviewer.input.ZoomMouseWheelListenerCursor;
import java.util.Arrays;
import java.util.HashSet;
import java.util.List;
import java.util.Set;
import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;

public class ItineraireViewer {
    private static final Color[] colors = {Color.RED, Color.BLUE, Color.GREEN};

    public static List<GeoPosition> createTrack(JSONArray itineraire) {
        List<GeoPosition> track = new ArrayList<>();

        for (int i = 0; i < itineraire.length(); i++) {
            JSONArray coord = itineraire.getJSONArray(i);
            GeoPosition pos = new GeoPosition(coord.getDouble(1), coord.getDouble(0));
            track.add(pos);
        }

        return track;
    }

    public static void showItineraire(String itineraire) {

    // public static void showItineraire(String itineraire,String color) {
        // Create a TileFactoryInfo for OpenStreetMap
        TileFactoryInfo info = new OSMTileFactoryInfo();
        DefaultTileFactory tileFactory = new DefaultTileFactory(info);

        // Setup local file cache
        File cacheDir = new File(System.getProperty("user.home") + File.separator + ".jxmapviewer2");
        tileFactory.setLocalCache(new FileBasedLocalCache(cacheDir, false));

        // Setup JXMapViewer
        final JXMapViewer mapViewer = new JXMapViewer();
        mapViewer.setTileFactory(tileFactory);

        // Add interactions
        MouseInputListener mia = new PanMouseInputListener(mapViewer);
        mapViewer.addMouseListener(mia);
        mapViewer.addMouseMotionListener(mia);

        mapViewer.addMouseListener(new CenterMapListener(mapViewer));

        mapViewer.addMouseWheelListener(new ZoomMouseWheelListenerCursor(mapViewer));

        mapViewer.addKeyListener(new PanKeyListener(mapViewer));

        // Add a selection painter
        SelectionAdapter sa = new SelectionAdapter(mapViewer);
        SelectionPainter sp = new SelectionPainter(sa);
        mapViewer.addMouseListener(sa);
        mapViewer.addMouseMotionListener(sa);
        mapViewer.setOverlayPainter(sp);

        // Display the viewer in a JFrame
        final JFrame frame = new JFrame();
        frame.setLayout(new BorderLayout());
        String text = "Use left mouse button to pan, mouse wheel to zoom";
        frame.add(new JLabel(text), BorderLayout.NORTH);
        frame.add(mapViewer);
        frame.setSize(800, 600);
        frame.setDefaultCloseOperation(WindowConstants.EXIT_ON_CLOSE);
        frame.setVisible(true);
        // Create a track from the geo-positions

        JSONArray jsonArray = new JSONArray(itineraire);
        List<GeoPosition>[] positions = new List[jsonArray.length()];

        for (int i = 0; i < jsonArray.length(); i++) {
            positions[i] = createTrack(jsonArray.getJSONArray(i));
        }

        for (int i = 0; i < positions.length; i++) {
            List<GeoPosition> track = positions[i];

            RoutePainter routePainter = new RoutePainter(track, colors[i]);

            // Create waypoints from the geo-positions
            Set<Waypoint> waypoints = new HashSet<Waypoint>(Arrays.asList(
                    new DefaultWaypoint(track.get(0)),
                    new DefaultWaypoint(track.get(track.size() - 1))));

            // Create a waypoint painter that takes all the waypoints
            WaypointPainter<Waypoint> waypointPainter = new WaypointPainter<Waypoint>();
            waypointPainter.setWaypoints(waypoints);

            // Create a compound painter that uses both the route-painter and the waypoint-painter
            List<Painter<JXMapViewer>> painters = new ArrayList<Painter<JXMapViewer>>();
            painters.add(routePainter);
            painters.add(waypointPainter);

            CompoundPainter<JXMapViewer> painter = new CompoundPainter<JXMapViewer>(painters);
            mapViewer.setOverlayPainter(painter);
            mapViewer.addPropertyChangeListener("zoom", new PropertyChangeListener() {
                @Override
                public void propertyChange(PropertyChangeEvent evt) {
                    updateWindowTitle(frame, mapViewer);
                }
            });

            mapViewer.addPropertyChangeListener("center", new PropertyChangeListener() {
                @Override
                public void propertyChange(PropertyChangeEvent evt) {
                    updateWindowTitle(frame, mapViewer);
                }
            });

            updateWindowTitle(frame, mapViewer);
        }
        // Set the focus
        mapViewer.zoomToBestFit(new HashSet<GeoPosition>(setDePosition(positions)), 0.7);
    }

    private static Set<GeoPosition> setDePosition(List<GeoPosition>[] list) {
        Set<GeoPosition> set = new HashSet<>();

        for (int i = 0; i < list.length; i++) {
            set.addAll(list[i]);
        }

        return set;
    }

    protected static void updateWindowTitle(JFrame frame, JXMapViewer mapViewer)
    {
        double lat = mapViewer.getCenterPosition().getLatitude();
        double lon = mapViewer.getCenterPosition().getLongitude();
        int zoom = mapViewer.getZoom();

        frame.setTitle(String.format("Let's Go Biking ! (%.2f / %.2f) - Zoom: %d", lat, lon, zoom));
    }


}
