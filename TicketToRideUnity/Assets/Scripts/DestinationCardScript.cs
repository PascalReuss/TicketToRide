using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DestinationCardScript : MonoBehaviour
{
    public string[] citiesAndPoints { get; set; }
    public string citiesAsString { get; set; }
    public int points { get; set; }

    private void Start()
    {
        citiesAndPoints = transform.GetChild(1).GetComponent<Image>().sprite.name.Split('_');
        citiesAsString = citiesAndPoints[0] + "_" + citiesAndPoints[1];
        points = int.Parse(citiesAndPoints[2]);
    }
}
