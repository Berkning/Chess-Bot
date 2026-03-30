using System;
using System.Diagnostics;
using System.Threading.Tasks;

public static class Perft
{
    private static long[] correctResults = { 1, 20, 400, 8902, 197281, 4865609, 119060324, 3195901860, 84998978956, 2439530234167 };

    //TODOne: Fix running perft on specific pos bc currently not passing engine board
    public static string RunDetailed(int depth, Board board)
    {
        MoveGenerator moveGenerator = new MoveGenerator(board);

        MoveGenerator.PromotionMode prevPromotionMode = MoveGenerator.promotionMode; //Save what the promotionmode was set to
        MoveGenerator.promotionMode = MoveGenerator.PromotionMode.All; //Set the promotion mode to all to ensure we get all possible moves

        Span<Move> moves = stackalloc Move[256];
        int moveCountInCurrentPosition = moveGenerator.GenerateMoves(ref moves);


        string results = "";

        long totalCount = 0;
        for (int i = 0; i < moveCountInCurrentPosition; i++)
        {

            ulong before = board.currentGameState;
            //Debug.Log("Trying to play " + BoardHelper.NameMove(move));
            board.MakeMove(moves[i], true);
            long result = RunSpecifiedDepth(depth - 1, board);
            Console.WriteLine(BoardHelper.GetMoveNameUCI(moves[i]) + ": " + result);
            results += BoardHelper.GetMoveNameUCI(moves[i]) + ": " + result + "\n";
            board.UnMakeMove(moves[i], true);
            ulong after = board.currentGameState;

            //Debug.Assert(before == after, "State mismatch - Before: " + before + " After:" + after);
            totalCount += result;
        }

        Console.WriteLine("Total Nodes: " + totalCount);

        MoveGenerator.promotionMode = prevPromotionMode; //Set the promotionmode back to what it was before

        return results;

        //Debug.Log("Comparative Result: " + RunSpecifiedDepth(depth));
        //RunForeachDepth(depth);
    }


    /*public static void RunForeachDepth(int depth, bool compareToCorrect = false)
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        for (int i = 0; i <= depth; i++)
        {
            stopwatch.Restart();
            long result = RunSpecifiedDepth(i);
            stopwatch.Stop();

            long timeMS = stopwatch.ElapsedMilliseconds;
            long nodesPerSecond = timeMS == 0 ? result * 1000L : result / timeMS * 1000L;

            Console.WriteLine("Depth: " + i + " ply   Result: " + result + " Positions   Time: " + timeMS + "ms   NPS: " + nodesPerSecond);
            if (compareToCorrect) Console.WriteLine("Passed: " + (result == correctResults[i]) + "   Off By: " + (correctResults[i] - result));
        }
    }*/


    /*public static async Task RunForeachDepthAsync(int depth, bool compareToCorrect = false)
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        for (int i = 0; i <= depth; i++)
        {
            stopwatch.Restart();
            long result = RunSpecifiedDepth(i);
            stopwatch.Stop();

            long timeMS = stopwatch.ElapsedMilliseconds;
            long nodesPerSecond = timeMS == 0 ? result * 1000L : result / timeMS * 1000L;

            Console.WriteLine("Depth: " + i + " ply   Result: " + result + " Positions   Time: " + timeMS + "ms   NPS: " + nodesPerSecond);
            if (compareToCorrect) Console.WriteLine("Passed: " + (result == correctResults[i]) + "   Off By: " + (correctResults[i] - result));

            await Task.Yield();
        }
    }*/

    public static long RunSpecifiedDepth(int depth, Board board)
    {
        if (depth == 0)
        {
            return 1L;
        }

        MoveGenerator moveGenerator = new MoveGenerator(board);

        Span<Move> moves = stackalloc Move[256];

        int moveCount = moveGenerator.GenerateMoves(ref moves);

        long numPositions = 0L;

        for (int i = 0; i < moveCount; i++)
        {
            board.MakeMove(moves[i], true);
            numPositions += RunSpecifiedDepth(depth - 1, board);
            board.UnMakeMove(moves[i], true);
        }

        return numPositions;
    }



    private static SuiteComponent[] testSuite = new SuiteComponent[]{
        new SuiteComponent("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", 6, 119060324),
        new SuiteComponent("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - ", 5, 193690690),
        new SuiteComponent("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1", 6, 11030083),
        new SuiteComponent("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1", 5, 15833292),
        new SuiteComponent("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8", 5,  89941194),
        new SuiteComponent("r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10", 5, 164075551),
        new SuiteComponent("r3k2r/8/8/8/8/8/8/R3K2R b KQkq - 0 1", 6, 179862938),
        new SuiteComponent("8/8/1B6/7b/7k/8/2B1b3/7K w - - 0 1", 6, 28861171),
        new SuiteComponent("R6r/8/8/2K5/5k2/8/8/r6R w - - 0 1", 5, 20506480),
        new SuiteComponent("6KQ/8/8/8/8/8/8/7k b - - 0 1", 7, 1750864),
        new SuiteComponent("8/2k1p3/3pP3/3P2K1/8/8/8/8 w - - 0 1", 9, 7618365),
        new SuiteComponent("K7/p7/k7/8/8/8/8/8 b - - 0 1", 10, 10949673),
        new SuiteComponent("8/8/3k4/3p4/8/3P4/3K4/8 b - - 0 1", 8, 7572916),
        new SuiteComponent("k7/8/8/3p4/4p3/8/8/7K w - - 0 1", 9, 5871381),
        new SuiteComponent("7k/8/8/1p6/P7/8/8/7K w - - 0 1", 9, 16217575),
        new SuiteComponent("3k4/3pp3/8/8/8/8/3PP3/3K4 w - - 0 1", 9, 121183847),
        new SuiteComponent("n1n5/1Pk5/8/8/8/8/5Kp1/5N1N w - - 0 1", 7, 690692460),
        new SuiteComponent("8/PPPk4/8/8/8/8/4Kppp/8 b - - 0 1", 7, 614154982),
        new SuiteComponent("r5k1/1p3p2/p1qb1P2/2p2RP1/2P4p/P2QBp2/1P3Pr1/3R2K1 w - - 1 1", 6, 79809396),
    };

    public static void RunFullSuite() //Wont keep current board position
    {
        MoveGenerator.PromotionMode prevPromotionMode = MoveGenerator.promotionMode; //Save what the promotionmode was set to
        MoveGenerator.promotionMode = MoveGenerator.PromotionMode.All; //Set the promotion mode to all to ensure we get all possible moves

        bool passedAll = true;

        Board board = new Board();

        MoveGenerator moveGenerator = new MoveGenerator(board);
        moveGenerator = new MoveGenerator(board);

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        for (int i = 0; i < testSuite.Length; i++)
        {
            SuiteComponent component = testSuite[i];

            FenUtility.LoadPositionFromFen(board, component.fen);
            long result = RunSpecifiedDepth(component.depth, board);
            bool passed = result == component.correctResult;

            if (!passed) passedAll = false;
            //Debug.Log("Fen: " + component.fen + " Ply: " + component.depth + " Nodes: " + result + " Passed: " + passed + (passed ? "" : " Expected: " + component.correctResult));
            Console.WriteLine("Test " + (i + 1) + '/' + testSuite.Length + " - Ply: " + component.depth + " Nodes: " + result + " Passed: " + passed + (passed ? "" : " Expected: " + component.correctResult) + "   Fen: " + component.fen);
        }

        stopwatch.Stop();

        Console.WriteLine((passedAll ? "Suite Passed in " : "Suite Failed in ") + stopwatch.ElapsedMilliseconds + "ms");

        MoveGenerator.promotionMode = prevPromotionMode;
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