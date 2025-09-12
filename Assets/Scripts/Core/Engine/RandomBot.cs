using System;

public static class RandomBot
{
    public static Move GetBestMove()
    {
        Span<Move> moves = MoveGenerator.GenerateMovesSlow();
        return moves[0];
        //return moves[UnityEngine.Random.Range(0, moves.Length)];
    }
}