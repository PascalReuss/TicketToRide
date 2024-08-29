using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    // human player or AI
    public bool isAI { get; set; }
    // KI #1
    public bool isHardcodedAI1 { get; set; }
    // KI #2
    public bool isHardcodedAI2 { get; set; }
    // human player
    public bool isHuman { get; set; }
    // evil AI
    public bool isEvilAI { get; set; }
    // whether it is the players turn
    public bool isActivePlayer { get; set; }
    
    public string playerName { get; set; }
    public string playerColorName { get; set; }
    public Color playerColor { get; set; }
    //Score of the player
    public int score { get; set; }
    public int longestRouteDistance { get; set; }

    //number of the still availableWagons wagons
    public int availableWagons { get; set; }

    // player's hand train cards
    public List<TrainCard> handtraincards { get; set; }

    // included citys
    public List<string> cities { get; set; }
    
    // player's hand destination cards
    public List<DestinationCard> handdestinationcards { get; set; }

    // all routes of the player
    public List<string> acquiredRoutes { get; set; }

    public PlayerScript()
    {
    }

    private void Awake()
    {
        handdestinationcards = new List<DestinationCard>();
        acquiredRoutes = new List<string>();
        handtraincards = new List<TrainCard>();
        availableWagons = 45;
        score = 0;
        longestRouteDistance = 0;
        cities = new List<string>();;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddTrainCardToHand(TrainCard card)
    {
        handtraincards.Add(card);
    }
    public void AddDestinationCardToHand(DestinationCard card)
    {
        handdestinationcards.Add(card);
    }
    
}
