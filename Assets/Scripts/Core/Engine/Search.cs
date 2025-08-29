using System;
using System.Collections.Generic;
using UnityEngine;

public static class Search
{
    private static List<ushort> testmoves = new List<ushort>();

    private const int immediateMateScore = 100000;
    private const int positiveInfinity = 9999999;
    private const int negativeInfinity = -positiveInfinity;

    private static int positionCount = 0;
    private static int quiescenseCount = 0;

    private static Move bestMove;
    private static int bestEval;

    public static Move StartSearch(int depth, bool test)
    {
        //return AlphaBeta(depth, negativeInfinity, positiveInfinity);

        bestMove = Move.nullMove;
        bestEval = negativeInfinity;

        positionCount = 0;
        quiescenseCount = 0;
        int result = AlphaBeta(depth, 0, negativeInfinity, positiveInfinity);//, test);
        Debug.Log(positionCount + " positions");
        Debug.Log(quiescenseCount + " quiescenseCount");
        Debug.Log("Eval: " + result / 100f);
        return bestMove;
    }

    public static float Eval(int depth, bool test)
    {
        return AlphaBeta(depth, 0, negativeInfinity, positiveInfinity/*, test*/) / 100f;
    }

    private static int AlphaBeta(int depth, int plyFromRoot, int alpha, int beta)//, bool test)
    {
        /*if (plyFromRoot > 0)
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
        }*/


        if (depth == 0)
        {
            //positionCount++;
            quiescenseCount++;
            return Evaluation.Evaluate();
            //return SearchAllCaptures(alpha, beta);
        }

        Span<Move> moves = stackalloc Move[256];

        int moveCount = MoveGenerator.GenerateMoves(ref moves);
        if (plyFromRoot == 1)
        {
            if (moveCount != testmoves.Count)
            {
                Debug.Log("Length didn't match");
                testmoves = new List<ushort>();

                for (int i = 0; i < moveCount; i++)
                {
                    testmoves.Add(0);
                }
            }

            for (int i = 0; i < moveCount; i++)
            {
                if (moves[i].data != testmoves[i])
                {
                    Debug.Log("Mismatch at i: " + i);
                    testmoves[i] = moves[i].data;
                }
            }
        }

        /*if (test)*/
        MoveOrdering.OrderMoves(ref moves, moveCount);



        if (moveCount == 0)
        {
            //Debug.Log("Found Mate");
            if (MoveGenerator.inCheck) return -(immediateMateScore - plyFromRoot); //Checkmate

            return 0; //Stalemate
        }

        Move bestMoveInPosition = Move.nullMove;


        for (int i = 0; i < moveCount; i++)
        {
            Board.MakeMove(moves[i]);
            int evaluation = -AlphaBeta(depth - 1, plyFromRoot + 1, -beta, -alpha);//, test);
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

    private static int SearchAllCaptures(int alpha, int beta)
    {
        // A player isn't forced to make a capture (typically), so see what the evaluation is without capturing anything.
        // This prevents situations where a player ony has bad captures available from being evaluated as bad,
        // when the player might have good non-capture moves available.
        int eval = Evaluation.Evaluate();
        positionCount++;

        if (eval >= beta)
        {
            return beta;
        }
        if (eval > alpha)
        {
            alpha = eval;
        }

        Span<Move> moves = stackalloc Move[256];

        int moveCount = MoveGenerator.GenerateMoves(ref moves, true);

        MoveOrdering.OrderMoves(ref moves, moveCount);

        for (int i = 0; i < moveCount; i++)
        {
            Board.MakeMove(moves[i]);
            eval = -SearchAllCaptures(-beta, -alpha);
            Board.UnMakeMove(moves[i]);
            //numQNodes++;

            if (eval >= beta)
            {
                //numCutoffs++;
                return beta;
            }
            if (eval > alpha)
            {
                alpha = eval;
            }
        }

        return alpha;
    }
}