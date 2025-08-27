using System;
using System.Collections.Generic;
using UnityEngine;

public static class RandomBot
{
    public static Move GetBestMove()
    {
        Span<Move> moves = MoveGenerator.GenerateMovesSlow();
        return moves[UnityEngine.Random.Range(0, moves.Length)];
    }
}