

public static class EngineDiagnostics
{
    private static readonly DiagnosticPosition[] positions = new DiagnosticPosition[]
    {
        new DiagnosticPosition("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", 0x463b96181691fc9c),
        new DiagnosticPosition("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - ", 0xc3ce103f01d15e1d),
        new DiagnosticPosition("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1", 0x63f923fed11bffdc),
        new DiagnosticPosition("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1", 0x297175ba443b0558),
        new DiagnosticPosition("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8", 0x4f874e21f78d3590),
        new DiagnosticPosition("r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10", 0x25c2b59e73b314e6),
        new DiagnosticPosition("r3k2r/8/8/8/8/8/8/R3K2R b KQkq - 0 1", 0x05741f66c60de55a),
        new DiagnosticPosition("8/8/1B6/7b/7k/8/2B1b3/7K w - - 0 1", 0xc4bf6581fe17fb8b),
        new DiagnosticPosition("R6r/8/8/2K5/5k2/8/8/r6R w - - 0 1", 0x7a71623749ba7190),
        new DiagnosticPosition("6KQ/8/8/8/8/8/8/7k b - - 0 1", 0xf27b57f3311e8b79),
        new DiagnosticPosition("8/2k1p3/3pP3/3P2K1/8/8/8/8 w - - 0 1", 0x2ffc55c68d312441),
        new DiagnosticPosition("K7/p7/k7/8/8/8/8/8 b - - 0 1", 0x351899effd649ac4),
        new DiagnosticPosition("8/8/3k4/3p4/8/3P4/3K4/8 b - - 0 1", 0xcec4d03c26f7b51e),
        new DiagnosticPosition("k7/8/8/3p4/4p3/8/8/7K w - - 0 1", 0xa132e97cbf51fc41),
        new DiagnosticPosition("7k/8/8/1p6/P7/8/8/7K w - - 0 1", 0xb59697ab898b918b),
        new DiagnosticPosition("3k4/3pp3/8/8/8/8/3PP3/3K4 w - - 0 1", 0x0b4b8f1c3fd3ad8b),
        new DiagnosticPosition("n1n5/1Pk5/8/8/8/8/5Kp1/5N1N w - - 0 1", 0x6d87d3865ae25deb),
        new DiagnosticPosition("8/PPPk4/8/8/8/8/4Kppp/8 b - - 0 1", 0x6f6bf4634654a196),
        new DiagnosticPosition("r5k1/1p3p2/p1qb1P2/2p2RP1/2P4p/P2QBp2/1P3Pr1/3R2K1 w - - 1 1", 0x5d98834545477b50),
        new DiagnosticPosition("3k4/3p4/8/K1P4r/8/8/8/8 b - - 0 1", 0x54ae051e1820fa70),
        new DiagnosticPosition("8/8/4k3/8/2p5/8/B2P2K1/8 w - - 0 1", 0x10305798b28fa6dd),
        new DiagnosticPosition("8/8/1k6/2b5/2pP4/8/5K2/8 b - d3 0 1", 0x97fbc41ca31550e0),
        new DiagnosticPosition("5k2/8/8/8/8/8/8/4K2R w K - 0 1", 0xde75dbdfb9f20194),
        new DiagnosticPosition("3k4/8/8/8/8/8/8/R3K3 w Q - 0 1", 0xc539c95e32882741),
        new DiagnosticPosition("r3k2r/1b4bq/8/8/8/8/7B/R3K2R w KQkq - 0 1", 0xeb5eb0a0a7878383),
        new DiagnosticPosition("r3k2r/8/3Q4/8/8/5q2/8/R3K2R b KQkq - 0 1", 0xa42b026339fbc594),
        new DiagnosticPosition("2K2r2/4P3/8/8/8/8/8/3k4 w - - 0 1", 0x1b17fb89c4ec9526),
        new DiagnosticPosition("8/8/1P2K3/8/2n5/1q6/8/5k2 b - - 0 1", 0x94ebc9527e5ccf7b),
        new DiagnosticPosition("4k3/1P6/8/8/8/8/K7/8 w - - 0 1", 0x168bc798e1c52b24),
        new DiagnosticPosition("8/P1k5/K7/8/8/8/8/8 w - - 0 1", 0x7ede4be5e08c712b),
        new DiagnosticPosition("K1k5/8/P7/8/8/8/8/8 w - - 0 1", 0x525713d278d29ebd),
        new DiagnosticPosition("8/k1P5/8/1K6/8/8/8/8 w - - 0 1", 0xa5da3a605de33806),
        new DiagnosticPosition("8/8/2k5/5q2/5n2/8/5K2/8 b - - 0 1", 0x683f95f5e9a96c22),
        new DiagnosticPosition("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1", 0x823c9b50fd114196),
        new DiagnosticPosition("rnbqkbnr/ppp1pppp/8/3p4/4P3/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 2", 0x0756b94461c50fb0),
        new DiagnosticPosition("rnbqkbnr/ppp1pppp/8/3pP3/8/8/PPPP1PPP/RNBQKBNR b KQkq - 0 2", 0x662fafb965db29d4),
        new DiagnosticPosition("rnbqkbnr/ppp1p1pp/8/3pPp2/8/8/PPPP1PPP/RNBQKBNR w KQkq f6 0 3", 0x22a48b5a8e47ff78),
        new DiagnosticPosition("rnbqkbnr/ppp1p1pp/8/3pPp2/8/8/PPPPKPPP/RNBQ1BNR b kq - 0 3", 0x652a607ca3f242c1),
        new DiagnosticPosition("rnbq1bnr/ppp1pkpp/8/3pPp2/8/8/PPPPKPPP/RNBQ1BNR w - - 0 4", 0x00fdd303c946bdd9),
        new DiagnosticPosition("rnbqkbnr/p1pppppp/8/8/PpP4P/8/1P1PPPP1/RNBQKBNR b KQkq c3 0 3", 0x3c8123ea7b067637),
        new DiagnosticPosition("rnbqkbnr/p1pppppp/8/8/P6P/R1p5/1P1PPPP1/1NBQKBNR b Kkq - 0 4", 0x5c3f9b829b279560),
    };

    private const int MovesPerPosition = 12;
    private const int VerificationDepth = 6;


    private static Board board = new Board();
    private static MoveGenerator moveGenerator = new MoveGenerator(board);

    public static void RunDiagnostics(int n = 0)
    {
        Console.WriteLine("");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\x1b[1mRunning Engine Diagnostics...\x1b[0m");
        Console.ResetColor();

        if (n == 0 || n == 1)
        {
            Console.WriteLine("");
            Console.WriteLine("\x1b[1m#1 Verifying Zobrist Hashing...\x1b[0m");
            VerifyZobristHashing();
        }

        if (n == 0 || n == 2)
        {
            Console.WriteLine("");
            Console.WriteLine("\x1b[1m#2 Verifying Polyglot Move Translation...\x1b[0m");
            VerifyPolyglotMoveTranslation();
        }


        if (n == 0 || n == 3)
        {
            Console.WriteLine("");
            Console.WriteLine("\x1b[1m#3 Verifying Make/UnMake Move...\x1b[0m");
            VerifyMakeUnmake();
        }


        if (n == 0 || n == 4)
        {
            Console.WriteLine("");
            Console.WriteLine("\x1b[1m#4 Verifying Search...\x1b[0m");
            VerifySearch();
        }


        if (n == 0 || n == 5)
        {
            Console.WriteLine("");
            Console.WriteLine("\x1b[1m#5 Verifying NullMove Make/UnMake...\x1b[0m");
            VerifyNullMove();
        }


        if (n == 0 || n == 6)
        {
            //Always keep this last bc user can just run perft separately
            Console.WriteLine("");
            Console.WriteLine("\x1b[1m#6 Verifying MoveGen...\x1b[0m");
            VerifyMoveGeneration();
        }
    }

    private static void VerifyMoveGeneration()
    {
        bool passed = Perft.RunFullSuite();

        Console.ForegroundColor = passed ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine(passed ? "Move Generation \x1b[1mPassed ✅\x1b[0m" : "Move Generation \x1b[1mFailed ❌\x1b[0m");
        Console.ResetColor();
    }



    #region Polyglot Move Translation

    private static void VerifyPolyglotMoveTranslation()
    {
        bool passed = true;

        for (int i = 0; i < positions.Length; i++)
        {
            FenUtility.LoadPositionFromFen(board, positions[i].fen);

            Span<Move> moves = stackalloc Move[256];

            int moveCount = moveGenerator.GenerateMoves(ref moves, false);

            for (int j = 0; j < int.Min(moveCount, MovesPerPosition); j++)
            {
                ushort polyglot = OpeningBook.TranslateMoveToPolyglot(moves[j]);
                Move retranslation = OpeningBook.TranslatePolyglotMove(board, polyglot);

                if (moves[j].data != retranslation.data)
                {
                    Console.WriteLine("Polyglot translation incorrect. Move: " + moves[j].data + "(" + BoardHelper.GetMoveNameUCI(moves[j]) + ")" + "Gave: " + polyglot + " Retranslation was: " + retranslation.data + "(" + BoardHelper.GetMoveNameUCI(retranslation) + ")");
                    passed = false;
                }
            }
        }

        Console.ForegroundColor = passed ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine(passed ? "Polyglot Translation \x1b[1mPassed ✅\x1b[0m" : "Polyglot Translation \x1b[1mFailed ❌\x1b[0m");
        Console.ResetColor();
    }


    #endregion



    #region Zobrist Hashing

    private static void VerifyZobristHashing()
    {
        bool passed = true;

        for (int i = 0; i < positions.Length; i++)
        {
            FenUtility.LoadPositionFromFen(board, positions[i].fen);
            ulong ourZobrist = Zobrist.Hash(board);

            if (ourZobrist != positions[i].zobrist)
            {
                passed = false;

                Console.WriteLine("Position " + (i + 1) + "/" + positions.Length + " Failed ❌");
                Console.WriteLine("Zobrist did not match in position: " + positions[i].fen);
                Console.WriteLine("Expected: " + positions[i].zobrist.ToString("x") + " Got: " + ourZobrist.ToString("x"));
                Console.WriteLine("Attempting to find issue...");

                ulong originalZobristRandom = positions[i].zobrist ^ ourZobrist;

                //Find the specific number that changed the zobrist
                if (originalZobristRandom == Zobrist.sideToMove) Console.WriteLine("SideToMove Zobrist wasn't applied correctly");

                for (int j = 0; j < 8; j++)
                {
                    if (originalZobristRandom == Zobrist.epArray[j]) Console.WriteLine("EP Zobrist at index " + j + " wasn't applied correctly");
                }

                for (int j = 0; j < 16; j++)
                {
                    if (originalZobristRandom == Zobrist.castlingArray[j]) Console.WriteLine("Castle Zobrist at index " + j + " wasn't applied correctly");
                }

                for (int p = 0; p < 6; p++)
                {
                    for (int c = 0; c < 2; c++)
                    {
                        for (int sq = 0; sq < 64; sq++)
                        {
                            if (originalZobristRandom == Zobrist.piecesArray[p, c, sq]) Console.WriteLine("Piece Zobrist at piece " + p + " with color " + c + " on square " + sq + " wasn't applied correctly");
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Position " + (i + 1) + "/" + positions.Length + " Zobrist: " + ourZobrist.ToString("x") + " ✅");
            }
        }


        Console.ForegroundColor = passed ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine(passed ? "Zobrist Hashing \x1b[1mPassed ✅\x1b[0m" : "Zobrist Hashing \x1b[1mFailed ❌\x1b[0m");
        Console.ResetColor();
    }

    #endregion


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
                for (int m = moveHistory.Count - 1; m >= 0; m--)
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

        Search search = new Search(board, new Action<Move, int>(SearchReturn), 0);
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
    private static void VerifyNullMove()
    {
        bool passed = true;

        for (int i = 0; i < positions.Length; i++)
        {
            FenUtility.LoadPositionFromFen(board, positions[i].fen);

            bool p = VerifyNullMoveRecursive(VerificationDepth, MovesPerPosition, new Stack<Move>(), positions[i].fen);

            passed = passed && p;

            Console.WriteLine("Position " + (i + 1) + "/" + positions.Length + (p ? " Passed" : " Failed"));
        }

        Console.ForegroundColor = passed ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine(passed ? "NullMove Make/UnMake Verification \x1b[1mPassed ✅\x1b[0m" : "NullMove Make/UnMake Verification \x1b[1mFailed ❌\x1b[0m");
        Console.ResetColor();
    }

    private static bool VerifyNullMoveRecursive(int depth, int movesPerPos, Stack<Move> moveHistory, string startFen) //TODO: finish - return list of moves that lead to fail
    {
        bool passed = true;

        Span<Move> moves = stackalloc Move[256];

        int moveCount = moveGenerator.GenerateMoves(ref moves);

        if (moveCount == 0) return true; //Checkmate/Stalemate

        bool checkedPosition = moveGenerator.inCheck || moveGenerator.inDoubleCheck || moveGenerator.checkRayBitMap != ulong.MaxValue || BoardHelper.InCheckSlow(board) || BoardHelper.OpponentInCheckSlow(board);

        for (int i = 0; i < Math.Min(movesPerPos, moveCount); i++)
        {
            int randomMoveIndex = Random.Shared.Next(moveCount);
            bool shouldNullMove = Random.Shared.Next(100) >= 50;

            BoardSnapshot snapshot = new BoardSnapshot(board);

            moveHistory.Push(moves[randomMoveIndex]);


            if (!checkedPosition && shouldNullMove)
            {
                board.MakeNullMove();
            }
            else board.MakeMove(moves[randomMoveIndex], true);


            if (depth > 0)
            {
                bool p = VerifyNullMoveRecursive(depth - 1, movesPerPos, moveHistory, startFen);
                passed = passed && p;
            }

            //if (BoardHelper.InCheckSlow(board)) Console.WriteLine("In check in middle");

            if (checkedPosition || !shouldNullMove) board.UnMakeMove(moves[randomMoveIndex], true);
            else board.UnMakeNullMove();

            if (!snapshot.Verify(board))
            {
                passed = false;

                string moveString = "";
                for (int m = moveHistory.Count - 1; m >= 0; m--)
                {
                    moveString += BoardHelper.GetMoveNameUCI(moveHistory.ElementAt(m)) + " ";
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("State was corrupted from position: " + startFen + " after moves: " + moveString + " Current fen is: " + FenUtility.GetCurrentFen(board) + " inCheck = " + checkedPosition + " opponentInCheck = " + BoardHelper.OpponentInCheckSlow(board));
                Console.ResetColor();
            }

            moveHistory.Pop();
        }



        return passed;
    }
    #endregion





    private struct DiagnosticPosition
    {
        public string fen;
        public ulong zobrist;

        public DiagnosticPosition(string _fen, ulong _zobrist)
        {
            fen = _fen;
            zobrist = _zobrist;
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
                Console.WriteLine("Before: " + zobrist.ToString("x") + " After: " + board.currentZobrist.ToString("x"));
                ulong originalZobristRandom = zobrist ^ board.currentZobrist;

                //Find the specific number that changed the zobrist
                if (originalZobristRandom == Zobrist.sideToMove) Console.WriteLine("SideToMove Zobrist wasn't applied correctly");

                for (int i = 0; i < 8; i++)
                {
                    if (originalZobristRandom == Zobrist.epArray[i]) Console.WriteLine("EP Zobrist at index " + i + " wasn't applied correctly");
                }

                for (int i = 0; i < 16; i++)
                {
                    if (originalZobristRandom == Zobrist.castlingArray[i]) Console.WriteLine("Castle Zobrist at index " + i + " wasn't applied correctly");
                }

                for (int p = 0; p < 6; p++)
                {
                    for (int c = 0; c < 2; c++)
                    {
                        for (int sq = 0; sq < 64; sq++)
                        {
                            if (originalZobristRandom == Zobrist.piecesArray[p, c, sq]) Console.WriteLine("Piece Zobrist at piece " + p + " with color " + c + " on square " + sq + " wasn't applied correctly");
                        }
                    }
                }



                success = false;
            }
            else if (zobrist != Zobrist.Hash(board))
            {
                Console.WriteLine("Zobrist not updated correctly");
                Console.WriteLine("Zobrist should be: " + Zobrist.Hash(board).ToString("x") + " but board holds: " + zobrist.ToString("x"));

                ulong originalZobristRandom = zobrist ^ Zobrist.Hash(board);

                //Find the specific number that changed the zobrist
                if (originalZobristRandom == Zobrist.sideToMove) Console.WriteLine("SideToMove Zobrist wasn't applied correctly");

                for (int i = 0; i < 8; i++)
                {
                    if (originalZobristRandom == Zobrist.epArray[i]) Console.WriteLine("EP Zobrist at index " + i + " wasn't applied correctly");
                }

                for (int i = 0; i < 16; i++)
                {
                    if (originalZobristRandom == Zobrist.castlingArray[i]) Console.WriteLine("Castle Zobrist at index " + i + " wasn't applied correctly");
                }

                for (int p = 0; p < 6; p++)
                {
                    for (int c = 0; c < 2; c++)
                    {
                        for (int sq = 0; sq < 64; sq++)
                        {
                            if (originalZobristRandom == Zobrist.piecesArray[p, c, sq]) Console.WriteLine("Piece Zobrist at piece " + p + " with color " + c + " on square " + sq + " wasn't applied correctly");
                        }
                    }
                }



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