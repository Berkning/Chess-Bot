using System;
using System.Diagnostics;

public static class Benchmark
{
    private static (string fen, int depth)[] positions = {
        ("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", 8),
        ("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - ", 8),
        ("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1 ", 13),
        ("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1", 10),
        ("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8  ", 8),
        ("r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10 ", 8),
    };

    /*public static void Run() //FIXME:
    {
        long totalTime = 0;
        Stopwatch stopwatch = new Stopwatch();

        for (int i = 0; i < positions.Length; i++)
        {
            Search.transpositionTable.Clear();
            FenUtility.LoadPositionFromFen(positions[i].fen);

            stopwatch.Restart();
            //Search.StartSearch(positions[i].depth, -2);
            stopwatch.Stop();

            Console.WriteLine("Benchmark " + (i + 1) + '/' + positions.Length + " - Ply: " + positions[i].depth + " Time: " + stopwatch.ElapsedMilliseconds + "ms   Fen: " + positions[i].fen);
            totalTime += stopwatch.ElapsedMilliseconds;
        }

        Console.WriteLine("Benchmark finished in " + totalTime + "ms");
    }*/
}