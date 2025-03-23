using System;
using System.Runtime.Serialization;
using Assets.Scripts.AI;

namespace Assets.Scripts.Utility
{
    [Serializable]
    public class Request
    {
        public Situation situation;
        public Case newCase;
        public string requestType;

		public Request()
		{

		}

		public Request(Situation situation, string requestType)
        {
            this.situation = situation;
            this.requestType = requestType;
        }
        
        public Request(string requestType)
        {
            this.requestType = requestType;
        }

        public Request (Case newCase, string requestType)
        {
            this.newCase = newCase;
            this.requestType = requestType;
        }

		public override string ToString()
        {
            return "Request: " + situation.ToString();
        }
    }


    [Serializable]
    public class AdaptionRequest
    {
        public const string UNFEASABLE_TYPE = "UNFEASABLE";

        public string requestType;
        public string adaptionType; // UNFEASABLE
        public SituationSolutionAnswer[] casesToAdapt;

        public AdaptionRequest()
        {

        }

        public AdaptionRequest(string adaptionType, SituationSolutionAnswer[] casesToAdapt)
        {
            requestType = "ADAPTION";
            this.adaptionType = adaptionType;
            this.casesToAdapt = casesToAdapt;
        }
    }

    [Serializable]
    class AnswerRoot
    {
        public SituationSolutionAnswer[] routes;
    }
    [Serializable]
    public enum SolutionType
    {
        DrawTrainCards,
        DrawDestinationCards,
        ClaimRoute
    }
    [Serializable]
    public class SituationSolutionAnswer
    {
        public string plan;
        public double quality;
        public RouteOptionsDummy routeOptions;
    }

    [Serializable]
    public class RouteOptionsDummy
    {
        public int destinationCardPoints;
        public int faceUpCards;
        public int trainCards;
        public string nameOfRoute;
        public int lengthOfRoute;

        public RouteOption Transform()
        {
            return new RouteOption(destinationCardPoints, faceUpCards, lengthOfRoute, nameOfRoute, trainCards,GameManager.ExtractSubstring(nameOfRoute));
        }
    }
}