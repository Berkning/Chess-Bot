using System.Collections.Generic;
using UnityEngine;

public static class RandomBot
{
    public static Move GetBestMove()
    {
        List<Move> moves = MoveGenerator.GenerateMoves();
        return moves[Random.Range(0, moves.Count)];
    }
}