using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class QTable
{
    private GameState gameState;
    private MetaLayer metaLayer;

    public Dictionary<string, List<CityConnection>> qDictionary { get; set;}
    public Dictionary<string, List<CityConnection>> qDictionaryBackup { get; set; }

    public QTable(GameState gameState, MetaLayer metaLayer)
    {
        this.gameState = gameState;
        this.metaLayer = metaLayer;
    }

    public void initializeQDictionary()
    {
        qDictionary = new Dictionary<string, List<CityConnection>>();
        qDictionaryBackup = new Dictionary<string, List<CityConnection>>();
        qDictionary = gameState.cityMap;
        qDictionaryBackup = qDictionary;
        List<string> qTargets = gameState.getPlayerTargetCities(gameState.Player);
        foreach (var x in qTargets)
            Debug.Log(x);
    }
    public void deleteRoutesInQDictionary(string routeName)
    {
        // Durchlaufe das gesamte qDictionary
        foreach (var cityConnections in qDictionary.Values)
        {
            // Entferne alle CityConnections, deren routeName dem übergebenen routeName entspricht
            cityConnections.RemoveAll(connection => connection.routeName == routeName);
        }
    }

    public void initializeQValues(List<string> qTargets, Dictionary<string, List<CityConnection>> qDictionary)
    {
        Debug.Log("init Q Values");
        foreach (var targetCity in qTargets)
        {
            if (qDictionary.ContainsKey(targetCity))
            {
                foreach (var connection in qDictionary[targetCity])
                {
                    connection.routeValue = 1000;
                }
                Debug.Log($"Updated all routeValues in {targetCity} to 1000");
            }
            else
            {
                Debug.LogWarning($"City {targetCity} not found in qDictionary");
            }
        }
    }
    public void UpdateRouteValue(string city, string routeName, double newValue)
    {
        if (qDictionary.ContainsKey(city))
        {
            foreach (var connection in qDictionary[city])
            {
                if (connection.routeName == routeName)
                {
                    connection.routeValue = newValue;
                    //Debug.Log($"Updated routeValue for {routeName} in {city} to {newValue}");
                    return;
                }
            }
            Debug.LogWarning($"Route {routeName} not found in {city}");
        }
        else
        {
            Debug.LogWarning($"City {city} not found in QTable");
        }
    }
    public void UpdateRouteValuesRecursive(List<string> currentTargets, double value, int stepsRemaining, Dictionary<string, string> previousCities, HashSet<string> usedRouteNames)
    {
        // Abbruchbedingung
        if (stepsRemaining <= 0 || currentTargets.Count == 0 || value <= 0)
            return;

        List<string> nextTargets = new List<string>();
        Dictionary<string, string> nextPreviousCities = new Dictionary<string, string>();

        foreach (var city in currentTargets)
        {
            if (qDictionary.ContainsKey(city))
            {
                foreach (var connection in qDictionary[city])
                {
                    // Rückverfolgung vermeiden
                    if (previousCities.ContainsKey(city) && connection.city == previousCities[city])
                    {
                        Debug.Log($"Skipping {connection.city} to prevent backtracking from {city}");
                        continue;
                    }

                    // Überprüfung, ob routeName bereits genutzt wurde
                    if (usedRouteNames.Contains(connection.routeName))
                    {
                        Debug.Log($"Skipping update for {connection.city} to {city} (RouteName {connection.routeName} already used)");
                        continue;
                    }

                    // Aktualisierung, wenn der Wert niedriger ist
                    if (connection.routeValue < value)
                    {
                        connection.routeValue = value;
                        usedRouteNames.Add(connection.routeName); // Speichert den routeName
                        Debug.Log($"Updated routeValue for {connection.city} to {city} via {connection.routeName} : {value}");

                        if (!nextTargets.Contains(connection.city))
                        {
                            nextTargets.Add(connection.city);
                            nextPreviousCities[connection.city] = city;
                        }
                    }
                    else
                    {
                        Debug.Log($"Skipping {connection.city} to {city} (Current value: {connection.routeValue}, skipped value: {value})");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"City {city} not found in qDictionary");
            }
        }

        // Rekursiver Aufruf mit aktualisiertem usedRouteNames
        UpdateRouteValuesRecursive(nextTargets, value - 50, stepsRemaining - 1, nextPreviousCities, usedRouteNames);
    }

    public void PrintQTable()
    {
        Debug.Log("=== QTable Contents ===");

        foreach (var city in qDictionary.Keys)
        {
            Debug.Log($"City: {city}");

            foreach (var connection in qDictionary[city])
            {
                Debug.Log($"  → Route: {connection.routeName}, Destination: {connection.city}, " +
                          $"Weight: {connection.weight}, RouteValue: {connection.routeValue}, " +
                          $"IsTarget: {connection.isTarget}");
            }
        }

        Debug.Log("=== End of QTable ===");
    }
    public void printQTableForCity(string city)
    {
        Debug.Log($"=== QTable für Stadt: {city} ===");

        foreach (var connection in qDictionary[city])
        {
            Debug.Log($"  → Route: {connection.routeName}, Destination: {connection.city}, " +
                      $"Weight: {connection.weight}, RouteValue: {connection.routeValue}, " +
                      $"IsTarget: {connection.isTarget}");
        }

        Debug.Log($"=== Ende der QTable für {city} ===");
    }
    public void PrintQTableGrid()
    {
        if (qDictionary == null || qDictionary.Count == 0)
        {
            Debug.Log("QTable is empty or not initialized.");
            return;
        }

        // Liste der Städte aus den Keys des Dictionaries extrahieren
        List<string> cityList = new List<string>(qDictionary.Keys);

        // Header ausgeben
        string header = "\t";  // Erste Spalte bleibt leer für Zeilenüberschriften
        foreach (var city in cityList)
        {
            header += city.PadRight(10);  // Städte als Spaltenüberschriften
        }
        Debug.Log(header);

        // Daten ausgeben
        foreach (var rowCity in cityList)
        {
            string row = rowCity.PadRight(30);  // Zeilenüberschrift

            foreach (var colCity in cityList)
            {
                // Finde die RouteValue, falls eine Verbindung existiert
                CityConnection connection = qDictionary[rowCity].FirstOrDefault(c => c.city == colCity);
                string value = connection != null ? connection.routeValue.ToString("F1") : "N/A";  // Wert auf eine Nachkommastelle runden, oder "N/A" 

                row += value.PadRight(30); // Q-Werte oder "N/A" falls keine Verbindung
            }

            Debug.Log(row);
        }
    }
    public void printQValue(string routeName)
    {
        foreach (var city in qDictionary.Keys)
        {
            foreach (var connection in qDictionary[city])
            {
                if (connection.routeName == routeName)
                    Debug.Log("RouteValue = " + connection.routeValue);
            }
        }
    }
}
