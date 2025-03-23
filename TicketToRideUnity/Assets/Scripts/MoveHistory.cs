using System.Collections.Generic;
using System.Linq;

public class MoveHistory
{
    private List<Move> moves;

    public MoveHistory()
    {
        moves = new List<Move>();
    }

    /// <summary>
    /// Adds a new move to the history.
    /// </summary>
    /// <param name="move">The move to be added.</param>
    public void AddMove(Move move)
    {
        if (move != null)
        {
            moves.Add(move);
        }
    }

    /// <summary>
    /// Returns all moves.
    /// </summary>
    /// <returns>A list of all moves.</returns>
    public List<Move> GetAllMoves()
    {
        return new List<Move>(moves); // Return a copy for safety
    }

    /// <summary>
    /// Returns the moves of a specific round.
    /// </summary>
    /// <param name="turnCounter">The round number.</param>
    /// <returns>A list of moves from the specified round.</returns>
    public List<Move> GetMovesForTurn(int turnCounter)
    {
        return moves.Where(m => m.TurnCounter == turnCounter).ToList();
    }

    /// <summary>
    /// Returns the moves of a specific player.
    /// </summary>
    /// <param name="playerName">The name of the player.</param>
    /// <returns>A list of moves by the specified player.</returns>
    public List<Move> GetMovesForPlayer(string playerName)
    {
        return moves.Where(m => m.Player.playerName == playerName).ToList();
    }

    /// <summary>
    /// Returns the moves of a specific action.
    /// </summary>
    /// <param name="action">The name of the action.</param>
    /// <returns>A list of moves for the specified action.</returns>
    public List<Move> GetMovesForAction(string action)
    {
        return moves.Where(m => m.Action == action).ToList();
    }
    /// <summary>
    /// Removes a move from the history based on the specified criteria.
    /// </summary>
    /// <param name="turnCounter">The turn number of the move to remove.</param>
    /// <param name="playerName">The name of the player who performed the move.</param>
    /// <returns>True if the move was removed, false otherwise.</returns>
    public void RemoveMove(int turnCounter, string playerName, string cardColor)    
    {
        // Find the move that matches the turnCounter and playerName and cardColor
        var moveToRemove = moves.FirstOrDefault(m => m.TurnCounter == turnCounter && m.Player.playerName == playerName && m.CardColor == cardColor);

        if (moveToRemove != null)
        {
            // Remove the found move from the list
            moves.Remove(moveToRemove);
        }

        // If no matching move was found, return false
    }
    /// <summary>
    /// Clears all stored moves.
    /// </summary>
    public void Clear()
    {
        moves.Clear();
    }
}
