using System;
using System.Diagnostics;

public static class Benchmark
{
    private static (string fen, int depth)[] positions = {
        ("f", 2),

    };

    public static void Run()
    {
        long totalTime = 0;
        Stopwatch stopwatch = new Stopwatch();

        for (int i = 0; i < positions.Length; i++)
        {
            Search.transpositionTable.Clear();
            FenUtility.LoadPositionFromFen(positions[i].fen);

            stopwatch.Restart();
            Search.StartSearch(positions[i].depth, -2);
            stopwatch.Stop();

            Console.WriteLine("Benchmark " + (i + 1) + '/' + positions.Length + " - Ply: " + positions[i].depth + " Time: " + stopwatch.ElapsedMilliseconds + "ms   Fen: " + positions[i].fen);
            totalTime += stopwatch.ElapsedMilliseconds;
        }

        Console.WriteLine("Benchmark finished in " + totalTime + "ms");
    }
}