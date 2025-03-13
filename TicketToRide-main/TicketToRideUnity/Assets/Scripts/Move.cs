using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Move
{
    // **Turn Counter**: The turn number in which this move occurred.
    public int TurnCounter { get; set; }

    // **Card Deck**: The current state of the card deck, with card colors and their respective counts.
    public Dictionary<string, int> CardDeck { get; set; }

    // **Drawn Cards**: The cards that have been drawn so far, along with their counts.
    public Dictionary<string, int> DrawnCards { get; set; }

    // **Destination Cards**: The destination cards that have been drawn, associated with the players.
    public Dictionary<DestinationCard, string> DestinationCards { get; set; }

    // **Player Name**: The name of the player who performed the action.
    public PlayerScript Player { get; set; }

    // **Action**: The action performed by the player (e.g., "DrawCard", "BuyRoute", etc.).
    public string Action { get; set; }

    // **Card Color**: The color of the card drawn (if relevant to the action, such as "Red").
    public string CardColor { get; set; }

    // **Route**: The name of the claimed route
    public string Route { get; set; }

    /// <summary>
    /// Constructor to create a new move object with the given details.
    /// </summary>
    /// <param name="turnCounter">The turn number when the move occurred.</param>
    /// <param name="cardDeck">The current state of the card deck.</param>
    /// <param name="drawnCards">The cards that have been drawn so far.</param>
    /// <param name="destinationCards">The destination cards associated with the players.</param>
    /// <param name="playerName">The name of the player who performed the action.</param>
    /// <param name="action">The action that the player performed.</param>
    /// <param name="cardColor">The color of the card drawn, if relevant.</param>
    /// <param name="route">The name of the claimed route.</param>
    public Move(int turnCounter, Dictionary<string, int> cardDeck, Dictionary<string, int> drawnCards,
                Dictionary<DestinationCard, string> destinationCards, PlayerScript player, string action, string cardColor, string route)
    {
        TurnCounter = turnCounter;
        CardDeck = new Dictionary<string, int>(cardDeck);  // Make a copy of the current card deck.
        DrawnCards = new Dictionary<string, int>(drawnCards);  // Make a copy of the drawn cards.
        DestinationCards = new Dictionary<DestinationCard, string>(destinationCards);  // Make a copy of the destination cards.
        Player = player;
        Action = action;
        CardColor = cardColor;
        Route = route;
    }

    /// <summary>
    /// Returns a human-readable string representing the move information.
    /// </summary>
    /// <returns>A formatted string with details about the move.</returns>
    public string MoveOutput()
    {
        // Format the different parts of the move (card deck, drawn cards, destination cards)
        string cardDeckString = FormatDictionary(CardDeck);
        string drawnCardsString = FormatDictionary(DrawnCards);
        string destinationCardsString = FormatDestinationCards(DestinationCards);

        // Combine all the parts into a single formatted string and return it
        return $"Move Information:\n" +
               $"- Turn: {TurnCounter}\n" +
               $"- Player: {Player.playerName}\n" +
               $"- Action: {Action}\n" +
               $"- Action: {Route}\n" +
               $"- Card Color: {CardColor}\n" +
               $"- Card Deck: {cardDeckString}\n" +
               $"- Drawn Cards: {drawnCardsString}\n" +
               $"- Destination Cards: {destinationCardsString}";
    }

    /// <summary>
    /// Formats a dictionary into a string where each key-value pair is displayed as "key: value".
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary's keys.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary's values.</typeparam>
    /// <param name="dictionary">The dictionary to be formatted.</param>
    /// <returns>A string representation of the dictionary.</returns>
    private string FormatDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
    {
        return string.Join(", ", dictionary.Select(kv => $"{kv.Key}: {kv.Value}"));
    }

    /// <summary>
    /// Formats the destination cards into a string representation showing each destination card and its associated player.
    /// </summary>
    /// <param name="destinationCards">The dictionary containing destination cards and associated players.</param>
    /// <returns>A string representation of the destination cards and their statuses.</returns>
    private string FormatDestinationCards(Dictionary<DestinationCard, string> destinationCards)
    {
        return string.Join(", ", destinationCards.Select(kv => $"{kv.Key.ToString()} - Status: {kv.Value}"));
    }
}