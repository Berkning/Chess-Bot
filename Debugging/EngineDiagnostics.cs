

public static class EngineDiagnostics
{
    private static readonly DiagnosticPosition[] positions = new DiagnosticPosition[]
    {
        new DiagnosticPosition("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"),
        new DiagnosticPosition("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - "),
        new DiagnosticPosition("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1"),
        new DiagnosticPosition("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1"),
        new DiagnosticPosition("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8"),
        new DiagnosticPosition("r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10"),
        new DiagnosticPosition("r3k2r/8/8/8/8/8/8/R3K2R b KQkq - 0 1"),
        new DiagnosticPosition("8/8/1B6/7b/7k/8/2B1b3/7K w - - 0 1"),
        new DiagnosticPosition("R6r/8/8/2K5/5k2/8/8/r6R w - - 0 1"),
        new DiagnosticPosition("6KQ/8/8/8/8/8/8/7k b - - 0 1"),
        new DiagnosticPosition("8/2k1p3/3pP3/3P2K1/8/8/8/8 w - - 0 1"),
        new DiagnosticPosition("K7/p7/k7/8/8/8/8/8 b - - 0 1"),
        new DiagnosticPosition("8/8/3k4/3p4/8/3P4/3K4/8 b - - 0 1"),
        new DiagnosticPosition("k7/8/8/3p4/4p3/8/8/7K w - - 0 1"),
        new DiagnosticPosition("7k/8/8/1p6/P7/8/8/7K w - - 0 1"),
        new DiagnosticPosition("3k4/3pp3/8/8/8/8/3PP3/3K4 w - - 0 1"),
        new DiagnosticPosition("n1n5/1Pk5/8/8/8/8/5Kp1/5N1N w - - 0 1"),
        new DiagnosticPosition("8/PPPk4/8/8/8/8/4Kppp/8 b - - 0 1"),
        new DiagnosticPosition("r5k1/1p3p2/p1qb1P2/2p2RP1/2P4p/P2QBp2/1P3Pr1/3R2K1 w - - 1 1"),
        new DiagnosticPosition("3k4/3p4/8/K1P4r/8/8/8/8 b - - 0 1"),
        new DiagnosticPosition("8/8/4k3/8/2p5/8/B2P2K1/8 w - - 0 1"),
        new DiagnosticPosition("8/8/1k6/2b5/2pP4/8/5K2/8 b - d3 0 1"),
        new DiagnosticPosition("5k2/8/8/8/8/8/8/4K2R w K - 0 1"),
        new DiagnosticPosition("3k4/8/8/8/8/8/8/R3K3 w Q - 0 1"),
        new DiagnosticPosition("r3k2r/1b4bq/8/8/8/8/7B/R3K2R w KQkq - 0 1"),
        new DiagnosticPosition("r3k2r/8/3Q4/8/8/5q2/8/R3K2R b KQkq - 0 1"),
        new DiagnosticPosition("2K2r2/4P3/8/8/8/8/8/3k4 w - - 0 1"),
        new DiagnosticPosition("8/8/1P2K3/8/2n5/1q6/8/5k2 b - - 0 1"),
        new DiagnosticPosition("4k3/1P6/8/8/8/8/K7/8 w - - 0 1"),
        new DiagnosticPosition("8/P1k5/K7/8/8/8/8/8 w - - 0 1"),
        new DiagnosticPosition("K1k5/8/P7/8/8/8/8/8 w - - 0 1"),
        new DiagnosticPosition("8/k1P5/8/1K6/8/8/8/8 w - - 0 1"),
        new DiagnosticPosition("8/8/2k5/5q2/5n2/8/5K2/8 b - - 0 1"),
    };

    private const int MovesPerPosition = 12;
    private const int VerificationDepth = 6;


    private static Board board = new Board();
    private static MoveGenerator moveGenerator = new MoveGenerator(board);

    public static void RunDiagnostics()
    {
        Console.WriteLine("");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\x1b[1mRunning Engine Diagnostics...\x1b[0m");
        Console.ResetColor();


        Console.WriteLine("");
        Console.WriteLine("\x1b[1m#1 Verifying Make/UnMake Move...\x1b[0m");
        VerifyMakeUnmake();

        Console.WriteLine("");
        Console.WriteLine("\x1b[1m#2 Verifying Search...\x1b[0m");
        VerifyMakeUnmake();


        //TODO: Verify null moves


        //Always keep this last bc user can just run perft separately
        Console.WriteLine("");
        Console.WriteLine("\x1b[1m#3 Verifying MoveGen...\x1b[0m");
        VerifyMoveGeneration();
    }

    private static void VerifyMoveGeneration()
    {
        bool passed = Perft.RunFullSuite();

        Console.ForegroundColor = passed ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine(passed ? "Move Generation \x1b[1mPassed ✅\x1b[0m" : "Move Generation \x1b[1mFailed ❌\x1b[0m");
        Console.ResetColor();
    }

    #region Make/UnMake
    private static void VerifyMakeUnmake()
    {
        bool passed = true;

        for (int i = 0; i < positions.Length; i++)
        {
            FenUtility.LoadPositionFromFen(board, positions[i].fen);

            bool p = VerifyMakeUnmakeRecursive(VerificationDepth, MovesPerPosition, new Stack<Move>(), positions[i].fen);

            passed = passed && p;

            Console.WriteLine("Position " + (i + 1) + "/" + positions.Length + (p ? " Passed" : " Failed"));
        }

        Console.ForegroundColor = passed ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine(passed ? "Make/UnMake Verification \x1b[1mPassed ✅\x1b[0m" : "Make/UnMake Verification \x1b[1mFailed ❌\x1b[0m");
        Console.ResetColor();
    }

    private static bool VerifyMakeUnmakeRecursive(int depth, int movesPerPos, Stack<Move> moveHistory, string startFen) //TODO: finish - return list of moves that lead to fail
    {
        bool passed = true;

        Span<Move> moves = stackalloc Move[256];

        int moveCount = moveGenerator.GenerateMoves(ref moves);

        for (int i = 0; i < Math.Min(movesPerPos, moveCount); i++)
        {
            int randomMoveIndex = Random.Shared.Next(moveCount);

            BoardSnapshot snapshot = new BoardSnapshot(board);

            moveHistory.Push(moves[randomMoveIndex]);

            board.MakeMove(moves[randomMoveIndex], true);


            if (depth > 0)
            {
                bool p = VerifyMakeUnmakeRecursive(depth - 1, movesPerPos, moveHistory, startFen);
                passed = passed && p;
            }


            board.UnMakeMove(moves[randomMoveIndex], true);

            if (!snapshot.Verify(board))
            {
                passed = false;

                string moveString = "";
                for (int m = 0; m < moveHistory.Count; m++)
                {
                    moveString += BoardHelper.GetMoveNameUCI(moveHistory.ElementAt(m)) + " ";
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("State was corrupted from position: " + startFen + " after moves: " + moveString + " Current fen is: " + FenUtility.GetCurrentFen(board));
                Console.ResetColor();
            }

            moveHistory.Pop();
        }



        return passed;
    }
    #endregion

    #region Search
    private static void VerifySearch()
    {
        bool passed = true;

        Search search = new Search(board, new Action<Move, int>(SearchReturn), 0, null);
        search.searchTime = 1000;

        for (int i = 0; i < positions.Length; i++)
        {
            FenUtility.LoadPositionFromFen(board, positions[i].fen);

            BoardSnapshot snapshot = new BoardSnapshot(board);
            search.StartSearch();

            bool p = true;

            if (!snapshot.Verify(board))
            {
                Console.WriteLine("Search Corrupted Board State in Position: " + positions[i]);
                p = false;
            }

            passed = passed && p;

            Console.WriteLine("Position " + (i + 1) + "/" + positions.Length + (p ? " Passed" : " Failed"));
        }

        Console.ForegroundColor = passed ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine(passed ? "Search Verification \x1b[1mPassed ✅\x1b[0m" : "Search Verification \x1b[1mFailed ❌\x1b[0m");
        Console.ResetColor();
    }

    private static void SearchReturn(Move move, int i)
    {

    }
    #endregion

    #region Make/UnMake Null-Move
    #endregion





    private struct DiagnosticPosition
    {
        public string fen;

        public DiagnosticPosition(string _fen)
        {
            fen = _fen;
        }
    }

    private struct BoardSnapshot
    {
        private ulong zobrist;
        private uint gameState;
        private int[] squares;
        private int colorToMove;
        private int friendlyColor;
        private int enemyColor;
        private int whiteKingSquare;
        private int blackKingSquare;

        private int opponentColorBit;
        private int friendlyColorBit;
        private int repetitionTableCount;

        public bool Verify(Board board)
        {
            bool success = true;

            if (zobrist != board.currentZobrist)
            {
                Console.WriteLine("Zobrist Corrupted");
                success = false;
            }

            if (gameState != board.currentGameState)
            {
                Console.WriteLine("Gamestate Corrupted");
                success = false;
            }

            if (squares != board.Squares)
            {
                Console.WriteLine("Square-Array Corrupted");
                success = false;
            }

            if (colorToMove != board.colorToMove)
            {
                Console.WriteLine("colorToMove Corrupted");
                success = false;
            }

            if (friendlyColor != board.friendlyColor)
            {
                Console.WriteLine("friendlyColor Corrupted");
                success = false;
            }

            if (enemyColor != board.enemyColor)
            {
                Console.WriteLine("enemyColor Corrupted");
                success = false;
            }

            if (whiteKingSquare != board.whiteKingSquare)
            {
                Console.WriteLine("whiteKingSquare Corrupted");
                success = false;
            }

            if (blackKingSquare != board.blackKingSquare)
            {
                Console.WriteLine("blackKingSquare Corrupted");
                success = false;
            }

            if (opponentColorBit != board.opponentColorBit)
            {
                Console.WriteLine("opponentColorBit Corrupted");
                success = false;
            }

            if (friendlyColorBit != board.friendlyColorBit)
            {
                Console.WriteLine("friendlyColorBit Corrupted");
                success = false;
            }

            if (repetitionTableCount != board.repetitionTable.Count)
            {
                Console.WriteLine("repetitionTable Count Corrupted");
                success = false;
            }

            return success;
        }

        public BoardSnapshot(Board board)
        {
            zobrist = board.currentZobrist;
            gameState = board.currentGameState;
            squares = board.Squares;
            colorToMove = board.colorToMove;
            friendlyColor = board.friendlyColor;
            enemyColor = board.enemyColor;
            whiteKingSquare = board.whiteKingSquare;
            blackKingSquare = board.blackKingSquare;
            opponentColorBit = board.opponentColorBit;
            friendlyColorBit = board.friendlyColorBit;
            repetitionTableCount = board.repetitionTable.Count;
        }
    }
}