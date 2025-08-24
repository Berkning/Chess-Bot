using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class Perft
{
    private static long[] correctResults = { 1, 20, 400, 8902, 197281, 4865609, 119060324, 3195901860, 84998978956, 2439530234167 };


    public static string RunDetailed(int depth)
    {
        List<Move> movesInCurrentPosition = MoveGenerator.GenerateMoves();

        string results = "";

        long totalCount = 0;
        foreach (Move move in movesInCurrentPosition)
        {

            ulong before = Board.currentGameState;
            Debug.Log("Trying to play " + BoardHelper.NameMove(move));
            Board.MakeMove(move);
            long result = RunSpecifiedDepth(depth - 1);
            Debug.Log(BoardHelper.NameMove(move) + ": " + result);
            results += BoardHelper.NameMove(move) + ": " + result + "\n";
            Board.UnMakeMove(move);
            ulong after = Board.currentGameState;

            Debug.Assert(before == after, "State mismatch - Before: " + before + " After:" + after);
            totalCount += result;
        }

        Debug.Log("Total Nodes: " + totalCount);

        return results;

        //Debug.Log("Comparative Result: " + RunSpecifiedDepth(depth));
        //RunForeachDepth(depth);
    }


    public static void RunForeachDepth(int depth, bool compareToCorrect = false)
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        for (int i = 0; i <= depth; i++)
        {
            stopwatch.Restart();
            long result = RunSpecifiedDepth(i);
            stopwatch.Stop();

            long timeMS = stopwatch.ElapsedMilliseconds;
            long nodesPerSecond = timeMS == 0 ? result * 1000L : result / timeMS * 1000L;

            Debug.Log("Depth: " + i + " ply   Result: " + result + " Positions   Time: " + timeMS + "ms   NPS: " + nodesPerSecond);
            if (compareToCorrect) Debug.Log("Passed: " + (result == correctResults[i]) + "   Off By: " + (correctResults[i] - result));
        }
    }


    public static async Task RunForeachDepthAsync(int depth, bool compareToCorrect = false)
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        for (int i = 0; i <= depth; i++)
        {
            stopwatch.Restart();
            long result = RunSpecifiedDepth(i);
            stopwatch.Stop();

            long timeMS = stopwatch.ElapsedMilliseconds;
            long nodesPerSecond = timeMS == 0 ? result * 1000L : result / timeMS * 1000L;

            Debug.Log("Depth: " + i + " ply   Result: " + result + " Positions   Time: " + timeMS + "ms   NPS: " + nodesPerSecond);
            if (compareToCorrect) Debug.Log("Passed: " + (result == correctResults[i]) + "   Off By: " + (correctResults[i] - result));

            await Task.Yield();
        }
    }

    public static long RunSpecifiedDepth(int depth)
    {
        if (depth == 0)
        {
            return 1L;
        }

        List<Move> moves = MoveGenerator.GenerateMoves();

        long numPositions = 0L;

        foreach (Move move in moves)
        {
            Board.MakeMove(move);
            numPositions += RunSpecifiedDepth(depth - 1);
            Board.UnMakeMove(move);
        }

        return numPositions;
    }
}