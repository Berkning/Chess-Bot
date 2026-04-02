using System.Diagnostics;
using System.Numerics;
using System.Runtime;
using System.Runtime.CompilerServices;

public class EngineUCI //TODO: GCsettings + TODO: https://learn.microsoft.com/en-us/dotnet/api/system.gc.trystartnogcregion?view=net-9.0 TODO: Adjust GCsettings and stuff on the fly based on the time we have left (time to search) and force collect when its our opponents turn
{
    private Engine engine;

    public EngineUCI()
    {
        engine = new Engine(this);
    }


    public void RecieveCommand(string command)
    {
        string[] args = command.Split(' ');

        switch (args[0])
        {
            case "uci":
                Console.WriteLine("id name BerkBot");
                Console.WriteLine("id author Berkning");

                Console.WriteLine("option name Hash type spin default 16 min 1 max 1024");
                Console.WriteLine("option name Threads type spin default 1 min 1 max 256");
                Console.WriteLine("option name Ponder type check default false");

                Console.WriteLine("uciok");
                break;
            case "isready":
                //AdjustTT();
                OpeningBook.Initialize();
                Console.WriteLine("readyok"); //TODOcant: Try adjusting TT here
                break;
            case "ucinewgame":
                isNewGame = true;
                playedMoves.Clear();
                Search.transpositionTable.Clear();
                //TODO: reset killers and history as well

                hasAdjustedThisGame = false;
                break;
            case "setoption":
                switch (args[2])
                {
                    case "Hash":
                        uint size = uint.Parse(args[4]);

                        if (!BitOperations.IsPow2(size))
                        {
                            uint upper = BitOperations.RoundUpToPowerOf2(size);
                            uint lower = upper >> 1;
                            uint lowerDif = size - lower;
                            uint upperDif = upper - size;

                            Console.WriteLine("Hash size not power of two. Value is between bounds: [" + upper + " > " + size + " > " + lower + "] Where " + (upperDif > lowerDif ? "lower is closest" : "upper is closest"));

                            if (upperDif > lowerDif && lower != 0) //If lower is closest and not 0
                            {
                                size = lower;
                            }
                            else if (upper < 1025) //If upper is closest or the same distance and not more than 2048
                            {
                                size = upper;
                            }
                            else size = lower; //If upper is closest or the same distance, but is larger than 2048
                        }

                        TranspositionTable.SizeMB = (int)size;
                        Search.transpositionTable = new TranspositionTable();
                        Console.WriteLine("info string Set Transposition Table size to " + TranspositionTable.SizeMB + "MB");
                        break;
                    case "Threads":
                        int count = int.Parse(args[4]);

                        if (count < 1)
                        {
                            Console.WriteLine("Threads cannot be less than 1");
                            break;
                        }

                        engine.SetThreadCount(count);
                        Console.WriteLine("info string Set Threads to " + count);
                        break;
                    case "Ponder":
                        Console.WriteLine("Currently not implemented");
                        break;
                    default:
                        Console.WriteLine("No such option as " + args[2]);
                        break;
                }
                break;
            case "position":
                InterpretPositionCommand(args);
                break;
            case "go":
                InterpretGoCommand(args); //TODO: add go perft 'depth' //FIXME: dont log cv 0 at partial depth
                break;
            case "time":
                Stopwatch timer = Stopwatch.StartNew();
                RecieveCommand(command.Remove(0, 5));
                timer.Stop();
                Console.WriteLine("Timer stopped at " + timer.ElapsedMilliseconds + "ms");
                break;
            case "stop":
                //TimeManagement.RevokeScheduledCancel();
                //Search.cancelSearch = true; //TODOne: move thread
                engine.StopSearch();
                break;
            case "table":
                if (args.Length == 1)
                {
                    break;
                }
                else if (args[1] == "clear")
                {
                    Search.transpositionTable.Clear();
                    Console.WriteLine("info string Table Cleared");
                }
                else if (args[1] == "size")
                {
                    if (args.Length == 2) Console.WriteLine(TranspositionTable.SizeMB);
                    else
                    {
                        TranspositionTable.SizeMB = int.Parse(args[2]);
                        Search.transpositionTable = new TranspositionTable();
                        Console.WriteLine("info string Set Transposition Table size to " + TranspositionTable.SizeMB + "MB");
                    }
                }
                else if (args[1] == "auto")
                {
                    if (args.Length == 2) Console.WriteLine(autoAdjustTT);
                    else
                    {
                        autoAdjustTT = bool.Parse(args[2]);
                        Console.WriteLine("info string Auto Adjusting " + (autoAdjustTT ? "Enabled" : "Disabled"));
                    }
                }
                else if (args[1] == "usage")
                {
                    Console.WriteLine("Transposition table is " + Search.transpositionTable.GetFilledPercent() + "% full");
                    //Console.WriteLine("Transposition table is " + Search.transpositionTable.GetTest() + "% full");
                }
                break;
            case "pv":
                if (args.Length == 1)
                {
                    Console.WriteLine(Search.logFullPV);
                    break;
                }

                bool value = bool.Parse(args[1]);
                Search.logFullPV = value;

                Console.WriteLine("Logging Full PV " + (value ? "Enabled" : "Disabled"));
                break;
            case "version":
                Console.WriteLine("Currently Running Version 1.14.8 FINAL");
                //Console.WriteLine("Currently Running Version 1.14.7 FINAL");
                break;
            case "numThreads":
                if (args.Length == 1) Console.WriteLine(engine.threadCount);
                else
                {
                    int count = int.Parse(args[1]);

                    if (count < 1)
                    {
                        Console.WriteLine("numThreads cannot be less than 1");
                        break;
                    }

                    engine.SetThreadCount(count);
                    Console.WriteLine("info string Set numThreads to " + count);
                }
                break;
            case "book":
                if (args.Length == 1) Console.WriteLine(OpeningBook.bookPath);
                else
                {
                    string path = args[1];
                    OpeningBook.bookPath = path;

                    OpeningBook.Initialize();

                    Console.WriteLine("Set book path to " + OpeningBook.bookPath);
                }
                break;
            case "d":
                ulong bitBoard = 0UL;

                if (args.Length > 1)
                {
                    bitBoard = ulong.Parse(args[1]);
                }

                ASCIIBoardDrawer.DrawBoard(Engine.mainBoard, bitBoard);
                Console.WriteLine(" ");
                Console.WriteLine("Fen: " + FenUtility.GetCurrentFen(Engine.mainBoard));
                Console.WriteLine(" ");
                Console.WriteLine("Zobrist: " + Convert.ToString((long)Engine.mainBoard.currentZobrist, 16));
                break;
            case "test":
                if (args.Length == 1) Console.WriteLine(/*MoveOrdering.jitterBias*/"Disabled");
                else
                {
                    int v = int.Parse(args[1]);
                    ulong result = (ulong)v;

                    Console.WriteLine("Unchecked: " + unchecked((ulong)v));
                    Console.WriteLine("Checked: " + (ulong)v);
                    Console.WriteLine("Uint: " + (uint)v);

                    Console.WriteLine("Bits in signed: " + Convert.ToString((int)result, 2));
                    //Console.WriteLine("Bits in unsigned: " + )

                    Console.WriteLine("Back: " + (int)result);
                    Console.WriteLine(TranspositionTable.Transposition.GetSize());
                    Console.WriteLine(Unsafe.SizeOf<TranspositionTable.Transposition>());

                    TranspositionTable.Transposition test = new TranspositionTable.Transposition(0b1010111100001111111111100000001111111100000100100100100100100111, -624, 23, 2, new Move(0b1110100010111110));

                    Console.WriteLine(Convert.ToString((long)test.data, 2));

                    Console.WriteLine((ulong)(long)test.key == test.key);

                    Console.WriteLine(Convert.ToString((long)test.key, 2));
                    Console.WriteLine(test.eval);
                    Console.WriteLine(test.depth);
                    Console.WriteLine(test.nodeType);
                    Console.WriteLine(Convert.ToString(test.move.data, 2));

                    Console.WriteLine("Piece Values ------------");
                    Console.WriteLine("Pawn: " + Evaluation.GetPieceTypeValue(Piece.Pawn));
                    Console.WriteLine("Knight: " + Evaluation.GetPieceTypeValue(Piece.Knight));
                    Console.WriteLine("Bishop: " + Evaluation.GetPieceTypeValue(Piece.Bishop));
                    Console.WriteLine("Rook: " + Evaluation.GetPieceTypeValue(Piece.Rook));
                    Console.WriteLine("Queen: " + Evaluation.GetPieceTypeValue(Piece.Queen));

                    Console.WriteLine("Vector Length: " + Vector<int>.Count);
                }
                break;
            case "gc":
                if (args.Length == 1) Console.WriteLine(Enum.GetName(typeof(GCLatencyMode), GCSettings.LatencyMode));
                else
                {
                    switch (args[1])
                    {
                        case "Toggle":
                            if (GCSettings.LatencyMode != GCLatencyMode.NoGCRegion) GC.TryStartNoGCRegion(256 * 1000 * 1000);
                            else GC.EndNoGCRegion();
                            break;
                        case "Batch":
                            GCSettings.LatencyMode = GCLatencyMode.Batch;
                            break;
                        case "Interactive":
                            GCSettings.LatencyMode = GCLatencyMode.Interactive;
                            break;
                        case "LowLatency":
                            GCSettings.LatencyMode = GCLatencyMode.LowLatency;
                            break;
                        case "SustainedLowLatency":
                            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
                            break;
                    }
                }
                break;
            case "bench":
                Benchmark.Run();
                break;
            case "runDiagnostics":
                EngineDiagnostics.RunDiagnostics();
                break;




                /*case "ucinewgame":
                    break;
                case "position":
                    Program.pipe.nextCommand = command;
                    break;
                case "go":
                    Program.pipe.nextCommand = command;
                    break;
                case "stop":
                    Program.pipe.nextCommand = command;
                    break;
                default:
                    Program.pipe.nextCommand = command; //If command not recognized here we should forward it to the engine
                    break;*/
        }
    }



    private bool autoAdjustTT = true;
    private bool hasAdjustedThisGame = false; //TODO: Just use isnewgame

    public void AdjustTT() //TODO: Should prob check if TT is already optimal size to avoid wasting first results if unnecessary - TODOne: When implementing opening book we can just adjust it there and then not waste any of the TT results from first move
    {
        if (autoAdjustTT && !hasAdjustedThisGame)
        {
            //Only here at first move of the game, so maxTime is the games base time - except if auto changed which shouldn't matter

            hasAdjustedThisGame = true;

            int maxTime = Engine.mainBoard.colorToMove == Piece.White ? TimeManagement.whiteTime : TimeManagement.blackTime; //Does mean we are white because the move we picked hasn't happened on our board yet - TODO: delete comment if not how adjusting works anymore

            float threadMultiplier = 1f + (engine.threadCount - 1) * 0.5f; //T : M ||| 1 : 1 ||| 2 : 1.5 ||| 3 : 2 ||| 4 : 2.5 ||| 5 : 3 ||| 6 : 3.5 ||| ... ||| 22 : 11.5 ||| 23 : 12 ||| 24 : 12.5

            //TODO: Prob make continous instead of incremental like this - would also mean float, better mult with threadmult
            if (maxTime <= 10000)//if less than 10s   //Sizes just picked arbitrarily or very vaguely based on testing
            {
                TranspositionTable.SizeMB = 8;
            }
            else if (maxTime <= 30000) TranspositionTable.SizeMB = 16; //If less than 30s
            else if (maxTime <= 60000) TranspositionTable.SizeMB = 64; //If less than 60s //TODO: Try halving this and everything after it
            else if (maxTime <= 120000) TranspositionTable.SizeMB = 128; //If less than 2m
            else if (maxTime <= 240000) TranspositionTable.SizeMB = 256; //If less than 4m
            else if (maxTime <= 480000) TranspositionTable.SizeMB = 512; //If less than 8m
            else if (maxTime <= 900000) TranspositionTable.SizeMB = 1024; //If less than 15m
            else if (maxTime <= 1800000) TranspositionTable.SizeMB = 2048; //If less than 30m
            else TranspositionTable.SizeMB = 4096; //If anywhere above 30m
            //TODO: longer games should cap at sys ram

            Console.WriteLine("info string Table adjusted to " + TranspositionTable.SizeMB + "mb"); //TODO: cap based on available system ram

            Search.transpositionTable = new TranspositionTable(); //Adjusting after first move does clear the table but hopefully isn't that bad - could fix but idk if worth it
        }
    }


    private void InterpretGoCommand(string[] args)
    {
        if (args.Length == 1)
        {
            engine.InitializeSearch(99, -2);
            return;
        }

        switch (args[1])
        {
            case "depth":
                int depth = int.Parse(args[2]);
                engine.InitializeSearch(depth, -2);
                return;
            case "infinite":
                engine.InitializeSearch(int.MaxValue, -2);
                return;
            case "movetime":
                engine.InitializeSearch(99, int.Parse(args[2]));
                return;
            case "wtime":
                int white = int.Parse(args[2]);
                int black = int.Parse(args[4]);
                TimeManagement.UpdateTimes(white, black);

                //TODO: Increments
                engine.InitializeSearch(99, -1);

                //Adjust TT here bc we will be waiting for opponent to respond anyway
                //if (Board.colorToMove == Piece.White) AdjustTT(white); //If we are playing white //FIXedME: fix auto adjust to only run when search thread has returned
                //else AdjustTT(black);


                return;
            case "perft":
                if (args[2] == "suite")
                {
                    Perft.RunFullSuite();
                    return;
                }

                Perft.RunDetailed(int.Parse(args[2]), Engine.mainBoard);
                return;
            case "mate":
                engine.RunMateSearch();
                return;
            case "bench":
                //Benchmark.Run();
                return;
        }
    }





    private bool isNewGame = true;
    private List<string> playedMoves = new List<string>();
    private int moveStartIndex = -1;

    private void InterpretPositionCommand(string[] args)
    {
        if (isNewGame)
        {
            string fen = "";
            moveStartIndex = -1;
            bool shouldPlayMoves = true;

            if (args[1] == "fen")
            {
                //Construct fen from args
                for (int i = 2; i < args.Length; i++)
                {
                    if (args[i] == "moves")
                    {
                        moveStartIndex = i + 1;
                        break;
                    }

                    fen += args[i] + ' ';
                }

                if (moveStartIndex == -1)
                {
                    moveStartIndex = args.Length + 1;
                    shouldPlayMoves = false; //Shouldn't play moves yet bc they dont exist
                }
            }
            else
            {
                fen = args[1]; //Prob startpos
                moveStartIndex = 3;

                if (args.Length < 4) shouldPlayMoves = false; //Shouldn't play moves yet bc they dont exist
            }

            //Console.WriteLine(moveStartIndex);

            //FenUtility.LoadPositionFromFen(Engine.board, fen);
            engine.LoadFen(fen);

            if (moveStartIndex == -1)
            {
                Console.WriteLine("MoveStartIndex Not set");
                return;
            }

            if (shouldPlayMoves) PlayUCIMoves(args, moveStartIndex);

            isNewGame = false;
        }

        if (moveStartIndex == -1)
        {
            Console.WriteLine("MoveStartIndex Not set");
            return;
        }

        PlayUCIMoves(args, moveStartIndex);
    }

    private void PlayUCIMoves(string[] args, int moveStartIndex)
    {
        for (int i = moveStartIndex; i < args.Length; i++)
        {
            int playedMovesIndex = i - moveStartIndex;

            //If move has already been played we skip to the next one
            if (playedMoves.Count > playedMovesIndex && playedMoves[playedMovesIndex] == args[i]) continue;

            //Console.WriteLine("Found new move to play: " + args[i]);
            //Engine.board.MakeMove(BoardHelper.GetMoveFromUCIName(Engine.board, args[i]));
            engine.PlayMove(BoardHelper.GetMoveFromUCIName(Engine.mainBoard, args[i]));


            playedMoves.Add(args[i]);
        }
    }
}
