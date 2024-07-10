using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityConnection
{
    
    public string city { get; set; }
    public string routeName { get; set; }
    public int weight { get; set; }
 
    public CityConnection(string neighborCity, string routeName)
    {
        this.city = neighborCity;
        this.routeName = routeName;
        this.weight = RouteLength();
    }

    public int RouteLength()
    {
        if (GameObject.Find(routeName) != null)
        {
            GameObject route = GameObject.Find(routeName);
            return route.GetComponent<RouteScript>().routeLength;
        } else 
        {
            Debug.Log("Routename not found: " + routeName + ", neighborCity: " + city);
            return 1000;
        }
         
    }

}
