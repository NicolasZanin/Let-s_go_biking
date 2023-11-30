package t;

import org.json.JSONArray;
import org.jxmapviewer.JXMapViewer;
import org.jxmapviewer.OSMTileFactoryInfo;
import org.jxmapviewer.input.PanMouseInputListener;
import org.jxmapviewer.painter.CompoundPainter;
import org.jxmapviewer.painter.Painter;
import javax.swing.event.MouseInputListener;
import java.awt.*;
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

public class ItineraireViewer {

    public static List<GeoPosition> createTrack(JSONArray itineraire) {
        List<GeoPosition> track = new ArrayList<>();

        for (int i = 0; i < itineraire.length(); i++) {
            JSONArray coord = itineraire.getJSONArray(i);
            GeoPosition pos = new GeoPosition(coord.getDouble(1), coord.getDouble(0));
            track.add(pos);
        }

        return track;
    }

    public static void showItineraire(List<String> itineraires) {
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
        List<List<GeoPosition>> tracks = new ArrayList<>();
        List<GeoPosition> track0 = createTrack(new JSONArray(itineraires.get(0)));
        tracks.add(track0);

        List<GeoPosition> track1 = new ArrayList<>();
        List<GeoPosition> track2 = new ArrayList<>();
        if (itineraires.size() > 1) {
            track1 = createTrack(new JSONArray(itineraires.get(1)));
            track2 = createTrack(new JSONArray(itineraires.get(2)));
            // Add tracks to the list
            tracks.add(track1);
            tracks.add(track2);
        }

        RoutePainter routePainter0 = new RoutePainter(track0, "RED");
        RoutePainter routePainter1 = new RoutePainter(track1, "BLUE");  // Use track1
        RoutePainter routePainter2 = new RoutePainter(track2, "RED");   // Use track2

// Set the focus
        HashSet<GeoPosition> allTracks = new HashSet<>();
        allTracks.addAll(track0);
        allTracks.addAll(track1);
        allTracks.addAll(track2);
        mapViewer.zoomToBestFit(allTracks, 0.7);

// Create waypoints from the geo-positions
        Set<Waypoint> waypoints = new HashSet<>();
        for (List<GeoPosition> track : tracks) {
            Set<Waypoint> waypointsX = new HashSet<>(Arrays.asList(
                    new DefaultWaypoint(track.get(0)),
                    new DefaultWaypoint(track.get(track.size() - 1))));
            waypoints.addAll(waypointsX);
        }


        // Create a waypoint painter that takes all the waypoints
        WaypointPainter<Waypoint> waypointPainter = new WaypointPainter<>();
        waypointPainter.setWaypoints(waypoints);

        // Create a compound painter that uses both the route-painter and the waypoint-painter
        List<Painter<JXMapViewer>> painters = new ArrayList<>();
        painters.add(routePainter0);
        painters.add(routePainter1);
        painters.add(routePainter2);
        painters.add(waypointPainter);

        CompoundPainter<JXMapViewer> painter = new CompoundPainter<>(painters);
        mapViewer.setOverlayPainter(painter);
        mapViewer.addPropertyChangeListener("zoom", evt -> updateWindowTitle(frame, mapViewer));

        mapViewer.addPropertyChangeListener("center", evt -> updateWindowTitle(frame, mapViewer));

        updateWindowTitle(frame, mapViewer);
        // Set the focus

        mapViewer.zoomToBestFit(allTracks, 0.7);
    }

    protected static void updateWindowTitle(JFrame frame, JXMapViewer mapViewer)
    {
        double lat = mapViewer.getCenterPosition().getLatitude();
        double lon = mapViewer.getCenterPosition().getLongitude();
        int zoom = mapViewer.getZoom();

        frame.setTitle(String.format("Let's Go Biking ! (%.2f / %.2f) - Zoom: %d", lat, lon, zoom));
    }


}
