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



    private static SuiteComponent[] testSuite = new SuiteComponent[]{
        new SuiteComponent("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", 6, 119060324),
        new SuiteComponent("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - ", 5, 193690690),
        new SuiteComponent("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1", 6, 11030083),
        new SuiteComponent("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1", 5, 15833292),
        new SuiteComponent("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8", 5,  89941194),
        new SuiteComponent("r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10", 5, 164075551)
    };

    public static void RunFullSuite() //Wont keep current board position
    {
        foreach (SuiteComponent component in testSuite)
        {
            FenUtility.LoadPositionFromFen(component.fen);
            long result = RunSpecifiedDepth(component.depth);
            bool passed = result == component.correctResult;
            Debug.Log("Fen: " + component.fen + " Ply: " + component.depth + " Nodes: " + result + " Passed: " + passed + (passed ? "" : " Expected: " + component.correctResult));
        }
    }

    private struct SuiteComponent
    {
        public string fen;
        public int depth;

        public long correctResult;

        public SuiteComponent(string _fen, int _depth, long _correct)
        {
            fen = _fen;
            depth = _depth;
            correctResult = _correct;
        }
    }
}