package client;

import java.util.logging.Formatter;
import java.util.logging.LogRecord;

public class FormatterLogger extends Formatter {

    @Override
    public String format(LogRecord rec) {
        return "\u001B[0;30m" + rec.getMessage() + "\n";
    }
}
