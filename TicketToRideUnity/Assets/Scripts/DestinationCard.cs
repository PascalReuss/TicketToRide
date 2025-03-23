using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationCard
{
    public string[] citiesAndPoints { get; set; }
    public string citiesAsString { get; set; }
    public Sprite spriteImage { get; set; }
    public int points { get; set; }
    public bool fulfilled {get; set;}

    public DestinationCard(string ressourceName, Sprite spriteImage)
    {
        this.citiesAndPoints = ressourceName.Split('_');
        this.citiesAsString = citiesAndPoints[0] + "_" + citiesAndPoints[1];
        this.spriteImage = spriteImage;
        this.points = int.Parse(citiesAndPoints[2]);
    }
}
