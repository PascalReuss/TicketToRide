using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EvilAI
{
    // **MetaLayer**: A reference to the MetaLayer class used to interact with the game state and gather information.
    private MetaLayer metaLayer;

    // **GameState**: A reference to the GameState class that holds the current state of the game.
    private GameState gameState;

    /// <summary>
    /// Initializes a new instance of the EvilAI class.
    /// </summary>
    /// <param name="gameState">The current game state used by the AI to make decisions.</param>
    public EvilAI(GameState gameState)
    {
        this.gameState = gameState; // Store the game state reference
        this.metaLayer = new MetaLayer(gameState); // Initialize MetaLayer to interact with game state
    }

    /// <summary>
    /// Selects a route for the player based on their available hand cards and potential target routes.
    /// </summary>
    /// <param name="edgesOfAllPathsArray">Array of route names that are potential targets.</param>
    /// <param name="player">The player for whom the route is being picked.</param>
    /// <param name="routes">The GameObject containing all route objects in the game.</param>
    /// <returns>The name of the selected route, or an empty string if no suitable route is found.</returns>
    public string PickRoute(string[] edgesOfAllPathsArray, PlayerScript player, GameObject routes)
    {
        string targetRoute = "";

        // Debug output to verify the contents of the edgesOfAllPathsArray
        Debug.Log($"Keys in array: {string.Join(", ", edgesOfAllPathsArray)}");

        // Get the player's hand cards as a dictionary of colors and their respective counts
        Dictionary<string, int> playerHandCards = gameState.GetPlayerHandCards(player);

        // Debug output to display the player's hand cards
        //Debug.Log("Player Hand Cards: " + string.Join(", ", playerHandCards.Select(kvp => $"{kvp.Key}: {kvp.Value}")));

        // Iterate through the player's hand cards
        foreach (var card in playerHandCards)
        {
            string color = card.Key; // The color of the current card
            int count = card.Value; // The number of cards of this color in the player's hand

            // Check all child objects of the routes GameObject
            for (int r = 0; r < routes.transform.childCount; r++)
            {
                // Get the RouteScript component for the current route
                RouteScript route = routes.transform.GetChild(r).GetComponent<RouteScript>();

                // Ensure the route has no owner (i.e., it is unclaimed)
                if (route.owner == null)
                {
                    // Check if the route matches the player's hand cards and is in the list of potential routes
                    if (route.routeLength == count &&
                        (route.routeColor == color ||
                        (route.routeColor == "Grey" && edgesOfAllPathsArray.Contains(route.name))))
                    {
                        // Set the target route to the name of this route
                        targetRoute = routes.transform.GetChild(r).name;
                    }
                }
            }
        }

        // Return the selected route's name, or an empty string if no route was selected
        return targetRoute;
    }

    /// <summary>
    /// Returns a list of destination cards of a player.
    /// </summary>
    /// <param name="playerName">The name of the player whose destination cards are to be retrieved.</param>
    /// <returns>A list of DestinationCard objects representing the destination cards of the specified player.</returns>
    public List<DestinationCard> GetDestinationCardsOfVictim(string playerName)
    {
        // Get the dictionary of DestinationCards and their associated player names
        Dictionary<DestinationCard, string> playerDestinationCardsDict = metaLayer.GetDestinationCardsOfPlayer(playerName);

        // Extract only the keys (DestinationCards) into a list
        List<DestinationCard> victimDestinationCards = playerDestinationCardsDict.Keys.ToList<DestinationCard>();
        return victimDestinationCards;
    }

    /// <summary>
    /// Chooses a random player (other than the AI itself) to target as a victim.
    /// If the game turn counter is 10 or higher, selects the player with the highest score.
    /// Otherwise, selects a random player.
    /// </summary>
    /// <returns>The selected victim player.</returns>
    public PlayerScript PickVictim()
    {
        // Get the list of other players, excluding Evil AI.
        var otherPlayers = metaLayer.GetOtherPlayersButNotEvilAI(metaLayer.GetOtherPlayers());
        PlayerScript victim;

        // Choose a victim based on the game turn counter.
        if (gameState.TurnCounter >= 10)
        {
            victim = GetVictimByScore(otherPlayers);
        }
        else
        {
            victim = GetRandomVictim(otherPlayers);
        }

        return victim;
    }


    /// <summary>
    /// Retrieves the most recent card color of the selected victim to target.
    /// </summary>
    /// <returns>The color of the last train card drawn by the victim.</returns>
    public string GetVictimTrainCardColor(string victim)
    {
        // Get the last drawn card color of the selected victim
        string targetColor = metaLayer.GetVictimColor(victim);

        // Log the victim and the selected card color
        Debug.Log("Victim = " + victim);
        Debug.Log("Colors to pick: " + targetColor);

        // Return the identified color
        return targetColor;
    }

    /// <summary>
    /// Selects a random player from the list of other players.
    /// </summary>
    /// <param name="otherPlayers">A list of other players to choose from.</param>
    /// <returns>A randomly selected player.</returns>
    public PlayerScript GetRandomVictim(List<PlayerScript> otherPlayers)
    {
        if (otherPlayers.Count > 0)
        {
            // Randomly select a player from the list of other players
            var random = new System.Random();
            int index = random.Next(0, otherPlayers.Count); // Select a random index
            PlayerScript victim = otherPlayers[index]; // Get the player's name at the random index

            // Log the selected victim
            Debug.Log("The victim is: " + victim);
            return victim; // Return the name of the victim
        }
        else
        {
            // If there are no other players, log the error and return a default player
            Debug.Log("No other players found in the list.");
            return gameState.PlayerList.First(); // Default fallback to first player in playerList
        }
    }
    /// <summary>
    /// Selects the player with the highest score from the list of other players.
    /// </summary>
    /// <param name="otherPlayers">A list of other players to evaluate.</param>
    /// <returns>The player with the highest score.</returns>
    public PlayerScript GetVictimByScore(List<PlayerScript> otherPlayers)
    {
        // Order List by score and select highest
        PlayerScript victim = otherPlayers.OrderByDescending(p => p.score).First();

        return victim;
    }
}