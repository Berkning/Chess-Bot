using System;
using System.Collections.Generic;
using UnityEngine;

public static class Search
{
    private const int immediateMateScore = 100000;
    private const int positiveInfinity = 9999999;
    private const int negativeInfinity = -positiveInfinity;

    private const int maxExtensions = 8;

    private static int positionCount = 0;
    private static int quiescenseCount = 0;
    private static int ttHits = 0;

    private static Move bestMove;
    private static int bestEval;

    private static RepetitionTable repetitionTable = new RepetitionTable();

    public static TranspositionTable transpositionTable = new TranspositionTable();



    public static bool cancelSearch = false;



    public static Move StartSearch(int searchDepth, int searchTime = -1) //-1 = let search decide, -2 = go infinite
    {
        //return AlphaBeta(depth, negativeInfinity, positiveInfinity);
        cancelSearch = false;

        bestMove = Move.nullMove;
        bestEval = negativeInfinity;
        repetitionTable.Copy(Board.repetitionTable);

        positionCount = 0;
        quiescenseCount = 0;
        ttHits = 0;

        if (searchTime == -1)
        {
            int maxSearchTime = TimeManagement.GetSearchTime(Board.colorToMove);

            TimeManagement.ScheduleSearchCancel(maxSearchTime);
        }
        else if (searchTime > 0) TimeManagement.ScheduleSearchCancel(searchTime); //If less than -1, let it go till stop is recieved

        for (int depth = 1; depth <= searchDepth; depth++)
        {
            int result = AlphaBeta(depth, 0, negativeInfinity, positiveInfinity);

            //Debug.Log("Depth " + depth + " eval: " + result / 100f + " move: " + BoardHelper.NameMove(bestMove));

            if (cancelSearch)
            {
                Debug.Log("info depth " + depth + " score cp " + result + " string partial search"); //TODO: Dont log score bc it always returns 0
                break;
            }
            else Debug.Log("info depth " + depth + " score cp " + result); //TODO: check if matescore and then exit - no need to wait if we have mate
        }

        //if (!cancelSearch) TimeManagement.RevokeScheduledCancel();

        Debug.Log(positionCount + " positions");
        Debug.Log(quiescenseCount + " quiescenseCount");
        Debug.Log(ttHits + " ttHits");
        //Debug.Log("Eval: " + result / 100f);
        return bestMove;
    }

    public static float Eval(int depth, bool test) //FIXME:
    {
        repetitionTable.Copy(Board.repetitionTable);
        return AlphaBeta(depth, 0, negativeInfinity, positiveInfinity/*, test*/) / 100f;
    }




    private static int AlphaBeta(int depth, int plyFromRoot, int alpha, int beta, int numExtensions = 0)//, bool test) //TODO?: Dont waste partial search when cancelled
    {
        if (cancelSearch) return 0;

        if (plyFromRoot > 0)
        {
            if (repetitionTable.Contains(Board.currentZobrist)) //TODO: 50 move rule as well
            {
                return 0;
            }

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

        int tableEval = transpositionTable.LookupEvaluation(depth, plyFromRoot, alpha, beta);
        if (tableEval != TranspositionTable.LookupFailed)
        {
            ttHits++;
            if (plyFromRoot == 0)
            {
                bestMove = transpositionTable.GetStoredMove();
            }
            return tableEval;
        }


        if (depth == 0)
        {
            //positionCount++;
            quiescenseCount++;
            //return Evaluation.Evaluate();
            return SearchAllCaptures(alpha, beta);
        }

        Span<Move> moves = stackalloc Move[256];

        int moveCount = MoveGenerator.GenerateMoves(ref moves);

        /*if (test)*/
        MoveOrdering.OrderMoves(ref moves, moveCount, bestMove);



        if (moveCount == 0) //Maybe check if moveCount = 1 && plyFromRoot == 0 to return bc force move
        {
            //Debug.Log("Found Mate");
            if (MoveGenerator.inCheck) return -(immediateMateScore - plyFromRoot); //Checkmate

            return 0; //Stalemate
        }

        Move bestMoveInPosition = Move.nullMove;
        int transpositionBound = TranspositionTable.UpperBound;

        if (plyFromRoot > 0) repetitionTable.Push(Board.currentZobrist);


        for (int i = 0; i < moveCount; i++)
        {
            Board.MakeMove(moves[i], true);

            int extensions = 0;
            if (numExtensions < maxExtensions)
            {
                //if (MoveGenerator.inCheck) extensions = 1;//TODO: Implement when we can easily calculate (with magics) if the move were about to make puts opponent in check.
                int targetRank = BoardHelper.IndexToRank(moves[i].targetSquare);
                if (Piece.Type(Board.Squares[moves[i].targetSquare]) == Piece.Pawn && (targetRank == 1 || targetRank == 6)) extensions = 1; //Extend when about to promote
            }



            int evaluation = -AlphaBeta(depth - 1 + extensions, plyFromRoot + 1, -beta, -alpha, numExtensions + extensions);//, test);
            Board.UnMakeMove(moves[i], true);

            if (cancelSearch) return 0;

            if (evaluation >= beta)
            {
                //Move was good opponent will avoid this position
                transpositionTable.StoreEvaluation(depth, plyFromRoot, beta, TranspositionTable.LowerBound, moves[i]);

                repetitionTable.PopNoRtn();
                return beta;
            }

            if (evaluation > alpha)
            {
                alpha = evaluation;
                bestMoveInPosition = moves[i];
                transpositionBound = TranspositionTable.Exact;

                if (plyFromRoot == 0)
                {
                    bestMove = bestMoveInPosition;
                }
            }
        }

        if (plyFromRoot > 0) repetitionTable.PopNoRtn();

        transpositionTable.StoreEvaluation(depth, plyFromRoot, alpha, transpositionBound, bestMoveInPosition);

        return alpha;
    }

    private static int SearchAllCaptures(int alpha, int beta)
    {
        if (cancelSearch) return 0; //From seb lague. Don't need to return 0 during the iterative part i guess, bc the main search calling this function will check if search is cancelled after this returns

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

        MoveOrdering.OrderMoves(ref moves, moveCount, bestMove);

        for (int i = 0; i < moveCount; i++)
        {
            Board.MakeMove(moves[i], true);
            eval = -SearchAllCaptures(-beta, -alpha);
            Board.UnMakeMove(moves[i], true);
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


    public static bool IsMateScore(int score)
    {
        if (score == int.MinValue)
        {
            return false;
        }
        return Mathf.Abs(score) > immediateMateScore - 1000;
    }
}