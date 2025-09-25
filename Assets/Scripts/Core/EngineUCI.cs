using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class EngineUCI
{
    public void RecieveCommand(string command)
    {
        string[] args = command.Split(' ');

        switch (args[0])
        {
            case "uci":
                Console.WriteLine("id name BerkBot");
                Console.WriteLine("id author Berkning");
                Console.WriteLine("uciok");
                break;
            case "isready":
                Console.WriteLine("readyok");
                break;
            case "ucinewgame":
                isNewGame = true;
                playedMoves.Clear();
                Search.transpositionTable.Clear();

                shouldAdjust = true;
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
                Search.cancelSearch = true; //TODO: move thread
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

                        shouldAdjust = true;
                    }
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
    private bool shouldAdjust = true; //Whether the table need to be adjusted on the next go

    private void AdjustTT(int maxTime)
    {
        if (autoAdjustTT && shouldAdjust)
        {
            //Only here at first move of the game, so maxTime is the games base time - except if auto changed which shouldn't matter

            shouldAdjust = false;

            if (maxTime <= 10000)//if less than 10s   //Sizes just picked arbitrarily or very vaguely based on testing
            {
                TranspositionTable.SizeMB = 8;
            }
            else if (maxTime <= 30000) TranspositionTable.SizeMB = 16; //If less than 30s
            else if (maxTime <= 60000) TranspositionTable.SizeMB = 64; //If less than 60s
            else if (maxTime <= 120000) TranspositionTable.SizeMB = 128; //If less than 2m
            else if (maxTime <= 240000) TranspositionTable.SizeMB = 256; //If less than 4m
            else if (maxTime <= 480000) TranspositionTable.SizeMB = 512; //If less than 8m
            else if (maxTime <= 900000) TranspositionTable.SizeMB = 1024; //If less than 15m
            else if (maxTime <= 1800000) TranspositionTable.SizeMB = 2048; //If less than 30m
            else TranspositionTable.SizeMB = 4096; //If anywhere above 30m

            Console.WriteLine("info string Table adjusted to " + TranspositionTable.SizeMB + "mb");

            Search.transpositionTable = new TranspositionTable(); //Adjusting after first move does clear the table but hopefully isn't that bad - could fix but idk if worth it
        }
    }


    private void InterpretGoCommand(string[] args)
    {
        Move move = Move.nullMove; //For some reason can't declare this multiple times within switch

        if (args.Length == 1)
        {
            move = Search.StartSearch(99, -2);
            Console.WriteLine("bestmove " + BoardHelper.GetMoveNameUCI(move));
            return;
        }

        switch (args[1])
        {
            case "depth":
                int depth = int.Parse(args[2]);
                move = Search.StartSearch(depth, -2);
                Console.WriteLine("bestmove " + BoardHelper.GetMoveNameUCI(move));
                return;
            case "infinite":
                move = Search.StartSearch(int.MaxValue, -2);
                Console.WriteLine("bestmove " + BoardHelper.GetMoveNameUCI(move));
                return;
            case "movetime":
                move = Search.StartSearch(99, int.Parse(args[2]));
                Console.WriteLine("bestmove " + BoardHelper.GetMoveNameUCI(move));
                return;
            case "wtime":
                int white = int.Parse(args[2]);
                int black = int.Parse(args[4]);
                TimeManagement.UpdateTimes(white, black);
                //TODO: Increments
                move = Search.StartSearch(99);
                Console.WriteLine("bestmove " + BoardHelper.GetMoveNameUCI(move));

                //Adjust TT here bc we will be waiting for opponent to respond anyway
                if (Board.colorToMove == Piece.White) AdjustTT(white); //If we are playing white
                else AdjustTT(black);

                return;
            case "perft":
                if (args[2] == "suite")
                {
                    Perft.RunFullSuite();
                    return;
                }

                Perft.RunDetailed(int.Parse(args[2]));
                return;
            case "bench":
                Benchmark.Run();
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

            FenUtility.LoadPositionFromFen(fen);

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
            Board.MakeMove(BoardHelper.GetMoveFromUCIName(args[i]));
            playedMoves.Add(args[i]);
        }
    }
}
