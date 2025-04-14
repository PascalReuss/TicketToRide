using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Usage of Sum()

public class GameState : MonoBehaviour
{
    // **MoveHistory**: Stores the history of moves made during the game.
    public MoveHistory MoveHistory { get; private set; } = new MoveHistory();

    // **Card Deck**: Stores the count of each card color in the deck.
    public Dictionary<string, int> CardDeck = new Dictionary<string, int>
    {
        { "Black", 0 },
        { "Blue", 0 },
        { "Green", 0 },
        { "Orange", 0 },
        { "Purple", 0 },
        { "Red", 0 },
        { "White", 0 },
        { "Yellow", 0 },
        { "Joker", 0 }
    };

    // **cityMap**: Reference to the original cityMap, modifications will affect the original data.
    public Dictionary<string, List<CityConnection>> cityMap { get; set; }

    // **OpenCards**: List of cards that are currently open
    public List<string> OpenCards { get; set; } = new List<string>();

    // **Destination Cards**: Stores destination cards for each player.
    public Dictionary<DestinationCard, string> DestinationCardsOfPlayers { get; set; } = new Dictionary<DestinationCard, string>();

    // List of drawn cards, tracks how many of each card color have been drawn
    public Dictionary<string, int> DrawnCards = new Dictionary<string, int>();

    // **Turn Counter**: Tracks the current game turn.
    public int TurnCounter { get; set; }

    // **Player**: The active player Object
    public PlayerScript Player { get; set; }

    // **pointsOfRouteLength**: The points of the routes length
    public Dictionary<int, int> pointsOfRouteLength { get; set; }
    // **Player Information**: List of all players in the game.
    public List<PlayerScript> PlayerList { get; set; } = new List<PlayerScript>();

    /// <summary>
    /// Creates a deep copy of the cityMap dictionary.
    /// Each CityConnection object within the cityMap is copied to ensure the original map is not modified.
    /// </summary>
    /// <returns>
    /// A new dictionary containing copies of the CityConnection objects from the original cityMap.
    /// </returns>
    public Dictionary<string, List<CityConnection>> copyCityMap()
    {
        var copy = new Dictionary<string, List<CityConnection>>();

        foreach (var entry in cityMap)
        {
            var connectionsCopy = new List<CityConnection>();

            foreach (var conn in entry.Value)
            {
                var newConnection = new CityConnection(conn.city, conn.routeName)
                {
                    weight = conn.weight,          // copy weight
                    routeValue = conn.routeValue   // copy routeValue
                };
                connectionsCopy.Add(newConnection);
            }

            copy.Add(entry.Key, connectionsCopy);
        }

        return copy;
    }
    public List<PlayerScript> getAllOtherPlayers()
    {
        List<PlayerScript> otherPlayers = new List<PlayerScript>(PlayerList);
        otherPlayers.Remove(Player);// removes active Player
        return otherPlayers;
    }
    public List<string> getPlayerCities(PlayerScript player)
    {
        List<string> targetCities = getPlayerTargetCities(player);
        List<string> playerCities = new List<string>();
        foreach (var city in targetCities)
            playerCities.Add(city);
        foreach (var city in player.cities)
            playerCities.Add(city);

        return playerCities;
    }

    public List<string> getAllAcquiredEnemyRoutes()
    {
        List<string> blockedRoutes = new List<string>();
        foreach (var player in getAllOtherPlayers())
        {
            blockedRoutes.AddRange(player.acquiredRoutes);
        }
        
        if (PlayerList.Count == 2) // add double connections to the list on a two player game
        {
            List<string> newRoutes = new List<string>(); // temp
            foreach (string route in blockedRoutes)
            {
                string[] part = route.Split('_');
                if(part.Length == 4)
                {
                    if (part[3] == "1")
                    {
                        newRoutes.Add(part[0] + "_" + part[1] + "_" + part[2] + "_2");
                    }
                    if (part[3] == "2")
                    {
                        newRoutes.Add(part[0] + "_" + part[1] + "_" + part[2] + "_1");
                    }
                }
            }
            blockedRoutes.AddRange(newRoutes);
        }
        return blockedRoutes;
    }

    /// <summary>
    /// Returns a list of target cities that the player must connect based on their destination cards.
    /// Each destination card contains two cities that are considered target cities.
    /// </summary>
    /// <param name="player">The player whose target cities are to be retrieved.</param>
    /// <returns>
    /// A list of cities the player needs to connect based on their destination cards.
    /// </returns>
    public List<string> getPlayerTargetCities(PlayerScript player)
    {
        List<DestinationCard> playerDestCards = player.handdestinationcards;
        List<string> targetCities = new List<string>();

        foreach (var card in playerDestCards)
        {
            targetCities.Add(card.citiesAndPoints[0]);
            targetCities.Add(card.citiesAndPoints[1]);
        }
        return targetCities;
    }

    /// <summary>
    /// Creates a list of all free (unoccupied) routes.
    /// </summary>
    /// <returns>
    /// A list of strings containing the names of all unoccupied routes.
    /// </returns>
    public List<string> getFreeRoutes()
    {
        List<string> routesFree = new List<string>();
        foreach (var city in cityMap)
        {
            List<CityConnection> connections = city.Value;
            foreach (var connection in connections)
            {
                if (!isRouteOccupied(connection.routeName))
                {
                    routesFree.Add(connection.routeName);
                }
            }
        }
        return routesFree;
    }

    /// <summary>
    /// Checks if a specific route is already occupied.
    /// </summary>
    /// <param name="routeName">The name of the route to check.</param>
    /// <returns>
    /// <c>true</c> if the route is occupied; otherwise, <c>false</c>.
    /// </returns>
    private bool isRouteOccupied(string routeName)
    {
        GameObject route = GameObject.Find(routeName);
        return route.GetComponent<RouteScript>().occupied;
    }

    /// <summary>
    /// Retrieves the owner of a specific route.
    /// </summary>
    /// <param name="routeName">The name of the route whose owner is to be determined.</param>
    /// <returns>
    /// A <see cref="PlayerScript"/> object representing the owner of the route.
    /// </returns>
    private PlayerScript GetRouteOwner(string routeName)
    {
        GameObject route = GameObject.Find(routeName);
        return route.GetComponent<RouteScript>().owner;
    }

    /// <summary>
    /// Creates a dictionary of all occupied routes and their respective owners.
    /// </summary>
    /// <returns>
    /// A dictionary where the keys are the names of the occupied routes, 
    /// and the values are the corresponding owners as <see cref="PlayerScript"/> objects.
    /// </returns>
    public Dictionary<string, PlayerScript> getRoutesOwner()
    {
        Dictionary<string, PlayerScript> routesOwner = new Dictionary<string, PlayerScript>();

        foreach (var city in cityMap)
        {
            List<CityConnection> connections = city.Value;
            foreach (var connection in connections)
            {
                if (isRouteOccupied(connection.routeName))
                {
                    PlayerScript owner = GetRouteOwner(connection.routeName);
                    // check if the route is already in the dict as Key
                    if (!routesOwner.ContainsKey(connection.routeName))
                    {
                        routesOwner.Add(connection.routeName, owner);
                        Debug.Log("The Weight: " + connection.weight);
                    }
                }
            }
        }
        return routesOwner;
    }

    /// <summary>
    /// Adds a move to the move history for a player.
    /// </summary>
    /// <param name="playerName">The name of the player performing the action.</param>
    /// <param name="action">The action the player performs (e.g., drawing a card).</param>
    /// <param name="cardColor">The color of the card drawn (if applicable).</param>
    public void AddMove(PlayerScript player, string action ,string cardColor, string route)
    {
        var move = new Move(
            TurnCounter,
            GetCardDeck(),
            GetDrawnCards(),
            GetDestinationCardsOfPlayer(player.playerName),
            player,
            action,
            cardColor, 
            route
        );
        MoveHistory.AddMove(move);
    }

    /// <summary>
    /// Returns all the moves for a specific turn.
    /// </summary>
    /// <param name="turnCounter">The turn number to retrieve moves for.</param>
    /// <returns>A list of moves performed during the specified turn.</returns>
    public List<Move> GetMovesForTurn(int turnCounter)
    {
        return MoveHistory.GetMovesForTurn(turnCounter); // Get all moves for the specified turn.
    }

    /// <summary>
    /// Returns a copy of the player list.
    /// </summary>
    /// <returns>A list containing the names of all players.</returns>
    public List<PlayerScript> GetPlayerList()
    {
        return new List<PlayerScript>(PlayerList); // Returns a copy of the player list to avoid direct manipulation.
    }

    /// <summary>
    /// Adds a player to the player list if not already present.
    /// </summary>
    /// <param name="name">The name of the player to be added.</param>
    public void AddPlayerToList(PlayerScript name)
    {
        if (!PlayerList.Contains(name)) // Prevents adding duplicate players.
        {
            PlayerList.Add(name);
        }
    }

    /// <summary>
    /// Retrieves a dictionary representing the player's hand cards,
    /// where the key is the card color and the value is the count of cards of that color.
    /// </summary>
    /// <param name="player">The player whose hand cards are being processed.</param>
    /// <returns>A dictionary mapping card colors to their respective counts in the player's hand.</returns>
    public Dictionary<string, int> GetPlayerHandCards(PlayerScript player)
    {
        Dictionary<string, int> handCards = new Dictionary<string, int>();

        // Iterate over all of the activeplayer's cards
        foreach (TrainCard card in player.handtraincards)
        {
            string color = card.GetColor();

            // Check, if the color already exists in the dictionary
            if (handCards.ContainsKey(color))
            {
                handCards[color]++; // increase counter for that color
            }
            else
            {
                handCards[color] = 1; // add color with amount 1
            }
        }
        
        // Sort the dictionary so that "Joker" comes first
        Dictionary<string, int> sortedHandCards = handCards
            .OrderByDescending(pair => pair.Key == "Joker") // Joker zuerst
            .ThenBy(pair => pair.Key) // Dann alphabetisch
            .ToDictionary(pair => pair.Key, pair => pair.Value);
        return sortedHandCards;
    }

    /// <summary>
    /// Returns the current card deck.
    /// </summary>
    /// <returns>A dictionary representing the card deck and its counts.</returns>
    public Dictionary<string, int> GetCardDeck()
    {
        return CardDeck; // Returns the current state of the card deck.
    }

    /// <summary>
    /// Resets the card deck by setting all card counts to zero.
    /// </summary>
    public void ClearDeck()
    {
        var keys = CardDeck.Keys.ToList();
        foreach (var key in keys)
        {
            CardDeck[key] = 0; // Reset all card counts to zero.
        }
    }

    /// <summary>
    /// Refills the deck by incrementing the count of a specific card color.
    /// </summary>
    /// <param name="color">The color of the card to increment.</param>
    public void RefillDeck(string color)
    {
        if (CardDeck.ContainsKey(color))
        {
            CardDeck[color]++; // Increment the count for the specified color.
        }
    }

    /// <summary>
    /// Returns the total number of cards in the deck.
    /// </summary>
    /// <returns>The total count of all cards in the deck.</returns>
    public int GetTotalCardsInDeck()
    {
        return CardDeck.Values.Sum(); // Sum the values of all cards in the deck.
    }

    /// <summary>
    /// Returns the number of cards of a specific color in the deck.
    /// </summary>
    /// <param name="color">The color of the cards to query.</param>
    /// <returns>The number of cards of the specified color.</returns>
    public int GetTotalCardsInDeckByColor(string color)
    {
        return CardDeck.ContainsKey(color) ? CardDeck[color] : 0; // Return the count for the specified color or 0 if not present.
    }

    /// <summary>
    /// Draws a card of a specified color from the deck.
    /// </summary>
    /// <param name="color">The color of the card to draw.</param>
    public void DrawCard(string color)
    {
        if (CardDeck.ContainsKey(color) && CardDeck[color] > 0)
        {
            CardDeck[color]--; // Decrease the card count for the specified color.
            if (DrawnCards.ContainsKey(color))
            {
                DrawnCards[color]++; // Increment the count for drawn cards of this color.
            }
            else
            {
                DrawnCards[color] = 1; // Initialize the count for the color if it doesn't exist.
            }
        }
    }

    /// <summary>
    /// Returns the dictionary of drawn cards.
    /// </summary>
    /// <returns>A dictionary of drawn cards and their counts.</returns>
    public Dictionary<string, int> GetDrawnCards()
    {
        return new Dictionary<string, int>(DrawnCards); // Return a copy of the drawn cards dictionary.
    }

    /// <summary>
    /// Returns the destination cards for a specified player.
    /// </summary>
    /// <param name="playerName">The name of the player to retrieve destination cards for.</param>
    /// <returns>A dictionary of destination cards for the player.</returns>
    public Dictionary<DestinationCard, string> GetDestinationCardsOfPlayer(string playerName)
    {
        Dictionary<DestinationCard, string> playerDestinationCards = new Dictionary<DestinationCard, string>();

        // Iterate through the destination cards and add those belonging to the specified player.
        foreach (var entry in DestinationCardsOfPlayers)
        {
            if (entry.Value == playerName)
            {
                playerDestinationCards.Add(entry.Key, entry.Value); // Add the destination card for the player.
            }
        }

        return playerDestinationCards; // Return the dictionary of the player's destination cards.
    }

    // **Debugging Methods**

    /// <summary>
    /// Logs the current state of the card deck to the console.
    /// </summary>
    public void LogCardDeck()
    {
        string logOutput = "The card deck contents:\n";
        foreach (var card in CardDeck)
        {
            logOutput += $"{card.Key}: {card.Value}\n"; // Log each card and its count.
        }
        Debug.Log(logOutput);
    }

    /// <summary>
    /// Logs all destination cards assigned to players.
    /// </summary>
    public void LogDestinationCardsOfPlayers()
    {
        if (DestinationCardsOfPlayers.Count == 0)
        {
            Debug.Log("No destination cards found for any players.");
            return;
        }

        string output = "";
        foreach (var entry in DestinationCardsOfPlayers)
        {
            DestinationCard card = entry.Key;
            string player = entry.Value;
            output += ($"Player: {player}, Destination Card: {card.citiesAsString} ({card.points})\n");
        }
        Debug.Log(output);
    }

    /// <summary>
    /// Logs the drawn cards to the console.
    /// </summary>
    public void LogDrawnCards()
    {
        string logOutput = "Drawn Cards Contents:\n";
        foreach (var card in DrawnCards)
        {
            logOutput += $"{card.Key}: {card.Value}\n"; // Log each drawn card and its count.
        }
        Debug.Log(logOutput);
    }

    /// <summary>
    /// Logs all moves in the game history to the console.
    /// </summary>
    public void LogMoves()
    {
        var allMoves = MoveHistory.GetAllMoves();
        string log = "";
        foreach (var move in allMoves)
        {
            log += $"Turn {move.TurnCounter}: Player {move.Player.playerName} performed action '{move.Action}' {move.CardColor} {move.Route}\n";
        }
        Debug.Log(log);
    }

    /// <summary>
    /// Logs the current state of the open cards list to the console.
    /// </summary>
    public void LogOpenCards()
    {
        string log = "";
        if (OpenCards == null || OpenCards.Count == 0)
        {
            Debug.Log("The list of open cards is empty.");
        }
        else
        {
            log += "Current open cards:\n";
            for (int i = 0; i < OpenCards.Count; i++)
            {
                string card = OpenCards[i];
                log += $"Index {i}: {card}\n";
            }
        }
        Debug.Log(log);
    }

    /// <summary>
    /// Logs all moves by a specific player to the console.
    /// </summary>
    public void LogMovesForPlayer(string player)
    {
        foreach (var move in MoveHistory.GetMovesForPlayer(player))
        {
            Debug.Log($"Turn {move.TurnCounter}: Player {move.Player.playerName} performed action '{move.Action}' " + move.CardColor + " " + move.Route);
        }
    }

    /// <summary>
    /// Logs all the moves for a specified turn to the console.
    /// </summary>
    /// <param name="turnCounter">The turn number to log moves for.</param>
    public void LogMovesForTurn(int turnCounter)
    {
        var movesForTurn = MoveHistory.GetMovesForTurn(turnCounter);

        foreach (var move in movesForTurn)
        {
            Debug.Log($"Turn {move.TurnCounter}: Player {move.Player.playerName} performed action '{move.Action}'");
        }
    }
}