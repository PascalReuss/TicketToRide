using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MetaLayer
{
    private GameState gameState;

    // Constructor that receives the GameState object
    public MetaLayer(GameState gameState)
    {
        this.gameState = gameState;
    }
    
    /// <summary>
    /// Returns a list of moves for a specific player in the current turn.
    /// </summary>
    /// <param name="playerName">The name of the player whose moves are being queried.</param>
    /// <returns>A list of moves for the player in the current turn.</returns>
    public List<Move> GetMoveHistorybyPlayer(string playerName)
    {
        // Create an empty list to store the moves of the player in the current turn
        List<Move> movesForLastTurn = new List<Move>();

        //foreach (var move in gameState.MoveHistory.GetMovesForTurn(getLastTurn())) // Loop through all moves of the last turn
        foreach (var move in gameState.MoveHistory.GetMovesForTurn(gameState.TurnCounter)) // Loop through all moves of the current turn
        {
            // Log for original List
            Debug.Log($"Player: {move.Player.playerName}, Move TurnCounter: {move.TurnCounter}, Action: {move.Action}: {move.CardColor}{move.Route}");
            //Debug.Log(move.MoveOutput());
            // Check if the player name matches and the turn counter matches the current round
            if (move.Player.playerName == playerName && move.TurnCounter == gameState.TurnCounter)
            {
                // Add the move to the list if both conditions are satisfied
                movesForLastTurn.Add(move);
            }
        }

        // Return the list of filtered moves
        return movesForLastTurn;
    }

    /// <summary>
    /// Returns a dictionary of destination cards for a specified player.
    /// </summary>
    /// <param name="playerName">The name of the player whose destination cards are being queried.</param>
    /// <returns>A dictionary containing the destination cards of the player.</returns>
    public Dictionary<DestinationCard, string> GetDestinationCardsOfPlayer(string playerName)
    {
        // Retrieve the destination cards for the player from the GameState
        Dictionary<DestinationCard, string> victimDestinationCards = gameState.GetDestinationCardsOfPlayer(playerName);
        return victimDestinationCards;
    }

    /// <summary>
    /// Retrieves the last turn of the game.
    /// </summary>
    /// <returns>
    /// The number of the last turn. If the current turn is the first turn, returns 1.
    /// </returns>
    public int GetLastTurn()
    {
        // Retrieve the current turn from the game state.
        int currentTurn = gameState.TurnCounter;

        // Calculate the last turn by decrementing the current turn.
        int lastTurn = currentTurn - 1;

        // If the current turn is 1, there is no prior turn. Return 1 as a fallback.
        if (currentTurn == 1)
            return 1;

        // Otherwise, return the calculated last turn.
        return lastTurn;
    }

    /// <summary>
    /// Calculates the most frequently drawn card color by the player in the current turn.
    /// </summary>
    /// <param name="playername">The name of the player whose drawn color is being calculated.</param>
    /// <returns>The most frequently drawn color for the player in the current turn.</returns>
    public string GetVictimColor(string playername)
    {
        // Retrieve the moves of the player
        var moves = GetMoveHistorybyPlayer(playername);
        
        // List to store all the drawn colors
        List<string> pickedColors = new List<string>();

        // Loop through the player's moves
        for (int i = 0; i < moves.Count; i++)
        {
            var move = moves[i];
            // "foreach" nicht möglich, da zur Laufzeit keine Variable aus einer Collection entfernt werden darf in c#
            // If the player has drawn a card, add the color to the list
            if (move.Action == "DrawCard")
            {
                pickedColors.Add(move.CardColor);
                Debug.Log($"Focus on Color: {move.CardColor}");

                // Remove the move at the current index
                moves.RemoveAt(i);
                // adjust the index to avoid skipping elements
                i--;
            }
        }

        // Set the default color to 'Joker' if no color was picked
        string targetColor = "Joker";

        // If any colors were picked, find the most frequent color
        if (pickedColors.Count > 0)
        {
            targetColor = pickedColors
                .GroupBy(color => color) // Group colors
                .OrderByDescending(group => group.Count()) // Sort by frequency
                .First().Key; // Select the most frequent color
        }
        return targetColor;
    }

    /// <summary>
    /// Returns a list of all players except the active player.
    /// </summary>
    /// <returns>A list of all players excluding the active player.</returns>
    public List<PlayerScript> GetOtherPlayers()
    {
        var activePlayer = gameState.Player;  // The currently active player
        var allPlayers = gameState.GetPlayerList();    // List of all players

        // Filter the list to exclude the active player
        var remainingPlayers = allPlayers.Where(player => player != activePlayer).ToList();
        return remainingPlayers;
    }
    /// <summary>
    /// Filters the list of players to exclude all Evil AIs.
    /// </summary>
    /// <param name="otherPlayers">A list of players to filter.</param>
    /// <returns>A new list of players without Evil AIs.</returns>
    public List<PlayerScript> GetOtherPlayersButNotEvilAI(List<PlayerScript> otherPlayers)
    {
        // Exclude EvilAI from the List
        List<PlayerScript> filteredPlayers = otherPlayers.Where(p => !p.isEvilAI).ToList();

        // Return the filtered list.
        return filteredPlayers;
    }

    /// <summary>
    /// Calculates the probability of drawing each card from the deck.
    /// </summary>
    /// <returns>A dictionary containing card colors and their respective probabilities.</returns>
    public Dictionary<string, float> GetCardDrawProbabilities()
    {
        // Retrieve the card deck from the GameState
        var cardDeck = gameState.GetCardDeck();

        // Calculate the total number of cards in the deck
        int totalCards = cardDeck.Values.Sum();

        // Create a dictionary to store probabilities
        var probabilities = new Dictionary<string, float>();

        // Compute the probability for each card in the deck
        foreach (var card in cardDeck)
        {
            probabilities[card.Key] = (float)card.Value / totalCards;
        }

        return probabilities;
    }

    /// <summary>
    /// Evaluates the probability of drawing a specific card color.
    /// </summary>
    /// <param name="color">The color of the card to evaluate.</param>
    /// <returns>The probability of drawing the specified card color.</returns>
    public float EvaluateSpecificCardDraw(string color)
    {
        var probabilities = GetCardDrawProbabilities();
        return probabilities.TryGetValue(color, out float probability) ? probability : 0f;
    }

    /// <summary>
    /// Returns the most probable card to draw based on the probabilities.
    /// </summary>
    /// <returns>The most probable card color to draw.</returns>
    public string GetMostProbableCardToDraw()
    {
        var probabilities = GetCardDrawProbabilities();
        var mostProbableCard = probabilities.OrderByDescending(p => p.Value).FirstOrDefault();
        return mostProbableCard.Key;
    }
}