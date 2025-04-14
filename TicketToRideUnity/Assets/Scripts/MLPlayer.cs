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
    public List<CityConnection> network { get; set; }

    private string fileName = Application.dataPath + "/CalculatedWay.csv";
    public List<CityConnection> bestWay { get; set; }
    public List<List<CityConnection>> bestWayList { get; set; }
    private bool hasCrashed = false;
    private double benefitForClaimedRoute = 50;
    private float epsilon = 0.5f; // 50% probabilty to pick rndm next city
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
        qTableBackup = qTable.DeepCopy();
        bestWay = new List<CityConnection>();
        bestWayList = new List<List<CityConnection>>();
    }
    /// <summary>
    /// update QTable, delete blocked routes, add benefit zu acquired routes
    /// </summary>
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
                if (player.acquiredRoutes.Contains(route.routeName))
                {
                    route.routeValue += benefitForClaimedRoute;
                }
            }
        }
    }

    /// <summary>
    /// Starts the Q-learning training process to find an optimal path between a start city and a target city.
    /// </summary>
    /// <param name="startCity">The city from which the pathfinding begins.</param>
    /// <param name="targetCity">The destination city the agent should reach.</param>
    /// <param name="episodes">The number of training episodes to run.</param>
    /// <param name="alpha">The learning rate used in the Q-learning update formula.</param>
    /// <param name="gamma">The discount factor for future rewards.</param>
    /// <param name="connectionCounter">The maximum depth or step limit for recursive pathfinding.</param>
    /// <remarks>
    /// Uses an epsilon-greedy strategy with exponential decay to control exploration.
    /// Periodically stores the best path and writes the Q-table to a CSV file.
    /// If a failure occurs (e.g., too deep recursion or invalid state), the Q-table is rolled back to the previous backup.
    /// </remarks>
    public void startTraining(string startCity, string targetCity, int episodes, double alpha, double gamma, int connectionCounter)
    {
        bool training = true;
        float epsilonStart = 1.0f;
        float epsilonMin = 0.05f;
        float epsilon = epsilonStart;
        float decayRate = (float)Math.Pow(epsilonMin / epsilonStart, 1.0 / episodes);
        for (int i = 1; i <= episodes; i++)
        {
            // Calculate epsilon
            epsilon = Mathf.Max(epsilonMin, epsilon * decayRate);
            epsilon = (float)Math.Round(epsilon, 2);

            // Check if getNextQValue has exceeded the limit of connectionCounter
            if (hasCrashed == true)
            {
                qTable = qTableBackup;
                hasCrashed = false;
                //Debug.Log("Crash at episode: " + i + " !!");
            }
            // Every 10% of episodes, findBestWay stores an optimal path.
            else if (i % (episodes/10) == 0)
                findBestWay(startCity, targetCity);
            else
            {
                // Two algorithms for Q-Learning:
                // 1. Recursive approach
                getNextQValue(startCity, targetCity, alpha, gamma, connectionCounter, training);
                // 2. Standard training method
                //trainQTable(startCity, targetCity, alpha, gamma, connectionCounter, training);
            }

            // The CSV file is updated with the current Q-table every 10% of the episodes
            if (i % (episodes/10) == 0)
            {
                writeQTableToCSV(i, startCity, targetCity);
            }
        }
    }

    /// <summary>
    /// Trains the Q-learning algorithm to find the shortest connection between a given city and the target city.
    /// </summary>
    /// <param name="targetCity">The destination city the player aims to reach.</param>
    /// <param name="episodes">The number of training episodes to perform.</param>
    /// <param name="alpha">The learning rate for the Q-learning algorithm.</param>
    /// <param name="gamma">The discount factor for future rewards.</param>
    /// <param name="connectionCounter">The number of connections considered when evaluating possible paths.</param>
    /// <remarks>
    /// This method uses an epsilon-greedy approach with decay to balance exploration and exploitation.
    /// Q-values are updated for each city in the player's list, and progress is saved periodically.
    /// If a crash occurs during pathfinding, the previous Q-table is restored.
    /// </remarks>
    public void findShortestConnection(string targetCity, int episodes, double alpha, double gamma, int connectionCounter)
    {
        float epsilonStart = 1.0f;
        float epsilonMin = 0.05f;
        float epsilon = epsilonStart;
        float decayRate = (float)Math.Pow(epsilonMin / epsilonStart, 1.0 / episodes);

        bool training = true;
        for (int i = 1; i <= episodes; i++)
        {
            foreach (var city in player.cities)
            {
                // Calculate epsilon
                epsilon = Mathf.Max(epsilonMin, epsilon * decayRate);
                epsilon = (float)Math.Round(epsilon, 2);
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

    /// <summary>
    /// Returns the build costs (number of cards per color) for a given route based on its name.
    /// </summary>
    /// <param name="routeName">The name of the route (must match the name of the GameObject).</param>
    /// <returns>
    /// A dictionary with the route color as the key and the required number of cards as the value.
    /// </returns>
    /// <remarks>
    /// This method uses Unity's GameObject system to find the route and retrieve its properties.
    /// </remarks>
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


  

    // check connection Start --------------------------------------------------------------------

    /// <summary>
    /// !deprecated! Alternative used for total score calculation
    /// </summary>
    /// <param name="startCity">Start city from destinationcard start</param>
    /// <param name="targetCity">Destination city from destinationcard</param>
    /// <param name="player">Active Player</param>
    /// <param name="counter">int value to stop rekursiv loop</param>
    /// <returns></returns>
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
    /// <summary>
    /// !deprecated! Alternative used for total score calculation 
    /// </summary>
    /// <param name="routes"></param>
    /// <returns></returns>
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
    /// Calculates the next Q-value for a given state-action pair in the Q-learning algorithm recursive.
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
                foreach (CityConnection connection in qTable.qDictionary[startCity])
                {
                    //if (connection.city == route.city)
                    if (connection.routeName == route.routeName)
                    {
                        // Set the Q-value in the dictionary
                        //qTable.UpdateRouteValue(startCity, connection.routeName, newQValue);
                        connection.routeValue = newQValue;

                        qTableBackup = qTable;
                        bestWay.Add(connection);
                        //qTable.printQValue(connection.routeName);
                    }
                }
            }
            //Debug.Log($"newQValue: {newQValue} ");
            return newQValue;
        }
        else
        {
            return 1000.0;
        }
    }
    /// <summary>
    /// Trains the Q-table based on standard Q-learning.
    /// </summary>
    /// <param name="startCity">The starting city.</param>
    /// <param name="targetCity">The target city.</param>
    /// <param name="alpha">Learning rate (alpha value).</param>
    /// <param name="gamma">Discount factor (gamma value).</param>
    /// <param name="maxSteps">Maximum number of steps per episode.</param>
    /// <param name="training">Indicates whether training is active.</param>
    public void trainQTable(string startCity, string targetCity, double alpha, double gamma, int maxSteps, bool training)
    {
        string currentCity = startCity;
        int steps = 0;

        while (currentCity != targetCity && steps < maxSteps)
        {
            string nextCity = training ? pickNextCity(currentCity) : pickMaxNextCity(currentCity);
            CityConnection route = getNextRoute(currentCity, nextCity);
            double reward = route.weight;

            // Find the maximum Q-value for the next state
            double maxNextQValue = getMaxQValue(nextCity);

            // Q-value update based on the Q-learning formula
            //Formula: Q(s, a) ←    Q(s, a)   +   α   *  (R(s, a) +   γ   * max Q(s', a')  - Q(s, a))
            double newQValue = route.routeValue + alpha * (reward + gamma * maxNextQValue - route.routeValue);

            // write the Q-value into the table
            qTable.UpdateRouteValue(currentCity, route.routeName, newQValue);
            qTableBackup = qTable;
            bestWay.Add(route);

            // move to the next city
            currentCity = nextCity;
            steps++;
        }

        // if the target is reached: set final routeValue
        //if (currentCity == targetCity)
        //{
        //    foreach (var connection in qTable.qDictionary[currentCity])
        //    {
        //        connection.routeValue = 1000; // High value for reaching the target
        //    }
        //}
    }

    /// <summary>
    /// Retrieves the highest Q-value for a given city.
    /// </summary>
    /// <param name="city">The city for which the maximum Q-value is determined.</param>
    /// <returns>The highest Q-value for the city or 0.0 if no connections exist.</returns>
    public double getMaxQValue(string city)
    {
        if (qTable.qDictionary.ContainsKey(city) && qTable.qDictionary[city].Count > 0)
        {
            return qTable.qDictionary[city].Max(connection => connection.routeValue);
        }
        return 0.0; // Default value if no connections exist
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
    /// Selects the next city based on an exploration-exploitation strategy, using epsilon-greedy with 50%
    /// </summary>
    /// <param name="city">The current city.</param>
    /// <returns>
    /// A randomly chosen city with a 50% probability or the city with the highest Q-value.
    /// </returns>
    private string pickNextCity(string city)
    {
        List<CityConnection> possibleConnections = new List<CityConnection>();
        
        // Check, if the city exists in the qDictionary
        if (qTable.qDictionary.ContainsKey(city))
        {
            possibleConnections = qTable.qDictionary[city];
        }

        // Pick a random route or the route with the highest routeValue
        if (UnityEngine.Random.Range(0f, 1f) < epsilon)
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
        Debug.Log($"Error: Found no route to the next city: { nextCity }");
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
        tw.WriteLine(player.playerName + ";" + "City; RouteName; RouteValue; Weight;" + "Turn: " + gameState.TurnCounter);
        tw.Close();
        tw = new StreamWriter(fileName, true);
        foreach (var part in bestWay)
        {
            tw.WriteLine(" ;" + part.city + "; " + part.routeName + "; " + part.routeValue + "; " + part.weight);
        }
        tw.WriteLine(" ; ; ; ;");
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
                // first row: Header with citynames
                writer.Write("Step_" + counter + ";");  // first col gives the step counter
                writer.WriteLine(string.Join(";", cityList));  // col titles

                // Daten ausgeben
                foreach (var rowCity in cityList)
                {
                    List<string> rowValues = new List<string> { rowCity };  // row titles

                    foreach (var colCity in cityList)
                    {
                        // if existes: find routeValue
                        CityConnection connection = qTable.qDictionary[rowCity].FirstOrDefault(c => c.city == colCity);
                        string value = connection != null ? connection.routeValue.ToString("F1") : "";

                        rowValues.Add(value);
                    }

                    // write in the rowvaules to CSV
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
            int stops = way.Count(); // Number of cities in the current path
            double maxPoints = way.Sum(bestWay => bestWay.weight); // Total reward for the current path
            double maxValue = way.Sum(bestWay => bestWay.routeValue); // Total Q-value for the current path

            // Comparison logic:
            // 1. Prefer fewer stops
            // 2. If equal stops, prefer higher reward (maxPoints)
            // 3. If equal stops and reward, prefer higher total Q-value (better learned policy)
            if (stops < highestStops
                || (stops == highestStops && maxPoints > highestMaxPoints)
                || (stops == highestStops && maxPoints == highestMaxPoints && maxValue > highestMaxValue)
                )
            {
                // Update best-known path so far
                highestStops = stops;
                highestMaxPoints = maxPoints;
                highestMaxValue = maxValue;
                theBestWay = way;

                // Save the best path to a CSV file
                writeBestWayToCSV(way);
            }
        }
        return theBestWay;
    }

    /// <summary>
    /// Finds the shortest route from a list of possible routes based on the number of stops and maximum points.
    /// The best route is determined by the following criteria: 
    /// 1. Fewer stops,
    /// 2. Lower maximum points (weight),
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
            int stops = way.Count(); // Number of cities in the current path
            double maxPoints = way.Sum(bestWay => bestWay.weight); // Total reward for the current path
            double maxValue = way.Sum(bestWay => bestWay.routeValue); // Total Q-value for the current path

            // Comparison logic:
            // 1. Prefer fewer stops
            // 2. If equal stops, prefer lower total reward (shorter route in terms of weight)
            // 3. If equal stops and reward, prefer higher Q-value (better learned policy)
            if (stops < highestStops
                || (stops == highestStops && maxPoints < highestMaxPoints)
                || (stops == highestStops && maxPoints == highestMaxPoints && maxValue < highestMaxValue)
                )
            {
                // Update best-known path so far
                highestStops = stops;
                highestMaxPoints = maxPoints;
                highestMaxValue = maxValue;
                theBestWay = way;

                // Save the best path to a CSV file
                writeBestWayToCSV(way);
            }
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

    // start Heuristik 1 : build ------------------------------------------------------------------
    /// <summary>
    /// Selects an affordable and unclaimed route for the player based on their current hand of cards and available wagons.
    /// </summary>
    /// <param name="player">The player for whom the route is being selected.</param>
    /// <param name="routes">The parent GameObject containing all route GameObjects as children.</param>
    /// <returns>
    /// The name of a route the player can afford and is part of their shortest path goals, or an empty string if no suitable route is found.
    /// </returns>
    /// <remarks>
    /// This method checks if the player has enough matching cards (including Jokers) and wagons to claim a route.
    /// It prioritizes routes that are part of the shortest paths calculated for the player.
    /// </remarks>
    public string selectAffordableRoute(PlayerScript player, GameObject routes)
    {
        string targetRoute = "";

        List<string> bestUniqueRoutes = new List<string>();
        foreach (var x in getShortWay())
        //foreach (var x in getBestWay())
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
                // Ensure the route has no owner and the player has enough wagons
                if (route.owner == null && route.routeLength <= player.availableWagons)
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
    // end Heuristik 1 : build --------------------------------------------------------------------

    // start Heuristik 2 : select colors ----------------------------------------------------------
    /// <summary>
    /// Calculates the colors that the player needs to draw, sorted in ascending order of missing cards.
    /// </summary>
    /// <param name="player">The player for whom the calculation is performed.</param>
    /// <param name="routes">The GameObject containing the routes.</param>
    /// <returns>A list of color names, sorted by the number of missing cards (ascending).</returns>
    public List<string> calculateColorsSortedByNeed(PlayerScript player)
    {
        Dictionary<string, int> playerHandcards = gameState.GetPlayerHandCards(player);
        Dictionary<string, Dictionary<string, int>> routeCosts = new Dictionary<string, Dictionary<string, int>>();
        // Joker-Karten extrahieren
        int jokerCount = playerHandcards.ContainsKey("Joker") ? playerHandcards["Joker"] : 0;

        foreach (CityConnection connection in player.bestWay)
        {
            if (gameState.getFreeRoutes().Contains(connection.routeName))
            {
                Dictionary<string, int> costs = getCostsForRoute(connection.routeName);
                routeCosts.Add(connection.routeName, costs);
            }
        }

        // dictionary to save the amount and the color of missing cards
        Dictionary<string, int> missingCardsPerColor = new Dictionary<string, int>();

        foreach (var route in routeCosts)
        {
            foreach (var cost in route.Value) // cost.Key = color, cost.Value = needed amount
            {
                int handCount = playerHandcards.ContainsKey(cost.Key) ? playerHandcards[cost.Key] : 0;
                int missingCards = Math.Max(0, cost.Value - (handCount + jokerCount));
                if (cost.Key == "Grey")
                {
                    foreach (var handCard in playerHandcards)
                    {
                        if (handCard.Key != "Joker") // Joker is treated separately
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
        return missingCardsPerColor.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();
    }
    // end Heuristik 2 : select colors ------------------------------------------------------------

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
