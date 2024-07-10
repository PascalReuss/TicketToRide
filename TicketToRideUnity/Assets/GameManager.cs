using Assets.Scripts.AI;
using Assets.Scripts.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.ConstrainedExecution;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;
using static GameManager;
using Debug = UnityEngine.Debug;

public class GameManager : MonoBehaviour
{
    // when this is true, the unity project will take care of starting the KI
    // otherwise the user must serve a KI on port 5555
    const bool START_STANDALONE_KI = true;

    Process foo;
    Connection connection;
    // --- GameObjects

    // list of player
    public List<PlayerScript> players;
    // the active player
    public PlayerScript activePlayer { get; set; }
    // the handcards shown on the screen (handcards of active player) - the clones of the handcards are stored here
    public GameObject handcardsPanel;
    // the hand destinationcards shown on the screen (handcards of active player)
    public GameObject handdestinationcardsPanel; 
    // the display panel for destinationcards drawn from the deck
    public GameObject destinationcardsDrawDisplay;
    // the display panel for destinationcards the active player has
    public GameObject destinationcardsPlayerDisplay;
    // empty gameobject container for the sprites of the display
    public GameObject spritesDestinationcardsDisplay;
    // panel for displaying the scores of the players
    public GameObject playerScorePanel;
    // turnCounter
    public GameObject turnCounterDisplay;
    // selecting RouteOption Text
    public GameObject selectingRouteOptionText;
    // button for confirming the chosen destination cards
    public Button confirmHandCardsBtn;
    // button for confirming the chosen destination cards
    public Button confirmDestinationCardsBtn;
    // routeParent GameObject of routes
    public GameObject routes;
    // panel of result screen
    public GameObject resultTable;
    // train card  deck ingame
    public GameObject traincardsDeck;
    // train card discard deck ingame
    public GameObject traincardsDiscardDeck;
    // train card  deck ingame
    public GameObject destinationcardsDeck;

    // --- prefabs ---

    // prefab of traincard
    public GameObject trainCardPrefab;
    // prefab of traincard on hand
    public GameObject trainCardOnHandPrefab;
    // prefab of destinationcard
    public GameObject destinationCardPrefab;
    // prefab of trainfig
    public GameObject trainfig;

    public Dictionary<string, List<string>> cityMap1 = new Dictionary<string, List<string>>();

    // traincards draw pile deck
    private List<TrainCard> traincards = new List<TrainCard>();
    // traincards discard pile deck
    private List<TrainCard> traincardsDiscard = new List<TrainCard>();
    // cardColors for the traincards
    private List<string> cardColors = new List<string> { "Purple", "White", "Blue", "Yellow", "Orange", "Black", "Red", "Green", "Joker" };

    private Dictionary<string, Color> colorList;

    // sprites for the traincards, which are set in Unity
    public Sprite[] spritesTraincards;

    // all destination cards
    private List<DestinationCard> destinationcards = new List<DestinationCard>();
    // sprites for the traincards, which are set in Unity
    public Sprite[] spritesDestinationcards;
    // a list for saving the drawn destination cards
    List<DestinationCard> drawnCards = new List<DestinationCard>();
    // text showing how many destination card the players has
    public Text destinationcardsCount;

    // the 5 open traincards
    private TrainCard[] faceupTraincards = new TrainCard[5];
    // the 5 open traincard-GameObjects
    public GameObject faceupTraincardsParent;

    public Dictionary<string, List<CityConnection>> cityMap { get; set; }
    public Dictionary<int, int> pointsOfRouteLength { get; set; }

    public GameObject dimmer;

    public GameObject train;

    public Material highlight;

    public new GameObject collider;
    public GameObject blocker;

    // placeholder for winning player
    PlayerScript winner;

    // Is used in the last round befor the game ends.
    public PlayerScript lastPlayerBeforeGameEnds { get; set; }

    bool playerConfirmsHandcardSelection;
    bool playerSelectingDestinationCards;
    bool playersSelectingStartDestinationCards;
    bool newTurnProcessing;
    bool firstCardDrawn;
    bool secondCardDrawn;
    bool recordingCases;
    int turnCounter = 0;
    bool logAIactions;
    bool openCardProcessing;
    bool gettingNewOpenCards;

    public RouteOption lastTargetRouteRecord { get; set; }
    public RouteScript selectedRoute { get; set; }

    string action;

    float turnDelayAI;

    System.Random random = new System.Random();

    public void Awake()
    {
        // Instantiate the train 
        //Instantiate(train, routes.transform.GetChild(4).GetChild(0).transform.position, Quaternion.identity);
        //Instantiate(the object, the position,  Quaternion.identity (the rotation);
        //Instantiate(trainCardOnHandPrefab, handcardsPanel.transform.position, Quaternion.identity, handcardsPanel.transform);
        //the same but with the last argument you specify the routeParent of the object - may be advantageous

        handdestinationcardsPanel.SetActive(true);

        // initialize trainCardDeck with 12 cards of each routeColor

        for (int i = 0; i < cardColors.Count; i++)
        {
            for (int j = 0; j < 12; j++)
                traincards.Add(new TrainCard(cardColors[i], spritesTraincards[i]));
        }
        // need 2 more Joker Cards (14 joker cards)
        traincards.Add(new TrainCard("Joker", spritesTraincards[8]));
        traincards.Add(new TrainCard("Joker", spritesTraincards[8]));

        // initialize 5 open cards
        gettingNewOpenCards = true;
        for (int i = 0; i < 5; i++)
            OpenTrainCardFromDeck(i);
        gettingNewOpenCards = false;

        spritesDestinationcards = Resources.LoadAll<Sprite>("DestinationcardsSprites");
        foreach (Sprite s in spritesDestinationcards)
            destinationcards.Add(new DestinationCard(s.name, s));  

        colorList = new Dictionary<string, Color>();
        colorList.Add("Blue", Color.blue);
        colorList.Add("Green", Color.green);
        colorList.Add("Yellow", Color.yellow);
        colorList.Add("Red", Color.red);
        colorList.Add("Purple", Color.magenta);
        colorList.Add("LightBlue", new Color(0f/255f, 140f/255f, 255f/255f));
        colorList.Add("LightGreen", new Color(120f/255f, 255f/255f, 120f/255f));
        colorList.Add("LightYellow", new Color(255f/255f, 255f/255f, 60f/255f));
        colorList.Add("LightRed", new Color(255f/255f, 120f/255f, 120f/255f));
        colorList.Add("LightPurple", new Color(255f/255f, 150f/255f, 255f/255f));

        // get playercount
        for (int i = 4; i >= PlayerPrefs.GetInt("playercount"); i--)
            players.RemoveAt(i);

        for (int i = 0; i < PlayerPrefs.GetInt("playercount"); i++)
        {
            // set player names and colors
            players[i].playerName = PlayerPrefs.GetString("playername" + i);
            players[i].playerColorName = PlayerPrefs.GetString("playercolor" + i);

            if (string.IsNullOrEmpty(players[i].playerColorName))
            {
                var freeColors = colorList.Keys.Where(color => !players.Any(player => player.playerColorName == color)).Where(color => !color.StartsWith("Light")).ToList();
                var randomColor = freeColors.ElementAt(random.Next(freeColors.Count));
                players[i].playerColorName = randomColor;
                players[i].playerColor = colorList[randomColor];
            } else
            {
                players[i].playerColor = colorList[players[i].playerColorName];
            }

            // set if player is AI or Human
            string name = "selectedAI" + i;
            switch (PlayerPrefs.GetString(name))
            {
                case "Human":
                    players[i].isHuman = true;
                    break;
                case "CBR AI":
                    players[i].isAI = true;
                    break;
                case "Rule AI #1":
                    players[i].isHardcodedAI1 = true;
                    break;
                case "Rule AI #2":
                    players[i].isHardcodedAI2 = true;
                    break;
            }
            //players[i].isAI = PlayerPrefs.GetInt(name) == 1 ? true : false;

            // set panel of player active
            playerScorePanel.transform.GetChild(i).gameObject.SetActive(true);
            // display name of player
            playerScorePanel.transform.GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>().text = players[i].playerName;
            // set panel with light routeColor
            playerScorePanel.transform.GetChild(i).GetComponent<Image>().color = colorList["Light" + players[i].playerColorName];
        }

        recordingCases = PlayerPrefs.GetInt("recordingCases") == 1 ? true : false;

        //generate map with citys and teir connections
        CreateCityMap();

        pointsOfRouteLength = new Dictionary<int, int>();
        pointsOfRouteLength.Add(1, 1);
        pointsOfRouteLength.Add(2, 2);
        pointsOfRouteLength.Add(3, 4);
        pointsOfRouteLength.Add(4, 7);
        pointsOfRouteLength.Add(5, 10);
        pointsOfRouteLength.Add(6, 15);
    }
     
    // Start is called before the first frame update
    void Start()
    {
        activePlayer = players[0];
        activePlayer.isActivePlayer = true;
        newTurnProcessing = false;


        System.Random random = new System.Random();
        // give every player 4 starting train cards
        foreach (PlayerScript p in players)
        {
            
            for (int i = 0; i < 4; i++)
            {
                // object for random number
                // random number between 0 (included) and card count of the train card deck (excl.) ((count = +1))
                int randomNumber = random.Next(0, traincards.Count);
                // get a card (not a gameobject) and save 
                TrainCard drawnCard = traincards[randomNumber];
                // remove the drawn card from the deck
                traincards.Remove(drawnCard);
                // drawn card to player's hand
                p.AddTrainCardToHand(drawnCard);

                //if (p.isAI)
                //{
                //    p.AddTrainCardToHand(new TrainCard("Joker", spritesTraincards[8]));
                //    p.AddTrainCardToHand(new TrainCard("Joker", spritesTraincards[8]));
                //    p.AddTrainCardToHand(new TrainCard("Joker", spritesTraincards[8]));
                //    p.AddTrainCardToHand(new TrainCard("Joker", spritesTraincards[8]));
                //}
            }
        }
        // display hand cards of starting player
        // iterate through players handcards and display each card
        foreach (TrainCard handcard in activePlayer.handtraincards)
        {
            // create and display this card
            GameObject card = Instantiate(trainCardOnHandPrefab, handcardsPanel.transform.position, Quaternion.identity, handcardsPanel.transform);
            // give the handcard the sprite from the train card 
            card.transform.GetChild(1).GetComponent<Image>().sprite = handcard.spriteImage;
            card.transform.GetComponent<TrainCardScript>().color = handcard.color;
        }

        if (players.Any(p => p.isAI) || recordingCases)
        {
            StartAIConnection();
        }

        // start process of each player drawing 3 destionation cards and selection at least 2
        // ------------- REMOVE FOLLOWING COMMENTS FOR WHOLE GAME EXPERIENCE --------------
        destinationcardsDrawDisplay.GetComponent<DestinationcardsDisplayScript>().startSelection = true;
        playersSelectingStartDestinationCards = true;
        StartCoroutine(DestinationCardsSelectionAtStartProcess());

        // time between moves of AI
        turnDelayAI = 3f;

        logAIactions = true;
    }

    private void StartAIConnection() 
    { 
        foo = new Process();
        if (START_STANDALONE_KI)
        {
            foo.StartInfo.FileName = Environment.CurrentDirectory + @"\Assets\CBRSystem.jar";
            Debug.Log(foo.StartInfo.FileName);
		    foo.StartInfo.Arguments = "--port=" + Constants.PORT + " --logout=Java-Console_Log.txt";
            foo.Start();
        }
        connection = new Connection();  
    }
 
    public string SendRequestToAI(Situation situation)
    {
        Request request = new Request();

        if (situation == null)
            situation = CreateSituation();

        // Anfrage, bestehend aus der Situation
        request = new Request(situation, "RETRIEVAL");
        return connection.SendRequest(request);
    }

    public void SendCaseToCasebase()
    {
        Case newCase = new Case(action, CreateRecordSituation());
        Request request = new Request(newCase, "NEWCASE");
        connection.SendRequest(request);
        Debug.Log("Case sent");
    }
    public Situation CreateRecordSituation()
    {
        // neue liste von einem this.selectedRoute
        List<RouteOption> routeOptions = new List<RouteOption>();
        routeOptions.Add(lastTargetRouteRecord);
        Situation situation = new Situation();
        situation = new Situation(
            activePlayer.handtraincards.Count,
            activePlayer.handdestinationcards.Count(x => !x.fulfilled),
            activePlayer.availableWagons,
            players.Min(player => player.availableWagons),
            CalculateAveragePointsOfRoutes(), routeOptions
        ) ;

        return situation;
    }

    public Situation CreateSituation()
    {
        //List<RouteOption> routeOptions = new List<RouteOption>();
        //routeOptions = ReturnRouteOptionList(activePlayer);

        Situation situation = new Situation();
        situation = new Situation(
            activePlayer.handtraincards.Count,
            activePlayer.handdestinationcards.Count(x => !x.fulfilled),
            activePlayer.availableWagons,
            players.Min(player => player.availableWagons),
            CalculateAveragePointsOfRoutes(), ReturnRouteOptionList(activePlayer)
        );

        return situation;
    }

    // Update is called once per frame
    void Update()
    {
        // amount of destination cards on the players hand
        destinationcardsCount.text = activePlayer.handdestinationcards.Count.ToString();

        if (traincardsDiscard.Count == 0)
        {
            traincardsDiscardDeck.transform.GetChild(0).gameObject.SetActive(false);
            traincardsDiscardDeck.transform.GetChild(1).gameObject.SetActive(false);
        }
        if (traincardsDiscard.Count > 0)
        {
            traincardsDiscardDeck.transform.GetChild(0).gameObject.SetActive(true);
            traincardsDiscardDeck.transform.GetChild(1).gameObject.SetActive(false);
        }
        if (traincardsDiscard.Count >= 30)
        {
            traincardsDiscardDeck.transform.GetChild(0).gameObject.SetActive(true);
            traincardsDiscardDeck.transform.GetChild(1).gameObject.SetActive(true);
        }

        if (traincards.Count == 0)
            traincardsDeck.gameObject.SetActive(false);
        else
            traincardsDeck.gameObject.SetActive(true);

        if (traincards.Count == 0 && traincardsDiscard.Count > 0)
        {
            foreach (TrainCard card in traincardsDiscard)
                traincards.Add(card);

            traincardsDiscard.Clear();
        }

        // values on screen 
        for (int i=0; i<players.Count; i++)
        {
            // game points of player
            playerScorePanel.transform.GetChild(i).GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = players[i].score.ToString();
            // available trains of player
            playerScorePanel.transform.GetChild(i).GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().text = players[i].availableWagons.ToString();

            // disable grey panel of active player
            if (players[i].isActivePlayer)
                playerScorePanel.transform.GetChild(i).GetChild(3).gameObject.SetActive(false);
            else
                playerScorePanel.transform.GetChild(i).GetChild(3).gameObject.SetActive(true);
        }

        turnCounterDisplay.transform.GetComponent<TextMeshProUGUI>().text = turnCounter.ToString();
    }

    IEnumerator DestinationCardsSelectionAtStartProcess()
    {
        foreach (PlayerScript p in players)
        {
            spritesDestinationcardsDisplay.SetActive(true);
            dimmer.SetActive(true);
            DisableGameObjects();
            DrawDestinationCardsFromDeck();

            if (!activePlayer.isHuman)
            {
                blocker.SetActive(true);
                yield return new WaitForSecondsRealtime(1);
            }
            else
            {
                blocker.SetActive(false);
            }

            if (activePlayer.isHardcodedAI1)
            {
                yield return new WaitForSeconds(1);
                List<int> selection = ElectDestinationcardsAtStartLowestPoints();
                StartCoroutine(AIselectsDestinationCards(selection));
                yield return new WaitForSeconds(1);
            }
            
            if (activePlayer.isHardcodedAI2 || activePlayer.isAI)
            {
                yield return new WaitForSeconds(1);
                List<int> selection = ElectDestinationcardsAtStartMostOverlap();
                StartCoroutine(AIselectsDestinationCards(selection));
                yield return new WaitForSeconds(1);
            }

            yield return WaitForPlayerDestinationCardSelection();

            // iterate through players and find activePlayer, to assign new active player
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].isActivePlayer)
                {
                    // if active player is at last position in the list
                    if (players[i].Equals(players[players.Count - 1]))
                    {
                        // then the active player is the player at first position in list
                        activePlayer = players[0];
                        activePlayer.isActivePlayer = true;
                    }
                    else
                    {
                        // else it is the player in the next position in list
                        activePlayer = players[i + 1];
                        activePlayer.isActivePlayer = true;
                    }
                    // set last player as not active player anymore
                    players[i].isActivePlayer = false;
                    // break the loop
                    break;
                }
            }
        }
        spritesDestinationcardsDisplay.SetActive(false);
        dimmer.SetActive(false);
        blocker.SetActive(false);
        EnableGameObjects();

        if (activePlayer.isAI)
        {
            string move = "";
            move = SendRequestToAI(null);

            StartCoroutine(ProcessPlan(move));
        }

        if (activePlayer.isHardcodedAI1)
        {
            StartCoroutine(StartRuleProcessAI1());
        }

        if (activePlayer.isHardcodedAI2)
        {
            StartCoroutine(StartRuleProcessAI2());
        }

        if (recordingCases && activePlayer.isHuman)
        {
            List<RouteOption> routeOptionList = ReturnRouteOptionList(activePlayer);
            // show edges of shortest paths
            foreach (RouteOption ro in routeOptionList)
            {
                GameObject routeParent = GameObject.Find(ro.nameOfRoute);
                for (int i = 0; i < routeParent.transform.childCount; i++)
                {
                    routeParent.transform.GetChild(i).GetChild(1).gameObject.SetActive(true);
                }
            }

            // let the user select the route option
            selectingRouteOptionText.SetActive(true);
            for (int i = 0; i < routes.transform.childCount; i++)
            {
                for (int c = 0; c < routes.transform.GetChild(i).transform.childCount; c++)
                {
                    routes.transform.GetChild(i).GetChild(c).GetComponent<RoutepartScript>().selectingRouteOption = true;
                }
            }
        }

        // enable gameobjects
        traincardsDeck.GetComponent<TrainCardDeckScript>().disabled = false;
        destinationcardsDeck.GetComponent<DestinationCardDeckScript>().disabled = false;
        // enable all face up cards
        for (int i = 0; i < faceupTraincardsParent.transform.childCount; i++)
            faceupTraincardsParent.transform.GetChild(i).GetChild(0).GetComponent<FaceUpTrainCardScript>().disabled = false;
        // enable all routes
        EnableAllRoutes();
        playersSelectingStartDestinationCards = false;
        destinationcardsDrawDisplay.GetComponent<DestinationcardsDisplayScript>().startSelection = false;
        turnCounter = 1;
        turnCounterDisplay.transform.parent.gameObject.SetActive(true);
    }

    IEnumerator WaitForPlayerDestinationCardSelection()
    {
        bool done = false;
        while (!done)
        {
            if (!playerSelectingDestinationCards)
                done = true;

            yield return null;
        }
    }

    private void DebugAi(string msg)
    {
        if (logAIactions)
        {
            Debug.Log(msg);
        }
    }
    IEnumerator ProcessPlan(string jsonAnswer)
    {
        //Debug.Log("Answer: " + answer);

        AnswerRoot answer = JsonUtility.FromJson<AnswerRoot>(jsonAnswer);
        SituationSolutionAnswer bestSolution = answer.routes.First();
        RouteOption targetRoute =  bestSolution.routeOptions.Transform();
        RouteOption alternativeRoute = new RouteOption();
        // bug, when answer.routes is 1 length
        if (answer.routes.Count() > 1)
            alternativeRoute = answer.routes[1].routeOptions.Transform();
        else
            alternativeRoute = targetRoute;
        

        yield return new WaitForSecondsRealtime(turnDelayAI);

        DebugAi("AI Action: " + bestSolution.plan);
        DebugAi("Target Route: " + targetRoute.nameOfRoute);

        yield return new WaitForSecondsRealtime(0.5f);
        switch (bestSolution.plan)
        {
            case "ClaimRoute":
                if (CheckHandCardsForClaimingOfRouteWithJoker(targetRoute.lengthOfRoute, targetRoute.colorOfRoute) && !firstCardDrawn)
                {
                    StartCoroutine(AIbuysRoute(targetRoute.nameOfRoute));
                }
                else
                {
                    StartCoroutine(ProcessUnfeasableAdaption(answer));
                }
                break;
            case "ClaimRoutePoints":
                StartCoroutine(StartRuleProcessAI1());
                break;
            case "DrawCardFromOpenCards":
                bool jokerCardDrawn = false;
                if (faceupTraincards.Any(card => card.color == targetRoute.colorOfRoute))
                {
                    DebugAi("Draw open Train Card: " + targetRoute.colorOfRoute);
                    openCardProcessing = true;
                    AIdrawsOpenCard(targetRoute.colorOfRoute);
                    yield return WaitUntilNewTrainCardIsOpen();
                }
                else
                {
                    int decision = random.Next(0, 100);

                    if (faceupTraincards.Any(card => card.color == "Joker") && !firstCardDrawn)
                    {
                        if (decision < 20) // 20% chance - Draw from deck
                        {
                            DebugAi("Draw Train Card from deck");
                            DrawTrainCardFromDeck();
                        }
                        else if (decision < 60) // 40% chance - Draw Joker Card
                        {
                            jokerCardDrawn = true;
                            DebugAi("Draw open Train Card: Joker");
                            AIdrawsOpenCard("Joker");
                        }
                        else // 40% chance - draw card for an other routeOption
                        {
                            if (faceupTraincards.Any(card => card.color == ExtractSubstring(alternativeRoute.nameOfRoute)))
                            {
                                string nameRoute = alternativeRoute.nameOfRoute;
                                string colorRoute = alternativeRoute.colorOfRoute;
                                DebugAi("Draw open Train Card: " + ExtractSubstring(alternativeRoute.nameOfRoute));
                                openCardProcessing = true;
                                AIdrawsOpenCard(alternativeRoute.colorOfRoute);
                                yield return WaitUntilNewTrainCardIsOpen();
                            }
                            else
                            {
                                DebugAi("Draw Train Card from deck");
                                DrawTrainCardFromDeck();
                            }
                        }
                    }
                    else
                    {
                        if (decision < 30) // 30% chance - Draw from deck
                        {
                            DebugAi("Draw Train Card from deck");
                            DrawTrainCardFromDeck();
                        }
                        else // 70% chance - Card for an other routeOption
                        {
                            if (faceupTraincards.Any(card => card.color == ExtractSubstring(alternativeRoute.nameOfRoute)))
                            {
                                string nameRoute = alternativeRoute.nameOfRoute;
                                string colorRoute = alternativeRoute.colorOfRoute;
                                DebugAi("Draw open Train Card: " + ExtractSubstring(alternativeRoute.nameOfRoute));
                                openCardProcessing = true;
                                AIdrawsOpenCard(alternativeRoute.colorOfRoute);
                                yield return WaitUntilNewTrainCardIsOpen();
                            }
                            else
                            {
                                DebugAi("Draw Train Card from deck");
                                DrawTrainCardFromDeck();
                            }
                        }
                    }
                }
                yield return new WaitForSecondsRealtime(0.2f);

                if (!jokerCardDrawn && !secondCardDrawn)
                    StartCoroutine(ProcessPlan(SendRequestToAI(null)));
                break;
            case "DrawCardFromDeck":
                DebugAi("Draw Train Card from deck");
                DrawTrainCardFromDeck();
                if (!secondCardDrawn)
                    StartCoroutine(ProcessPlan(SendRequestToAI(null)));
                break;
            case "DrawDestinationCards":
                destinationcardsDeck.GetComponent<DestinationCardDeckScript>().OnMouseUp();
                yield return new WaitForSecondsRealtime(1);
                int destinationCardPosition = ElectDestinationCardsInGame();
                StartCoroutine(AIselectsDestinationCard(destinationCardPosition));
                break;
        }   

        yield return new WaitForSecondsRealtime(0.1f);
    }

    private IEnumerator ProcessUnfeasableAdaption(AnswerRoot answer)
    {
        var request = new AdaptionRequest(AdaptionRequest.UNFEASABLE_TYPE, answer.routes);
        var adaptionAnswerJson = connection.SendRequest(request);
        var adaption = JsonUtility.FromJson<AnswerRoot>(adaptionAnswerJson);

        yield return StartCoroutine(ProcessPlan(adaptionAnswerJson));
    }

    private IEnumerator ProcessDrawTrainCards(String targetRouteName, String color)
    {
        // first card
        if (faceupTraincards.Any(card => card.color == color))
        {
            DebugAi("1) Draw open Train Card: " + color);
            AIdrawsOpenCard(targetRouteName);
        }
        else
        {
            int decision = random.Next(0, 100);

            if (faceupTraincards.Any(card => card.color == "Joker"))
            {
                if (decision < 20) // 20% chance - Draw from deck
                {
                    DebugAi("1) Draw Train Card from deck");
                    DrawTrainCardFromDeck();
                }
                else if (decision < 60) // 40% chance - Draw Joker Card
                {
                    DebugAi("1) Draw open Train Card: Joker");
                    AIdrawsOpenCard("Joker");
                }
                else // 40% chance - Card for an other routeOption
                {
                    if (faceupTraincards.Any(card => card.color == ExtractSubstring(targetRouteName)))
                    {
                        DebugAi("1) Draw open Train Card: " + ExtractSubstring(targetRouteName));
                        AIdrawsOpenCard(ExtractSubstring(targetRouteName));
                    }
                    else
                    {
                        DebugAi("1) Draw Train Card from deck");
                        DrawTrainCardFromDeck();
                    }
                }
            }
            else
            {
                DebugAi("1) Draw Train Card from deck");
                DrawTrainCardFromDeck();
            }
        }

        //yield return new WaitForSecondsRealtime(1);
        
        // second card
        if (faceupTraincards.Any(card => card.color == color))
        {
            DebugAi("2) Draw open Train Card: " + color);
            AIdrawsOpenCard(color);
        }
        else
        {
            int decision = random.Next(0, 100);

            if (faceupTraincards.Any(card => card.color == "Joker"))
            {
                if (decision < 20) // 20% chance - Draw from deck
                {
                    DebugAi("2) Draw Train Card from deck");
                    DrawTrainCardFromDeck();
                }
                else if (decision < 60) // 40% chance - Draw Joker Card
                {
                    DebugAi("2) Draw open Train Card: Joker");
                    AIdrawsOpenCard("Joker");
                }
                else // 40% chance - Card for an other routeOption
                {
                    if (faceupTraincards.Any(card => card.color == ExtractSubstring(targetRouteName)))
                    {
                         DebugAi("2) Draw open Train Card: " + ExtractSubstring(targetRouteName));
                        AIdrawsOpenCard(ExtractSubstring(targetRouteName));
                    }
                    else
                    {
                        DebugAi("2) Draw Train Card from deck");
                        DrawTrainCardFromDeck();
                    }
                }
            }
            else
            {
                DebugAi("1) Draw Train Card from deck");
                DrawTrainCardFromDeck();
            }
        }

        yield return new WaitForSecondsRealtime(0.1f);
    }

    public static string ExtractSubstring(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        int firstUnderscoreIndex = input.IndexOf('_');
        if (firstUnderscoreIndex == -1)
        {
            return string.Empty;
        }

        int secondUnderscoreIndex = input.IndexOf('_', firstUnderscoreIndex + 1);
        if (secondUnderscoreIndex == -1)
        {
            return string.Empty;
        }

        int thirdUnderscoreIndex = input.IndexOf('_', secondUnderscoreIndex + 1);
        string value;

        if (thirdUnderscoreIndex == -1)
        {
            // Falls kein dritter Unterstrich vorhanden ist, nehmen wir den Rest des Strings
            value = input.Substring(secondUnderscoreIndex + 1);
        } else
        {
            value = input.Substring(secondUnderscoreIndex + 1, thirdUnderscoreIndex - secondUnderscoreIndex - 1);
        }
        return char.ToUpper(value[0]) + value.Substring(1);
    }


    //method return true if the AI can draw a destination card
    public bool CheckDrawingOfDestinationcard()
    {
        //conditions that have to be fulfilled to draw a card
        bool sufficientNumberOfWagons = activePlayer.availableWagons >= 12;
        bool opponentsHaveEnoughWagons = players.Min(player => player.availableWagons) >= 10;
        bool allHandDestinationcardsAreFulfilled = CheckIfAllHandDestinationcardsAreFulfilledOrNotReachable();
        //bool enoughCardsOnHand = activePlayer.handtraincards.Count >= 7;
        return sufficientNumberOfWagons && opponentsHaveEnoughWagons && allHandDestinationcardsAreFulfilled;
    }

    //erreichbarkeit muss noch geprueft werden
    public bool CheckIfAllHandDestinationcardsAreFulfilledOrNotReachable()
    {
        int counter = 0;
        for (int d = 0; d < activePlayer.handdestinationcards.Count; d++)
        {
            bool citiesAreConnected = CheckCityConnection(activePlayer.handdestinationcards[d].citiesAndPoints[0], 
                activePlayer.handdestinationcards[d].citiesAndPoints[1], activePlayer);
            bool citiesConnectionIsPossible = CheckIfCityConnectionIsReachable(activePlayer.handdestinationcards[d].citiesAndPoints[0], 
                activePlayer.handdestinationcards[d].citiesAndPoints[1], activePlayer);

            if (!citiesConnectionIsPossible)
                Debug.Log("citiesConnectionIsPossible is not possible");

            if (citiesAreConnected && citiesConnectionIsPossible)
                counter++;
        }
        return counter == activePlayer.handdestinationcards.Count;
    }

    IEnumerator StartRuleProcessAI1()
    {
        // --- variables ---
        string[] openCards = new string[5];
        // Maximum number of cards to be collected
        int maxNumberCardsCollecting = 6;

        // handcards count of each routeColor on hand
        Dictionary<string, int> handcardsColorsCount = new Dictionary<string, int>();
        // amount of jokers on hand
        int jokerCount = 0;

        // available routes of length 6, 5, 4
        List<string> routes6 = new List<string>();
        List<string> routes5 = new List<string>();
        List<string> routes4 = new List<string>();
        List<string> routes3 = new List<string>();
        List<string> routes2 = new List<string>();
        List<string> routes1 = new List<string>();

        for (int i=0; i<routes.transform.childCount; i++)
        {
            RouteScript route = routes.transform.GetChild(i).GetComponent<RouteScript>();
            if (route.owner == null)
            {
                if (route.routeLength == 6)
                {
                    routes6.Add(route.routeColor);
                }
                else if (route.routeLength == 5)
                {
                    routes5.Add(route.routeColor);
                }
                else if (route.routeLength == 4)
                {
                    routes4.Add(route.routeColor);
                }
                else if (route.routeLength == 3)
                {
                    routes3.Add(route.routeColor);
                }
                else if (route.routeLength == 2)
                {
                    routes2.Add(route.routeColor);
                }
                else if (route.routeLength == 1)
                {
                    routes1.Add(route.routeColor);
                }
            }
        }
        if (routes6.Count == 0)
            maxNumberCardsCollecting = 5;
        if (routes5.Count == 0)
            maxNumberCardsCollecting = 4;
        if (routes4.Count == 0)
            maxNumberCardsCollecting = 3;

        if (activePlayer.availableWagons < 6)
            maxNumberCardsCollecting = activePlayer.availableWagons;

        if (activePlayer.availableWagons < 6)
            routes6.Clear();
        if (activePlayer.availableWagons < 5)
            routes5.Clear();
        if (activePlayer.availableWagons < 4)
            routes4.Clear();
        if (activePlayer.availableWagons < 3)
            routes3.Clear();
        if (activePlayer.availableWagons < 2)
            routes2.Clear();
        if (activePlayer.availableWagons < 1)
            routes1.Clear();

        bool firstCardDrawn = false;
        
        // --- rules ---
        for (int i = 0; i < 2; i++)
        {
            // open cards
            for (int o = 0; o < 5; o++)
                openCards[o] = faceupTraincards[o].color;

            handcardsColorsCount.Clear();

            foreach (string color in cardColors)
            {
                if (color != "Joker")
                    handcardsColorsCount.Add(color, 0);
            }

            foreach (TrainCard t in activePlayer.handtraincards)
            {
                if (t.color != "Joker")
                    handcardsColorsCount[t.color] = handcardsColorsCount[t.color] + 1;
                else
                    jokerCount++;
            }

            // Dictionary nach dem Wert absteigend sortieren
            var sortedDict = handcardsColorsCount.OrderByDescending(x => x.Value).ToList();

            // H�chsten und zweith�chsten Key ausw�hlen
            var firstColor = sortedDict[0].Key;
            var secondColor = sortedDict.Count > 1 ? sortedDict[1].Key : null;

            //Debug.Log("First: " + firstColor);
            //Debug.Log("Second: " + secondColor);

            //Debug.Log("vor wait: " + activePlayer.playerName + " - " + i);
            yield return new WaitForSecondsRealtime(2);
            //Debug.Log("nach wait: " + activePlayer.playerName + " - " + i);

            // -------------------- last round rule process --------------------
            if (lastPlayerBeforeGameEnds != null)
            {
                if (!firstCardDrawn && (routes6.Contains(firstColor) || routes6.Contains("Grey")) && handcardsColorsCount[firstColor] + 
                    jokerCount >= 6 && activePlayer.availableWagons >= 6)
                {
                    // strecke erwerben
                    for (int r = 0; r < routes.transform.childCount; r++)
                    {
                        RouteScript route = routes.transform.GetChild(r).GetComponent<RouteScript>();
                        if (route.owner == null)
                        {
                            if (route.routeLength == 6 && (route.routeColor == firstColor || route.routeColor == "Grey"))
                            {
                                string routeName = routes.transform.GetChild(r).name;
                                StartCoroutine(AIbuysRoute(routeName));
                            }
                        }
                    }
                    break;
                }
                // wenn Regel 1 und 2 nicht zutreffen, eventuelle 5er Strecke erschlie�en, wenn die 6er in der Farbe nicht vorhanden
                else if (!firstCardDrawn && (routes5.Contains(firstColor) || routes5.Contains("Grey")) && handcardsColorsCount[firstColor] + 
                    jokerCount >= 5 && activePlayer.availableWagons >= 5)
                {
                    // strecke erwerben
                    for (int r = 0; r < routes.transform.childCount; r++)
                    {
                        RouteScript route = routes.transform.GetChild(r).GetComponent<RouteScript>();
                        if (route.owner == null)
                        {
                            if (route.routeLength == 5 && (route.routeColor == firstColor || route.routeColor == "Grey"))
                            {
                                string routeName = routes.transform.GetChild(r).name;
                                StartCoroutine(AIbuysRoute(routeName));
                            }
                        }
                    }
                    break;
                }
                // 
                else if (!firstCardDrawn && (routes4.Contains(firstColor) || routes4.Contains("Grey")) && handcardsColorsCount[firstColor] + 
                    jokerCount >= 4 && activePlayer.availableWagons >= 4)
                {
                    // strecke erwerben
                    for (int r = 0; r < routes.transform.childCount; r++)
                    {
                        RouteScript route = routes.transform.GetChild(r).GetComponent<RouteScript>();
                        if (route.owner == null)
                        {
                            if (route.routeLength == 4 && (route.routeColor == firstColor || route.routeColor == "Grey"))
                            {
                                string routeName = routes.transform.GetChild(r).name;
                                StartCoroutine(AIbuysRoute(routeName));
                            }
                        }
                    }
                    break;
                }
                else if (!firstCardDrawn && (routes3.Contains(firstColor) || routes3.Contains("Grey")) && handcardsColorsCount[firstColor] + 
                    jokerCount >= 3 && activePlayer.availableWagons >= 3)
                {
                    // strecke erwerben
                    for (int r = 0; r < routes.transform.childCount; r++)
                    {
                        RouteScript route = routes.transform.GetChild(r).GetComponent<RouteScript>();
                        if (route.owner == null)
                        {
                            if (route.routeLength == 3 && (route.routeColor == firstColor || route.routeColor == "Grey"))
                            {
                                string routeName = routes.transform.GetChild(r).name;
                                StartCoroutine(AIbuysRoute(routeName));
                            }
                        }
                    }
                    break;
                }
                else if (!firstCardDrawn && (routes2.Contains(firstColor) || routes2.Contains("Grey")) && handcardsColorsCount[firstColor] + 
                    jokerCount >= 2 && activePlayer.availableWagons >= 2)
                {
                    // strecke erwerben
                    for (int r = 0; r < routes.transform.childCount; r++)
                    {
                        RouteScript route = routes.transform.GetChild(r).GetComponent<RouteScript>();
                        if (route.owner == null)
                        {
                            if (route.routeLength == 2 && (route.routeColor == firstColor || route.routeColor == "Grey"))
                            {
                                string routeName = routes.transform.GetChild(r).name;
                                StartCoroutine(AIbuysRoute(routeName));
                            }
                        }
                    }
                    break;
                }
                else if (!firstCardDrawn && (routes1.Contains(firstColor) || routes1.Contains("Grey")) && handcardsColorsCount[firstColor] + 
                    jokerCount >= 1 && activePlayer.availableWagons >= 1)
                {
                    // strecke erwerben
                    for (int r = 0; r < routes.transform.childCount; r++)
                    {
                        RouteScript route = routes.transform.GetChild(r).GetComponent<RouteScript>();
                        if (route.owner == null)
                        {
                            if (route.routeLength == 1 && (route.routeColor == firstColor || route.routeColor == "Grey"))
                            {
                                string routeName = routes.transform.GetChild(r).name;
                                StartCoroutine(AIbuysRoute(routeName));
                            }
                        }
                    }
                    break;
                }
            }

            // -------------------- normal rule process --------------------

            // 1. Regel: h�ufigste Farbe nehmen
            if (openCards.Contains(firstColor) && handcardsColorsCount[firstColor] < maxNumberCardsCollecting && lastPlayerBeforeGameEnds == null)
            {
                AIdrawsOpenCard(firstColor);
                firstCardDrawn = true;
            }
            // 2. Regel: zweith�ufigste Farbe nehmen
            else if (openCards.Contains(secondColor) && handcardsColorsCount[secondColor] < maxNumberCardsCollecting && lastPlayerBeforeGameEnds == null)
            {
                // wenn diese Farbe nur 1 mal auf Hand und Joker vorhanden, dann Joker nehmen
                if (handcardsColorsCount[secondColor] == 1 && openCards.Contains("Joker") && !firstCardDrawn)
                {
                    AIdrawsOpenCard("Joker");
                    break;
                }
                // sonst zweith�ufigste Farbe nehmen
                else
                {
                    AIdrawsOpenCard(secondColor);
                    firstCardDrawn = true;
                }
            } 
            // wenn Regel 1 und 2 nicht zutreffen, eventuelle 6er Strecke erschlie�en
            else if (!firstCardDrawn && ( routes6.Contains(firstColor) || routes6.Contains("Grey") ) && handcardsColorsCount[firstColor]+jokerCount >= 6 && 
                activePlayer.availableWagons >= 6)
            {
                // strecke erwerben
                for (int r = 0; r < routes.transform.childCount; r++)
                {
                    RouteScript route = routes.transform.GetChild(r).GetComponent<RouteScript>();
                    if (route.owner == null)
                    {
                        if (route.routeLength == 6 && ( route.routeColor == firstColor || route.routeColor == "Grey" ))
                        {
                            string routeName = routes.transform.GetChild(r).name;
                            StartCoroutine(AIbuysRoute(routeName));
                        }
                    }
                }
                break;
            }
            // wenn Regel 1 und 2 nicht zutreffen, eventuelle 5er Strecke erschlie�en, wenn die 6er in der Farbe nicht vorhanden
            else if (!firstCardDrawn && (routes5.Contains(firstColor) || routes5.Contains("Grey")) && !routes6.Contains(firstColor) 
                    && handcardsColorsCount[firstColor] + jokerCount >= 5 && activePlayer.availableWagons >= 5)
            {
                // strecke erwerben
                for (int r = 0; r < routes.transform.childCount; r++)
                {
                    RouteScript route = routes.transform.GetChild(r).GetComponent<RouteScript>();
                    if (route.owner == null)
                    {
                        if (route.routeLength == 5 && (route.routeColor == firstColor || route.routeColor == "Grey"))
                        {
                            string routeName = routes.transform.GetChild(r).name;
                            StartCoroutine(AIbuysRoute(routeName));
                        }
                    }
                }
                break;
            }
            // 
            else if (!firstCardDrawn && (routes4.Contains(firstColor) || routes4.Contains("Grey")) && !routes6.Contains(firstColor) && 
                !routes5.Contains(firstColor) 
                && handcardsColorsCount[firstColor] + jokerCount >= 4 && activePlayer.availableWagons >= 4)
            {
                // strecke erwerben
                for (int r = 0; r < routes.transform.childCount; r++)
                {
                    RouteScript route = routes.transform.GetChild(r).GetComponent<RouteScript>();
                    if (route.owner == null)
                    {
                        if (route.routeLength == 4 && ( route.routeColor == firstColor || route.routeColor == "Grey"))
                        {
                            string routeName = routes.transform.GetChild(r).name;
                            StartCoroutine(AIbuysRoute(routeName));
                        }
                    }
                }
                break;
            }
            else if (!firstCardDrawn && (routes3.Contains(firstColor) || routes3.Contains("Grey")) && !routes6.Contains(firstColor) && !routes5.Contains(firstColor) 
                && !routes4.Contains(firstColor) && handcardsColorsCount[firstColor] + jokerCount >= 3 && activePlayer.availableWagons >= 3)
            {
                // strecke erwerben
                for (int r = 0; r < routes.transform.childCount; r++)
                {
                    RouteScript route = routes.transform.GetChild(r).GetComponent<RouteScript>();
                    if (route.owner == null)
                    {
                        if (route.routeLength == 3 && ( route.routeColor == firstColor || route.routeColor == "Grey"))
                        {
                            string routeName = routes.transform.GetChild(r).name;
                            StartCoroutine(AIbuysRoute(routeName));
                        }
                    }
                }
                break;
            }
            else if (!firstCardDrawn && (routes2.Contains(firstColor) || routes2.Contains("Grey")) && !routes6.Contains(firstColor) 
                    && !routes5.Contains(firstColor) && !routes4.Contains(firstColor) && !routes3.Contains(firstColor) && 
                    handcardsColorsCount[firstColor] + jokerCount >= 2 && activePlayer.availableWagons >= 2)
            {
                // strecke erwerben
                for (int r = 0; r < routes.transform.childCount; r++)
                {
                    RouteScript route = routes.transform.GetChild(r).GetComponent<RouteScript>();
                    if (route.owner == null)
                    {
                        if (route.routeLength == 2 && ( route.routeColor == firstColor || route.routeColor == "Grey" ))
                        {
                            string routeName = routes.transform.GetChild(r).name;
                            StartCoroutine(AIbuysRoute(routeName));
                        }
                    }
                }
                break;
            }
            else if (!firstCardDrawn && (routes1.Contains(firstColor) || routes1.Contains("Grey")) && !routes6.Contains(firstColor) && !routes5.Contains(firstColor)
                    && !routes4.Contains(firstColor) && !routes3.Contains(firstColor) && !routes2.Contains(firstColor) 
                    && handcardsColorsCount[firstColor] + jokerCount >= 1 && activePlayer.availableWagons >= 1)
            {
                // strecke erwerben
                for (int r = 0; r < routes.transform.childCount; r++)
                {
                    RouteScript route = routes.transform.GetChild(r).GetComponent<RouteScript>();
                    if (route.owner == null)
                    {
                        if (route.routeLength == 1 && ( route.routeColor == firstColor || route.routeColor == "Grey"))
                        {
                            string routeName = routes.transform.GetChild(r).name;
                            StartCoroutine(AIbuysRoute(routeName));
                        }
                    }
                }
                break;
            }
            // 3. Regel: nehme Joker, falls vorhanden
            else if (openCards.Contains("Joker") && !firstCardDrawn && lastPlayerBeforeGameEnds == null)
            {
                AIdrawsOpenCard("Joker");
                break;
            }
            // 4. Regel: ziehe vom Deck
            else
            {
                DrawTrainCardFromDeck();
                firstCardDrawn = true;
            }
        }
    }
 
    IEnumerator StartRuleProcessAI2()
    {
        // if all destination cards are done
        if (CheckIfAllHandDestinationcardsAreFulfilledOrNotReachable())
        {
            // check if a new dest card should be drawed
            if (CheckDrawingOfDestinationcard())
            {
                Debug.Log("Ziehe neue Destination Cards!");
                destinationcardsDeck.GetComponent<DestinationCardDeckScript>().OnMouseUp();
                yield return new WaitForSecondsRealtime(1);
                int destinationCardPosition = ElectDestinationCardsInGame();
                StartCoroutine(AIselectsDestinationCard(destinationCardPosition));
            }
            // else use the rule process of AI1
            else
            {
                // da keine RouteOptions mehr vorgegeben werden, hier versuchen die l�ngsten Routen aufzukaufen?
                StartCoroutine(StartRuleProcessAI1());
            }
        }
        // if not all destination cards are done and are still reachable, use normale rule process
        else
        {
            // --- variables ---
            string[] openCards = new string[5];
            // handcards count of each routeColor on hand
            Dictionary<string, int> handcardsColorsCount = new Dictionary<string, int>();
            // amount of jokers on hand
            int jokerCount = 0;

            // --- rules ---
            for (int i = 0; i < 2; i++)
            {
                // open cards
                for (int o = 0; o < 5; o++)
                    openCards[o] = faceupTraincards[o].color;

                handcardsColorsCount.Clear();

                foreach (string color in cardColors)
                {
                    if (color != "Joker")
                        handcardsColorsCount.Add(color, 0);
                }

                foreach (TrainCard t in activePlayer.handtraincards)
                {
                    if (t.color != "Joker")
                        handcardsColorsCount[t.color] = handcardsColorsCount[t.color] + 1;
                    else
                        jokerCount++;
                }

                // Dictionary nach dem Wert absteigend sortieren
                var sortedDict = handcardsColorsCount.OrderByDescending(x => x.Value).ToList();
                // H�chsten Key ausw�hlen
                var firstColor = sortedDict[0].Key;

                List<RouteOption> routeOptionList = ReturnRouteOptionList(activePlayer);
                // routes for which the fewest number of cards is still required
                List<RouteOption> closestRouteOptionsList = new List<RouteOption>();
                var obj = new RouteOption();
                obj.lengthOfRoute = 0;
                obj.trainCards = 0;
                if (routeOptionList.Count != 0)
                    obj = routeOptionList.OrderByDescending(x => x.lengthOfRoute - x.trainCards).LastOrDefault();

                // get the fewest number of cards
                int minValueOfNeededCards = obj.lengthOfRoute - obj.trainCards;
                if (minValueOfNeededCards < 0)
                    minValueOfNeededCards = 0;

                //Debug.Log("obj.lengthOfRoute - obj.trainCards: " + obj.lengthOfRoute + " - " + obj.trainCards);

                // add each route which fulfills this number
                foreach (RouteOption r in routeOptionList)
                {
                    if (r.lengthOfRoute - r.trainCards <= minValueOfNeededCards)
                        closestRouteOptionsList.Add(r);
                }

                bool enoughWagons = false;
                foreach (RouteOption r in closestRouteOptionsList)
                {
                    if (activePlayer.availableWagons >= r.lengthOfRoute)
                    {
                        enoughWagons = true;
                        break;
                    }
                }

                bool matchingOpenTrainCardAvailabe = false;
                foreach (RouteOption r in closestRouteOptionsList)
                {
                    foreach (string c in openCards)
                    {
                        if (r.colorOfRoute == c)
                            matchingOpenTrainCardAvailabe = true;
                        if (r.colorOfRoute == "Grey")
                        {
                            if (r.colorOfRoute == firstColor)
                                matchingOpenTrainCardAvailabe = true;
                        }
                    }
                }

                yield return new WaitForSecondsRealtime(1);

                // if a route can be claimed, claim it
                if (!firstCardDrawn && enoughWagons && minValueOfNeededCards == 0 && closestRouteOptionsList.Count != 0) //&& activePlayer.availableWagons >= 6)
                {
                    string routeName = "";
                    // iterate through the routeoptions
                    for (int ro = 0; ro < closestRouteOptionsList.Count; ro++)
                    {
                        // search for the route in unity
                        for (int r = 0; r < routes.transform.childCount; r++)
                        {
                            RouteScript route = routes.transform.GetChild(r).GetComponent<RouteScript>();
                            if (route.owner == null && activePlayer.availableWagons >= routes.transform.GetChild(r).childCount)
                                // check if name is equal
                                if (closestRouteOptionsList[ro].nameOfRoute == routes.transform.GetChild(r).name)
                                {
                                    routeName = routes.transform.GetChild(r).name;
                                    StartCoroutine(AIbuysRoute(routeName));
                                    break;
                                }
                        }
                        if (routeName.Length > 2)
                            break;
                    }
                    break;
                }
                // else look at the open cards
                else if (matchingOpenTrainCardAvailabe)
                {
                    bool breakLoop = false;
                    foreach (RouteOption r in closestRouteOptionsList)
                    {
                        if (r.colorOfRoute == "Grey")
                        {
                            foreach (string c in openCards)
                            {
                                if (r.colorOfRoute == firstColor)
                                {
                                    AIdrawsOpenCard(firstColor);
                                    firstCardDrawn = true;
                                    breakLoop = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            foreach (string c in openCards)
                            {
                                if (r.colorOfRoute == c)
                                {
                                    AIdrawsOpenCard(c);
                                    firstCardDrawn = true;
                                    breakLoop = true;
                                    break;
                                }
                            }
                        }
                        // if breakLoop is true, the outer loop has to be leaved
                        if (breakLoop)
                            break;
                    }
                }
                // else draw a card from deck
                else
                {
                    DrawTrainCardFromDeck();
                    firstCardDrawn = true;
                }
            }
        }
    }

    IEnumerator AIselectsDestinationCard(int position)
    {
        yield return new WaitForSecondsRealtime(1);
        destinationcardsDrawDisplay.transform.GetChild(position).GetComponent<CardSelectScript>().OnClickDestinationCard();
        yield return new WaitForSecondsRealtime(1);
        OnClickConfirmDestinationCards();
    }
    
    IEnumerator AIselectsDestinationCards(List<int> positions)
    {
        yield return new WaitForSecondsRealtime(1);
        foreach (int position in positions)
            destinationcardsDrawDisplay.transform.GetChild(position).GetComponent<CardSelectScript>().OnClickDestinationCard();
        yield return new WaitForSecondsRealtime(1);
        OnClickConfirmDestinationCards();
    }

    IEnumerator AIbuysRoute(string routeName)
    {
        //Debug.Log("Kaufe Route: " + routeName);
        GameObject route = GameObject.Find(routeName);
        route.transform.GetChild(0).GetComponent<RoutepartScript>().OnMouseOver();
        yield return new WaitForSecondsRealtime(1);
        route.transform.GetChild(0).GetComponent<RoutepartScript>().OnMouseDown();
        string routeColor = route.GetComponent<RouteScript>().routeColor;
        int counter = 0;

        int jokerCount = 0;
        // handcards count of each routeColor on hand
        Dictionary<string, int> handcardsColorsCount = new Dictionary<string, int>();
        foreach (string color in cardColors)
        {
            if (color != "Joker")
                handcardsColorsCount.Add(color, 0);
        }

        foreach (TrainCard t in activePlayer.handtraincards)
        {
            if (t.color != "Joker")
                handcardsColorsCount[t.color] = handcardsColorsCount[t.color] + 1;
            else
                jokerCount++;
        }

        string selectedColor = routeColor;
        if (routeColor == "Grey")
        {
            // Filter out colors that do not have enough cards for the route length
            var sufficientColors = handcardsColorsCount.Where(x => x.Value + jokerCount >= route.GetComponent<RouteScript>().routeLength)
                                                       .OrderBy(x => x.Value)
                                                       .ToList();

            // Choose the color with the minimum count that is sufficient
            selectedColor = sufficientColors.Any() ? sufficientColors.First().Key : null;

            // Use the selected color or fallback to the most frequent color if no sufficient color is found
            if (selectedColor == null)
            {
                var sortedDict = handcardsColorsCount.OrderByDescending(x => x.Value).ToList();
                selectedColor = sortedDict.First().Key;
            }
        }

        // search for cards on players hand with needed routeColor
        int routeLength = route.GetComponent<RouteScript>().routeLength;
        for (int i = 0; i < handcardsPanel.transform.childCount; i++)
        {
            if (handcardsPanel.transform.GetChild(i).GetComponent<TrainCardScript>().color == selectedColor)
            {
                handcardsPanel.transform.GetChild(i).GetComponent<CardSelectScript>().OnClick();
                counter++;
            }
            // if enough cards selected, leave loop
            if (counter == routeLength)
                break;
        }

        // if not enough cards were selected, joker cards are needed
        if (counter < routeLength)
        {
            for (int i = 0; i < handcardsPanel.transform.childCount; i++)
            {
                if (handcardsPanel.transform.GetChild(i).GetComponent<TrainCardScript>().color == "Joker")
                {
                    handcardsPanel.transform.GetChild(i).GetComponent<CardSelectScript>().OnClick();
                    counter++;
                }
                if (counter == routeLength)
                    break;
            }
        }

        yield return new WaitForSecondsRealtime(1);

        if (counter == routeLength)
        {
            OnClickConfirmCardsBtn();
        }
    }



    public void AIdrawsOpenCard(string color)
    {
        for (int i = 0; i < faceupTraincardsParent.transform.childCount; i++)
        {
            string c = faceupTraincardsParent.transform.GetChild(i).GetChild(0).GetComponent<TrainCardScript>().color;
            if (faceupTraincardsParent.transform.GetChild(i).GetChild(0).GetComponent<TrainCardScript>().color == color)
            {
                DrawTrainCardFromOpenCards(faceupTraincardsParent.transform.GetChild(i).GetChild(0).GetComponent<FaceUpTrainCardScript>());
                break;
            }          
        }
    }

    /*
     * This method is called, if a player has to do a new turn. The method ensures that only legal actions are done.
     */
    IEnumerator NewTurn()
    {
        if (!newTurnProcessing)
        {
            yield return new WaitForSecondsRealtime(0.5F);
            newTurnProcessing = true;

            // check if any destinationCard of active player is fulfilled
            for (int d = 0; d < activePlayer.handdestinationcards.Count; d++)
            {
                if (CheckCityConnection(activePlayer.handdestinationcards[d].citiesAndPoints[0],
                    activePlayer.handdestinationcards[d].citiesAndPoints[1], activePlayer))
                    activePlayer.handdestinationcards[d].fulfilled = true;
            }

            // reset action
            action = "";

            // reset values
            firstCardDrawn = false;
            secondCardDrawn = false;
            // enable gameobjects
            traincardsDeck.GetComponent<TrainCardDeckScript>().disabled = false;
            destinationcardsDeck.GetComponent<DestinationCardDeckScript>().disabled = false;
            // enable all face up cards
            for (int i = 0; i < faceupTraincardsParent.transform.childCount; i++)
                faceupTraincardsParent.transform.GetChild(i).GetChild(0).GetComponent<FaceUpTrainCardScript>().disabled = false;
            // enable all routes
            EnableAllRoutes();

            // change active player
            for (int i=0; i<players.Count; i++)
            {
                if (players[i].isActivePlayer)
                {
                    // if active player is at last position in the list
                    if (players[i].Equals(players[players.Count - 1]))
                    {
                        // then the active player is the player at first position in list
                        activePlayer = players[0];
                        activePlayer.isActivePlayer = true;
                    } else
                    {
                        // else it is the player in the next position in list
                        activePlayer = players[i + 1];
                        activePlayer.isActivePlayer = true;
                    }
                    // set last player as not active player anymore
                    players[i].isActivePlayer = false;
                    // break the loop
                    break;
                }
            }

            // increment turncounter
            if (activePlayer == players[0])
                turnCounter++;


            //Debug.Log("Hier folgen die Routen der k�rzesten Wege: ");
            List<RouteOption> routeOptionList =  ReturnRouteOptionList(activePlayer);
            for (int i=0; i < routes.transform.childCount; i++)
            {
                for (int c=0; c<routes.transform.GetChild(i).transform.childCount; c++)
                {
                    routes.transform.GetChild(i).GetChild(c).GetChild(1).gameObject.SetActive(false);
                }
            }
            if (recordingCases && activePlayer.isHuman)
            {
                // show edges of shortest paths
                foreach (RouteOption ro in routeOptionList)
                {
                    GameObject routeParent = GameObject.Find(ro.nameOfRoute);
                    for (int i = 0; i < routeParent.transform.childCount; i++)
                    {
                        routeParent.transform.GetChild(i).GetChild(1).gameObject.SetActive(true);
                    }
                }

                // let the user select the route option
                selectingRouteOptionText.SetActive(true);
                for (int i = 0; i < routes.transform.childCount; i++)
                {
                    for (int c = 0; c < routes.transform.GetChild(i).transform.childCount; c++)
                    {
                        routes.transform.GetChild(i).GetChild(c).GetComponent<RoutepartScript>().selectingRouteOption = true;
                    }
                }
            }

            // disable double routes for active player
            DisableDoubleRoutes();

            // update handcards of active player
            for (int i = handcardsPanel.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(handcardsPanel.transform.GetChild(i).gameObject);
            }
            // iterate through players handcards and display each card
            foreach (TrainCard handcard in activePlayer.handtraincards)
            {
                // create and display this card
                GameObject card = Instantiate(trainCardOnHandPrefab, handcardsPanel.transform.position, Quaternion.identity, handcardsPanel.transform);
                // give the handcard the sprite from the train card 
                card.transform.GetChild(1).GetComponent<Image>().sprite = handcard.spriteImage;
                card.transform.GetComponent<TrainCardScript>().color = handcard.color;
            }         

            if (!activePlayer.isHuman)
            {
                collider.SetActive(true);
                yield return new WaitForSecondsRealtime(0.1F);
            }
            else
            {
                collider.SetActive(false);
            }

            if (activePlayer.isAI)
            {
                string move = "";
                move = SendRequestToAI(null);
                StartCoroutine(ProcessPlan(move));
            }

            if (activePlayer.isHardcodedAI1)
            {
                yield return new WaitForSecondsRealtime(0.5F);
                StartCoroutine(StartRuleProcessAI1());
                yield return new WaitForSecondsRealtime(0.5F);
            }
            
            if (activePlayer.isHardcodedAI2)
            {
                yield return new WaitForSecondsRealtime(0.5F);
                StartCoroutine(StartRuleProcessAI2());
                yield return new WaitForSecondsRealtime(0.5F);
            }

            newTurnProcessing = false;
        }
        yield return new WaitForSeconds(0.5F);
    }

    public void SetCurrentRouteOption(RouteScript route)
    {
        RouteOption cor = new RouteOption();
        int faceUpCards = faceupTraincards.Count(card => card.color == route.routeColor);
        faceUpCards += faceupTraincards.Count(card => card.color == "Joker");
        int trainCards = activePlayer.handtraincards.Count(card => card.color == route.routeColor);
        trainCards += activePlayer.handtraincards.Count(card => card.color == "Joker");
        cor.lengthOfRoute = route.transform.childCount;
        cor.nameOfRoute = route.name;
        cor.faceUpCards = faceUpCards;
        cor.trainCards = trainCards;

        if (route.routeColor == "Grey")
        {
            if (activePlayer.handtraincards.Count != 0)
            {
                // Gruppiere die Karten nach Farbe und zaehle die Anzahl jeder Farbe
                var colorCounts = activePlayer.handtraincards
                    .GroupBy(card => card.color)
                     .Select(group => new { color = group.Key, Count = group.Count() + 
                        (group.Key == "Joker" ? 0 : activePlayer.handtraincards.Count(card => card.color == "Joker"))})
                    .OrderBy(item => item.Count);

                // Finde die kleinste Gruppe, die ausreicht, um die Laenge der Route zu erfüllen
                foreach (var group in colorCounts)
                {
                    if (group.Count >= cor.lengthOfRoute)
                    {
                        trainCards = group.Count;
                        break;
                    }
                }

                // Berücksichtige die Joker in faceUpCards nur einmal
                faceUpCards = faceupTraincards.Count(card => card.color == "Joker");

                // Finde die Farbe mit der kleinsten ausreichenden Gruppe in faceUpCards
                foreach (var group in colorCounts)
                {
                    int count = faceupTraincards.Count(card => card.color == group.color);
                    if (count + faceUpCards >= cor.lengthOfRoute)
                    {
                        faceUpCards += count;
                        break;
                    }
                }
            }
            cor.faceUpCards = faceUpCards;
            cor.trainCards = trainCards;
        }

        Dictionary<string, int> edgesOfAllPaths = ReturnEdgesOfPaths(activePlayer);
        foreach (KeyValuePair<string, int> kvp in edgesOfAllPaths)
        {
            if (kvp.Key == route.name)
            {
                cor.destinationCardPoints = kvp.Value;
                break;
            }
        }

        selectingRouteOptionText.SetActive(false);
        for (int i = 0; i < routes.transform.childCount; i++)
        {
            for (int c = 0; c < routes.transform.GetChild(i).transform.childCount; c++)
            {
                routes.transform.GetChild(i).GetChild(c).GetComponent<RoutepartScript>().selectingRouteOption = false;
            }
        }

        lastTargetRouteRecord = cor;
    }

    IEnumerator Wait(int time)
    {
        yield return new WaitForSeconds(time);
    }


    /*
     * Opens a train card from deck and adds it to the 5 open cards
     */
    public void OpenTrainCardFromDeck(int index)
    {
        // random card
        int randomNumber = random.Next(0, traincards.Count);
        faceupTraincards[index] = traincards[randomNumber];
        // remove the card from the deck
        traincards.Remove(faceupTraincards[index]);
        // instantiate a card on the map
        Instantiate(trainCardPrefab, faceupTraincardsParent.transform
            .GetChild(index).transform.position, faceupTraincardsParent.transform
            .GetChild(index).transform.rotation, faceupTraincardsParent.transform.GetChild(index).transform);

        // the new card as a Transform variable
        Transform cardTransform = faceupTraincardsParent.transform.GetChild(index).transform.GetChild(0);
        // set sprite of the card (GameObject)
        cardTransform.GetComponent<SpriteRenderer>().sprite = faceupTraincards[index].spriteImage;
        // set routeColor of the card (GameObject)
        cardTransform.GetComponent<TrainCardScript>().color = faceupTraincards[index].color;
        // add tag "ToBlock"
        cardTransform.tag = "ToBlock";
        // this is somehow needed. The gameobject can only be destroyed, if the new card was created
        if (faceupTraincardsParent.transform.GetChild(index).transform.childCount > 1)
            Destroy(faceupTraincardsParent.transform.GetChild(index).GetChild(1).transform.gameObject);

        if (!gettingNewOpenCards)
            StartCoroutine(CheckFaceUpCardsForThreeJokerCards());

        openCardProcessing = false;
    }

    /*
     * draws a train card from the deck and adds it to the players hand
     */
    public void DrawTrainCardFromDeck()
    {
        // object for random number
        System.Random random = new System.Random();
        // random number between 0 (included) and card count of the train card deck (excl.) ((count = +1))
        int randomNumber = random.Next(0, traincards.Count);
        // get a card (not a gameobject) and save 
        TrainCard drawnCard = traincards[randomNumber];
        // remove the drawn card from the deck
        traincards.Remove(drawnCard);
        // drawn card to player's hand
        activePlayer.AddTrainCardToHand(drawnCard);
        // create and display this card
        GameObject card = Instantiate(trainCardOnHandPrefab, handcardsPanel.transform.position, Quaternion.identity, handcardsPanel.transform);
        // give the handcard the sprite from the drawn card
        card.transform.GetChild(1).GetComponent<Image>().sprite = drawnCard.spriteImage;
        card.transform.GetComponent<TrainCardScript>().color = drawnCard.color;

        action = "DrawCardFromDeck";
        if (recordingCases && activePlayer.isHuman)
            SendCaseToCasebase();

        if (firstCardDrawn)
            secondCardDrawn = true;

        // if it was the second drawn card, the players turn is over
        if (firstCardDrawn)
        {
            if (!CheckEndingConditionOfGame())
            {
                // if player has claimed the route, his turn is over
                //Debug.Log("Rufe new Turn auf - DrawTrainCardFromDeck");
                StartCoroutine(NewTurn());
            } else 
            {
                CalculateTotalScore(); 
            }
            // disable gameobjects
            traincardsDeck.GetComponent<TrainCardDeckScript>().disabled = true;
            destinationcardsDeck.GetComponent<DestinationCardDeckScript>().disabled = true;
            // disable all face up cards
            for (int i = 0; i < faceupTraincardsParent.transform.childCount; i++)
                faceupTraincardsParent.transform.GetChild(i).GetChild(0).GetComponent<FaceUpTrainCardScript>().disabled = true;
            // disable all routes
            DisableAllRoutes();
        }
        else
        {
            firstCardDrawn = true;
            // disable all face up joker cards
            for (int i = 0; i < faceupTraincardsParent.transform.childCount; i++)
            {
                if (faceupTraincardsParent.transform.GetChild(i).GetChild(0).GetComponent<TrainCardScript>().color.Equals("Joker"))
                    faceupTraincardsParent.transform.GetChild(i).GetChild(0).GetComponent<FaceUpTrainCardScript>().disabled = true;
            }
            // disable gameobjects
            destinationcardsDeck.GetComponent<DestinationCardDeckScript>().disabled = true;
            // disable all routes
            DisableAllRoutes();

            if (recordingCases && activePlayer.isHuman)
            {
                List<RouteOption> routeOptionList = ReturnRouteOptionList(activePlayer);
                // show edges of shortest paths
                foreach (RouteOption ro in routeOptionList)
                {
                    GameObject routeParent = GameObject.Find(ro.nameOfRoute);
                    for (int i = 0; i < routeParent.transform.childCount; i++)
                    {
                        routeParent.transform.GetChild(i).GetChild(1).gameObject.SetActive(true);
                    }
                }

                // let the user select the route option
                selectingRouteOptionText.SetActive(true);
                for (int i = 0; i < routes.transform.childCount; i++)
                {
                    for (int c = 0; c < routes.transform.GetChild(i).transform.childCount; c++)
                    {
                        routes.transform.GetChild(i).GetChild(c).GetComponent<RoutepartScript>().selectingRouteOption = true;
                    }
                }
            }
        }
    }

    /*
     * draws a train card from the 5 open cards and adds it to the players hand
     */
    public void DrawTrainCardFromOpenCards(FaceUpTrainCardScript card)
    {
        // get the card (not a gameobject) and save 
        TrainCard drawnCard = new TrainCard(card.GetComponent<TrainCardScript>().color,
            card.transform.GetComponent<SpriteRenderer>().sprite);

        // drawn card to player's hand
        //Debug.Log("Farbe :  " + drawnCard.routeColor);
        activePlayer.AddTrainCardToHand(drawnCard);
        // create and display this card
        Instantiate(trainCardOnHandPrefab, handcardsPanel.transform.position, Quaternion.identity, handcardsPanel.transform);
        // give the handcard the sprite from the drawn card
        handcardsPanel.transform.GetChild(handcardsPanel.transform.childCount - 1).GetChild(1).GetComponent<Image>().sprite = drawnCard.spriteImage;
        handcardsPanel.transform.GetChild(handcardsPanel.transform.childCount - 1).GetComponent<TrainCardScript>().color = drawnCard.color;

        // save number 
        int position = card.gameObject.transform.parent.GetComponent<PositionScript>().position;

        // draw new random card from deck
        OpenTrainCardFromDeck(position);

        action = "DrawCardFromOpenCards";
        if (recordingCases && activePlayer.isHuman)
            SendCaseToCasebase();

        if (firstCardDrawn)
            secondCardDrawn = true;

        // if it was a joker card, the players turn is over
        if (drawnCard.color.Equals("Joker"))
        {
            if (!CheckEndingConditionOfGame())
            {
                // if player has claimed the route, his turn is over
                //Debug.Log("Rufe new Turn auf - DrawTrainCardFromOpenCards - if");
                StartCoroutine(NewTurn());
            } else 
            {
                CalculateTotalScore(); 
            }
        }
        // if it was the second drawn card, the players turn is over
        else if (firstCardDrawn)
        {
            if (!CheckEndingConditionOfGame())
            {
                // if player has claimed the route, his turn is over
                //Debug.Log("Rufe new Turn auf - DrawTrainCardFromOpenCards - else if");
                StartCoroutine(NewTurn());
            } else 
            {
                CalculateTotalScore(); 
            }
            // disable gameobjects
            traincardsDeck.GetComponent<TrainCardDeckScript>().disabled = true;
            destinationcardsDeck.GetComponent<DestinationCardDeckScript>().disabled = true;
            // disable all face up cards
            for (int i = 0; i < faceupTraincardsParent.transform.childCount; i++)
                faceupTraincardsParent.transform.GetChild(i).GetChild(0).GetComponent<FaceUpTrainCardScript>().disabled = true;
            // disable all routes
            DisableAllRoutes();
        }
        // if it was a normal card, the player is only allowed to draw a non-joker card
        else
        {
            firstCardDrawn = true;
            // disabled all face up joker cards
            for (int i=0; i<faceupTraincardsParent.transform.childCount; i++)
            {
                if (faceupTraincardsParent.transform.GetChild(i).GetChild(0).GetComponent<TrainCardScript>().color.Equals("Joker"))
                    faceupTraincardsParent.transform.GetChild(i).GetChild(0).GetComponent<FaceUpTrainCardScript>().disabled = true;
            }

            // disable gameobjects
            destinationcardsDeck.GetComponent<DestinationCardDeckScript>().disabled = true;
            // disable all routes
            DisableAllRoutes();

            if (recordingCases && activePlayer.isHuman)
            {
                List<RouteOption> routeOptionList = ReturnRouteOptionList(activePlayer);
                // show edges of shortest paths
                foreach (RouteOption ro in routeOptionList)
                {
                    GameObject routeParent = GameObject.Find(ro.nameOfRoute);
                    for (int i = 0; i < routeParent.transform.childCount; i++)
                    {
                        routeParent.transform.GetChild(i).GetChild(1).gameObject.SetActive(true);
                    }
                }

                // let the user select the route option
                selectingRouteOptionText.SetActive(true);
                for (int i = 0; i < routes.transform.childCount; i++)
                {
                    for (int c = 0; c < routes.transform.GetChild(i).transform.childCount; c++)
                    {
                        routes.transform.GetChild(i).GetChild(c).GetComponent<RoutepartScript>().selectingRouteOption = true;
                    }
                }
            }
        }
    }

    /*
     * draws 3 (or less) destination cards from the decks and displays them to the screen
     */
    public void DrawDestinationCardsFromDeck() {

        playerSelectingDestinationCards = true;
        // object for random number
        System.Random random = new System.Random();
        // clear drawnCards list
        drawnCards.Clear();

        // this has to be adjusted later if there are no more 3 cards left in the deck
        // draw 3 cards
        for (int i=0; i<3; i++)
        {
            if (destinationcards.Count == 0)
                break;

            // random number between 0 (included) and card count of the destination card deck (excl.) ((count = +1))
            int randomNumber = random.Next(0, destinationcards.Count);

            // get a card (not a gameobject) and save 
            DestinationCard drawnCard = destinationcards[randomNumber];
            // remove the drawn card from the deck
            destinationcards.Remove(drawnCard);
            // add the card to the list to save them for later
            drawnCards.Add(drawnCard);

            // create and display this card on the display for destination cards
            Instantiate(destinationCardPrefab, destinationcardsDrawDisplay.transform.position, Quaternion.identity, destinationcardsDrawDisplay.transform);
            // give the card the sprite from the drawn card
            destinationcardsDrawDisplay.transform.GetChild(destinationcardsDrawDisplay.transform.childCount - 1).
                GetChild(1).GetComponent<Image>().sprite = drawnCard.spriteImage;
        }
        // disable gameobjects
        traincardsDeck.GetComponent<TrainCardDeckScript>().disabled = true;
        destinationcardsDeck.GetComponent<DestinationCardDeckScript>().disabled = true;
        // disable all face up cards
        for (int i = 0; i < faceupTraincardsParent.transform.childCount; i++)
            faceupTraincardsParent.transform.GetChild(i).GetChild(0).GetComponent<FaceUpTrainCardScript>().disabled = true;
        // disable all routes
        DisableAllRoutes();

        if (turnCounter != 0)
        {
            action = "DrawDestinationCards";
            if (recordingCases && activePlayer.isHuman)
                SendCaseToCasebase();
        }
    }

    /* 
     * shows the window for selecting the drawn destination cards, including buttons for confirming and so on
     */
    public void ShowDestinationCardsSelectionWindow()
    {
        dimmer.SetActive(true);
        DisableGameObjects();

        // show the x, which hides the destination cards when clicked
        handdestinationcardsPanel.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);

        foreach (DestinationCard card in activePlayer.handdestinationcards)
        {
            // create and display this card on the display for destination cards
            Instantiate(destinationCardPrefab, destinationcardsPlayerDisplay.transform.position, Quaternion.identity, destinationcardsPlayerDisplay.transform);
            // give the card the sprite from the handcard of the player
            destinationcardsPlayerDisplay.transform.GetChild(destinationcardsPlayerDisplay.transform.childCount - 1).
                GetChild(1).GetComponent<Image>().sprite = card.spriteImage;
            // disable button component
            destinationcardsPlayerDisplay.transform.GetChild(destinationcardsPlayerDisplay.transform.childCount - 1).GetComponent<Button>().interactable = false;
        }
    }

    /* 
     * closes the window for selecting the drawn destination cards, after the player has chosen cards
     */
    public void HideDestinationCardsSelectionWindow()
    {
        foreach (Transform child in destinationcardsPlayerDisplay.transform)
            GameObject.Destroy(child.gameObject);

        dimmer.SetActive(false);
        EnableGameObjects();
        handdestinationcardsPanel.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
    }

    /* 
     * is triggered, when the user hits the confirm button after choosing destination cards
     * adds the selected destinations cards to the players hand
     */
    public void OnClickConfirmDestinationCards()
    {
        for (int i = 0; i < destinationcardsDrawDisplay.transform.childCount; i++)
        {
            if (destinationcardsDrawDisplay.transform.GetChild(i).GetComponent<CardSelectScript>().getSelected())
            {
                // add to player's hand
                activePlayer.AddDestinationCardToHand(drawnCards[i]);

                // still good code for displaying the destination cards?
                //// create and display this card
                //Instantiate(destinationCardPrefab, handdestinationcardsPanel.transform.position, Quaternion.identity, handdestinationcardsPanel.transform);
                //// give the handcard the sprite from the drawn card
                //handdestinationcardsPanel.transform.GetChild(handdestinationcardsPanel.transform.childCount - 1).GetChild(1).GetComponent<Image>().sprite = drawnCards[i].spriteImage;
            }
            // put back not selected cards
            else
            {
                destinationcards.Add(drawnCards[i]);
            }
        }

        // delete instantiated clones
        foreach (Transform child in destinationcardsDrawDisplay.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        spritesDestinationcardsDisplay.SetActive(false);
        dimmer.SetActive(false);
        EnableGameObjects();

        playerSelectingDestinationCards = false;

        if (destinationcards.Count == 0)
            destinationcardsDeck.SetActive(false);

        // after clicking on the confirm button, the players turn is over (if not start cards)
        if (!playersSelectingStartDestinationCards)
        {
            if (!CheckEndingConditionOfGame())
            {
                // if player has claimed the route, his turn is over
                //Debug.Log("Rufe new Turn auf - OnClickConfirmDestinationCards  ");
                StartCoroutine(NewTurn());
            }
            else
            {
                CalculateTotalScore();
            }
        }

        //Debug.Log(activePlayer.handdestinationcards[0].citiesAndPoints);
    }


    public IEnumerator StartTrainCardSelection(int routeLength, string routeColor, Transform routeParent)
    {
        // disable other gameobjects
        traincardsDeck.GetComponent<TrainCardDeckScript>().disabled = true;
        destinationcardsDeck.GetComponent<DestinationCardDeckScript>().disabled = true;
        // disable all face up cards
        for (int i = 0; i < faceupTraincardsParent.transform.childCount; i++)
            faceupTraincardsParent.transform.GetChild(i).GetChild(0).GetComponent<FaceUpTrainCardScript>().disabled = true;

        // highlight the route
        routeParent.GetComponent<RouteScript>().highlighted = true;
        // disable all routes but the selected one
        for (int i = 0; i < routes.transform.childCount; i++)
        {
            for (int j = 0; j < routes.transform.GetChild(i).transform.childCount; j++)
            {
                if (!routes.transform.GetChild(i).GetComponent<RouteScript>().highlighted)
                {
                    routes.transform.GetChild(i).GetChild(j).GetComponent<RoutepartScript>().disabled = true;
                }
            }
        }

        // set button for confirming the card selection active 
        confirmHandCardsBtn.gameObject.SetActive(true);
        // set button interactable
        confirmHandCardsBtn.gameObject.GetComponent<Button>().interactable = false;
        // enable moving (selecting) the handcards
        for (int i = 0; i < handcardsPanel.transform.childCount; i++)
        {
            handcardsPanel.transform.GetChild(i).GetComponent<CardSelectScript>().IsEnabled = true;
            //Debug.Log(handcardsPanel.transform.GetChild(i).GetComponent<CardSelectScript>().IsEnabled);
        }
        // start coroutine and wait for player card selection
        yield return WaitForPlayerCardSelection(routeLength, routeColor, routeParent);

        // check, if player has clicked on confirmButton -> if highlighted is false, routeSelection was canceled
        if (routeParent.GetComponent<RouteScript>().highlighted)
        {
            // when player is done, remove the selected handcards
            RemoveSelectedHandcards();
            // place trains
            PlaceTrains(routeParent);

            action = "ClaimRoute";
            if (recordingCases && activePlayer.isHuman)
                SendCaseToCasebase();

            // reset highlightning and select state
            routeParent.GetComponent<RouteScript>().highlighted = false;
            for (int i = 0; i < routeParent.transform.childCount; i++)
            {
                routeParent.GetChild(i).GetComponent<RoutepartScript>().selected = false;
                routeParent.GetChild(i).GetChild(0).gameObject.SetActive(false);
            }

            // set route as occupied
            routeParent.GetComponent<RouteScript>().occupied = true;
            // set owner of route
            routeParent.GetComponent<RouteScript>().owner = activePlayer;

            // add route to player list
            activePlayer.acquiredRoutes.Add(routeParent.name);

            string[] citiesArray = routeParent.GetComponent<RouteScript>().cities;
            
            if (!activePlayer.cities.Contains(citiesArray[0]))
            {
                activePlayer.cities.Add(citiesArray[0]);
            }
            if (!activePlayer.cities.Contains(citiesArray[1]))
            {
                activePlayer.cities.Add(citiesArray[1]);
            }

            foreach (string city in activePlayer.cities) {
                //Debug.Log(citystring + city + " ");
            }
            //Debug.Log(activePlayer.playerName + ":  " + citystring);
            
            CreateWeights();
            ShortestPathFinder shortestPathFinder = new ShortestPathFinder(cityMap);
            activePlayer.longestRouteDistance = shortestPathFinder.findLongestRoute(activePlayer);

            // delete the other double route 
            if (players.Count <= 3 && routeParent.GetComponent<RouteScript>().isDoubleRoute)
                DeleteDoubleRoute(routeParent.gameObject.name);

            if (!CheckEndingConditionOfGame())
            {
                // if player has claimed the route, his turn is over
                StartCoroutine(NewTurn());
            } else 
            {
                CalculateTotalScore();
            }
        } else
        {
            //// enable gameobjects
            //traincardsDeck.GetComponent<TrainCardDeckScript>().disabled = false;
            //destinationcardsDeck.GetComponent<DestinationCardDeckScript>().disabled = false;
            //// enable all face up cards
            //for (int i = 0; i < faceupTraincardsParent.transform.childCount; i++)
            //    faceupTraincardsParent.transform.GetChild(i).GetChild(0).GetComponent<FaceUpTrainCardScript>().disabled = false;
            //// enable all routes
            //EnableAllRoutes();
        }
    }

    public IEnumerator WaitForPlayerCardSelection(int routeLength, string routeColor, Transform routeParent)
    {
        bool done = false;
        while (!done)
        {
            if (CheckSelectedHandCards(routeLength, routeColor))
            {
                confirmHandCardsBtn.interactable = true;
            }
            else
            {
                confirmHandCardsBtn.interactable = false;
            }

            if (!confirmHandCardsBtn.isActiveAndEnabled)
                done = true;

            if (!routeParent.GetComponent<RouteScript>().highlighted)
                done = true;

            yield return null;
        }
    }

    public IEnumerator WaitUntilNewTrainCardIsOpen()
    {
        bool done = false;
        while (!done)
        {
            if (!openCardProcessing)
                done = true;

            yield return null;
        }
    }

    /*
    * checks if the player has the correct cards on hand which are used to claim the route
    * is called when the players clicks on the route
    */
    public bool CheckHandCardsForClaimingOfRouteWithoutJoker(int routeLength, string routeColor)
    {
        int count = 0;

        // if the route is gray, check whether the handcards contains any routeColor with enoght cards to chose the selected route
         if (routeColor == "Grey") 
        {
            if (CheckHandcardsIfGrayRouteIsSelectable(routeLength)) 
            {
                return true;
            }
        }       

        // check if enough cards on hand
        // loop goes through the handcards, which are saved in handcardsPanel and check whether the required cards are available
        for (int i = 0; i < activePlayer.handtraincards.Count; i++)
        {
            //if condition compare the routeColor of the route with the routeColor of die selected handcard
            if (routeColor.Equals(activePlayer.handtraincards[i].color)) 
            {
                // if the cardColors are identical the variable count increase by the value one
                count++;
            }
        }

        //check, whether the number of necessary handcards is higher than the length of the route
        if (count >= routeLength)
            return true;
        else
            return false;

        // return (count >= routeLength) ? true : false;
    }
    
    public bool CheckHandCardsForClaimingOfRouteWithJoker(int routeLength, string routeColor)
    {
        int count = 0;

        // if the route is gray, check whether the handcards contains any routeColor with enoght cards to chose the selected route
         if (routeColor == "Grey") 
        {
            if (CheckHandcardsWithJokerIfGrayRouteIsSelectable(routeLength)) 
            {
                return true;
            }
        }  

        // check if enough cards on hand
        // loop goes through the handcards, which are saved in handcardsPanel and check whether the required cards are available
        for (int i = 0; i < activePlayer.handtraincards.Count; i++)
        {
            //if condition compare the routeColor of the route with the routeColor of die selected handcard
            if (routeColor.Equals(activePlayer.handtraincards[i].color) || activePlayer.handtraincards[i].color.Equals("Joker")) 
            {
                // if the cardColors are identical the variable count increase by the value one
                count++;
            }
        }

        //check, whether the number of necessary handcards is higher than the length of the route
        if (count >= routeLength)
            return true;
        else
            return false;
    }

    public bool CheckSelectedHandCards(int routeLength, string routeColor)
    {
        int count = 0;
        
        // if the route is gray, check whether the handcards contains any routeColor with enoght cards to chose the selected route
         if (routeColor == "Grey") 
        {
            if (CheckSelectedHandcardsIfGrayRouteIsSelectable(routeLength)) 
            {
                return true;
            }
        }

        // check if enough cards on hand
        // loop goes through the handcards, which are saved in handcardsPanel and check whether the required cards are available
        for (int i = 0; i < handcardsPanel.transform.childCount; i++)
        {
            //if condition compare the routeColor of the route with the routeColor of die selected handcard
            if (handcardsPanel.transform.GetChild(i).GetComponent<CardSelectScript>().selected)
            {
                if (handcardsPanel.transform.GetChild(i).GetComponent<TrainCardScript>().color.Equals(routeColor) ||
                    handcardsPanel.transform.GetChild(i).GetComponent<TrainCardScript>().color.Equals("Joker"))
                {
                    count++;
                } else {
                    return false;
                }
            }
        }

        //check, whether die number auf necessary handcards is higher than the length of the route
        if (count == routeLength)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    // Check if the handcards are sufficient to chose the selected route
    public bool CheckSelectedHandcardsIfGrayRouteIsSelectable(int routeLength)
    {
        foreach (string cardColor in cardColors) 
        {
            if (CheckSelectedHandCards(routeLength, cardColor)) 
            {
                return true;
            }
        }
        return false;
    }

    // Check if the handcards are sufficient to chose the selected route
    public bool CheckHandcardsIfGrayRouteIsSelectable(int routeLength)
    {
        foreach (string cardColor in cardColors) 
        {
            if (CheckHandCardsForClaimingOfRouteWithoutJoker(routeLength, cardColor)) 
            {
                return true;
            }
        }
        return false;
    }

    // Check if the handcards are sufficient to chose the selected route
    public bool CheckHandcardsWithJokerIfGrayRouteIsSelectable(int routeLength)
    {
        foreach (string cardColor in cardColors) 
        {
            if (CheckHandCardsForClaimingOfRouteWithJoker(routeLength, cardColor)) 
            {
                return true;
            }
        }
        return false;
    }


    /*
    * user confirms the selected cards for claiming the desired route
    * checks players selected cards, removes them (if check succeeds) and claims the desired route
    */
    public void OnClickConfirmCardsBtn()
    {
        confirmHandCardsBtn.gameObject.SetActive(false);
        //if (CheckHandCards("Color", 1))
        //{
        //    RemoveCardsFromHand("Color", 1);
        //    UpdateHandcards();
        //}
        //else
        //    Debug.Log("TODO");

        //// TODO: claim the route   ---> ??
    }

    /* 
     * Removes the necessary number of cards in the selected routeColor
     */
    public void RemoveCardsFromHand(string color, int number)
    {
        // here you have to start at the of the list, because some objects of the list are removed:
        // if you start at the beginning and remove the first object, the second object becomes the first one and you skip it
        for (int i = handcardsPanel.transform.childCount - 1; i >= 0; i--)
        {
            // with handcardsPanel.transform.GetChild(i) you access the handcards
            if (number > 0 && handcardsPanel.transform.GetChild(i).GetComponent<CardSelectScript>().selected)
            {
                // if the card is selected, remove it from the players hand
                activePlayer.handtraincards.Remove(activePlayer.handtraincards[i]); 
                // decrease the number of cards that needs to be removed
                number--;
            }
        }

        UpdateHandcards();

        // disable the confirm button
        confirmHandCardsBtn.gameObject.SetActive(false);
    }

    public void RemoveSelectedHandcards()
    {
        for (int i = handcardsPanel.transform.childCount - 1; i >= 0; i--)
        {
            if (handcardsPanel.transform.GetChild(i).GetComponent<CardSelectScript>().selected)
            {
                foreach (TrainCard card in activePlayer.handtraincards)
                {
                    if (card.color.Equals(handcardsPanel.transform.GetChild(i).GetComponent<TrainCardScript>().color))
                    {
                        traincardsDiscard.Add(card);
                        activePlayer.handtraincards.Remove(card);
                        ReduceNumberOfWagons(activePlayer, 1); // for each card one wagon will removed
                        break;
                    }
                }
                Destroy(handcardsPanel.transform.GetChild(i).gameObject);     
            }
        }
        UpdateHandcards();
    }

    /* 
     * updates the players handcards, including deselecting all handcards (TODO?)
     */
    void UpdateHandcards()
    {
        foreach (Transform child in handcardsPanel.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        for (int i = 0; i < activePlayer.handtraincards.Count; i++)
        {
            GameObject card = Instantiate(trainCardOnHandPrefab, handcardsPanel.transform.position, Quaternion.identity, handcardsPanel.transform);

            card.transform.GetChild(1).GetComponent<Image>().sprite = activePlayer.handtraincards[i].spriteImage;
            card.GetComponent<TrainCardScript>().color = activePlayer.handtraincards[i].color;
        }
    }


    public void PlaceTrains(Transform routeParent)
    {
        int waggonCounter = 0;
        // this loop is for transforming the route
        for (int i = 0; i < routeParent.transform.childCount; i++)
        {
            // routeParent.GetChild(i).GetComponent<Transform>().transform.localScale += new Vector3(0.05f, 0.05f, 0.05f);

            SpawnTrain(i, routeParent);
            routeParent.GetChild(i).GetComponent<TrainFigScript>().created = true;
            // TODOOOOOOOOOOOOOOOOOOOOOOOOO

            waggonCounter++;
        }

        // raise the score of the active player 
        RaisePlayerScore(activePlayer, waggonCounter);
    }

    private void SpawnTrain(int i, Transform routeParent)
    {
        Quaternion rote;

        Vector3 trainPos = new Vector3(routeParent.GetChild(i).GetComponent<Transform>().transform.position.x,
            routeParent.GetChild(i).GetComponent<Transform>().transform.position.y + 0.1f,
            routeParent.GetChild(i).GetComponent<Transform>().transform.position.z);

        Vector3 trainRot = new Vector3(routeParent.GetChild(i).GetComponent<Transform>().transform.eulerAngles.x,
            routeParent.GetChild(i).GetComponent<Transform>().transform.eulerAngles.y + 90,
            routeParent.GetChild(i).GetComponent<Transform>().transform.eulerAngles.z);
        rote = Quaternion.Euler(trainRot);

        GameObject trainClone = Instantiate(trainfig, trainPos, rote);
        trainClone.GetComponent<Renderer>().material.color = activePlayer.playerColor;

        trainClone.transform.parent = routeParent.GetChild(i).transform;
        trainClone.name = "trainClone" + (i + 1);
    }

    /* 
     * disables the gameobjects if the player has to interact with certain (other) gameobject
     */
    public void DisableGameObjects()
    {
        GameObject[] gameobjectsToBlock = GameObject.FindGameObjectsWithTag("ToBlock");
        GameObject[] buttonsToBlock = GameObject.FindGameObjectsWithTag("ToBlockButton");

        foreach(GameObject go in gameobjectsToBlock)
        {
            go.GetComponent<BoxCollider>().enabled = false;
        }
        foreach(GameObject go in buttonsToBlock)
        {
            go.GetComponent<Button>().interactable = false;
        }
    }

    /* 
     * enables the gameobjects if the player had to interact with certain (other) gameobject and is done
     */
    public void EnableGameObjects()
    {
        GameObject[] gameobjectsToBlock = GameObject.FindGameObjectsWithTag("ToBlock");
        GameObject[] buttonsToBlock = GameObject.FindGameObjectsWithTag("ToBlockButton");
        foreach (GameObject go in gameobjectsToBlock)
        {
            go.GetComponent<BoxCollider>().enabled = true;
        }
        foreach (GameObject go in buttonsToBlock)
        {
            go.GetComponent<Button>().interactable = true;
        }
    }

    public void DisableAllRoutes()
    {
        for (int i = 0; i < routes.transform.childCount; i++)
        {
            for (int j = 0; j < routes.transform.GetChild(i).transform.childCount; j++)
                routes.transform.GetChild(i).GetChild(j).GetComponent<RoutepartScript>().disabled = true;
        }
    }

    public void EnableAllRoutes()
    {
        for (int i = 0; i < routes.transform.childCount; i++)
        {
            for (int j = 0; j < routes.transform.GetChild(i).transform.childCount; j++)
                routes.transform.GetChild(i).GetChild(j).GetComponent<RoutepartScript>().disabled = false;
        }
    }

    public void DisableDoubleRoutes()
    {
        // iterate through all routes
        for (int i=0; i<routes.transform.childCount; i++)
        {
            RouteScript route = routes.transform.GetChild(i).GetComponent<RouteScript>();
            // if the active player owns this route AND it is a double route
            if (route.owner != null)
            {
                if (route.owner.Equals(activePlayer) && route.isDoubleRoute)
                {
                    // get the name of the route -> city1_city2 to get the other route by name
                    string routeName = routes.transform.GetChild(i).gameObject.name;
                    // returns the substring up to the second occurrence of "_" -> result: city1_city2
                    int firstIndex = routeName.IndexOf("_");
                    int secondIndex = routeName.IndexOf("_", firstIndex + 1);
                    routeName = routeName.Substring(0, secondIndex);
                    // disable routes with that name
                    for (int r = 0; r < routes.transform.childCount; r++)
                    {
                        if (routes.transform.GetChild(r).gameObject.name.Contains(routeName))
                        {
                            for (int j = 0; j < routes.transform.GetChild(r).transform.childCount; j++)
                                routes.transform.GetChild(r).GetChild(j).GetComponent<RoutepartScript>().disabled = true;
                        }
                    }
                }
            }
        }
    }

    public void DeleteDoubleRoute(string routeName)
    {
        // returns the substring up to the second occurrence of "_" -> result: city1_city2
        int firstIndex = routeName.IndexOf("_");
        int secondIndex = routeName.IndexOf("_", firstIndex + 1);
        string routeNameModified = routeName.Substring(0, secondIndex);

        // delete the other route with that name
        for (int r = 0; r < routes.transform.childCount; r++)
        {
            // find the other route and also check, if it has no owner -> that is the route to be deleted
            if (routes.transform.GetChild(r).gameObject.name.Contains(routeNameModified) && routes.transform.GetChild(r).GetComponent<RouteScript>().owner == null)
            {
                // delete gameobject
                Destroy(routes.transform.GetChild(r).gameObject);
                string[] cities = routeNameModified.Split('_');
                // remove from dictionary
                cityMap[cities[0]].RemoveAll(con => con.city == cities[1] && con.routeName != routeName);
                cityMap[cities[1]].RemoveAll(con => con.city == cities[0] && con.routeName != routeName);
                break;
            }
        }
    }

    public IEnumerator CheckFaceUpCardsForThreeJokerCards()
    {
        int counter = 0;
        foreach (TrainCard t in faceupTraincards)
        {
            if (t.color.Equals("Joker"))
                counter++;
        }

        if (counter >= 3)
        {
            collider.SetActive(true);
            yield return new WaitForSecondsRealtime(2);
            collider.SetActive(false);
            gettingNewOpenCards = true;
            for (int i=0; i<5; i++)
            {
                traincardsDiscard.Add(faceupTraincards[i]);
                OpenTrainCardFromDeck(i);
                if (faceupTraincardsParent.transform.GetChild(i).transform.childCount > 1)
                    Destroy(faceupTraincardsParent.transform.GetChild(i).GetChild(1).transform.gameObject);
            }
        }

        for (int i = 0; i < 5; i++)
        {
            if (faceupTraincardsParent.transform.GetChild(i).transform.childCount > 1)
                Destroy(faceupTraincardsParent.transform.GetChild(i).GetChild(1).transform.gameObject);
        }
        gettingNewOpenCards = false;
    }

    /*
     * the score of the current player is raising depending on the range of the chosen route 
     */
    public void RaisePlayerScore(PlayerScript player, int numberOfWagons)
    {
        int points = 0;
        if (numberOfWagons == 1) {
            points = 1;
        } else if (numberOfWagons == 2) {
            points = 2;
        } else if (numberOfWagons == 3) {
            points = 4;
        } else if (numberOfWagons == 4) {
            points = 7;
        } else if (numberOfWagons == 5) {
            points = 10;
        } else if (numberOfWagons == 6) {
            points = 15;
        }
        player.score += points;
    }

     /*
     * the number of available Wagons will reduced 
     */
    public void ReduceNumberOfWagons(PlayerScript player, int numberOfWagons)
    {
        //testCityMap1();
        player.availableWagons -= numberOfWagons;
        //Debug.Log("Wagon was removed: " + player.availableWagons);
        //Debug.Log("Weight: " + cityMap["Helena"][0].weight + " so soll: " + GetRouteLength("LosAngeles_LasVegas_grey") + "    Nachtr�glich: " + cityMap["Helena"][0].RouteLength());
        //Debug.Log("Route: " + cityMap["Helena"][0].routeName);
    }

     /*
     * check if the number of available Wagons is sufficient to chose the route
     * Every player have 45 wagons 
     */
    public bool CheckNumberOfWagons(int numberOfWagons)
    {
        if (activePlayer.availableWagons < numberOfWagons)
        {
            //Debug.Log("Available wagons are not enough");    
            return false;
        } else {
            return true;
        }
    }

    /*
     * check if the number of available Wagons is sufficient to chose the route
     * Every player have 45 wagons 
     */
    public bool CheckEndingConditionOfGame()
    {
        if (lastPlayerBeforeGameEnds == activePlayer)
        {
            Debug.Log("Game is over.");    
            return true;
        }        
        if (activePlayer.availableWagons < 3 && lastPlayerBeforeGameEnds == null)
        {
            Debug.Log("Every player have one move left.");    
            lastPlayerBeforeGameEnds = activePlayer;
        }
        return false;
    }

    /*
    public void CreateCityMap1()
    {
        cityMap1.Add("Vancouver", new List<string> { "Calgary", "Seattle" });
        cityMap1.Add("Seattle", new List<string> { "Vancouver", "Calgary", "Portland", "Helena" });
        cityMap1.Add("Calgary", new List<string> { "Vancouver", "Seattle", "Portland", "Winnipeg" });
        cityMap1.Add("Portland", new List<string> { "Seattle", "SaltLakeCity", "SanFrancisco" });
        cityMap1.Add("Helena", new List<string> { "Calgary", "Seattle", "SaltLakeCity", "Denver", "Omaha", "Duluth", "winnipeg" });
        cityMap1.Add("SaltLakeCity", new List<string> { "Portland", "Helena", "Denver", "LasVegas", "SanFrancisco" });
        cityMap1.Add("SanFrancisco", new List<string> { "Portland", "SaltLakeCity", "LosAngeles" });
        cityMap1.Add("LosAngeles", new List<string> { "SanFrancisco", "LasVegas", "Phoenix", "ElPaso" });
        cityMap1.Add("LasVegas", new List<string> { "SaltLakeCity", "LosAngeles"});
        cityMap1.Add("Phoenix", new List<string> { "LosAngeles", "ElPaso", "Denver", "SantaFe" });
        cityMap1.Add("SantaFe", new List<string> { "Phoenix", "Denver", "OklahomaCity", "ElPaso" });
        cityMap1.Add("Denver", new List<string> { "Helena", "SaltLakeCity", "Phoenix", "SantaFe", "OklahomaCity", "KansasCity", "Omaha" });
        cityMap1.Add("ElPaso", new List<string> { "Phoenix", "LosAngeles", "SantaFe", "OklahomaCity", "Dallas", "Houston" });
        cityMap1.Add("Winnipeg", new List<string> { "Calgary", "Helena", "Duluth", "SaultStMarie" });
        cityMap1.Add("Duluth", new List<string> { "SaultStMarie", "Winnipeg", "Helena", "Omaha", "Chicago", "Toronto" });
        cityMap1.Add("Omaha", new List<string> { "Helena", "Denver", "Duluth", "Chicago", "KansasCity" });
        cityMap1.Add("KansasCity", new List<string> { "Denver", "Omaha", "OklahomaCity", "SaintLouis" });
        cityMap1.Add("OklahomaCity", new List<string> { "KansasCity", "SantaFe", "Denver", "ElPaso", "Dallas", "LittleRock" });
        cityMap1.Add("Houston", new List<string> { "ElPaso", "Dallas", "NewOrleans" });
        cityMap1.Add("Dallas", new List<string> { "LittleRock", "OklahomaCity", "ElPaso", "Houston" });
        cityMap1.Add("LittleRock", new List<string> { "NewOrleans", "Dallas", "OklahomaCity", "SaintLouis", "Nashville" });
        cityMap1.Add("SaintLouis", new List<string> { "LittleRock", "KansasCity", "Chicago", "Pittsburgh", "Nashville" });
        cityMap1.Add("Chicago", new List<string> { "SaintLouis", "Omaha", "Duluth", "Toronto", "Pittsburgh" });
        cityMap1.Add("SaultStMarie", new List<string> { "Winnipeg", "Duluth", "Montreal", "Toronto" });
        cityMap1.Add("Toronto", new List<string> { "Montreal", "SaultStMarie", "Duluth", "Chicago", "Pittsburgh" });
        cityMap1.Add("Montreal", new List<string> { "SaultStMarie", "Toronto", "NewYork", "Boston" });
        cityMap1.Add("Boston", new List<string> { "NewYork", "Montreal" });
        cityMap1.Add("NewYork", new List<string> { "Boston", "Montreal", "Pittsburgh", "Washington" });
        cityMap1.Add("Pittsburgh", new List<string> { "NewYork", "Toronto", "Chicago", "SaintLouis", "Nashville", "Raleigh", "Washington" });
        cityMap1.Add("Washington", new List<string> { "NewYork", "Pittsburgh", "Raleigh" });
        cityMap1.Add("Raleigh", new List<string> { "Washington", "Pittsburgh", "Nashville", "Atlanta", "Charleston" });
        cityMap1.Add("Atlanta", new List<string> { "Nashville", "Raleigh", "Charleston", "Miami", "NewOrleans" });
        cityMap1.Add("Charleston", new List<string> { "Atlanta", "Raleigh", "Miami" });
        cityMap1.Add("Miami", new List<string> { "Charleston", "Atlanta", "NewOrleans" });
        cityMap1.Add("NewOrleans", new List<string> { "Houston", "LittleRock", "Atlanta", "Miami" });
        cityMap1.Add("Nashville", new List<string> { "SaintLouis", "LittleRock", "Atlanta", "Raleigh" });
    }
    */

    public void CreateDestinationCards()
    {

    }

    // Stadt1, liste(citycCon1, cityCon2)

    public void CreateCityMap()
    {
        cityMap = new Dictionary<string, List<CityConnection>>();
        cityMap.Add("Vancouver", new List<CityConnection> { new CityConnection("Calgary", "Vancouver_Calgary_Grey"), new CityConnection("Seattle", "Vancouver_Seattle_Grey_2"), new CityConnection("Seattle", "Vancouver_Seattle_Grey_1") });
        cityMap.Add("Seattle", new List<CityConnection> { new CityConnection("Vancouver", "Vancouver_Seattle_Grey_2"), new CityConnection("Vancouver", "Vancouver_Seattle_Grey_1"), new CityConnection("Calgary", "Seattle_Calgary_Grey"), new CityConnection("Portland", "Seattle_Portland_Grey_1"), new CityConnection("Portland", "Seattle_Portland_Grey_2"), new CityConnection("Helena", "Seattle_Helena_Yellow") });
        cityMap.Add("Calgary", new List<CityConnection> { new CityConnection("Vancouver", "Vancouver_Calgary_Grey"), new CityConnection("Seattle", "Seattle_Calgary_Grey"), new CityConnection("Helena", "Calgary_Helena_Grey"), new CityConnection("Winnipeg", "Calgary_Winnipeg_White") });
        cityMap.Add("Portland", new List<CityConnection> { new CityConnection("Seattle", "Seattle_Portland_Grey_1"), new CityConnection("Seattle", "Seattle_Portland_Grey_2"), new CityConnection("SaltLakeCity", "Portland_SaltLakeCity_Blue"), new CityConnection("SanFrancisco", "Portland_SanFrancisco_Green"), new CityConnection("SanFrancisco", "Portland_SanFrancisco_Purple") });
        cityMap.Add("Helena", new List<CityConnection> { new CityConnection("Calgary", "Calgary_Helena_Grey"), new CityConnection("Seattle", "Seattle_Helena_Yellow"), new CityConnection("SaltLakeCity", "Helena_SaltLakeCity_Purple"), new CityConnection("Denver", "Helena_Denver_Green"), new CityConnection("Omaha", "Helena_Omaha_Red"), new CityConnection("Duluth", "Helena_Duluth_Orange"), new CityConnection("Winnipeg", "Helena_Winnipeg_Blue") });
        cityMap.Add("SaltLakeCity", new List<CityConnection> { new CityConnection("Portland", "Portland_SaltLakeCity_Blue"), new CityConnection("Helena", "Helena_SaltLakeCity_Purple"), new CityConnection("Denver", "SaltLakeCity_Denver_Yellow"), new CityConnection("Denver", "SaltLakeCity_Denver_Red"), new CityConnection("LasVegas", "SaltLakeCity_LasVegas_Orange"), new CityConnection("SanFrancisco", "SanFrancisco_SaltLakeCity_Orange"), new CityConnection("SanFrancisco", "SanFrancisco_SaltLakeCity_White") });
        cityMap.Add("SanFrancisco", new List<CityConnection> { new CityConnection("Portland", "Portland_SanFrancisco_Green"), new CityConnection("Portland", "Portland_SanFrancisco_Purple"), new CityConnection("SaltLakeCity", "SanFrancisco_SaltLakeCity_Orange"), new CityConnection("SaltLakeCity", "SanFrancisco_SaltLakeCity_White"), new CityConnection("LosAngeles", "SanFrancisco_LosAngeles_Yellow"), new CityConnection("LosAngeles", "SanFrancisco_LosAngeles_Purple") });
        cityMap.Add("LosAngeles", new List<CityConnection> { new CityConnection("SanFrancisco", "SanFrancisco_LosAngeles_Yellow"), new CityConnection("SanFrancisco", "SanFrancisco_LosAngeles_Purple"), new CityConnection("LasVegas", "LosAngeles_LasVegas_Grey"), new CityConnection("Phoenix", "Phoenix_LosAngeles_Grey"), new CityConnection("ElPaso", "LosAngeles_ElPaso_Black") });
        cityMap.Add("LasVegas", new List<CityConnection> { new CityConnection("SaltLakeCity", "SaltLakeCity_LasVegas_Orange"), new CityConnection("LosAngeles", "LosAngeles_LasVegas_Grey") });
        cityMap.Add("Phoenix", new List<CityConnection> { new CityConnection("LosAngeles", "Phoenix_LosAngeles_Grey"), new CityConnection("ElPaso", "Phoenix_ElPaso_Grey"), new CityConnection("Denver", "Phoenix_Denver_White"), new CityConnection("SantaFe", "Phoenix_SantaFe_Grey") });
        cityMap.Add("SantaFe", new List<CityConnection> { new CityConnection("Phoenix", "Phoenix_SantaFe_Grey"), new CityConnection("Denver", "Denver_SantaFe_Grey"), new CityConnection("OklahomaCity", "SantaFe_OklahomaCity_Blue"), new CityConnection("ElPaso", "SantaFe_ElPaso_Grey") });
        cityMap.Add("Denver", new List<CityConnection> { new CityConnection("Helena", "Helena_Denver_Green"), new CityConnection("SaltLakeCity", "SaltLakeCity_Denver_Yellow"), new CityConnection("SaltLakeCity", "SaltLakeCity_Denver_Red"), new CityConnection("Phoenix", "Phoenix_Denver_White"), new CityConnection("SantaFe", "Denver_SantaFe_Grey"), new CityConnection("OklahomaCity", "Denver_OklahomaCity_Red"), new CityConnection("KansasCity", "Denver_KansasCity_Orange"), new CityConnection("KansasCity", "Denver_KansasCity_Black"), new CityConnection("Omaha", "Denver_Omaha_Purple") });
        cityMap.Add("ElPaso", new List<CityConnection> { new CityConnection("Phoenix", "Phoenix_ElPaso_Grey"), new CityConnection("LosAngeles", "LosAngeles_ElPaso_Black"), new CityConnection("SantaFe", "SantaFe_ElPaso_Grey"), new CityConnection("OklahomaCity", "ElPaso_OklahomaCity_Yellow"), new CityConnection("Dallas", "ElPaso_Dallas_Red"), new CityConnection("Houston", "ElPaso_Houston_Green") });
        cityMap.Add("Winnipeg", new List<CityConnection> { new CityConnection("Calgary", "Calgary_Winnipeg_White"), new CityConnection("Helena", "Helena_Winnipeg_Blue"), new CityConnection("Duluth", "Winnipeg_Duluth_Black"), new CityConnection("SaultStMarie", "Winnipeg_SaultStMarie_Grey") });
        cityMap.Add("Duluth", new List<CityConnection> { new CityConnection("SaultStMarie", "Duluth_SaultStMarie_Grey"), new CityConnection("Winnipeg", "Winnipeg_Duluth_Black"), new CityConnection("Helena", "Helena_Duluth_Orange"), new CityConnection("Omaha", "Duluth_Omaha_Grey_2"), new CityConnection("Omaha", "Duluth_Omaha_Grey_1"), new CityConnection("Chicago", "Duluth_Chicago_Red"), new CityConnection("Toronto", "Duluth_Toronto_Purple") });
        cityMap.Add("Omaha", new List<CityConnection> { new CityConnection("Helena", "Helena_Omaha_Red"), new CityConnection("Denver", "Denver_Omaha_Purple"), new CityConnection("Duluth", "Duluth_Omaha_Grey_2"), new CityConnection("Duluth", "Duluth_Omaha_Grey_1"), new CityConnection("Chicago", "Omaha_Chicago_Blue"), new CityConnection("KansasCity", "Omaha_KansasCity_Grey_1"), new CityConnection("KansasCity", "Omaha_KansasCity_Grey_2") });
        cityMap.Add("KansasCity", new List<CityConnection> { new CityConnection("Denver", "Denver_KansasCity_Orange"), new CityConnection("Denver", "Denver_KansasCity_Black"), new CityConnection("Omaha", "Omaha_KansasCity_Grey_1"), new CityConnection("Omaha", "Omaha_KansasCity_Grey_2"), new CityConnection("OklahomaCity", "KansasCity_OklahomaCity_Grey_1"), new CityConnection("OklahomaCity", "KansasCity_OklahomaCity_Grey_2"), new CityConnection("SaintLouis", "KansasCity_SaintLouis_Blue"), new CityConnection("SaintLouis", "KansasCity_SaintLouis_Purple") });
        cityMap.Add("OklahomaCity", new List<CityConnection> { new CityConnection("KansasCity", "KansasCity_OklahomaCity_Grey_1"), new CityConnection("KansasCity", "KansasCity_OklahomaCity_Grey_2"), new CityConnection("SantaFe", "SantaFe_OklahomaCity_Blue"), new CityConnection("Denver", "Denver_OklahomaCity_Red"), new CityConnection("ElPaso", "ElPaso_OklahomaCity_Yellow"), new CityConnection("Dallas", "OklahomaCity_Dallas_Grey_1"), new CityConnection("Dallas", "OklahomaCity_Dallas_Grey_2"), new CityConnection("LittleRock", "OklahomaCity_LittleRock_Grey") });
        cityMap.Add("Houston", new List<CityConnection> { new CityConnection("ElPaso", "ElPaso_Houston_Green"), new CityConnection("Dallas", "Dallas_Houston_Grey_1"), new CityConnection("Dallas", "Dallas_Houston_Grey_2"), new CityConnection("NewOrleans", "Houston_NewOrleans_Grey") });
        cityMap.Add("Dallas", new List<CityConnection> { new CityConnection("LittleRock", "LittleRock_Dallas_Grey"), new CityConnection("OklahomaCity", "OklahomaCity_Dallas_Grey_1"), new CityConnection("OklahomaCity", "OklahomaCity_Dallas_Grey_2"), new CityConnection("ElPaso", "ElPaso_Dallas_Red"), new CityConnection("Houston", "Dallas_Houston_Grey_1"), new CityConnection("Houston", "Dallas_Houston_Grey_2") });
        cityMap.Add("LittleRock", new List<CityConnection> { new CityConnection("NewOrleans", "LittleRock_NewOrleans_Green"), new CityConnection("Dallas", "LittleRock_Dallas_Grey"), new CityConnection("OklahomaCity", "OklahomaCity_LittleRock_Grey"), new CityConnection("SaintLouis", "SaintLouis_LittleRock_Grey"), new CityConnection("Nashville", "LittleRock_Nashville_White") });
        cityMap.Add("SaintLouis", new List<CityConnection> { new CityConnection("LittleRock", "SaintLouis_LittleRock_Grey"), new CityConnection("KansasCity", "KansasCity_SaintLouis_Blue"), new CityConnection("KansasCity", "KansasCity_SaintLouis_Purple"), new CityConnection("Chicago", "SaintLouis_Chicago_Green"), new CityConnection("Chicago", "SaintLouis_Chicago_White"), new CityConnection("Pittsburgh", "SaintLouis_Pittsburgh_Green"), new CityConnection("Nashville", "SaintLouis_Nashville_Grey") });
        cityMap.Add("Chicago", new List<CityConnection> { new CityConnection("SaintLouis", "SaintLouis_Chicago_Green"), new CityConnection("SaintLouis", "SaintLouis_Chicago_White"), new CityConnection("Omaha", "Omaha_Chicago_Blue"), new CityConnection("Duluth", "Duluth_Chicago_Red"), new CityConnection("Toronto", "Chicago_Toronto_White"), new CityConnection("Pittsburgh", "Chicago_Pittsburgh_Orange"), new CityConnection("Pittsburgh", "Chicago_Pittsburgh_Black") });
        cityMap.Add("SaultStMarie", new List<CityConnection> { new CityConnection("Winnipeg", "Winnipeg_SaultStMarie_Grey"), new CityConnection("Duluth", "Duluth_SaultStMarie_Grey"), new CityConnection("Montreal", "SaultStMarie_Montreal_Black"), new CityConnection("Toronto", "SaultStMarie_Toronto_Grey") });
        cityMap.Add("Toronto", new List<CityConnection> { new CityConnection("Montreal", "Toronto_Montreal_Grey"), new CityConnection("SaultStMarie", "SaultStMarie_Toronto_Grey"), new CityConnection("Duluth", "Duluth_Toronto_Purple"), new CityConnection("Chicago", "Chicago_Toronto_White"), new CityConnection("Pittsburgh", "Toronto_Pittsburgh_Grey") });
        cityMap.Add("Montreal", new List<CityConnection> { new CityConnection("SaultStMarie", "SaultStMarie_Montreal_Black"), new CityConnection("Toronto", "Toronto_Montreal_Grey"), new CityConnection("NewYork", "Montreal_NewYork_Blue"), new CityConnection("Boston", "Montreal_Boston_Grey_1"), new CityConnection("Boston", "Montreal_Boston_Grey_2") });
        cityMap.Add("Boston", new List<CityConnection> { new CityConnection("NewYork", "Boston_NewYork_Yellow"), new CityConnection("NewYork", "Boston_NewYork_Red"), new CityConnection("Montreal", "Montreal_Boston_Grey_1"), new CityConnection("Montreal", "Montreal_Boston_Grey_2") });
        cityMap.Add("NewYork", new List<CityConnection> { new CityConnection("Boston", "Boston_NewYork_Yellow"), new CityConnection("Boston", "Boston_NewYork_Red"), new CityConnection("Montreal", "Montreal_NewYork_Blue"), new CityConnection("Pittsburgh", "Pittsburgh_NewYork_Green"), new CityConnection("Pittsburgh", "Pittsburgh_NewYork_White"), new CityConnection("Washington", "NewYork_Washington_Orange"), new CityConnection("Washington", "NewYork_Washington_Black") });
        cityMap.Add("Pittsburgh", new List<CityConnection> { new CityConnection("NewYork", "Pittsburgh_NewYork_Green"), new CityConnection("NewYork", "Pittsburgh_NewYork_White"), new CityConnection("Toronto", "Toronto_Pittsburgh_Grey"), new CityConnection("Chicago", "Chicago_Pittsburgh_Orange"), new CityConnection("Chicago", "Chicago_Pittsburgh_Black"), new CityConnection("SaintLouis", "SaintLouis_Pittsburgh_Green"), new CityConnection("Nashville", "Pittsburgh_Nashville_Yellow"), new CityConnection("Raleigh", "Pittsburgh_Raleigh_Grey"), new CityConnection("Washington", "Pittsburgh_Washington_Grey") });
        cityMap.Add("Washington", new List<CityConnection> { new CityConnection("NewYork", "NewYork_Washington_Orange"), new CityConnection("NewYork", "NewYork_Washington_Black"), new CityConnection("Pittsburgh", "Pittsburgh_Washington_Grey"), new CityConnection("Raleigh", "Washington_Raleigh_Grey_1"), new CityConnection("Raleigh", "Washington_Raleigh_Grey_2") });
        cityMap.Add("Raleigh", new List<CityConnection> { new CityConnection("Washington", "Washington_Raleigh_Grey_1"), new CityConnection("Washington", "Washington_Raleigh_Grey_2"), new CityConnection("Pittsburgh", "Pittsburgh_Raleigh_Grey"), new CityConnection("Nashville", "Nashville_Raleigh_Black"), new CityConnection("Atlanta", "Raleigh_Atlanta_Grey_1"), new CityConnection("Atlanta", "Raleigh_Atlanta_Grey_2"), new CityConnection("Charleston", "Raleigh_Charleston_Grey") });
        cityMap.Add("Atlanta", new List<CityConnection> { new CityConnection("Nashville", "Nashville_Atlanta_Grey"), new CityConnection("Raleigh", "Raleigh_Atlanta_Grey_1"), new CityConnection("Raleigh", "Raleigh_Atlanta_Grey_2"), new CityConnection("Charleston", "Atlanta_Charleston_Grey"), new CityConnection("Miami", "Atlanta_Miami_Blue"), new CityConnection("NewOrleans", "Atlanta_NewOrleans_Yellow"), new CityConnection("NewOrleans", "NewOrleans_Atlanta_Orange") });
        cityMap.Add("Charleston", new List<CityConnection> { new CityConnection("Atlanta", "Atlanta_Charleston_Grey"), new CityConnection("Raleigh", "Raleigh_Charleston_Grey"), new CityConnection("Miami", "Charleston_Miami_Purple") });
        cityMap.Add("Miami", new List<CityConnection> { new CityConnection("Charleston", "Charleston_Miami_Purple"), new CityConnection("Atlanta", "Atlanta_Miami_Blue"), new CityConnection("NewOrleans", "NewOrleans_Miami_Red") });
        cityMap.Add("NewOrleans", new List<CityConnection> { new CityConnection("Houston", "Houston_NewOrleans_Grey"), new CityConnection("LittleRock", "LittleRock_NewOrleans_Green"), new CityConnection("Atlanta", "Atlanta_NewOrleans_Yellow"), new CityConnection("Atlanta", "NewOrleans_Atlanta_Orange"), new CityConnection("Miami", "NewOrleans_Miami_Red") });
        cityMap.Add("Nashville", new List<CityConnection> { new CityConnection("SaintLouis", "SaintLouis_Nashville_Grey"), new CityConnection("LittleRock", "LittleRock_Nashville_White"), new CityConnection("Atlanta", "Nashville_Atlanta_Grey"), new CityConnection("Raleigh", "Nashville_Raleigh_Black"), new CityConnection("Pittsburgh", "Pittsburgh_Nashville_Yellow") });
    }

    public void CreateWeights()
    {
        foreach (var city in cityMap)
        {
            string cityName = city.Key;
            List<CityConnection> connections = city.Value;
            foreach (var connection in connections)
            {
                connection.weight = GetRouteLength(connection.routeName);
                //Debug.Log(connection.routeName + ": " +  connection.RouteLength());
                //Debug.Log(connection.routeName + ": " +  GetRouteLength(connection.routeName));
            }
        }
    }

    public string GetRouteColor(string routeName)
    {
        GameObject route = GameObject.Find(routeName);
        return route.GetComponent<RouteScript>().routeColor; 
    }

    public PlayerScript GetRouteOwner(string routeName)
    {
        GameObject route = GameObject.Find(routeName);
        return route.GetComponent<RouteScript>().owner; 
    }

    public int GetRouteLength(string routeName)
    {
        GameObject route = GameObject.Find(routeName);
        return route.GetComponent<RouteScript>().routeLength; 
    }

    public bool GetRouteIsOccupied(string routeName)
    {
        GameObject route = GameObject.Find(routeName);
        return route.GetComponent<RouteScript>().occupied; 
    } 
  
    public void CalculateTotalScore()
    {
        CreateWeights();   //--------------------------------> wo anders einf�gen

        //find Length of the longest Route
        int longestRoute = players.Max(player => player.longestRouteDistance);

        // iterate through players and calculate the final score
        foreach (PlayerScript p in players)
        {
            //ShortestPathFinder shortestPathFinder = new ShortestPathFinder(cityMap);    ---> zum Testen
            //Dictionary<string, int> shortestPathList = shortestPathFinder.DijkstraReduceWeightsOfOwnRoutes("Dallas", p);    ---> zum Testen
            //Debug.Log(p.playerName + ":  from Dallas to LitteRock: " +  shortestPathList["LittleRock"]);    ---> zum Testen

            //Increase the Score of all players with the longest route by 10 points
            if (p.longestRouteDistance == longestRoute) 
            {
                p.score += 10;
            }
            
            //Schleife die durch alle Zielkarten iteriert
            for (int d=0; d < p.handdestinationcards.Count; d++)
            {
                if (CheckCityConnection(p.handdestinationcards[d].citiesAndPoints[0], p.handdestinationcards[d].citiesAndPoints[1], p))
                {
                    Debug.Log("Zielkarte abgeschlossen: " + p.handdestinationcards[d].citiesAndPoints[0] + 
                        " nach " + p.handdestinationcards[d].citiesAndPoints[1]);
                    p.score += int.Parse(p.handdestinationcards[d].citiesAndPoints[2]);
                } else
                {
                    Debug.Log("Zielkarte nicht abgeschlossen: " + p.handdestinationcards[d].citiesAndPoints[0] +
                        " nach " + p.handdestinationcards[d].citiesAndPoints[1]);
                    p.score -= int.Parse(p.handdestinationcards[d].citiesAndPoints[2]);
                }
            }           

        }

        //if two or more players have an equal score, the winner have to determine on the basis of further criteria
        DetermineWinner();

        foreach (PlayerScript p in players)
        {
            //Debug.Log("Spieler: " + p.playerName + "; Score: " + p.score);
        }

        // Aufruf des Endmen�s
        resultTable.SetActive(true);
        for (int i=0; i<players.Count; i++)
        {
            resultTable.transform.GetChild(resultTable.transform.childCount - 3).GetChild(i).gameObject.SetActive(true);
            resultTable.transform.GetChild(resultTable.transform.childCount - 3).GetChild(i).GetChild(0).GetComponent<Text>().text = players[i].playerName;
            resultTable.transform.GetChild(resultTable.transform.childCount - 3).GetChild(i).GetChild(1).GetComponent<Text>().text = players[i].score.ToString();

            if (players[i] == winner)
                resultTable.transform.GetChild(resultTable.transform.childCount - 3).GetChild(i).GetChild(2).gameObject.SetActive(true);
        }
    }
    
    public void DetermineWinner()
    {
        // get highest score for comparison later
        int highestScore = players.Max(player => player.score);
        // get players with most points
        List<PlayerScript> playersWithHighestScore = new List<PlayerScript>();
        // Hier k�nnten Sie Logik hinzuf�gen, um die Liste zu erstellen
        List<PlayerScript> playersWithMostCompletedCards = new List<PlayerScript>();

        //Debug.Log("Players: " + players.Count);
        //FIRST CONDITION: Determine the players with the highest score
        foreach (PlayerScript p in players)
        {
            if (p.score == highestScore)
            {
                playersWithHighestScore.Add(p);
            }
        }
        //Debug.Log("Players1: " + playersWithHighestScore.Count);
        //SECOND CONDITION: if this list contains more than 1 player, the players have the same amount of point -> check for most completed destination cards
        if (playersWithHighestScore.Count > 1)
        {
            playersWithMostCompletedCards = GetPlayersWithMostCompletedCards(playersWithHighestScore);
        }
        else 
        {
            winner = playersWithHighestScore[0];
        }
        //Debug.Log("Players2: " + playersWithMostCompletedCards.Count);
        //THIRD CONDITION: if this list contains more than 1 player, the players have the same amount of completed cards -> check for longest route of these players
        if (playersWithMostCompletedCards.Count > 1) //if no player completed a destination card, the winner is the player with the longest route
        {
            int longestRouteDistanceOverAll = playersWithMostCompletedCards.Max(player => player.longestRouteDistance);
            winner = playersWithMostCompletedCards.FirstOrDefault(player => player.longestRouteDistance == longestRouteDistanceOverAll);

        }
        else if (playersWithMostCompletedCards.Count == 0)
        {
            int longestRouteDistanceOverAll = playersWithHighestScore.Max(player => player.longestRouteDistance);
            winner = playersWithHighestScore.FirstOrDefault(player => player.longestRouteDistance == longestRouteDistanceOverAll);
        }
        else 
        {
            winner = playersWithMostCompletedCards[0];
        }
    }

    public List<PlayerScript> GetPlayersWithMostCompletedCards(List<PlayerScript> playersWithHighestScore)
    {
        // Hier k�nnten Sie Logik hinzuf�gen, um die Liste zu erstellen
        List<PlayerScript> playersWithMostCompletedCards = new List<PlayerScript>();

        // another array to count the completed cards of each player
        int[] completedDestinationCardsCount = new int[playersWithHighestScore.Count];
        for (int i=0; i < playersWithHighestScore.Count; i++)
        {
            int counter = 0;
            for (int d = 0; d < playersWithHighestScore[i].handdestinationcards.Count; d++) 
            { 
                if (CheckCityConnection(playersWithHighestScore[i].handdestinationcards[d].citiesAndPoints[0], 
                    playersWithHighestScore[i].handdestinationcards[d].citiesAndPoints[1], playersWithHighestScore[i]))
                {
                    counter++;
                }
            }
            completedDestinationCardsCount[i] = counter;
        }

        // get highest number of completed cards for comparison later
        int mostCompletedCards = completedDestinationCardsCount.Max();
        for (int i = 0; i < playersWithHighestScore.Count; i++)
        {
            if (completedDestinationCardsCount[i] == mostCompletedCards)
            {
                playersWithMostCompletedCards.Add(playersWithHighestScore[i]);
            }
        }

        return playersWithMostCompletedCards;
    }

    // return true if the destinationcard is fulfilled
    public bool CheckCityConnection(string city1, string city2, PlayerScript player)
    {
        CreateWeights();
        ShortestPathFinder shortestPathFinder = new ShortestPathFinder(cityMap);
        Dictionary<string, int> shortestPathList = shortestPathFinder.DijkstraReduceWeightsOfOwnRoutes(city1, player);
        
        int shortestPathValue = 1000;
        
        if (shortestPathList.ContainsKey(city2))
        {
            shortestPathValue = shortestPathList[city2];
        }
        else
        {
             Debug.Log("Schluessel ist nicht vorhanden: " + city2);  //zur �berpr�fung, keine Relevanz f�r das Spiel
        }
        
        //Debug.Log(" K�rzeste Weg von " + city1 + " nach " + city2 + " ist:  " + shortestPathValue);

        //if the value of the shortest path is 0, the two cities are connected by routes of the selected player
        if (shortestPathValue == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // return false if destination card is not more reachable
    public bool CheckIfCityConnectionIsReachable(string city1, string city2, PlayerScript player)
    {
        CreateWeights();
        ShortestPathFinder shortestPathFinder = new ShortestPathFinder(cityMap);
        Dictionary<string, int> shortestPathList = shortestPathFinder.DijkstraReduceWeightsOfOwnRoutes(city1, player);

        int shortestPathValue = 1000;

        if (shortestPathList.ContainsKey(city2))
        {
            shortestPathValue = shortestPathList[city2];
        }
        else
        {
            Debug.Log("Schluessel ist nicht vorhanden: " + city2);  //zur �berpr�fung, keine Relevanz f�r das Spiel
        }

        //check if the wagons are enough to aquire the route 
        if (shortestPathValue > player.availableWagons)
        {
            return false;
        }

        //Debug.Log(" K�rzeste Weg von " + city1 + " nach " + city2 + " ist:  " + shortestPathValue);

        //if the value of the shortest path is 0, the two cities are connected by routes of the selected player
        if (shortestPathValue == int.MaxValue)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public float CalculateAveragePointsOfRoutes()
    {
        int sum = 0;
        foreach (string route in activePlayer.acquiredRoutes)
        {
            sum += pointsOfRouteLength[GetRouteLength(route)];
        }

        if (activePlayer.acquiredRoutes.Count == 0)
            return 0;
        else
            return (float) sum / activePlayer.acquiredRoutes.Count;
    } 

    public void GoToMenu()
    {
        if (foo != null)
            foo.Kill();
        SceneManager.LoadScene(1);
    }
    
    public void Restart()
    {
        if (foo != null)
            foo.Kill();
        SceneManager.LoadScene(0);
    }

    // this method returns a list of route names which are a part of the shortest path between the given cities  
    public List<string> GetShortestPathEdges(string city1, string city2, PlayerScript player)
    {
        List<string> edges = new List<string>();   
        
        ShortestPathFinder shortestPathFinder = new ShortestPathFinder(cityMap);
        Dictionary<string, string> predecessorsList = shortestPathFinder.DijkstraWithPredecessors(city1, player);

        string currentCity = city2;
        string predecessors = predecessorsList[city2];

        while(currentCity != city1)
        {
            CityConnection foundConnection = cityMap[currentCity].FirstOrDefault(c => c.city == predecessors);
            edges.Add(foundConnection.routeName);

            currentCity = predecessors;
            predecessors = predecessorsList[currentCity];
        }

        return edges;
    }

    public int ElectDestinationCardsInGame()
    {
        List<int> valuesOfShortestPathes = new List<int>();
        List<DestinationCardScript> drawnDestinationCards = new List<DestinationCardScript>();
        for (int i = 0; i < destinationcardsDrawDisplay.transform.childCount; i++)
            drawnDestinationCards.Add(destinationcardsDrawDisplay.transform.GetChild(i).GetComponent<DestinationCardScript>());       

        foreach(DestinationCardScript d in drawnDestinationCards)
        {
            ShortestPathFinder shortestPathFinder = new ShortestPathFinder(cityMap);
            Dictionary<string, int> shortestPaths = shortestPathFinder.DijkstraReduceWeightsOfOwnRoutes(d.citiesAndPoints[0], activePlayer);
            valuesOfShortestPathes.Add(shortestPaths[d.citiesAndPoints[1]]);
        }

        int position = 0;
        int minValue = valuesOfShortestPathes[0];

        for (int i = 0; i < valuesOfShortestPathes.Count; i++)
        {
            if (valuesOfShortestPathes[i] < minValue)
            {
                minValue = valuesOfShortestPathes[i];
                position = i;
            }
            else if (valuesOfShortestPathes[i] == minValue)
            {
                if (drawnDestinationCards[i].points > drawnDestinationCards[position].points )
                {
                    position = i;
                }
            }
        }

        return position;
    }

    //TODO: Diese Methode muss noch in die Spiellogik integriert werden
    //the method determine the destinationscards that are to pick up by the active player
    public List<int> ElectDestinationcardsAtStartMostOverlap()
    {
        List<int> destinationCardsPositions = new List<int>();
        List<string>[] arrayOfEdgeLists = new List<string>[3];

        List<DestinationCardScript> drawnDestinationCards = new List<DestinationCardScript>();
        for (int i = 0; i < destinationcardsDrawDisplay.transform.childCount; i++)
            drawnDestinationCards.Add(destinationcardsDrawDisplay.transform.GetChild(i).GetComponent<DestinationCardScript>());

        //determine routes of the shortest path between the citys of the destination cards
        arrayOfEdgeLists[0] = GetShortestPathEdges(drawnDestinationCards[0].citiesAndPoints[0], drawnDestinationCards[0].citiesAndPoints[1], activePlayer);
        arrayOfEdgeLists[1] = GetShortestPathEdges(drawnDestinationCards[1].citiesAndPoints[0], drawnDestinationCards[1].citiesAndPoints[1], activePlayer);
        arrayOfEdgeLists[2] = GetShortestPathEdges(drawnDestinationCards[2].citiesAndPoints[0], drawnDestinationCards[2].citiesAndPoints[1], activePlayer);

        // variables contain the number of common edges
        int pair1 = arrayOfEdgeLists[0].Intersect(arrayOfEdgeLists[1]).ToList().Count;
        int pair2 = arrayOfEdgeLists[0].Intersect(arrayOfEdgeLists[2]).ToList().Count;
        int pair3 = arrayOfEdgeLists[1].Intersect(arrayOfEdgeLists[2]).ToList().Count;

        int thresholdValue = 1;
        //select all destination cards that have more common routes than the amount of thresholdValue 
        if (pair1 > thresholdValue)
        {
            destinationCardsPositions.Add(0);
            destinationCardsPositions.Add(1);
        }
        if (pair2 > thresholdValue)
        {
            destinationCardsPositions.Add(0);
            destinationCardsPositions.Add(2);
        }
        if (pair3 > thresholdValue)
        {
            destinationCardsPositions.Add(1);
            destinationCardsPositions.Add(2);
        }

        //if there are no common routes select the destination cards 1 and 2
        if (destinationCardsPositions.Count == 0)
        {
            destinationCardsPositions.Add(0);
            destinationCardsPositions.Add(1);
        }

        // remove all duplicate Elements:
        destinationCardsPositions = destinationCardsPositions.Distinct().ToList();

        return destinationCardsPositions;
    }

    public List<int> ElectDestinationcardsAtStartLowestPoints()
    {
        List<int> selectedDestinationCardsPositions = new List<int>();
        List<DestinationCardScript> drawnDestinationCards = new List<DestinationCardScript>();
        for (int i = 0; i < destinationcardsDrawDisplay.transform.childCount; i++)
            drawnDestinationCards.Add(destinationcardsDrawDisplay.transform.GetChild(i).GetComponent<DestinationCardScript>());

        DestinationCardScript cardWithHighestPoints = drawnDestinationCards.OrderByDescending(card => card.points).FirstOrDefault();
        int positionOfCardWithHighestPoints = drawnDestinationCards.IndexOf(cardWithHighestPoints);

        for (int i = 0; i < drawnDestinationCards.Count; i++)
        {
            if (i != positionOfCardWithHighestPoints)
                selectedDestinationCardsPositions.Add(i);
        }

        return selectedDestinationCardsPositions;
    }

    // 
    public int CheckNumberOfAquerizedRoutes(string city1, string city2)
    {
        int number = 0;
        List<string> edges = GetShortestPathEdges("","",activePlayer);

        return number;
    }

    //return a list with all routeOptions
    public List<RouteOption> ReturnRouteOptionList(PlayerScript player)
    {
        Dictionary<string, int> edgesOfAllPaths = ReturnEdgesOfPaths(player);

        return GenerateRouteOptionsList(player, edgesOfAllPaths);
    }

    public Dictionary<string, int> ReturnEdgesOfPaths(PlayerScript player)
    {
        Dictionary<string, int> edgesOfAllPaths = new Dictionary<string, int>();
        List<string> acquiredRoutes = player.acquiredRoutes;
        CreateWeights();
        foreach (DestinationCard destinationcard in player.handdestinationcards)
        {
            ShortestPathFinder shortestPathFinder = new ShortestPathFinder(cityMap);
            //find the shortest path from city1 (citiesAndPoints[0]) to city2 (citiesAndPoints[1]) and get back the predecessors
            Dictionary<string, string> predecessorsList = shortestPathFinder.DijkstraReduceWeightsOfOwnRoutesAndReturnsPredecessors(destinationcard.citiesAndPoints[0], player);
            //determine the edges of the shortest way from city1 to city2
            List<string> edges = GetPathEdges(destinationcard.citiesAndPoints[0], destinationcard.citiesAndPoints[1], player, predecessorsList);
            //find the shortest path from city1 (citiesAndPoints[0]) to city2 (citiesAndPoints[1]) and get back the predecessors by reducing the costs with the cards on hands
            ShortestPathFinder shortestPathFinder1 = new ShortestPathFinder(cityMap);
            Dictionary<string, string> predecessorsList1 = shortestPathFinder1.DijkstraReduceWeightsOfOwnRoutesAndConsideredHandcardsAndReturnsPredecessors(destinationcard.citiesAndPoints[0], player);
            //determine the edges of the shortest way from city1 to city2 by reducing the costs with the cards on hands
            List<string> edges1 = GetPathEdges(destinationcard.citiesAndPoints[0], destinationcard.citiesAndPoints[1], player, predecessorsList1);
            // add all elements from edges and edges1 to edgesOfAllPaths:
            foreach (string edge in edges)
            {
                if (!edgesOfAllPaths.ContainsKey(edge))
                    edgesOfAllPaths.Add(edge, destinationcard.points);
            }
            foreach (string edge in edges1)
            {
                if (!edgesOfAllPaths.ContainsKey(edge))
                    edgesOfAllPaths.Add(edge, destinationcard.points);
            }
        }

        List<string> keysToRemove = edgesOfAllPaths
            .Where(kvp => acquiredRoutes.Contains(kvp.Key))
            .Select(kvp => kvp.Key)
            .ToList();

        // Entferne die identifizierten Schl�ssel aus dem Dictionary
        foreach (string key in keysToRemove)
        {
            edgesOfAllPaths.Remove(key);
        }

        return edgesOfAllPaths;
    }

    //return a list with all routeOptions
    //use a list with all edges to create routeOptions
    public List<RouteOption> GenerateRouteOptionsList(PlayerScript player, Dictionary<string, int> edgesOfAllPaths)
    { 
        List<RouteOption> routeOptionList = new List<RouteOption>();
        
        foreach (string edgeName in edgesOfAllPaths.Keys)
        {
            GameObject route = GameObject.Find(edgeName);
            RouteScript routescript = route.GetComponent<RouteScript>();

            string routeColor = routescript.routeColor;
            string nameOfRoute = edgeName;
            int destinationCardPoints = edgesOfAllPaths[edgeName];
            int lengthOfRoute = routescript.routeLength;
            int trainCards = player.handtraincards.Count(card => card.color == routeColor);
            trainCards += player.handtraincards.Count(card => card.color == "Joker");
            int faceUpCards = faceupTraincards.Count(card => card.color == routeColor);
            faceUpCards += faceupTraincards.Count(card => card.color == "Joker");

            if (routeColor == "Grey")
            {
                if (player.handtraincards.Count != 0)
                {
                    // Gruppiere die Karten nach Farbe und z�hle die Anzahl jeder Farbe
                    var colorCounts = player.handtraincards.GroupBy(card => card.color)
                        .Select(group => new { color = group.Key, Count = group.Count() })
                        .OrderByDescending(item => item.Count);

                    // Die Farbe mit der h�chsten Anzahl ist die erste in der sortierten Sequenz
                    string mostFrequentColor = colorCounts.First().color;
                    trainCards = player.handtraincards.Count(card => card.color == mostFrequentColor);
                    trainCards += player.handtraincards.Count(card => card.color == "Joker");
                    faceUpCards = faceupTraincards.Count(card => card.color == mostFrequentColor);
                    faceUpCards += faceupTraincards.Count(card => card.color == "Joker");
                }
            }        

            routeOptionList.Add(new RouteOption(destinationCardPoints, faceUpCards, lengthOfRoute, nameOfRoute, trainCards, routeColor));
        }

        //ConsoleLogRouteOptions(player, routeOptionList);

        return routeOptionList;
    }

    public void ConsoleLogRouteOptions(PlayerScript player, List<RouteOption> routeOptionList)
    {
        foreach(RouteOption routeOption in routeOptionList)
        {
            Debug.Log(player.playerName + ", Option " + (routeOptionList.IndexOf(routeOption)+1) + ": (" 
                + "nameOfRoute: " + routeOption.nameOfRoute 
                + ", lengthOfRoute: " + routeOption.lengthOfRoute 
                + ", trainCards: " + routeOption.trainCards
                + ", faceUpCards: " + routeOption.faceUpCards 
                + ", destinationCardPoints: " + routeOption.destinationCardPoints + ")");
        }
    }

    // this method returns a list of route names which are a part of the shortest path between the given cities  
    public List<string> GetPathEdges(string city1, string city2, PlayerScript player, Dictionary<string, string> predecessorsList)
    {
        List<string> edges = new List<string>();

        string currentCity = city2;
        string predecessors = predecessorsList[city2];

        while(currentCity != city1)
        {
            if (predecessorsList[city2] == null)
            {
                Debug.Log("Predecessor form " + currentCity + " is null!");
                return edges;
            }


            CityConnection foundConnection = cityMap[currentCity].FirstOrDefault(c => c.city == predecessors);
            edges.Add(foundConnection.routeName);

            currentCity = predecessors;
            predecessors = predecessorsList[currentCity];
        }

        return edges;
    }

    public void QuitGame()
    {
        if (foo != null)
            foo.Kill();
        EditorApplication.ExecuteMenuItem("Edit/Play");
    }
}