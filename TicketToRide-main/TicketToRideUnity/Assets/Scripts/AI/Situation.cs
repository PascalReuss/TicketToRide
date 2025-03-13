using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Assets.Scripts.AI
{
    [DataContract]
    [Serializable]
    public class Situation
    {
        public int handTrainCardsCount;
        public int destinationCardsCount;
        public int availableWagons;
        public int minimalAvailableWagons;
        public float averageScoreOfRoutes;
        //[DataMember]
        //public int[] lengthOfFollowedRoute { get; set; }
        //[DataMember]
        //public int[] trainCardsForFollowedRoute { get; set; }
        //[DataMember]
        //public int[] faceUpCardsForFollowedRoute { get; set; }
        //[DataMember]
        //public int[] destinationCardPointsOfFollowedRoute { get; set; }
        [DataMember]
        public List<RouteOption> routeOptions;
        [DataMember]
        public int placeholder7;

        public Situation()
        {

        }

        public Situation(int handTrainCardsCount, int destinationCardsCount, int availableWagons, int minimalAvailableWagons,
            float averageScoreByRoutes, List<RouteOption> routeOptions)
        {
            this.handTrainCardsCount = handTrainCardsCount;
            this.destinationCardsCount = destinationCardsCount;
            this.availableWagons = availableWagons;
            this.minimalAvailableWagons = minimalAvailableWagons;
            this.averageScoreOfRoutes = averageScoreByRoutes;
            this.routeOptions = routeOptions;
        }

        public Situation(int handTrainCardsCount, int destinationCardsCount, int availableWagons, int minimalAvailableWagons, 
            float averageScoreByRoutes, int[] lengthOfFollowedRoute, int[] trainCardsForFollowedRoute, int[] faceUpCardsForFollowedRoute, 
            int[] destinationCardPointsOfFollowedRoute)
        {
            this.handTrainCardsCount = handTrainCardsCount;
            this.destinationCardsCount = destinationCardsCount;
            this.availableWagons = availableWagons;
            this.minimalAvailableWagons = minimalAvailableWagons;
            this.averageScoreOfRoutes = averageScoreByRoutes;
            //this.lengthOfFollowedRoute = lengthOfFollowedRoute;
            //this.trainCardsForFollowedRoute = trainCardsForFollowedRoute;
            //this.faceUpCardsForFollowedRoute = faceUpCardsForFollowedRoute;
            //this.destinationCardPointsOfFollowedRoute = destinationCardPointsOfFollowedRoute;
        }
    }
}
