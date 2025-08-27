using System;
using System.Collections.Generic;
using UnityEngine;

public static class Search
{
    private const int immediateMateScore = 100000;
    private const int positiveInfinity = 9999999;
    private const int negativeInfinity = -positiveInfinity;

    private static int positionCount = 0;

    private static Move bestMove;
    private static int bestEval;

    public static Move StartSearch(int depth, bool test)
    {
        //return AlphaBeta(depth, negativeInfinity, positiveInfinity);

        bestMove = Move.nullMove;
        bestEval = negativeInfinity;

        positionCount = 0;
        int result = AlphaBeta(depth, 0, negativeInfinity, positiveInfinity, test);
        Debug.Log(positionCount + " positions");
        Debug.Log("Eval: " + result / 100f);
        return bestMove;
    }

    public static float Eval(int depth, bool test)
    {
        return AlphaBeta(depth, 0, negativeInfinity, positiveInfinity, test) / 100f;
    }

    private static int AlphaBeta(int depth, int plyFromRoot, int alpha, int beta, bool test)
    {
        if (plyFromRoot > 0)
        {
            // Skip this position if a mating sequence has already been found earlier in
            // the search, which would be shorter than any mate we could find from here.
            // This is done by observing that alpha can't possibly be worse (and likewise
            // beta can't  possibly be better) than being mated in the current position.
            alpha = Mathf.Max(alpha, -immediateMateScore + plyFromRoot);
            beta = Mathf.Min(beta, immediateMateScore - plyFromRoot);
            if (alpha >= beta)
            {
                return alpha;
            }
        }


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
            //Debug.Log("Found Mate");
            if (MoveGenerator.inCheck) return -immediateMateScore; //Checkmate

            return 0; //Stalemate
        }

        Move bestMoveInPosition = Move.nullMove;

        for (int i = 0; i < moveCount; i++)
        {
            Board.MakeMove(moves[i]);
            int evaluation = -AlphaBeta(depth - 1, plyFromRoot + 1, -beta, -alpha, test);
            Board.UnMakeMove(moves[i]);
            if (evaluation >= beta)
            {
                //Move was good opponent will avoid this position
                return beta;
            }

            if (evaluation > alpha)
            {
                alpha = evaluation;
                bestMoveInPosition = moves[i];

                if (plyFromRoot == 0)
                {
                    bestMove = bestMoveInPosition;
                }
            }
        }

        return alpha;
    }
}