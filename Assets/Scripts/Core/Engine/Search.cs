using System;

public static class Search
{
    private const int immediateMateScore = 100000;
    private const int positiveInfinity = 9999999;
    private const int negativeInfinity = -positiveInfinity;

    private const int maxExtensions = 8;

    private static int nodeCount = 0;
    //private static int quiescenseCount = 0;
    //private static int ttHits = 0;

    private static Move bestMove;
    private static int bestEval;

    private static RepetitionTable repetitionTable = new RepetitionTable();

    public static TranspositionTable transpositionTable = new TranspositionTable();



    public static bool cancelSearch = false;



    public static Move StartSearch(int searchDepth, int searchTime = -1) //-1 = let search decide, -2 = go infinite TODO: Change to enum //TODO: Killer moves
    {
        //return AlphaBeta(depth, negativeInfinity, positiveInfinity);
        cancelSearch = false;

        bestMove = Move.nullMove;
        bestEval = negativeInfinity;
        repetitionTable.Copy(Board.repetitionTable);

        nodeCount = -1; //Dont want to include start node - bc stockfish doesn't
        //quiescenseCount = 0;
        //ttHits = 0;

        if (searchTime == -1)
        {
            int maxSearchTime = TimeManagement.GetSearchTime(Board.colorToMove);

            TimeManagement.ScheduleSearchCancel(maxSearchTime);
        }
        else if (searchTime > 0) TimeManagement.ScheduleSearchCancel(searchTime); //If less than -1, let it go till stop is recieved


        int prevResult = negativeInfinity;

        for (int depth = 1; depth <= searchDepth; depth++)
        {
            //AlphaBeta(depth, 0, negativeInfinity, positiveInfinity);
            prevResult = AspirationSearch(depth, prevResult == negativeInfinity ? 0 : prevResult);

            //Debug.Log("Depth " + depth + " eval: " + result / 100f + " move: " + BoardHelper.NameMove(bestMove));

            if (cancelSearch)
            {
                //Console.WriteLine("info depth " + depth + " score cp " + result + " string partial search");
                //Console.WriteLine("info depth " + depth + " string partial");
                LogSearchInfo(depth, nodeCount, true);
                break;
            }
            else LogSearchInfo(depth, nodeCount, false); //TODO: check if matescore and then exit if were low on time
        }

        //if (!cancelSearch) TimeManagement.RevokeScheduledCancel();

        //Console.WriteLine(positionCount + " positions");
        //Console.WriteLine(quiescenseCount + " quiescenseCount");
        //Console.WriteLine(ttHits + " ttHits");
        //Debug.Log("Eval: " + result / 100f);
        return bestMove;
    }

    private static int AspirationSearch(int depth, int prevResult)
    {
        int alpha = prevResult - 51;
        int beta = prevResult + 51;

        int result = AlphaBeta(depth, 0, alpha, beta);

        if (result >= beta)
        {
            Console.WriteLine("Failed High");
            return AlphaBeta(depth, 0, negativeInfinity, positiveInfinity);
        }
        else if (result <= alpha)
        {
            Console.WriteLine("Failed Low");
            return AlphaBeta(depth, 0, negativeInfinity, positiveInfinity);
        }

        return result;
    }



    private static void LogSearchInfo(int depth, int nodeCount, bool isPartial)
    {
        Console.WriteLine("info depth " + depth + " score " + GetScoreLogString(bestEval) + " pv " + BoardHelper.GetMoveNameUCI(bestMove) + " nodes " + nodeCount + (isPartial ? " string partial" : ""));
    }

    // public static float Eval(int depth, bool test) //FIXMEn't:
    // {
    //     repetitionTable.Copy(Board.repetitionTable);
    //     return AlphaBeta(depth, 0, negativeInfinity, positiveInfinity/*, test*/) / 100f;
    // }




    private static int AlphaBeta(int depth, int plyFromRoot, int alpha, int beta, int numExtensions = 0)//, bool test)
    {
        nodeCount++;

        if (cancelSearch) return 0;

        if (plyFromRoot > 0)
        {
            //Two fold repetion instead of 3 fold for performance
            if (repetitionTable.Contains(Board.currentZobrist)) //TODO: 50 move rule as well
            {
                return 0;
            }

            // Skip this position if a mating sequence has already been found earlier in
            // the search, which would be shorter than any mate we could find from here.
            // This is done by observing that alpha can't possibly be worse (and likewise
            // beta can't  possibly be better) than being mated in the current position.
            alpha = Math.Max(alpha, -immediateMateScore + plyFromRoot);
            beta = Math.Min(beta, immediateMateScore - plyFromRoot);
            if (alpha >= beta)
            {
                return alpha;
            }
        }

        int tableEval = transpositionTable.LookupEvaluation(depth, plyFromRoot, alpha, beta);
        if (tableEval != TranspositionTable.LookupFailed)
        {
            //ttHits++;
            if (plyFromRoot == 0)
            {
                bestMove = transpositionTable.GetStoredMove();
                bestEval = tableEval;
            }
            return tableEval;
        }

        if (depth == 0)
        {
            //quiescenseCount++;
            //return Evaluation.Evaluate();
            return SearchAllCaptures(alpha, beta);
        }


        Span<Move> moves = stackalloc Move[256];

        int moveCount = MoveGenerator.GenerateMoves(ref moves);

        /*if (test)*/
        MoveOrdering.OrderMoves(ref moves, moveCount, bestMove, plyFromRoot);

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
            Board.MakeMove(moves[i], true); //TODO: test having ref to move instead of accesing array - prob already done by compiler though

            int extensions = 0;
            if (numExtensions < maxExtensions)
            {
                //TODO: Search extenstions
                //if (MoveGenerator.inCheck) extensions = 1;//TODOnt?: Implement when we can easily calculate (with magics) if the move were about to make puts opponent in check.
                int targetRank = BoardHelper.IndexToRank(moves[i].targetSquare);
                if (Piece.Type(Board.Squares[moves[i].targetSquare]) == Piece.Pawn && (targetRank == 1 || targetRank == 6)) extensions = 1; //Extend when about to promote
            }



            int evaluation = -AlphaBeta(depth - 1 + extensions, plyFromRoot + 1, -beta, -alpha, numExtensions + extensions);//, test);
            Board.UnMakeMove(moves[i], true);


            if (cancelSearch) return 0;

            //Move was good opponent will avoid this position
            if (evaluation >= beta)
            {
                transpositionTable.StoreEvaluation(depth, plyFromRoot, beta, TranspositionTable.LowerBound, moves[i]);

                //TODO: Test without checking for ep for performance maybe
                if (Board.Squares[moves[i].targetSquare] == Piece.None && moves[i].flag != Move.Flag.EnPassantCapture) //If not a capture - only add killer moves that aren't captures, bc these are always ranked highly i guess?
                {
                    if (plyFromRoot < MoveOrdering.MaxKillerPlys)
                    {
                        MoveOrdering.killerMoves[plyFromRoot].Add(moves[i]);
                    }
                }

                if (plyFromRoot > 0) repetitionTable.PopNoRtn();
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
                    bestEval = evaluation;
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
        //positionCount++;

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

        MoveOrdering.OrderMoves(ref moves, moveCount, bestMove, -1); //TODO: Could prob optimize moveordering here to not worry about things that only apply to quiet moves

        for (int i = 0; i < moveCount; i++)
        {
            nodeCount++;

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
        return Math.Abs(score) > immediateMateScore - 1000;
    }

    public static string GetScoreLogString(int score)
    {
        if (!IsMateScore(score)) return "cp " + score.ToString();

        int absMateScore = immediateMateScore - Math.Abs(score); //FIXME:

        return "mate " + (absMateScore * Math.Sign(score)).ToString();
    }
}