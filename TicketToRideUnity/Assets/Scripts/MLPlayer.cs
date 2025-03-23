using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using System;

public class MLPlayer
{
    // **MetaLayer**: A reference to the MetaLayer class used to interact with the game state and gather information.
    private MetaLayer metaLayer;

    // **GameState**: A reference to the GameState class that holds the current state of the game.
    private GameState gameState;
    public PlayerScript player { get; set; }
    // **QTable**: A reference to the QTable class to access and update q values. V3
    public QTable qTable { get; set; }
    public QTable qTableBackup { get; set; }

    private string fileName = Application.dataPath + "/BestWays.csv";
    public List<CityConnection> bestWay { get; set; }
    public List<List<CityConnection>> bestWayList { get; set; }
    private bool hasCrashed = false;

    /// <summary>
    /// Initializes a new instance of the MLPlayer class.
    /// </summary>
    /// <param name="gameState">The current gameState used by the AI to make decisions.</param>
    /// <param name="metaLayer">The current metaLayer used by the AI to make decisions.</param>
    public MLPlayer(GameState gameState, MetaLayer metaLayer)
    {
        this.gameState = gameState; // Store the game state reference
        this.metaLayer = metaLayer; // Initialize MetaLayer to interact with game state
        qTable = new QTable(gameState); // Initialize qTable to store the Q Values
        qTableBackup = new QTable(gameState);
    }
    /// <summary>
    /// Initialisiert QTable, QTableBackup, bestWay, bestWayList
    /// </summary>
    public void startQTable()
    {
        qTable.initializeQDictionary();
        qTableBackup = qTable;
        bestWay = new List<CityConnection>();
        bestWayList = new List<List<CityConnection>>();
    }

    public void updateQTable()
    {
        bestWayList.Clear();
        foreach (string routeName in gameState.getAllAcquiredEnemyRoutes())
        {
            qTable.deleteRoutesInQDictionary(routeName);
        }
        foreach (var connection in qTable.qDictionary.Values)
        {
            foreach (var route in connection)
            {
                if (gameState.getPlayerTargetCities(player).Contains(route.city))
                {
                    route.routeValue = route.routeValue + 1000;
                }
            }
        }
    }

    /// <summary>
    /// Starts the training of the Q-learning algorithm for pathfinding between two cities.
    /// </summary>
    /// <param name="startCity">The starting city of the training.</param>
    /// <param name="targetCity">The target city of the training.</param>
    /// <param name="episodes">The number of training episodes.</param>
    /// <param name="alpha">The learning rate of the algorithm.</param>
    /// <param name="gamma">The discount factor for future rewards.</param>
    /// <param name="connectionCounter">The number of possible connections in the network.</param>
    public void startTraining(string startCity, string targetCity, int episodes, double alpha, double gamma, int connectionCounter)
    {
        bool training = true;
        for (int i = 1; i <= episodes; i++)
        {
            // Abfrage, ob die Pfadsuche beide Städte gefunden hat.
            if (hasCrashed == true)
            {
                qTable = qTableBackup;
                hasCrashed = false;
                //Debug.Log("Crash at episode: " + i + " !!");
            }
            // 10 Mal wird ein "bester" Weg in die Liste bester Wege aufgenommen.
            else if (i % (episodes/10) == 0)
                findBestWay(startCity, targetCity);

            getNextQValue(startCity, targetCity, alpha, gamma, connectionCounter, training);
            
            // 10 Mal wird die CSV Datei mit dem aktuellen Q-Table erweitert
            if (i % (episodes/10) == 0)
            {
                writeQTableToCSV(i, startCity, targetCity);
            }
        }
    }
    public void findShortestConnection(string targetCity, int episodes, double alpha, double gamma, int connectionCounter)
    {
        bool training = true;
        for (int i = 1; i <= episodes; i++)
        {
            foreach (var city in player.cities)
            {
                // Abfrage, ob die Pfadsuche beide Städte gefunden hat.
                if (hasCrashed == true)
                {
                    qTable = qTableBackup;
                    hasCrashed = false;
                    //Debug.Log("Crash at episode: " + i + " !!");
                }
                else if (i % (episodes / 10) == 0)
                    findBestWay(city, targetCity);

                getNextQValue(city, targetCity, alpha, gamma, connectionCounter, training);
                if (i % (episodes / 10) == 0)
                {
                    writeQTableToCSV(i, city, targetCity);
                }
            }
        }
    }
    public string selectAffordableRoute(PlayerScript player, GameObject routes)
    {
        string targetRoute = "";

        List<string> bestUniqueRoutes = new List<string>();
        foreach (var x in getShortWay())
        {
            bestUniqueRoutes.Add(x.routeName);
        }
        int jokerCount = 0;
        Dictionary<string, int> playerHandCards = gameState.GetPlayerHandCards(player);
        foreach (var card in playerHandCards)
        {
            string color = card.Key; // The color of the current card
            int count = card.Value; // The number of cards of this color in the player's hand
            if (color == "Joker")
            {
                jokerCount = count;
            }
            else
            {
                count += jokerCount;
            }

            //Debug.Log("color: " + color + "\n" + "count: " + count);
            // Check all child objects of the routes GameObject
            for (int r = 0; r < routes.transform.childCount; r++)
            {
                // Get the RouteScript component for the current route
                RouteScript route = routes.transform.GetChild(r).GetComponent<RouteScript>();
                // Ensure the route has no owner (i.e., it is unclaimed)
                if (route.owner == null)
                {
                    // Check if the route matches the player's hand cards and is in the list of potential routes
                    if (route.routeLength <= count &&
                        (bestUniqueRoutes.Contains(route.name) &&
                        (route.routeColor == "Grey" || route.routeColor == color)))
                    {
                        // Set the target route to the name of this route
                        targetRoute = routes.transform.GetChild(r).name;
                    }
                }
            }
        }
        // Return the selected route's name, or an empty string if no route was selected
        return targetRoute;
    }

    public Dictionary<string, int> getCostsForRoute(string routeName)
    {
        Dictionary<string, int> costs = new Dictionary<string, int>();
        GameObject route = GameObject.Find(routeName);
        string color = route.GetComponent<RouteScript>().routeColor;
        int count = route.GetComponent<RouteScript>().routeLength;
        costs.Add(color, count);
        Debug.Log("Route: " + route.name + " color: " + color + " count: " + count);
        return costs;
    }

    public List<string> calculateColorsSortedByNeed(PlayerScript player, GameObject routes)
    {
        Dictionary<string, int> playerHandcards = gameState.GetPlayerHandCards(player);
        Dictionary<string, Dictionary<string, int>> routeCosts = new Dictionary<string, Dictionary<string, int>>();

        // Joker-Karten extrahieren
        int jokerCount = playerHandcards.ContainsKey("Joker") ? playerHandcards["Joker"] : 0;

        // Sammle die benötigten Karten für jede Route
        foreach (CityConnection connection in player.bestWay)
        {
            if (gameState.getFreeRoutes().Contains(connection.routeName))
            {
                Dictionary<string, int> costs = getCostsForRoute(connection.routeName);
                routeCosts.Add(connection.routeName, costs);
            }
        }

        // Dictionary zur Speicherung der fehlenden Karten pro Farbe
        Dictionary<string, int> missingCardsPerColor = new Dictionary<string, int>();

        // Überprüfe, welche Farbe am wenigsten nachgezogen werden muss
        foreach (var route in routeCosts)
        {
            foreach (var cost in route.Value) // cost.Key = Farbe, cost.Value = benötigte Anzahl
            {
                int handCount = playerHandcards.ContainsKey(cost.Key) ? playerHandcards[cost.Key] : 0;
                int missingCards = Math.Max(0, cost.Value - (handCount + jokerCount));

                if (cost.Key == "Grey")
                {
                    foreach (var handCard in playerHandcards)
                    {
                        if (handCard.Key != "Joker") // Joker wird separat behandelt
                        {
                            int available = handCard.Value + jokerCount;
                            int missing = Math.Max(0, cost.Value - available);

                            if (!missingCardsPerColor.ContainsKey(handCard.Key) || missing < missingCardsPerColor[handCard.Key])
                            {
                                missingCardsPerColor[handCard.Key] = missing;
                            }
                        }
                    }
                }
                else
                {
                    if (!missingCardsPerColor.ContainsKey(cost.Key) || missingCards < missingCardsPerColor[cost.Key])
                    {
                        missingCardsPerColor[cost.Key] = missingCards;
                    }
                }
            }
        }

        // Farben sortiert nach benötigten Karten zurückgeben
        return missingCardsPerColor.OrderBy(x => x.Value).Select(x => x.Key).ToList();
    }

    // check connection Start --------------------------------------------------------------------

    public List<string> getCityConnection(string startCity, string targetCity, PlayerScript player, int counter)
    {
        if (counter <= 0)
            return null;
        List<string> routes = new List<string>(); // Speichert Name der Route
        List<string> kiRoutes = player.acquiredRoutes;

        foreach (var route in kiRoutes)
        {
            string[] cities = route.Split('_');
            string city1 = cities[0];
            string city2 = cities[1];

            // Prüfen, ob die Route eine Verbindung von startCity ist
            if ((city1 == startCity) && (getCityConnection(city2, targetCity, player, counter-1) != null))
                routes.Add(route);
            if ((city2 == startCity) && (getCityConnection(city1, targetCity, player, counter-1) != null))
                routes.Add(route);
        }
        return routes;
    }
    public Dictionary<string, int> getCityConnectionWeights(List<string> routes)
    {
        Dictionary<string, int> routesWithWeights = new Dictionary<string, int>();
        foreach (var r in routes)
        {
            GameObject route = GameObject.Find(r);
            routesWithWeights.Add(route.name, route.GetComponent<RouteScript>().routeLength);
        }
        return routesWithWeights;
    }

    public bool checkConnection(string startCity, string targetCity, HashSet<string> visited = null)
    {
        //Debug.Log("Check from: " + startCity + " to: " + targetCity);
        if (startCity == targetCity)
            return true; // Connection found!

        if (visited == null)
            visited = new HashSet<string>();

        if (visited.Contains(startCity)) // Exit on endless loop
        {
            //Debug.Log("Endless Loop");
            return false;
        }

        visited.Add(startCity); // mark city as visited

        List<string> kiRoutes = player.acquiredRoutes;

        foreach (var route in kiRoutes)
        {
            string[] cities = route.Split('_');
            string city1 = cities[0];
            string city2 = cities[1];

            // Check if the route is a connection from startCity
            if (city1 == startCity && checkConnection(city2, targetCity, visited))
                return true;
            if (city2 == startCity && checkConnection(city1, targetCity, visited))
                return true;
        }

        return false;  // No connection found
    }
    // check connection End -----------------------------------------------------------------------


    // Q-Learning Start --------------------------------------------------------------------------

    /// <summary>
    /// Calculates the next Q-value for a given state-action pair in the Q-learning algorithm.
    /// </summary>
    /// <param name="startCity">The current city in the route.</param>
    /// <param name="targetCity">The target city to reach.</param>
    /// <param name="alpha">The learning rate of the algorithm.</param>
    /// <param name="gamma">The discount factor for future rewards.</param>
    /// <param name="connectionCounter">The remaining number of allowed connections before termination.</param>
    /// <param name="training">Indicates whether the function is running in training mode.</param>
    /// <returns>The updated Q-value for the given state-action pair.</returns>
    public double getNextQValue(string startCity, string targetCity, double alpha, double gamma, int connectionCounter, bool training)
    {
        //Debug.Log("getNextQValue(" + startCity + ", " + targetCity + ", " + alpha + ", " + gamma + ", " + training + ")");

        // Termination condition 1: As long as target is not reached -> Loop Q Learning, otherwise -> target reached, return 1000
        if (startCity != targetCity)
        {
            // Termination condition 2: Number of "connectionCounter" connections are checked, return -100
            if (connectionCounter == 0)
            {
                //Debug.Log("Exeeded connection limit! Stuck in " + startCity + " to " + targetCity);
                hasCrashed = true;
                return -100;
            }
            connectionCounter = connectionCounter - 1;
            
            string nextCity;
            if (training)
            {
                nextCity = pickNextCity(startCity);
            }
            else
            {
                nextCity = pickMaxNextCity(startCity);
            }
            CityConnection route = getNextRoute(startCity, nextCity);
            double reward = route.weight;
            double qValue = route.routeValue;
            double nextQValue = getNextQValue(nextCity, targetCity, alpha, gamma, connectionCounter, training);
            //Debug.Log("alpha = " + alpha + "\n" + "gamma = " + gamma + "\n" + "city = " + city + "\n" + "qValue = " + qValue + "\n" + "nextCity = " + nextCity + "\n" + "route.routeName = " + route.routeName);

            //Formula: Q(s, a) ← Q(s, a) + α   * (R(s, a)+   γ   * max Q(s', a')  - Q(s, a))
            double newQValue = qValue + alpha * (reward + gamma * nextQValue - qValue);
            
            if (qTable.qDictionary.ContainsKey(startCity))
            {
                // Wenn es eine Verbindung von der aktuellen Stadt (startCity) zur Zielstadt (targetCity) gibt
                foreach (CityConnection connection in qTable.qDictionary[startCity])
                {
                    if (connection.city == route.city)
                    {
                        // Setze den Q-Wert in das Dictionary
                        qTable.UpdateRouteValue(startCity, connection.routeName, newQValue);
                        qTableBackup = qTable;
                        bestWay.Add(connection);
                        //qTable.printQValue(connection.routeName);
                    }
                }
            }
            //Debug.Log("newQValue: " + newQValue);
            return newQValue;
        }
        else
        {
            foreach (var connection in qTable.qDictionary[startCity])
            {
                connection.routeValue = 1000;
            }
            return 1000.0;
        }
    }

    /// <summary>
    /// Selects the next city with the highest Q-value from the given city's available routes.
    /// </summary>
    /// <param name="city">The current city.</param>
    /// <returns>The next city with the highest Q-value, or null if no connections are available.</returns>
    private string pickMaxNextCity(string city)
    {
        // Die Stadt mit dem höchsten routeValue auswählen
        var bestConnection = qTable.qDictionary[city]
            .OrderByDescending(action => action.routeValue) // Sortiere nach routeValue absteigend
            .FirstOrDefault(); // Nimm den höchsten Wert

        return bestConnection?.city; // Falls kein Eintrag vorhanden, gib null zurück
    }

    /// <summary>
    /// Selects the next city based on an exploration-exploitation strategy.
    /// </summary>
    /// <param name="city">The current city.</param>
    /// <returns>
    /// A randomly chosen city with a 50% probability or the city with the highest Q-value.
    /// </returns>
    private string pickNextCity(string city)
    {
        List<CityConnection> possibleConnections = new List<CityConnection>();

        // Überprüfen, ob die Stadt in der Q-Tabelle vorhanden ist
        if (qTable.qDictionary.ContainsKey(city))
        {
            possibleConnections = qTable.qDictionary[city];
        }

        // 50% Chance: Zufällige Stadt oder Stadt mit höchstem Q-Wert wählen
        if (UnityEngine.Random.Range(0f, 1f) < 0.5f) // 50% Wahrscheinlichkeit
        {
            // select random city
            int randomIndex = UnityEngine.Random.Range(0, possibleConnections.Count); // used UnityEngine.Random and not System.Random
            return possibleConnections[randomIndex].city;
        }
        else
        {
            // select city with highest routeValue
            return possibleConnections.OrderByDescending(conn => conn.routeValue).First().city;
        }
    }

    /// <summary>
    /// Retrieves the route between the current city and the next city from the Q-table.
    /// </summary>
    /// <param name="currentCity">The current city in the route.</param>
    /// <param name="nextCity">The next city to travel to.</param>
    /// <returns>The corresponding <see cref="CityConnection"/> if a route exists; otherwise, null.</returns>
    private CityConnection getNextRoute(string currentCity, string nextCity)
    {
        CityConnection route;
        foreach (var connection in qTable.qDictionary[currentCity])
        {
            if (connection.city == nextCity)
            {
                route = connection;
                return route;
            }
        }
        Debug.Log("Fehler: keine Route zur nächsten Stadt gefunden" + nextCity);
        return null;
    }
    // Q-Learning End -----------------------------------------------------------------------------


    // CSV Start ----------------------------------------------------------------------------------

    /// <summary>
    /// Writes the details of the best way to a CSV file.
    /// </summary>
    /// <param name="bestWay">A list of <see cref="CityConnection"/> objects representing the best route.</param>
    public void writeBestWayToCSV(List<CityConnection> bestWay)
    {
        TextWriter tw = new StreamWriter(fileName, true);
        tw.WriteLine("City; RouteName; RouteValue; Weight");
        tw.Close();
        tw = new StreamWriter(fileName, true);
        foreach (var part in bestWay)
        {
            tw.WriteLine(part.city + "; " + part.routeName + "; " + part.routeValue + "; " + part.weight);
        }
        tw.Close();
    }

    /// <summary>
    /// Saves the current Q-table as a CSV file, including city names and their corresponding route values.
    /// </summary>
    /// <param name="counter">The current step or episode number, included in the CSV filename and header.</param>
    /// <param name="city1">The starting city for the Q-table export.</param>
    /// <param name="city2">The destination city for the Q-table export.</param>
    public void writeQTableToCSV(int counter, string startCity, string targetCity)
    {
        string filePath = Path.Combine(Application.dataPath, "Q-Tables", player.playerName + " Q-Table " + startCity + " to " + targetCity + ".csv");
        if (qTable.qDictionary == null || qTable.qDictionary.Count == 0)
        {
            Debug.Log("QTable is empty or not initialized.");
            return;
        }

        // Liste der Städte aus den Keys des Dictionaries extrahieren
        List<string> cityList = new List<string>(qTable.qDictionary.Keys);

        try
        {
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                // Erste Zeile: Header mit Städtenamen
                writer.Write("Step_" + counter + ";");  // Erste Spalte bleibt für Zeilenüberschriften
                writer.WriteLine(string.Join(";", cityList));  // Spaltenüberschriften für Städte

                // Daten ausgeben
                foreach (var rowCity in cityList)
                {
                    List<string> rowValues = new List<string> { rowCity };  // Zeilenüberschrift

                    foreach (var colCity in cityList)
                    {
                        // Finde den RouteValue, falls eine Verbindung existiert
                        CityConnection connection = qTable.qDictionary[rowCity].FirstOrDefault(c => c.city == colCity);
                        string value = connection != null ? connection.routeValue.ToString("F1") : "";

                        rowValues.Add(value);  // Wert zur Zeile hinzufügen
                    }

                    // Ganze Zeile in CSV schreiben
                    writer.WriteLine(string.Join(";", rowValues));
                }
            }

            //Debug.Log("QTable successfully saved to CSV: " + filePath);
        }
        catch (IOException e)
        {
            Debug.LogError("Error writing file: " + e.Message);
        }
    }
    // CSV End ------------------------------------------------------------------------------------

    // Best Way Start -----------------------------------------------------------------------------

    /// <summary>
    /// Determines the best route between the given city and the target city using the Q-learning algorithm.
    /// </summary>
    /// <param name="city">The starting city for finding the best route.</param>
    /// <param name="targetCity">The destination city for the best route search.</param>
    public void findBestWay(string city, string targetCity)
    {
        bestWay.Clear();
        bool training = false;
        double alpha = 1;
        double gamma = 1;
        int connectionCounter = 100;
        
        getNextQValue(city, targetCity, alpha, gamma, connectionCounter, training);

        bool containsCity = false;
        bool containsTargetCity = false;

        foreach (var connection in getUniqueBestRoutes())
        {
            if (connection.routeName.Contains(city))
                containsCity = true;
            if (connection.routeName.Contains(targetCity))
                containsTargetCity = true;
            if (containsCity && containsTargetCity)
                break;
        }
        if (containsCity && containsTargetCity)
            bestWayList.Add(getUniqueBestRoutes());
    }

    /// <summary>
    /// Finds the best route from a list of possible routes based on the number of stops and maximum points.
    /// The best route is determined by the following criteria: 
    /// 1. Fewer stops,
    /// 2. Higher maximum points (weight),
    /// 3. Higher total route value (sum of route values).
    /// </summary>
    /// <returns>A list of <see cref="CityConnection"/> representing the best route.</returns>
    public List<CityConnection> getBestWay()
    {
        List<CityConnection> theBestWay = new List<CityConnection>();
        int highestStops = 100;
        double highestMaxPoints = double.MinValue;
        double highestMaxValue = double.MinValue;

        foreach (var way in bestWayList)
        {
            int stops = way.Count(); // Anzahl der Verbindungen
            double maxPoints = way.Sum(bestWay => bestWay.weight); // Summe der Streckenlänge
            double maxValue = way.Sum(bestWay => bestWay.routeValue); // Summe der Q-Werte

            // Vergleichslogik: Zuerst nach stops, dann maxPoints, dann maxValue
            if (stops < highestStops
                || (stops == highestStops && maxPoints > highestMaxPoints) // more valueabel route
                //|| (stops == highestStops && maxPoints < highestMaxPoints) // shortest route
                || (stops == highestStops && maxPoints == highestMaxPoints && maxValue < highestMaxValue) // shortest route
                )
            {
                Debug.Log("####################### Found new better way #######################");
                highestStops = stops;
                highestMaxPoints = maxPoints;
                highestMaxValue = maxValue;
                theBestWay = way;
                writeBestWayToCSV(way); // Insert the best way into csv file
            }
            //writeToCSV(way); // Insert all calculated ways into csv file
        }
        return theBestWay;
    }

    /// <summary>
    /// Finds the shortest route from a list of possible routes based on the number of stops and maximum points.
    /// The best route is determined by the following criteria: 
    /// 1. Fewer stops,
    /// 2. Higher maximum points (weight),
    /// 3. Higher total route value (sum of route values).
    /// </summary>
    /// <returns>A list of <see cref="CityConnection"/> representing the shortest route.</returns>
    public List<CityConnection> getShortWay()
    {
        List<CityConnection> theBestWay = new List<CityConnection>();
        int highestStops = 100;
        double highestMaxPoints = double.MinValue;
        double highestMaxValue = double.MinValue;

        foreach (var way in bestWayList)
        {
            int stops = way.Count(); // Anzahl der Verbindungen
            double maxPoints = way.Sum(bestWay => bestWay.weight); // Summe der Streckenlänge
            double maxValue = way.Sum(bestWay => bestWay.routeValue); // Summe der Q-Werte

            // Vergleichslogik: Zuerst nach stops, dann maxPoints, dann maxValue
            if (stops < highestStops
                //|| (stops == highestStops && maxPoints > highestMaxPoints) // more valueabel route
                || (stops == highestStops && maxPoints < highestMaxPoints) // shortest route
                || (stops == highestStops && maxPoints == highestMaxPoints && maxValue < highestMaxValue) // shortest route
                )
            {
                Debug.Log("####################### Found new shorter way #######################");
                highestStops = stops;
                highestMaxPoints = maxPoints;
                highestMaxValue = maxValue;
                theBestWay = way;
                writeBestWayToCSV(way); // Insert the best way into csv file
            }
            //writeToCSV(way); // Insert all calculated ways into csv file
        }
        return theBestWay;
    }
    /// <summary>
    /// Retrieves a list of unique routes based on their route names, sorted by route value in descending order.
    /// Only the first occurrence of each route name is included in the result.
    /// </summary>
    /// <returns>A list of <see cref="CityConnection"/> representing the unique best routes, sorted by route value.</returns>
    public List<CityConnection> getUniqueBestRoutes()
    {
        HashSet<string> uniqueRoutes = new HashSet<string>();
        return bestWay
            .Where(x => uniqueRoutes.Add(x.routeName)) // Nur eindeutige routeName-Werte behalten
            .OrderByDescending(x => x.routeValue) // Nach routeValue absteigend sortieren
            .ToList();
    }

    /// <summary>
    /// Logs the details of the best way (list of city connections) to the console.
    /// </summary>
    /// <param name="theBestWay">A list of <see cref="CityConnection"/> representing the best route to be logged.</param>
    public void testLOG(List<CityConnection> theBestWay)
    {
        Debug.Log("Best Ways:");
        theBestWay.ForEach(x => Debug.Log("City = " + x.city));
    }
    // Bestway End --------------------------------------------------------------------------------

    // Tests Start --------------------------------------------------------------------------------
    public string getOneConnection(PlayerScript dummy)
    {
        string bestWay = "default";
        double highestWeight = double.MinValue;  // Setzt die größte Länge zu Beginn auf den kleinsten möglichen Wert

        List<string> targetCities = gameState.getPlayerTargetCities(dummy); // Zielbahnhöfe

        foreach (var startHBF in targetCities)  // Schleife über alle Zielbahnhöfe
        {
            if (gameState.cityMap.ContainsKey(startHBF))
            {
                List<CityConnection> routes = gameState.cityMap[startHBF];

                foreach (var route in routes)
                {
                    // Debug-Ausgabe der Stadt und des Gewichts
                    Debug.Log("Points from " + startHBF + " to " + route.city + ": " + route.weight);

                    // Überprüft, ob das aktuelle Gewicht größer ist als das bisher höchste Gewicht
                    if (route.weight > highestWeight)
                    {
                        highestWeight = route.weight;
                        bestWay = route.city; // Aktualisiert bestWay mit der Stadt, die das höchste Gewicht hat
                    }
                }
            }
        }
        return bestWay;
    }
    // Tests End ----------------------------------------------------------------------------------

}
