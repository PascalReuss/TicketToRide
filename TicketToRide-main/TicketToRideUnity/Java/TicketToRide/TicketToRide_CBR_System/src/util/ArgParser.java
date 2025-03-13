package util;

import java.util.HashMap;
import java.util.Map;

public class ArgParser {

    private final Map<String, String> arguments = new HashMap<String, String>();

    public ArgParser(String[] args) {
        for (String arg : args) {
            if (arg.startsWith("--")) {
                String[] parts = arg.substring(2).split("=", 2);
                if (parts.length == 1) {
                    arguments.put(parts[0], null);
                } else {
                    arguments.put(parts[0], parts[1]);
                }
            }
        }
    }

    public boolean hasArgument(String key) {
        return arguments.containsKey(key);
    }

    public String getArgument(String key) {
        return arguments.get(key);
    }
}