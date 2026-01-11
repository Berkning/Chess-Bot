using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public class Search
{
    private const int ImmediateMateScore = short.MaxValue;
    private const int PositiveInfinity = 9999999;
    private const int NegativeInfinity = -PositiveInfinity;

    private const int MaxExtensions = 8;

    private int nodeCount = 0;
    private const int CancelDelay = 1023; //Amount of nodes to check before next check of cancelSearch value - HAS to be mask - like ending in only ones -> 0b0001111111
    //private static int quiescenseCount = 0;
    //private static int ttHits = 0;

    //private static Move[] principledVariation = new Move[100]; //TODOne: Could just convert to array indexed by depth to store PV
    private Move bestMove;
    private int bestEval;

    private RepetitionTable repetitionTable = new RepetitionTable();

    public static TranspositionTable transpositionTable = new TranspositionTable();

    private Board board;
    private MoveGenerator moveGenerator;
    private MoveOrdering moveOrdering;
    private Evaluation evaluator;
    //private int threadShuffle;
    private int threadID;
    private Action<Move, int> callback;


    private Stopwatch clock = new Stopwatch();


    public Search(Board _board, Action<Move, int> _callback, int _threadID)
    {
        threadID = _threadID;
        callback = _callback;


        board = _board;
        moveGenerator = new MoveGenerator(board);
        moveOrdering = new MoveOrdering(board, moveGenerator, threadID);
        evaluator = new Evaluation();
    }

    public int searchDepth = -1;
    //Might be necessary to mark as volatile so it's synced when main thread wants to stop this thread
    public int searchTime = -1; //-2 : infinite,  -1 : use time management,  x : use x amount of time


    public void StartSearch()
    {
        bestMove = Move.nullMove;
        bestEval = NegativeInfinity;
        repetitionTable.Copy(board.repetitionTable);

        nodeCount = -1; //Dont want to include start node

        clock.Reset();

        if (searchTime == -1)
        {
            searchTime = TimeManagement.GetSearchTime(board.colorToMove); //Doesn't matter that other threads will have same search time and are started with delay. Their searchTime will be set to a negative number when first thread finishes anyway

            clock.Start();
        }
        else if (searchTime > 0)
        {
            clock.Start();
        }
        else if (searchTime == 0)
        {
            Console.WriteLine("Searchtime was zero!!");
            Console.WriteLine("bestmove searchtimewaszero");
            return;
        }
        else //Go forever till stop is recieved
        {
            //TODO: Remove
            //Console.WriteLine("bestmove timeWasUnder-1=" + searchTime);

            searchTime = int.MaxValue;
            clock.Start();
        }


        int prevResult = NegativeInfinity;


        int resultFromLastSearch = transpositionTable.LookupEvaluation(board.currentZobrist, 1, 0, PositiveInfinity, NegativeInfinity); //TODO: Test if this works as intended. With alpha and beta as well

        if (resultFromLastSearch != TranspositionTable.LookupFailed)
        {
            prevResult = resultFromLastSearch; //Use TT eval of current position as guess of current eval
        }

        for (uint depth = 1; depth <= searchDepth; depth++)
        {
            prevResult = AspirationSearch(depth, prevResult);

            if (clock.ElapsedMilliseconds >= searchTime)
            {
                //If we haven't found a move to play, and search is being cancelled, run an emergency full width search to a depth of 1
                if (bestMove.data == 0)
                {
                    Console.WriteLine("info string Running emergency search");
                    AlphaBeta(1, 0, NegativeInfinity, PositiveInfinity);
                }

                LogSearchInfo(depth, nodeCount, true, threadID);
                break;
            }
            else LogSearchInfo(depth, nodeCount, false, threadID); //TODO: check if matescore and then exit if were low on time
        }


        clock.Stop();

        callback.Invoke(bestMove, threadID);
    }





    #region Aspiration Window
    //public static class AspirationWindow //TODO: Try making non-static
    //{
    private readonly static int[] windowIncrements = { 25, 50, 100, 200, 400, 800, 1600 }; //TODO: Tweak
    private const int InstabilityMargin = 25;

    private int AspirationSearch(uint depth, int prevResult) //TODO: Think maybe the illegal moves and infinite evals come from not getting a proper width search before search is cancelled so we have to use the capped eval???
    {
        int incrementIndex = 0;

        int alpha = prevResult - windowIncrements[incrementIndex];
        int beta = prevResult + windowIncrements[incrementIndex];

        if (prevResult == NegativeInfinity) //If no previous result, we search with a full window
        {
            alpha = NegativeInfinity;
            beta = PositiveInfinity;
        }

        int result;

        while (true)
        {
            result = AlphaBeta(depth, 0, alpha, beta);

            //if ((nodeCount & CancelDelay) == 0) //Obv don't only check when canceldelay has passed, otherwise if nodecount is off by just 1 we keep running the aspiration search even if search is cancelled
            //{
            if (clock.ElapsedMilliseconds >= searchTime) return 0;
            //}

            incrementIndex++; //If we fail, we have to increment this index anyway, and if we don't, we won't continue the loop anyway

            if (result >= beta)
            {
                //Console.WriteLine("Failed High");
                //Console.WriteLine("New Increment: " + windowIncrements[incrementIndex]);
                if (incrementIndex == windowIncrements.Length) //If we still fail after having gone through all increments
                {
                    return AlphaBeta(depth, 0, NegativeInfinity, PositiveInfinity);
                }

                beta = prevResult + windowIncrements[incrementIndex];
                alpha = result - InstabilityMargin; //Have to still keep some space for search instability
            }
            else if (result <= alpha)
            {
                //Console.WriteLine("Failed Low");
                //Console.WriteLine("New Increment: " + windowIncrements[incrementIndex]);
                if (incrementIndex == windowIncrements.Length) //If we still fail after having gone through all increments
                {
                    return AlphaBeta(depth, 0, NegativeInfinity, PositiveInfinity);
                }

                alpha = prevResult - windowIncrements[incrementIndex];
                beta = result + InstabilityMargin;
            }
            else break; //Result is within window and we can stop re-searching
        }

        return result;
    }
    //}

    #endregion


    // public static float Eval(int depth, bool test) //FIXMEn't:
    // {
    //     repetitionTable.Copy(Board.repetitionTable);
    //     return AlphaBeta(depth, 0, negativeInfinity, positiveInfinity/*, test*/) / 100f;
    // }



    #region Search

    private int AlphaBeta(uint depth, int plyFromRoot, int alpha, int beta, uint numExtensions = 0)//, bool test)
    {
        nodeCount++;

        if ((nodeCount & CancelDelay) == 0) //TODO: test with removing this
        {
            if (clock.ElapsedMilliseconds >= searchTime) return 0;
        }

        if (plyFromRoot > 0)
        {
            //Two fold repetion instead of 3 fold for performance
            if (repetitionTable.Contains(board.currentZobrist)) //TODO: 50 move rule as well
            {
                return 0;
            }

            // Skip this position if a mating sequence has already been found earlier in
            // the search, which would be shorter than any mate we could find from here.
            // This is done by observing that alpha can't possibly be worse (and likewise
            // beta can't  possibly be better) than being mated in the current position.
            alpha = Math.Max(alpha, -ImmediateMateScore + plyFromRoot);
            beta = Math.Min(beta, ImmediateMateScore - plyFromRoot);
            if (alpha >= beta)
            {
                return alpha;
            }
        }

        int tableEval = transpositionTable.LookupEvaluation(board.currentZobrist, depth, plyFromRoot, alpha, beta);
        if (transpositionTable.IsSuccessfulLookup(tableEval))
        {
            //ttHits++;
            if (plyFromRoot == 0)
            {
                bestMove = transpositionTable.GetStoredMove(board.currentZobrist);
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

        int moveCount = moveGenerator.GenerateMoves(ref moves);


        //TODO: Try setting hash move to the global bestmove if plyfromroot == 0
        Move hashMove = Move.nullMove;

        if (tableEval == TranspositionTable.DepthFailed) hashMove = transpositionTable.GetStoredMove(board.currentZobrist);

        /*if (test)*/
        moveOrdering.OrderMoves(ref moves, moveCount, hashMove, plyFromRoot); //TODOnt: Try this after the mate check - somehow basically makes zero to worse difference

        //TODO: Could prob optimize to avoid this if statement
        //TODO: try this -> if (plyFromRoot == 0 && threadID % 2 == 1) moves.Reverse();//moveOrdering.ThreadRootShuffle(ref moves, moveCount, threadShuffle);

        //TODOnt: move this above move ordering bc obv no reason to try to do move ordering if there aren't any moves - somehow basically makes zero to worse difference
        if (moveCount == 0) //Maybe check if moveCount = 1 && plyFromRoot == 0 to return bc force move
        {
            //Debug.Log("Found Mate");
            if (moveGenerator.inCheck) return -(ImmediateMateScore - plyFromRoot); //Checkmate

            return 0; //Stalemate
        }



        Move bestMoveInPosition = Move.nullMove;
        ulong transpositionBound = TranspositionTable.UpperBound;

        if (plyFromRoot > 0) repetitionTable.Push(board.currentZobrist);


        for (int i = 0; i < moveCount; i++)
        {
            //Move move = moves[i];

            board.MakeMove(moves[i], true); //TODOne: test having ref to move instead of accesing array - prob already done by compiler though

            uint extensions = 0;
            if (numExtensions < MaxExtensions)
            {
                //TODO: Search extensions
                //if (MoveGenerator.inCheck) extensions = 1;//TODOnt?: Implement when we can easily calculate (with magics) if the move were about to make puts opponent in check.
                int targetRank = BoardHelper.IndexToRank(moves[i].targetSquare);
                if (Piece.Type(board.Squares[moves[i].targetSquare]) == Piece.Pawn && (targetRank == 1 || targetRank == 6)) extensions = 1; //Extend when about to promote //TODO: test properly
            }

            int evaluation = NegativeInfinity;
            bool searchFullDepth = true;

            //Late Move Reduction
            if (i > 4 && extensions == 0 && depth > 3)  //Assuming move ordering isn't completely wrong
            {
                evaluation = -AlphaBeta(depth - 2, plyFromRoot + 1, -beta, -alpha, numExtensions);

                //TODOne: if eval jumps, analyse at full depth
                //If evals better than anything else so far well search to full depth - horizon effect but outweighed by speed
                searchFullDepth = evaluation > alpha && !((nodeCount & CancelDelay) == 0 && clock.ElapsedMilliseconds >= searchTime); //TODOnt?: Move cancel check to separate if before this
            }

            if (searchFullDepth) evaluation = -AlphaBeta(depth - 1 + extensions, plyFromRoot + 1, -beta, -alpha, numExtensions + extensions);//, test);


            board.UnMakeMove(moves[i], true);


            if ((nodeCount & CancelDelay) == 0) //Makes perfect sense to have this here now - //Seemingly doesn't work properly without this check, but works fine without the check at the start of the function. Doesn't make any sense - also doesn't work do the correct amount of checks without the check at the start, but still stops in reasonable amount of time
            {
                if (clock.ElapsedMilliseconds >= searchTime) return 0;
            }

            //Move was good opponent will avoid this position
            if (evaluation >= beta)
            {
                transpositionTable.StoreEvaluation(board.currentZobrist, depth, plyFromRoot, beta, TranspositionTable.LowerBound, moves[i]);

                //TODO: Test without checking for ep for performance bc we already check if this is the case in the moveordering but this will ofc override the space of a valid killer move with an invalid ep move if possible - maybe too rare?
                if (board.Squares[moves[i].targetSquare] == Piece.None && moves[i].flag != Move.Flag.EnPassantCapture) //If not a capture - only add killer moves that aren't captures, bc these are always ranked highly i guess?
                {
                    if (plyFromRoot < MoveOrdering.MaxKillerPlys)
                    {
                        moveOrdering.killerMoves[plyFromRoot].Add(moves[i]);
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

                //if (evaluation > 1000) Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId}: suspicious eval={evaluation}, plyFromRoot={plyFromRoot}");

                if (plyFromRoot == 0) //TODO: PV
                {
                    bestMove = bestMoveInPosition;
                    bestEval = evaluation;
                }
            }
        }

        if (plyFromRoot > 0) repetitionTable.PopNoRtn();

        transpositionTable.StoreEvaluation(board.currentZobrist, depth, plyFromRoot, alpha, transpositionBound, bestMoveInPosition);

        return alpha;
    }



    private int SearchAllCaptures(int alpha, int beta) //TODO: maybe try including non-capture promotions - Checks??
    {
        if ((nodeCount & CancelDelay) == 0) //TODO: Try removing this
        {
            if (clock.ElapsedMilliseconds >= searchTime) return 0; //We are checking in the iterative part im just stupid - //From seb lague. Don't need to return 0 during the iterative part i guess, bc the main search calling this function will check if search is cancelled after this returns
        }

        // A player isn't forced to make a capture (typically), so see what the evaluation is without capturing anything.
        // This prevents situations where a plcancelSearchayer ony has bad captures available from being evaluated as bad,
        // when the player might have good non-capture moves available.
        int eval = evaluator.Evaluate(board);
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

        int moveCount = moveGenerator.GenerateMoves(ref moves, true);

        moveOrdering.OrderMoves(ref moves, moveCount, bestMove, -1); //TODO: Could prob optimize moveordering here to not worry about things that only apply to quiet moves

        for (int i = 0; i < moveCount; i++)
        {
            nodeCount++;

            board.MakeMove(moves[i], true);
            eval = -SearchAllCaptures(-beta, -alpha);
            board.UnMakeMove(moves[i], true);
            //numQNodes++;

            if ((nodeCount & CancelDelay) == 0)
            {
                if (clock.ElapsedMilliseconds >= searchTime) return 0;
            }

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

    #endregion



    #region Helpers

    public static bool logFullPV = false;

    private void LogSearchInfo(uint depth, int nodeCount, bool isPartial, int id)
    {
        if (bestMove.data == 0)
        {
            Console.WriteLine("bestmove " + FenUtility.GetCurrentFen(board) + " GHU#EGWHOIBHVDUBWRUBGUJOBJCBJOBNOWJRNOJNJNCBJNWUIORNBJUHBNWONRFIOBNWROJBNOJCNSOJBNRJOWNORJNBJUONBJONWERJONB");
            return;
        }

        string pv = logFullPV ? GetPVFromTranspositionTable() : GetBasicPVString();

        Console.WriteLine("info depth " + depth + " score " + GetScoreLogString(bestEval) + " pv" + pv + " nodes " + nodeCount + " nps " + Math.Round(nodeCount / clock.Elapsed.TotalSeconds) + (isPartial ? " string partial" : "") + " id " + id);
    }

    private string GetBasicPVString()
    {
        return ' ' + BoardHelper.GetMoveNameUCI(bestMove);
    }

    private string GetPVFromTranspositionTable()
    {
        string result = "";

        int tableResult = transpositionTable.LookupEvaluation(board.currentZobrist, 0, 0, 0, 0);

        if (tableResult == TranspositionTable.LookupFailed) return result;


        Move move = transpositionTable.GetStoredMove(board.currentZobrist);

        if (move.data != 0)
        {
            result += ' ' + BoardHelper.GetMoveNameUCI(move);

            board.MakeMove(move, true);
            result += GetPVFromTranspositionTable();
            board.UnMakeMove(move, true);
        }

        return result;
    }



    public static bool IsMateScore(int score)
    {
        if (score == short.MinValue)
        {
            return false;
        }
        return Math.Abs(score) > ImmediateMateScore - 1000;
    }

    private static string GetScoreLogString(int score)
    {
        if (!IsMateScore(score)) return "cp " + score.ToString();

        int absMateScore = ImmediateMateScore - Math.Abs(score); //FIXME:

        return "mate " + (absMateScore * Math.Sign(score)).ToString();
    }

    #endregion
}