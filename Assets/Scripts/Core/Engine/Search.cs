using System;
using System.Collections.Generic;
using UnityEngine;

public static class Search
{
    private const int immediateMateScore = 100000;
    private const int positiveInfinity = 9999999;
    private const int negativeInfinity = -positiveInfinity;

    private static int positionCount = 0;

    public static int StartSearch(int depth, bool test)
    {
        //return AlphaBeta(depth, negativeInfinity, positiveInfinity);

        positionCount = 0;
        int result = AlphaBeta(depth, negativeInfinity, positiveInfinity, test);
        Debug.Log(positionCount + " positions");
        return result;
    }

    private static int AlphaBeta(int depth, int alpha, int beta, bool test)
    {
        if (depth == 0)
        {
            positionCount++;
            return Evaluation.Evaluate();
        }

        Span<Move> moves = stackalloc Move[256];

        int moveCount = MoveGenerator.GenerateMoves(ref moves);

        if (test) MoveOrdering.OrderMoves(ref moves, moveCount);



        if (moveCount == 0)
        {
            if (MoveGenerator.inCheck) return immediateMateScore; //Checkmate

            return 0; //Stalemate
        }

        for (int i = 0; i < moveCount; i++)
        {
            Board.MakeMove(moves[i]);
            int evaluation = -AlphaBeta(depth - 1, -beta, -alpha, test);
            Board.UnMakeMove(moves[i]);
            if (evaluation >= beta)
            {
                //Move was good opponent will avoid this position
                return beta;
            }

            alpha = Math.Max(alpha, evaluation);
        }

        return alpha;
    }
}