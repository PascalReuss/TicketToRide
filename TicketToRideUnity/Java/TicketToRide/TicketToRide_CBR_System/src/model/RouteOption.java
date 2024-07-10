package model;

public class RouteOption {
	
	public int destinationCardPoints;
    public int faceUpCards;
    public int lengthOfRoute;
    public String nameOfRoute;
    public int trainCards;

    public RouteOption(int destinationCardPoints, int faceUpCards, int lengthOfRoute, String nameOfRoute, int trainCards)
    {
        this.destinationCardPoints = destinationCardPoints;
        this.faceUpCards = faceUpCards;
        this.lengthOfRoute = lengthOfRoute;
        this.nameOfRoute = nameOfRoute;
        this.trainCards = trainCards;
    }

}
