using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class Perft
{
    private static long[] correctResults = { 1, 20, 400, 8902, 197281, 4865609, 119060324, 3195901860, 84998978956, 2439530234167 };


    public static string RunDetailed(int depth)
    {
        MoveGenerator.PromotionMode prevPromotionMode = MoveGenerator.promotionMode; //Save what the promotionmode was set to
        MoveGenerator.promotionMode = MoveGenerator.PromotionMode.All; //Set the promotion mode to all to ensure we get all possible moves

        Span<Move> moves = stackalloc Move[256];
        int moveCountInCurrentPosition = MoveGenerator.GenerateMoves(ref moves);


        string results = "";

        long totalCount = 0;
        for (int i = 0; i < moveCountInCurrentPosition; i++)
        {

            ulong before = Board.currentGameState;
            //Debug.Log("Trying to play " + BoardHelper.NameMove(move));
            Board.MakeMove(moves[i], true);
            long result = RunSpecifiedDepth(depth - 1);
            Debug.Log(BoardHelper.NameMove(moves[i]) + ": " + result);
            results += BoardHelper.NameMove(moves[i]) + ": " + result + "\n";
            Board.UnMakeMove(moves[i], true);
            ulong after = Board.currentGameState;

            Debug.Assert(before == after, "State mismatch - Before: " + before + " After:" + after);
            totalCount += result;
        }

        Debug.Log("Total Nodes: " + totalCount);

        MoveGenerator.promotionMode = prevPromotionMode; //Set the promotionmode back to what it was before

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

        Span<Move> moves = stackalloc Move[256];

        int moveCount = MoveGenerator.GenerateMoves(ref moves);

        long numPositions = 0L;

        for (int i = 0; i < moveCount; i++)
        {
            Board.MakeMove(moves[i], true);
            numPositions += RunSpecifiedDepth(depth - 1);
            Board.UnMakeMove(moves[i], true);
        }

        return numPositions;
    }
}