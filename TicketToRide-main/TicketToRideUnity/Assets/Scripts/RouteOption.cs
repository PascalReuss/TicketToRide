using System;


[Serializable]
public class RouteOption
{
    public int destinationCardPoints;
    public int faceUpCards;
    public int lengthOfRoute;
    public string nameOfRoute;
    public int trainCards;
    public string colorOfRoute;


    public RouteOption()
    {

    }
    public RouteOption(int destinationCardPoints, int faceUpCards, int lengthOfRoute, string nameOfRoute, int trainCards)
    {
        this.destinationCardPoints = destinationCardPoints;
        this.faceUpCards = faceUpCards;
        this.lengthOfRoute = lengthOfRoute;
        this.nameOfRoute = nameOfRoute;
        this.trainCards = trainCards;
    }

    public RouteOption(int destinationCardPoints, int faceUpCards, int lengthOfRoute, string nameOfRoute, int trainCards, string colorOfRoute) : 
        this(destinationCardPoints, faceUpCards, lengthOfRoute, nameOfRoute, trainCards)
    {
        this.colorOfRoute = colorOfRoute;
    }
}
