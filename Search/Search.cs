using System;
using System.Diagnostics;
using System.Dynamic;
using System.Runtime.CompilerServices;

public class Search
{
    private const int AssuredWinScore = 20000;
    private const int ImmediateMateScore = short.MaxValue;
    private const int PositiveInfinity = 9999999;
    private const int NegativeInfinity = -PositiveInfinity;

    private const int MaxExtensions = 8; //TODO: Make tunable somehow

    private const int MaxDepth = 255;

    public int nodeCount { get; private set; } = 0;
    private const int CancelDelay = 1023; //Amount of nodes to check before next check of cancelSearch value - HAS to be mask - like ending in only ones -> 0b0001111111 //TODO: Make tunable somehow
    //private static int quiescenseCount = 0;
    //private static int ttHits = 0;

    //private Move[][] principledVariation = new Move[100];

    private Move bestMove;
    private int bestEval;

    private RepetitionTable repetitionTable = new RepetitionTable();

    public static TranspositionTable transpositionTable = new TranspositionTable();

    private Board board;
    private MoveGenerator moveGenerator;
    private MoveOrdering moveOrdering;
    private Evaluation evaluator;
    private Engine engine;
    //private int threadShuffle;
    private int threadID;
    private Action<Move, int> callback;


    private Stopwatch clock = new Stopwatch();


    public Search(Board _board, Action<Move, int> _callback, int _threadID, Engine _engine)
    {
        threadID = _threadID;
        callback = _callback;


        board = _board;
        moveGenerator = new MoveGenerator(board);
        moveOrdering = new MoveOrdering(board, moveGenerator, threadID);
        evaluator = new Evaluation();
        engine = _engine;
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


        moveOrdering.DecayHistory(); //TODO: maybe do after search when it is our opponents turn


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


        searchDepth = int.Min(searchDepth, MaxDepth); //Clamp searchDepth to never be above MaxDepth


        int prevResult = NegativeInfinity;

        Transposition lastTTEntry = transpositionTable.Probe(board.currentZobrist);

        if (lastTTEntry.IsValid())
        {
            //TODO: Should check that 1 is the correct value to pass when correcting - not very important at all, since only applicable in positions with a forced mate
            prevResult = TranspositionTable.CorrectRetrievedMateScore(lastTTEntry.eval, 1); //Use TT eval of current position as guess of current eval
        }


        for (uint depth = 1; depth <= searchDepth; depth++)
        {
            if (engine != null)
            {
                engine.StartHelperThreads((int)depth);
                prevResult = AspirationSearch(depth, prevResult);
                engine.StopHelperThreads();
            }
            else prevResult = AspirationSearch(depth, prevResult);

            if (clock.ElapsedMilliseconds >= searchTime)
            {
                //If we haven't found a move to play, and search is being cancelled, run an emergency full width search to a depth of 1
                /*if (bestMove.data == 0)
                {
                    Console.WriteLine("info string Running emergency search");
                    AlphaBeta(1, 0, NegativeInfinity, PositiveInfinity); //FIXME: Emergency search doesn't help at all if clock has run out, because search will return 0 immediatly
                }*/

                LogSearchInfo(depth, nodeCount, true, threadID);
                break;
            }
            else LogSearchInfo(depth, nodeCount, false, threadID); //TODO: check if matescore and then exit if were low on time
        }


        clock.Stop();

        callback.Invoke(bestMove, threadID);
    }

    public (Move bestMove, int eval) StartInternalSearch(int time)
    {
        bestMove = Move.nullMove;
        bestEval = NegativeInfinity;
        repetitionTable.Copy(board.repetitionTable);

        nodeCount = -1; //Dont want to include start node

        clock.Reset();

        moveOrdering.DecayHistory();

        searchTime = time;
        clock.Start();


        int prevResult = NegativeInfinity;

        Transposition lastTTEntry = transpositionTable.Probe(board.currentZobrist);

        if (lastTTEntry.IsValid())
        {
            //TODO: Should check that 1 is the correct value to pass when correcting - not very important at all, since only applicable in positions with a forced mate
            prevResult = TranspositionTable.CorrectRetrievedMateScore(lastTTEntry.eval, 1); //Use TT eval of current position as guess of current eval
        }


        for (uint depth = 1; depth <= searchDepth; depth++)
        {
            if (engine != null)
            {
                engine.StartHelperThreads((int)depth);
                prevResult = AspirationSearch(depth, prevResult);
                engine.StopHelperThreads();
            }
            else prevResult = AspirationSearch(depth, prevResult);

            if (clock.ElapsedMilliseconds >= searchTime)
            {
                break;
            }
        }

        clock.Stop();

        return (bestMove, bestEval);
    }





    #region Aspiration Window
    //public static class AspirationWindow //TODO: Try making non-static
    //{
    private readonly static int[] windowIncrements =
    {
        TunableConstants.AspWindowIncrement0, TunableConstants.AspWindowIncrement1, TunableConstants.AspWindowIncrement2, TunableConstants.AspWindowIncrement3,
        TunableConstants.AspWindowIncrement4, TunableConstants.AspWindowIncrement5, TunableConstants.AspWindowIncrement6
    };

    private int AspirationSearch(uint depth, int prevResult) //TODO: Think maybe the illegal moves and infinite evals come from not getting a proper width search before search is cancelled so we have to use the capped eval???
    {
        int incrementIndex = 0;

        int alpha = prevResult - windowIncrements[incrementIndex];
        int beta = prevResult + windowIncrements[incrementIndex];

        if (prevResult == NegativeInfinity) //If no previous result, we search with a full window
        {
            //TODO: Maybe try just centering the search around 0 - Would be a MUCH faster search, but we would ofc prob have to re-search a few times - maybe makes up for it tho?
            alpha = NegativeInfinity;
            beta = PositiveInfinity;
        }

        int result;

        while (true)
        {
            result = AlphaBeta(depth, 0, alpha, beta);

            //if ((nodeCount & CancelDelay) == 0) //Obv don't only check when canceldelay has passed, otherwise if nodecount is off by just 1 we keep running the aspiration search even if search is cancelled
            //{
            if (clock.ElapsedMilliseconds >= searchTime && !bestMove.IsNullMove()) return 0;
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
                alpha = result - TunableConstants.AspInstabilityMargin; //Have to still keep some space for search instability
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
                beta = result + TunableConstants.AspInstabilityMargin;
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
            if (clock.ElapsedMilliseconds >= searchTime && !bestMove.IsNullMove()) return 0;
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


        Transposition ttEntry = transpositionTable.Probe(board.currentZobrist);
        int ttScore = int.MinValue; //TODO: try checking that this is below AssuredWinScore before doing any pruning
        Move hashMove = Move.nullMove;

        if (ttEntry.IsValid())
        {
            ttScore = TranspositionTable.CorrectRetrievedMateScore(ttEntry.eval, plyFromRoot);
            hashMove = ttEntry.move;

            // Only use stored evaluation if it has been searched to at least the same depth as would be searched now
            if (ttEntry.depth >= depth)
            {
                //ttHits++;

                // We have stored the exact evaluation for this position, so return it
                if (ttEntry.nodeType == TranspositionTable.Exact)
                {
                    if (plyFromRoot == 0)
                    {
                        bestMove = hashMove;
                        bestEval = ttScore;
                    }
                    return ttScore;
                }
                // We have stored the upper bound of the eval for this position. If it's less than alpha then we don't need to
                // search the moves in this position as they won't interest us; otherwise we will have to search to find the exact value
                if (ttEntry.nodeType == TranspositionTable.UpperBound && ttScore <= alpha)
                {
                    if (plyFromRoot == 0)
                    {
                        bestMove = hashMove;
                        bestEval = ttScore;
                    }
                    return ttScore;
                }
                // We have stored the lower bound of the eval for this position. Only return if it causes a beta cut-off.
                if (ttEntry.nodeType == TranspositionTable.LowerBound && ttScore >= beta)
                {
                    if (plyFromRoot == 0)
                    {
                        bestMove = hashMove;
                        bestEval = ttScore;
                    }
                    return ttScore;
                }
            }
        }

        //TODO: Move this above TT cutoff - will cause problems when we start storing static eval, bc we would be assuming it is a valid substitute for q-search in case of a cutoff
        if (depth == 0)
        {
            //quiescenseCount++;
            //return Evaluation.Evaluate();
            return SearchAllCaptures(alpha, beta);
        }


        int staticEval = evaluator.Evaluate(board); //TODO: Try getting from TT if DepthFailed - could also be used to reduce margins bc we have a more reliable evaluation

        //Reverse Futility Pruning
        if (Math.Abs(beta) < AssuredWinScore && staticEval >= beta + TunableConstants.RFPMargin * depth) //TODO: Try not doing RFP if position is in check
        {
            return staticEval; //Fail soft
        }





        Span<Move> moves = stackalloc Move[256]; //TODO: Try just allocating 218, technically no need for the extra 38 at all except maybe fits nicer into memory

        int moveCount = moveGenerator.GenerateMoves(ref moves);



        bool isMoveListFullySorted = false;
        //If a hash-move is either not found, or just illegal in the current position, we sort our moves normally.
        //Otherwise we just move the hash-move to the "front" of the movelist so our search will look at it first,
        //and hopefully not need to look at any other moves, thereby avoiding having to sort all of them.
        if (hashMove.IsNullMove() || !moveOrdering.OrderHashMove(ref moves, hashMove))
        {
            moveOrdering.OrderMoves(ref moves, moveCount, plyFromRoot);
            isMoveListFullySorted = true;
        }


        //TODO: Could prob optimize to avoid this if statement
        //TODO: try this -> if (plyFromRoot == 0 && threadID % 2 == 1) moves.Reverse();//moveOrdering.ThreadRootShuffle(ref moves, moveCount, threadShuffle);

        //TODOnt: move this above move ordering bc obv no reason to try to do move ordering if there aren't any moves - somehow basically makes zero to worse difference
        if (moveCount == 0) //Maybe check if moveCount = 1 && plyFromRoot == 0 to return bc force move
        {
            //Debug.Log("Found Mate");
            if (moveGenerator.inCheck) return -(ImmediateMateScore - plyFromRoot); //Checkmate

            return 0; //Stalemate
        }

        //Razoring
        if (depth <= 3) //TODO: && !isPV //TODO: Should be limited to positions in which alpha < big number, bc otherwise we risk being blind to potential mates (according to the CPW)
        {
            if (staticEval + TunableConstants.RazoringMargin * depth <= alpha)
            {
                int score = SearchAllCaptures(alpha, alpha + 1);
                if (score <= alpha) return score;
            }
        }

        //Null-Move pruning
        if (depth > 3 && !moveGenerator.inCheck)
        {
            if (evaluator.GetRawPhase(board) < 24) // if still reasonably far from being in the endgame
            {
                board.MakeNullMove();
                uint nullReduction = 3;
                int nullEval = -AlphaBeta(depth - nullReduction, plyFromRoot + 1, -beta, -(beta - 1), numExtensions);
                board.UnMakeNullMove();

                if ((nodeCount & CancelDelay) == 0)
                {
                    if (clock.ElapsedMilliseconds >= searchTime && !bestMove.IsNullMove()) return 0;
                }

                if (nullEval >= beta) return nullEval;
            }
        }



        Move bestMoveInPosition = Move.nullMove;
        ulong transpositionBound = TranspositionTable.UpperBound;

        if (plyFromRoot > 0) repetitionTable.Push(board.currentZobrist);


        for (int i = 0; i < moveCount; i++)
        {
            board.MakeMove(moves[i], true);

            uint extensions = 0;
            if (numExtensions < MaxExtensions)
            {
                if (moveGenerator.inCheck && depth < 2) extensions = 1;

                //TODO: try combining these - as in increment extensions, allowing them to stack (i imagine this will just be slightly worse bc rare but idk)

                int targetRank = BoardHelper.IndexToRank(moves[i].targetSquare);
                if (Piece.Type(board.Squares[moves[i].targetSquare]) == Piece.Pawn && (targetRank == 1 || targetRank == 6)) extensions = 1; //Extend when about to promote //TODO: test properly
            }

            int evaluation = NegativeInfinity;
            bool searchFullDepth = true;

            //Late Move Reduction
            if (i > 2 && extensions == 0 && depth > 2)  //Assuming move ordering isn't completely wrong
            {
                uint reduction = 1;

                if (i > 5) reduction++;

                evaluation = -AlphaBeta(depth - 1 - reduction, plyFromRoot + 1, -alpha - 1, -alpha, numExtensions);

                //If evals better than anything else so far we'll search to full depth
                searchFullDepth = evaluation > alpha && !((nodeCount & CancelDelay) == 0 && clock.ElapsedMilliseconds >= searchTime && !bestMove.IsNullMove()); //TODOnt?: Move cancel check to separate if before this
            }

            if (searchFullDepth) evaluation = -AlphaBeta(depth - 1 + extensions, plyFromRoot + 1, -beta, -alpha, numExtensions + extensions);//, test);


            board.UnMakeMove(moves[i], true);


            if ((nodeCount & CancelDelay) == 0) //Makes perfect sense to have this here now - //Seemingly doesn't work properly without this check, but works fine without the check at the start of the function. Doesn't make any sense - also doesn't work do the correct amount of checks without the check at the start, but still stops in reasonable amount of time
            {
                if (clock.ElapsedMilliseconds >= searchTime && !bestMove.IsNullMove()) return 0;
            }

            //Move was too good opponent will avoid this position
            if (evaluation >= beta)
            {
                transpositionTable.StoreEvaluation(board.currentZobrist, depth, plyFromRoot, beta, TranspositionTable.LowerBound, moves[i]);

                //TODO: Test without checking for ep for performance bc we already check if this is the case in the moveordering but this will ofc override the space of a valid killer move with an invalid ep move if possible - maybe too rare?
                if (board.Squares[moves[i].targetSquare] == Piece.None && moves[i].flag != Move.Flag.EnPassantCapture) //If not a capture - only add killer moves that aren't captures, bc these are always ranked highly i guess?
                {
                    moveOrdering.UpdateHistory((int)(depth * depth * TunableConstants.HistoryBonusMultiplier), board.friendlyColorBit, moves[i].startSquare, moves[i].targetSquare);
                    //moveOrdering.history[board.friendlyColorBit][moves[i].startSquare][moves[i].targetSquare] += (int)(depth * depth); //TODO: maybe just store depth as int to avoid this?

                    if (plyFromRoot < MoveOrdering.MaxKillerPlys)
                    {
                        moveOrdering.killerMoves[plyFromRoot].Add(moves[i]);
                    }
                }

                if (plyFromRoot > 0) repetitionTable.PopNoRtn();
                return beta; //TODO: Fail-soft
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


            //If we had a hash-move in this position, looked at it in this iteration of the search,
            //and found out we still need to look at other moves in the movelist, we have to sort those now before next iteration
            if (!isMoveListFullySorted)
            {
                moveOrdering.OrderMoves(ref moves, moveCount, plyFromRoot, true);
                isMoveListFullySorted = true;
            }
        }

        if (plyFromRoot > 0) repetitionTable.PopNoRtn();

        transpositionTable.StoreEvaluation(board.currentZobrist, depth, plyFromRoot, alpha, transpositionBound, bestMoveInPosition);

        return alpha; //TODO: Fail-soft
    }


    private int SearchAllCaptures(int alpha, int beta) //TODO: maybe try including non-capture promotions - Checks??
    {
        if ((nodeCount & CancelDelay) == 0) //TODO: Try removing this
        {
            if (clock.ElapsedMilliseconds >= searchTime && !bestMove.IsNullMove()) return 0; //We are checking in the iterative part im just stupid - //From seb lague. Don't need to return 0 during the iterative part i guess, bc the main search calling this function will check if search is cancelled after this returns
        }

        // A player isn't forced to make a capture (typically), so see what the evaluation is without capturing anything.
        // This prevents situations where a player only has bad captures available from being evaluated as bad,
        // when the player might have good non-capture moves available.
        int eval = evaluator.Evaluate(board);
        //positionCount++;

        if (eval >= beta) //TODO: Should not be allowed to return standing pat when in check i believe
        {
            return beta;
        }
        if (eval > alpha)
        {
            alpha = eval;
        }

        Span<Move> moves = stackalloc Move[256];

        int moveCount = moveGenerator.GenerateMoves(ref moves, true);

        //FIXME: this should absolutely not pass -1 as the ply, as this is not checked by the moveordering. This is only saved by the fact that killer moves don't apply to captures, but if we ever search check-evasion or other non-capture moves, this will throw. Just pass a value greater than MaxKillerPlys (which will be globally accessible when made tunable anyway)
        moveOrdering.OrderMoves(ref moves, moveCount, -1); //TODO: Could prob optimize moveordering here to not worry about things that only apply to quiet moves

        for (int i = 0; i < moveCount; i++)
        {
            nodeCount++;

            board.MakeMove(moves[i], true);
            eval = -SearchAllCaptures(-beta, -alpha);
            board.UnMakeMove(moves[i], true);
            //numQNodes++;

            if ((nodeCount & CancelDelay) == 0)
            {
                if (clock.ElapsedMilliseconds >= searchTime && !bestMove.IsNullMove()) return 0;
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
        if (bestMove.IsNullMove())
        {
            //Console.WriteLine("bestmove " + FenUtility.GetCurrentFen(board) + " GHU#EGWHOIBHVDUBWRUBGUJOBJCBJOBNOWJRNOJNJNCBJNWUIORNBJUHBNWONRFIOBNWROJBNOJCNSOJBNRJOWNORJNBJUONBJONWERJONB");
            Console.WriteLine("bestmove bestMoveWasANullMove");
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

        Transposition ttEntry = transpositionTable.Probe(board.currentZobrist);

        if (!ttEntry.IsValid()) return result;


        Move move = ttEntry.move;

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