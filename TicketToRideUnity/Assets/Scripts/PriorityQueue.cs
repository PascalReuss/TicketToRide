using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class PriorityQueue : MonoBehaviour
{
    private List<CityConnection> cityConnections = new List<CityConnection>();

    //Enqueue: Adds an element with a certain priority to the queue.
    public void Enqueue(CityConnection cityConnection)
    {
        cityConnections.Add(cityConnection);
        cityConnections = cityConnections.OrderBy(element => element.weight).ToList();
    }

    //Dequeue: Removes the element with the highest priority from the queue and returns it.
    public CityConnection Dequeue()
    {
        if (cityConnections.Count == 0)
        {
            throw new InvalidOperationException("PriorityQueue is empty");
        }

        CityConnection prioritizedCityConnection = cityConnections[0];
        cityConnections.RemoveAt(0);
        return prioritizedCityConnection;
    }

    //Count: Returns the number of elements in the queue.
    public int Count => cityConnections.Count;
    
}