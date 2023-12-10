
package client;

import java.awt.Rectangle;
import java.awt.event.MouseAdapter;
import java.awt.event.MouseEvent;
import java.awt.geom.Point2D;

import org.jxmapviewer.JXMapViewer;

/**
 * Creates a selection rectangle based on mouse input
 * Also triggers repaint events in the viewer
 * @author Martin Steiger
 */
public class SelectionAdapter extends MouseAdapter 
{
    private boolean dragging;
    private final JXMapViewer viewer;

    private final Point2D startPos = new Point2D.Double();
    private final Point2D endPos = new Point2D.Double();

    /**
     * Constructeur par défaut
     * @param viewer the jxmapviewer
     */
    public SelectionAdapter(JXMapViewer viewer) {
        this.viewer = viewer;
    }

    @Override
    public void mousePressed(MouseEvent mouseEvent) {
        if (mouseEvent.getButton() != MouseEvent.BUTTON3)
            return;

        startPos.setLocation(mouseEvent.getX(), mouseEvent.getY());
        endPos.setLocation(mouseEvent.getX(), mouseEvent.getY());

        dragging = true;
    }

    @Override
    public void mouseDragged(MouseEvent mouseEvent)  {
        if (dragging) {
            endPos.setLocation(mouseEvent.getX(), mouseEvent.getY());
            viewer.repaint();
        }
    }

    @Override
    public void mouseReleased(MouseEvent mouseEvent) {
        if (dragging && mouseEvent.getButton() == MouseEvent.BUTTON3) {
            viewer.repaint();
            dragging = false;
        }
    }

    /**
     * @return the selection rectangle
     */
    public Rectangle getRectangle()
    {
        if (dragging) {
            int x1 = (int) Math.min(startPos.getX(), endPos.getX());
            int y1 = (int) Math.min(startPos.getY(), endPos.getY());
            int x2 = (int) Math.max(startPos.getX(), endPos.getX());
            int y2 = (int) Math.max(startPos.getY(), endPos.getY());

            return new Rectangle(x1, y1, x2-x1, y2-y1);
        }

        return null;
    }

}
