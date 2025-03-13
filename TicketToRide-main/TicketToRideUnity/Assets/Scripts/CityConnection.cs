using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityConnection
{
    public string city { get; set; }
    public string routeName { get; set; }
    public int weight { get; set; }
    // Added for MLPlayer : QLearning
    public double routeValue { get; set; }
    public bool isTarget { get; set; }

    public CityConnection(string neighborCity, string routeName)
    {
        this.city = neighborCity;
        this.routeName = routeName;
        this.weight = RouteLength();
        // Added 2 lines for MLPlayer : QLearning
        this.routeValue = 0;
        this.isTarget = false;
    }

    public int RouteLength()
    {
        if (GameObject.Find(routeName) != null)
        {
            GameObject route = GameObject.Find(routeName);
            return route.GetComponent<RouteScript>().routeLength;
        }
        else
        {
            Debug.Log("Routename not found: " + routeName + ", neighborCity: " + city);
            return 1000;
        }
    }

    // Added for QLearning
    public double RouteValue()
    {
        if (GameObject.Find(routeName) != null)
        {
            GameObject route = GameObject.Find(routeName);
            //double routeValue = route.GetComponent<RouteScript>().routeLength;
            double routeValue = 0;
            return routeValue;
        }
        else
        {
            Debug.Log("Routename not found: " + routeName + ", neighborCity: " + city);
            return 0;
        }
    }
}
