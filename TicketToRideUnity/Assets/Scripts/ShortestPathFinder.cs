using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class ShortestPathFinder
{
    private Dictionary<string, List<CityConnection>> cityMap;
    private Dictionary<string, List<CityConnection>> edgesPath;
    private List<string> priorityQueue;
    private List<string> visitedNodes;
    private Dictionary<string, int> distances;
    private Dictionary<string, string> predecessors;
    private List<TrainCard> handtraincards;

    public ShortestPathFinder(Dictionary<string, List<CityConnection>> cityMap)
    {
        this.cityMap = cityMap;
    }
    
    //Normal version of dijkstra algorithm
    public Dictionary<string, int> Dijkstra(string startNode, PlayerScript player)
    {
        //Dictionary<string, string[]> distancesPredecessors = new Dictionary<string, string[]>(); // string[0]: kürzester Weg, string[1]: Vorgänger
        distances = InitializeDistances(startNode); 
        predecessors = InitializePredecessors(startNode); 
        priorityQueue = new List<string>();
        visitedNodes = new List<string>();
        

        Enqueue(startNode);
        while (priorityQueue.Count > 0)
        {           
            // Knoten mit der höchsten Priorität wird aus der Warteschlage geholt und dort entfernt
            string currentNode = Dequeue();
            //Es werden alle Nachbarknoten des aktuellen Knoten betrachtet
            foreach (CityConnection connection in cityMap[currentNode])
            {
                int costs = distances[currentNode] + connection.weight;
                if (costs < distances[connection.city])
                {
                    distances[connection.city] = costs;
                    predecessors[connection.city] = currentNode;
                }
                
                Enqueue(connection.city);
            }
        }
        
        //foreach (string node in distances.Keys)
        //{
        //    Debug.Log("Form " + startNode + " to " + node + ", Kosten:  "+ distances[node] + ", Vorgaenger:  "+ predecessors[node]);
        //}        

        return distances;
    }
    

    //the method returns the shortest distance between the start node and all other nodes
    private Dictionary<string, int> InitializeDistances(string startNode)
    {
        distances = new Dictionary<string, int>();

        foreach (var city in cityMap.Keys)
        {
            //Maximum possible value for the int data type (instead of infinite)
            distances.Add(city, int.MaxValue);
        }

        distances[startNode] = 0;
        return distances;
    }

    //the method returns the predecessors of every nodes by calculate the shortest distance between the start node and all other nodes
    private Dictionary<string, string> InitializePredecessors(string startNode)
    {
        predecessors = new Dictionary<string, string>();
        foreach (var city in cityMap.Keys)
        {
            predecessors.Add(city, null);
        }

        return predecessors;
    }


    //the method identifies the node with the highest priority
    public string FindElementInQueueWithHighestPriority()
    {
        string elementWithHighestPriority = priorityQueue[0];
        int lowestValue = distances[elementWithHighestPriority];

        // Iterieren durch die Liste und Vergleichen der Werte im Dictionary
        foreach (string element in priorityQueue)
        {
            int currentValue = distances[element];

            if (currentValue < lowestValue)
            {
                lowestValue = currentValue;
                elementWithHighestPriority = element;
            }
        }

        return elementWithHighestPriority;
    }

    //Enqueue: Adds an element with a certain priority to the queue.
    public void Enqueue(string city)
    {
        if ((!visitedNodes.Contains(city)) && (!priorityQueue.Contains(city))) 
        {
            priorityQueue.Add(city);
        }        
    }

    //Dequeue: Removes the element with the highest priority from the queue and returns it.
    public string Dequeue()
    {
        if (priorityQueue.Count == 0)
        {
            Debug.Log("PriorityQueue is empty");
        }

        string elementWithHighestPriority = FindElementInQueueWithHighestPriority();
        priorityQueue.Remove(elementWithHighestPriority);
        visitedNodes.Add(elementWithHighestPriority);
        return elementWithHighestPriority;
    }

//-------------------------------------- below you will find modified versions of dijkstra ------------------------------------------------


    //This version of Dijkstra reduces the weights of the edges of the given player to 0 and exclude acquired routes of other players.
    public Dictionary<string, int> DijkstraReduceWeightsOfOwnRoutes(string startNode, PlayerScript player)
    {
        //Dictionary<string, string[]> distancesPredecessors = new Dictionary<string, string[]>(); // string[0]: kürzester Weg, string[1]: Vorgänger
        distances = InitializeDistances(startNode); 
        predecessors = InitializePredecessors(startNode); 
        priorityQueue = new List<string>();
        visitedNodes = new List<string>();
        
        Enqueue(startNode);
        while (priorityQueue.Count > 0)
        {           
            // Knoten mit der höchsten Priorität wird aus der Warteschlage geholt und dort entfernt
            string currentNode = Dequeue();
            //Es werden alle Nachbarknoten des aktuellen Knoten betrachtet
            foreach (CityConnection connection in cityMap[currentNode])
            {
                if (checkIfEdgeIsAvailable(connection, player)) 
                {
                    int costs = distances[currentNode] + calculateWeight(connection, player);
                    if (costs < distances[connection.city])
                    {
                        distances[connection.city] = costs;
                        predecessors[connection.city] = currentNode;
                    }
                
                    Enqueue(connection.city);
                }
            }
        }
        
        //foreach (string node in distances.Keys)
        //{
        //    Debug.Log("Form " + startNode + " to " + node + ", Kosten:  "+ distances[node] + ", Vorgaenger:  "+ predecessors[node]);
        //}        

        return distances;
    }

    //This version of Dijkstra reduces the weights of the edges of the given player to 0 and exclude acquired routes of other players and returns the predecessors.
    public Dictionary<string, string> DijkstraReduceWeightsOfOwnRoutesAndReturnsPredecessors(string startNode, PlayerScript player)
    {
        //Dictionary<string, string[]> distancesPredecessors = new Dictionary<string, string[]>(); // string[0]: kürzester Weg, string[1]: Vorgänger
        distances = InitializeDistances(startNode); 
        predecessors = InitializePredecessors(startNode); 
        priorityQueue = new List<string>();
        visitedNodes = new List<string>();
        
        Enqueue(startNode);
        while (priorityQueue.Count > 0)
        {           
            // Knoten mit der höchsten Priorität wird aus der Warteschlage geholt und dort entfernt
            string currentNode = Dequeue();
            //Es werden alle Nachbarknoten des aktuellen Knoten betrachtet
            foreach (CityConnection connection in cityMap[currentNode])
            {
                if (checkIfEdgeIsAvailable(connection, player)) 
                {
                    int costs = distances[currentNode] + calculateWeight(connection, player);
                    if (costs < distances[connection.city])
                    {
                        distances[connection.city] = costs;
                        predecessors[connection.city] = currentNode;
                    }
                
                    Enqueue(connection.city);
                }
            }
        }
        
        //foreach (string node in distances.Keys)
        //{
        //    Debug.Log("Form " + startNode + " to " + node + ", Kosten:  "+ distances[node] + ", Vorgaenger:  "+ predecessors[node]);
        //}        

        return predecessors;
    }

    public int calculateWeight(CityConnection connection, PlayerScript player) 
    {
        GameObject route = GameObject.Find(connection.routeName);
        PlayerScript owner = route.GetComponent<RouteScript>().owner;

        if (owner == player) 
        {
            return 0;
        }
        else
        {
            return connection.weight;
        }
    }

    //This method returns if the player is the owner of the selected route
    public bool checkIfEdgeIsAvailable(CityConnection connection, PlayerScript player)
    {
        GameObject route = GameObject.Find(connection.routeName);
        if (route.GetComponent<RouteScript>().occupied && player != route.GetComponent<RouteScript>().owner) 
        {
            return false;
        } 
        else 
        {
            return true;
        }
    }


    //This version of Dijkstra reduces the weights of the edges of the given player to 0 and increase the weights of the edges of other players
    public Dictionary<string, int> DijkstraToCheckIfCitiesAreReachable(string startNode, PlayerScript player)
    {
        //Dictionary<string, string[]> distancesPredecessors = new Dictionary<string, string[]>(); // string[0]: kürzester Weg, string[1]: Vorgänger
        distances = InitializeDistances(startNode);
        predecessors = InitializePredecessors(startNode);
        priorityQueue = new List<string>();
        visitedNodes = new List<string>();

        Enqueue(startNode);
        while (priorityQueue.Count > 0)
        {
            // Knoten mit der höchsten Priorität wird aus der Warteschlage geholt und dort entfernt
            string currentNode = Dequeue();
            //Es werden alle Nachbarknoten des aktuellen Knoten betrachtet
            foreach (CityConnection connection in cityMap[currentNode])
            {
                int costs = distances[currentNode] + calculateWeight1(connection, player);
                if (costs < distances[connection.city])
                {
                    distances[connection.city] = costs;
                    predecessors[connection.city] = currentNode;
                }

                Enqueue(connection.city);
            }
        }

        //foreach (string node in distances.Keys)
        //{
        //    Debug.Log("Form " + startNode + " to " + node + ", Kosten:  "+ distances[node] + ", Vorgaenger:  "+ predecessors[node]);
        //}        

        return distances;
    }

    public int calculateWeight1(CityConnection connection, PlayerScript player)
    {
        GameObject route = GameObject.Find(connection.routeName);
        PlayerScript owner = route.GetComponent<RouteScript>().owner;

        if (owner == player)
        {
            return 0;
        }
        else if (owner != null)
        {
            return connection.weight;
        }
        else
        {
            return int.MaxValue;
        }
    }


    //---------------------------------------------------------------------------------------------------------------------------------------------

    //his version of dijkstra algorithm find the longest route
    public Dictionary<string, int> DijkstraToFindTheLongestRoute(string startNode, PlayerScript player)
    {
        distances = InitializeDistancesForLongestRoute(startNode);
        predecessors = InitializePredecessors(startNode);
        priorityQueue = new List<string>();
        visitedNodes = new List<string>();
        edgesPath = InitializeEdgePath(startNode);

        Enqueue(startNode);
        while (priorityQueue.Count > 0)
        {           
            // Knoten mit der höchsten Priorität wird aus der Warteschlage geholt und dort entfernt
            string currentNode = DequeueForLongestRoute();
            //Es werden alle Nachbarknoten des aktuellen Knoten betrachtet
            foreach (CityConnection connection in cityMap[currentNode])
            {
                //If condition checks if the player is the owner of the current connection. Edges which are not owned by the player are neglected. 
                if (checkOwnerOfEdge(connection, player) && (predecessors[currentNode] != connection.city)) 
                {
                    int costs = distances[currentNode] + connection.weight;
                    if (costs > distances[connection.city])
                    {
                        distances[connection.city] = costs;
                       predecessors[connection.city] = currentNode;
                        //edgesPath[connection.city].Add(connection);
                   }
              
                    Enqueue(connection.city);
                }
            }
        }
        /*
        foreach (string node in distances.Keys)
        {
           Debug.Log("Form " + startNode + " to " + node + ", Kosten:  "+ distances[node] + ", Vorgaenger:  "+ predecessors[node]);
        }
        */
        return distances;
    }

    //the method returns the shortest distance between the start node and all other nodes
    private Dictionary<string, List<CityConnection>> InitializeEdgePath(string startNode)
    {
        edgesPath = new Dictionary<string, List<CityConnection>>();

        foreach (var city in cityMap.Keys)
        {
            //Maximum possible value for the int data type (instead of infinite)
            edgesPath.Add(city, new List<CityConnection>());
        }

        return edgesPath;
    }

    //the method returns the longest distance between the start node and all other nodes
    private Dictionary<string, int> InitializeDistancesForLongestRoute(string startNode)
    {
        distances = new Dictionary<string, int>();

        foreach (var city in cityMap.Keys)
        {
            //Minimum possible value for the int data type (instead of infinite)
            distances.Add(city, 0);
        }

        distances[startNode] = 0;
        return distances;
    }

    //Dequeue: Removes the element with the highest priority from the queue and returns it.
    public string DequeueForLongestRoute()
    {
        if (priorityQueue.Count == 0)
        {
            Debug.Log("PriorityQueue is empty");
        }

        string elementWithHighestPriority = FindElementInQueueWithHighestPriorityForLongestRoute();
        priorityQueue.Remove(elementWithHighestPriority);
        visitedNodes.Add(elementWithHighestPriority);
        return elementWithHighestPriority;
    }

    //the method identifies the node with the highest priority
    public string FindElementInQueueWithHighestPriorityForLongestRoute()
    {
        string elementWithHighestPriority = priorityQueue[0];
        int highestValue = distances[elementWithHighestPriority];

        // Iterieren durch die Liste und Vergleichen der Werte im Dictionary
        foreach (string element in priorityQueue)
        {
            int currentValue = distances[element];

            if (currentValue > highestValue)
            {
                highestValue = currentValue;
                elementWithHighestPriority = element;
            }
        }

        return elementWithHighestPriority;
    }

    //This method returns if the player is the owner of the selected route
    public bool checkOwnerOfEdge(CityConnection connection, PlayerScript player)
    {
        PlayerScript routeOwner = getRouteOwner(connection.routeName);
        
        if (player == routeOwner) 
        {
            return true;
        }
        else 
        {
            return false;
        }
    }

    public PlayerScript getRouteOwner(string routeName)
    {
        GameObject route = GameObject.Find(routeName);
        return route.GetComponent<RouteScript>().owner; 
    }

    public int findLongestRoute(PlayerScript player)
    {
        int longestDistance = 0;
        foreach(string city in player.cities)
        {
            Dictionary<string, int> distanceDictionary = DijkstraToFindTheLongestRoute(city, player);
            if (longestDistance < distanceDictionary.Values.Max())
            {
                longestDistance = distanceDictionary.Values.Max();
            }
        }
        return longestDistance;
    }

//--------------------------------------------------------------------------------------------


    //Normal version of dijkstra algorithm with distances and predecessors as return
    public Dictionary<string, string[]> DijkstraWithDistancesPredecessors(string startNode, PlayerScript player)
    {
        Dictionary<string, string[]> distancesPredecessors = new Dictionary<string, string[]>(); // string[0]: kürzester Weg, string[1]: Vorgänger
        distances = InitializeDistances(startNode); 
        predecessors = InitializePredecessors(startNode); 
        priorityQueue = new List<string>();
        visitedNodes = new List<string>();

        Enqueue(startNode);
        while (priorityQueue.Count > 0)
        {           
            // Knoten mit der höchsten Priorität wird aus der Warteschlage geholt und dort entfernt
            string currentNode = Dequeue();
            //Es werden alle Nachbarknoten des aktuellen Knoten betrachtet
            foreach (CityConnection connection in cityMap[currentNode])
            {
                int costs = distances[currentNode] + connection.weight;
                if (costs < distances[connection.city])
                {
                    distances[connection.city] = costs;
                    predecessors[connection.city] = currentNode;
                }
                
                Enqueue(connection.city);
            }
        }


        foreach (string city in distances.Keys) 
        {
            distancesPredecessors.Add(city, new string[] { distances[city].ToString(), predecessors[city] });
        }
        
        //foreach (string node in distances.Keys)
        //{
        //    Debug.Log("Form " + startNode + " to " + node + ", Kosten:  "+ distances[node] + ", Vorgaenger:  "+ predecessors[node]);
        //}        

        return distancesPredecessors;
    }

    //Normal version of dijkstra algorithm which returns the predecessors
    public Dictionary<string, string> DijkstraWithPredecessors(string startNode, PlayerScript player)
    {
        distances = InitializeDistances(startNode); 
        predecessors = InitializePredecessors(startNode); 
        priorityQueue = new List<string>();
        visitedNodes = new List<string>();

        Enqueue(startNode);
        while (priorityQueue.Count > 0)
        {           
            // Knoten mit der höchsten Priorität wird aus der Warteschlage geholt und dort entfernt
            string currentNode = Dequeue();
            //Es werden alle Nachbarknoten des aktuellen Knoten betrachtet
            foreach (CityConnection connection in cityMap[currentNode])
            {
                int costs = distances[currentNode] + connection.weight;
                if (costs < distances[connection.city])
                {
                    distances[connection.city] = costs;
                    predecessors[connection.city] = currentNode;
                }
                
                Enqueue(connection.city);
            }
        }

        
        //foreach (string node in distances.Keys)
        //{
        //    Debug.Log("Form " + startNode + " to " + node + ", Kosten:  "+ distances[node] + ", Vorgaenger:  "+ predecessors[node]);
        //}        

        return predecessors;
    }

//------------------------------------------------------------------------------------------------------------------------


    //This version of Dijkstra reduces the weights of the edges of the given player to 0 and exclude acquired routes of other players and considered the handcards of the active player.
    public Dictionary<string, string> DijkstraReduceWeightsOfOwnRoutesAndConsideredHandcardsAndReturnsPredecessors(string startNode, PlayerScript player)
    {
        //Dictionary<string, string[]> distancesPredecessors = new Dictionary<string, string[]>(); // string[0]: kürzester Weg, string[1]: Vorgänger
        distances = InitializeDistances(startNode); 
        predecessors = InitializePredecessors(startNode); 
        priorityQueue = new List<string>();
        visitedNodes = new List<string>();
        handtraincards = player.handtraincards;
        
        Enqueue(startNode);
        while (priorityQueue.Count > 0)
        {           
            // Knoten mit der höchsten Priorität wird aus der Warteschlage geholt und dort entfernt
            string currentNode = Dequeue();
            //Es werden alle Nachbarknoten des aktuellen Knoten betrachtet
            foreach (CityConnection connection in cityMap[currentNode])
            {
                if (checkIfEdgeIsAvailable(connection, player)) 
                {
                    int costs = distances[currentNode] + calculateWeightIncludesHandcards(connection, player);
                    if (costs < distances[connection.city])
                    {
                        distances[connection.city] = costs;
                        predecessors[connection.city] = currentNode;
                    }
                
                    Enqueue(connection.city);
                }
            }
        }
        
        //foreach (string node in distances.Keys)
        //{
        //    Debug.Log("Form " + startNode + " to " + node + ", Kosten:  "+ distances[node] + ", Vorgaenger:  "+ predecessors[node]);
        //}        

        return predecessors;
    }

    public int calculateWeightIncludesHandcards(CityConnection connection, PlayerScript player) 
    {
        GameObject route = GameObject.Find(connection.routeName);
        PlayerScript owner = route.GetComponent<RouteScript>().owner;
        string routeColor = route.GetComponent<RouteScript>().routeColor;

        //determine the number of handcards with the respective color like the route
        int trainCards = player.handtraincards.Count(card => card.color == routeColor);
        trainCards += player.handtraincards.Count(card => card.color == "Joker");

        if (owner == player) 
        {
            return 0;
        }
        else
        {
            if (connection.weight - trainCards > 0)
            {
                return connection.weight - trainCards;
            }
            else
            {
                return 0;
            }
        }
    }



}