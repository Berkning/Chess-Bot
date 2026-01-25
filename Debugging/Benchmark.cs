using System;
using System.Diagnostics;

public static class Benchmark
{
    private static (string fen, int dd)[] positions = {
        ("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", 9),
        ("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - ", 8),
        ("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1 ", 13),
        ("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1", 10),
        ("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8  ", 8),
        ("r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10 ", 8),
        ("r1bq1r1k/b1p1npp1/p2p3p/1p6/3PP3/1B2NN2/PP3PPP/R2Q1RK1 w - - 1 16", 99),
        ("2K5/p7/7P/5pR1/8/5k2/r7/8 w - - 4 3", 99),
        ("8/6pk/1p6/8/PP3p1p/5P2/4KP1q/3Q4 w - - 0 1", 99),
        ("8/2p5/8/2kPKp1p/2p4P/2P5/3P4/8 w - - 0 1", 99),
        ("8/1p3pp1/7p/5P1P/2k3P1/8/2K2P2/8 w - - 0 1", 99),
        ("8/pp2r1k1/2p1p3/3pP2p/1P1P1P1P/P5KR/8/8 w - - 0 1", 99),
        ("8/3p4/p1bk3p/Pp6/1Kp1PpPp/2P2P1P/2P5/5B2 b - - 0 1", 99),
        ("5k2/7R/4P2p/5K2/p1r2P1p/8/8/8 b - - 0 1", 99),
        ("6k1/6p1/P6p/r1N5/5p2/7P/1b3PP1/4R1K1 w - - 0 1", 99),
        ("1r3k2/4q3/2Pp3b/3Bp3/2Q2p2/1p1P2P1/1P2KP2/3N4 w - - 0 1", 99),
        ("6k1/4pp1p/3p2p1/P1pPb3/R7/1r2P1PP/3B1P2/6K1 w - - 0 1", 99),
        ("8/3p3B/5p2/5P2/p7/PP5b/k7/6K1 w - - 0 1", 99),
        ("r3k2r/3nnpbp/q2pp1p1/p7/Pp1PPPP1/4BNN1/1P5P/R2Q1RK1 w kq - 0 16", 99),
        ("4k3/3q1r2/1N2r1b1/3ppN2/2nPP3/1B1R2n1/2R1Q3/3K4 w - - 5 1", 99),
        ("8/3k4/8/8/8/4B3/4KB2/2B5 w - - 0 1", 99),
        ("8/8/3P3k/8/1p6/8/1P6/1K3n2 b - - 0 1", 99),
        ("8/8/8/8/8/6k1/6p1/6K1 w - - 0 1", 99),
        ("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", 99),
    };

    private static Action<Move, int> callback = (result, id) => RunFinished(result, id);
    private static Search search;
    private static Board board;

    private static Stopwatch stopwatch = new Stopwatch();
    private static int totalNodes = 0;
    private static int currentIndex = 0;


    public static void Run()
    {
        Stopwatch stopwatch = new Stopwatch();

        board = new Board();

        search = new Search(board, callback, 0);
        search.searchTime = -2;
        stopwatch.Reset();
        currentIndex = -1;
        totalNodes = 0;

        //Kinda jank
        RunFinished(Move.nullMove, -1);
    }

    private static void RunFinished(Move result, int id)
    {
        stopwatch.Stop();

        if (currentIndex > -1)
        {
            totalNodes += search.nodeCount;
            Console.WriteLine("Benchmark " + (currentIndex + 1) + '/' + positions.Length + " - Time: " + stopwatch.ElapsedMilliseconds + "ms   Fen: " + positions[currentIndex].fen);
        }

        currentIndex++;


        if (currentIndex == positions.Length)
        {
            Console.WriteLine("Benchmark finished in " + stopwatch.ElapsedMilliseconds + "ms");

            Console.WriteLine(totalNodes + " nodes " + Math.Round(totalNodes / stopwatch.Elapsed.TotalSeconds) + " nps");
            return;
        }

        Search.transpositionTable.Clear();
        FenUtility.LoadPositionFromFen(board, positions[currentIndex].fen);
        search.searchDepth = 99;
        search.searchTime = 1000;

        stopwatch.Start();
        search.StartSearch();
    }
}