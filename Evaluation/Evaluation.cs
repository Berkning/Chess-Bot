
using System.Runtime.CompilerServices;

public class Evaluation
{
    //TODO: Maybe make non-static for multithreaded performance
    private static readonly int[] Weights = {
        -1, 1, 0, -8, 3, -9, 20, -4, 0, 0, 0, -5, -6, 0,
        5, -1, 0, 0, 0, 0, 0, 1, 1, -2, 0, 0, 0, 0, 1, 1,
        0, -1, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1,
        1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, -13, -4, -11, -20, -12,
        11, 22, -2, -12, -11, -2, -9, 3, -9, 17, 1, -16,
        -6, -8, 8, 5, -5, -9, -24, 0, 1, -5, 3, 12, 1, 0,
        -3, 1, 1, 1, -1, 1, 4, 1, 1, 3, 2, 1, 1, 1, 1, 1,
        1, 0, 0, 0, 0, 0, 0, 0, 0, -1, -21, -3, -4, -6, -1,
        -19, -1, -1, -1, -1, 4, 2, 0, -1, -1, -5, -3, 15,
        2, 3, 14, 3, -5, -5, 0, 6, 8, 10, 5, 0, -6, 0, 3,
        1, 14, 12, 4, 1, 0, -1, 0, 1, 3, 2, 2, 1, 0, -3,
        -2, 2, 0, 1, 1, 0, -1, -2, 0, 0, 0, 0, -1, 0, -1,
        -1, -1, -12, -3, -2, -19, 0, -2, -1, 8, 0, -1, 0,
        1, 24, 0, 0, 2, 3, 8, 9, 6, 2, 0, -1, 0, 4, 3, 5,
        5, 0, -3, -2, -3, 0, 2, 3, 0, 0, -2, -2, 0, 2, 1,
        1, 1, 1, 2, -4, -1, -2, 0, 0, 1, 0, -1, -1, 0, 0,
        0, 0, 0, 0, -1, -13, -5, 6, 9, 7, 0, -7, -15, -4,
        -1, -1, -1, -1, 0, 0, -2, -2, -1, -1, 0, 0, -1, 0,
        0, -1, -1, -1, 0, 0, 0, 0, 0, -1, 0, 1, 1, 0, 0, 0,
        1, 0, 0, 0, 1, 1, 1, 0, 0, 1, 1, 2, 2, 1, 1, 1, 0,
        1, 1, 1, 0, 0, 0, 0, 0, 1, -3, -3, 6, -3, -2, -1,
        0, -2, 0, 3, 4, 6, 1, 0, 0, -2, 0, 1, 2, 1, 4, 1,
        -1, -7, -1, -1, 1, 2, 1, 2, -1, -2, -2, -1, -1, 1,
        2, 0, 2, -2, -1, 0, 1, 2, 2, 2, 3, -4, -9, -1, 0,
        0, 2, 1, 1, -1, 0, 0, 0, 0, 0, 0, 0, -2, -2, -2,
        -7, -7, -8, -8, -12, -2, -2, 0, 1, 3, 4, 4, -6,
        -2, -1, 1, 3, 5, 6, 2, -4, -2, -1, 1, 3, 5, 5, 2,
        -3, -1, 0, 2, 3, 3, 4, 3, 0, 0, 1, 1, 1, 2, 3, 3,
        0, 0, 1, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, -2, 1, 4, -2, 0, 4, -2, -8,
        -7, -1, -3, 0, 0, -1, -2, -7, 1, 1, -5, -5, -5, -5,
        -1, -4, 9, 5, 1, -4, -2, 0, 4, 3, 14, 9, 6, 3, 2,
        5, 6, 8, 12, 8, 6, 5, 3, 4, 5, 7, 0, 0, 0, 0, 0, 0,
        0, 0, -1, -5, -2, -2, -3, -1, -5, -1, -1, -1, -1,
        -1, -1, -1, -1, -1, -2, -1, 1, 1, 1, 1, 0, -2, -1,
        0, 3, 3, 4, 1, -1, -2, -1, 1, 1, 5, 4, 2, 0, -1,
        -1, -1, 1, 2, 1, 1, 0, -1, -2, -2, 0, -1, 0, 0, -1,
        -1, -2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -8, -2,
        -2, -5, -1, -1, -1, -1, -1, 0, 1, 0, 2, -1, -1, 0, 2,
        3, 5, 2, -1, -1, -1, 0, 2, 1, 2, 3, -1, -1, -1, 1, 1,
        2, 2, 0, 0, -2, -1, 0, 1, 1, 0, 1, 0, 0, -2, -1, -1,
        -1, 0, 0, 0, -2, -1, -1, -1, -1, -1, -1, -1, -1, -7,
        -1, 1, 7, 1, -4, -2, -10, -2, -1, 0, 0, -1, -1, 0,
        -1, -2, -1, -1, -1, -1, -1, -1, -1, -1, 0, 0, 0, 0,
        -1, -1, -1, 0, 0, 1, 1, 0, 0, -1, -1, 1, 1, 1, 1, 0,
        0, 0, -1, 3, 5, 4, 3, 1, 2, 1, 0, 3, 2, 2, 1, 1, 0,
        0, 0, 0, -1, -1, -4, -1, -1, 0, 0, -1, 0, 0, 1, 1, 0,
        0, 0, -1, -1, 1, 1, 1, 1, 1, 0, -1, 0, 0, 2, 1, 1, 1,
        0, -1, 0, 0, 1, 2, 1, 0, 1, -1, -1, 1, 1, 2, 1, 1, 1,
        -1, -2, 0, 0, 0, 2, 1, 0, -1, 0, 0, 0, 0, 0, 0, 0, 89,
        302, 310, 477, 912, 37, -12, -4, -18, -15, -3, 18, 53,
        59, 12, -11
        };

    private const int Bias = 2;

    private int[] Features = new int[Weights.Length];

    public int Evaluate(Board board)
    {
        for (int i = 0; i < Features.Length; i++)
        {
            Features[i] = 0;
        }

        CalculatePhase(board);

        CalculateFeatures(board);

        int result = Bias;

        for (int i = 0; i < Weights.Length; i++)
        {
            result += Features[i] * Weights[i]; //TODO: SIMD
        }

        int perspective = board.colorToMove == Piece.White ? 1 : -1;

        return result * perspective;
    }

    private void CalculateFeatures(Board board)
    {
        //LogData();
        CalculatePieceSquareTables(board);
        //LogData();
        CalculateMaterial(board);
        //LogData();
        CalculatePawnStructure(board);
        //LogData();
        CalculateKingSafety(board);
    }

    public static int GetPieceTypeValue(int piece)
    {
        Piece.Type(piece);

        return Weights[768 + piece - 2];
    }

    #region Phase

    private const int KnightPhase = 1;
    private const int BishopPhase = 1;
    private const int RookPhase = 2;
    private const int QueenPhase = 4;

    private const int MaxPhase = KnightPhase * 4 + BishopPhase * 4 + RookPhase * 4 + QueenPhase * 2;
    private int phase; //Phase is between 0 (MG) and 100 (EG)

    private void CalculatePhase(Board board)
    {
        phase = MaxPhase;

        phase -= board.GetPieceList(Piece.Knight, 0).Count * KnightPhase;
        phase -= board.GetPieceList(Piece.Knight, 1).Count * KnightPhase;
        phase -= board.GetPieceList(Piece.Bishop, 0).Count * BishopPhase;
        phase -= board.GetPieceList(Piece.Bishop, 1).Count * BishopPhase;
        phase -= board.GetPieceList(Piece.Rook, 0).Count * RookPhase;
        phase -= board.GetPieceList(Piece.Rook, 1).Count * RookPhase;
        phase -= board.GetPieceList(Piece.Queen, 0).Count * QueenPhase;
        phase -= board.GetPieceList(Piece.Queen, 1).Count * QueenPhase;

        phase = (phase * 100 + (MaxPhase / 2)) / MaxPhase; //TODO: Implement in a better way
        //TODO: Precompute phase/100 and 1 - (phase / 100)
    }

    #endregion


    #region Features

    #region PSQT
    //6 pieces * 64 squares * 2 game stages = 768 features
    private void CalculatePieceSquareTables(Board board)
    {
        //TODO: find more efficient way to do this?

        //TODO: Use the allPieceList array instead of calling GetPiecelist

        //SetPSQTFeaturesWhite(board.GetPieceList(Piece.King, 0), 64 * 0);
        Features[board.whiteKingSquare] += 1 - (phase / 100);
        Features[board.whiteKingSquare + 384] += phase / 100;

        SetPSQTFeaturesWhite(board.GetPieceList(Piece.Pawn, 0), 64 * 1);
        SetPSQTFeaturesWhite(board.GetPieceList(Piece.Knight, 0), 64 * 2);
        SetPSQTFeaturesWhite(board.GetPieceList(Piece.Bishop, 0), 64 * 3);
        SetPSQTFeaturesWhite(board.GetPieceList(Piece.Rook, 0), 64 * 4);
        SetPSQTFeaturesWhite(board.GetPieceList(Piece.Queen, 0), 64 * 5);


        //SetPSQTFeaturesBlack(board.GetPieceList(Piece.King, 1), 64 * 0);
        Features[BoardHelper.FlipIndex(board.blackKingSquare)] -= 1 - (phase / 100);
        Features[BoardHelper.FlipIndex(board.blackKingSquare) + 384] -= phase / 100;

        SetPSQTFeaturesBlack(board.GetPieceList(Piece.Pawn, 1), 64 * 1);
        SetPSQTFeaturesBlack(board.GetPieceList(Piece.Knight, 1), 64 * 2);
        SetPSQTFeaturesBlack(board.GetPieceList(Piece.Bishop, 1), 64 * 3);
        SetPSQTFeaturesBlack(board.GetPieceList(Piece.Rook, 1), 64 * 4);
        SetPSQTFeaturesBlack(board.GetPieceList(Piece.Queen, 1), 64 * 5);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetPSQTFeaturesWhite(PieceList list, int PSQTOffset)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Features[list[i] + PSQTOffset] += 1 - (phase / 100); //Activate middlegame PSQT at this square with intensity equal to how "much" we are in the middlegame

            Features[list[i] + PSQTOffset + 384] += phase / 100; //Activate endgame PSQT at this square with intensity equal to how "much" we are in the endgame 
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetPSQTFeaturesBlack(PieceList list, int PSQTOffset)
    {
        for (int i = 0; i < list.Count; i++)
        {
            //TODO: Rename "flipindex" to "mirrorindex"
            int mirroredSquare = BoardHelper.FlipIndex(list[i]);

            Features[mirroredSquare + PSQTOffset] -= 1 - (phase / 100); //Activate middlegame PSQT at this square with intensity equal to how "much" we are in the middlegame

            Features[mirroredSquare + PSQTOffset + 384] -= phase / 100; //Activate endgame PSQT at this square with intensity equal to how "much" we are in the endgame 
        }
    }
    #endregion


    #region Material
    //5 material differences + 1 bishop pair difference = 6 features
    private void CalculateMaterial(Board board)
    {
        //TODO: Use the allPieceList array instead of calling GetPiecelist
        Features[768] = board.GetPieceList(Piece.Pawn, 0).Count - board.GetPieceList(Piece.Pawn, 1).Count;
        Features[769] = board.GetPieceList(Piece.Knight, 0).Count - board.GetPieceList(Piece.Knight, 1).Count;
        Features[770] = board.GetPieceList(Piece.Bishop, 0).Count - board.GetPieceList(Piece.Bishop, 1).Count;
        Features[771] = board.GetPieceList(Piece.Rook, 0).Count - board.GetPieceList(Piece.Rook, 1).Count;
        Features[772] = board.GetPieceList(Piece.Queen, 0).Count - board.GetPieceList(Piece.Queen, 1).Count;

        int whiteBishopPair = board.GetPieceList(Piece.Bishop, 0).Count > 1 ? 1 : 0; //TODO: Obv don't call this again
        int blackBishopPair = board.GetPieceList(Piece.Bishop, 1).Count > 1 ? 1 : 0; //TODO: Obv don't call this again

        Features[773] = whiteBishopPair - blackBishopPair;
    }
    #endregion

    #region Pawn Structure
    //1 doubled pawn difference + 1 isolated pawn difference + (0) backward pawn difference + 6 passed pawn buckets + 1 connected passed pawn difference = 9 features
    private void CalculatePawnStructure(Board board)
    {
        ulong whitePawnBoard = board.GetPieceList(Piece.Pawn, 0).bitboard;
        ulong blackPawnBoard = board.GetPieceList(Piece.Pawn, 1).bitboard;

        int doubledPawnDifference = 0;

        //Doubled/Tripled Pawns
        for (int file = 0; file < 8; file++)
        {
            ulong fileMask = PrecomputedData.fileMasks[file];

            doubledPawnDifference += Math.Max(0, BitBoardHelper.BitCount(whitePawnBoard & fileMask) - 1);
            doubledPawnDifference -= Math.Max(0, BitBoardHelper.BitCount(blackPawnBoard & fileMask) - 1);
        }

        Features[774] = doubledPawnDifference;
        //Console.WriteLine("doubledPawnDifference: " + doubledPawnDifference);


        //Isolated Pawns
        int isolatedPawnDifference = 0;

        ulong whitePawns = whitePawnBoard;

        while (whitePawns != 0)
        {
            int pawnSquare = BitBoardHelper.PopFirstBit(ref whitePawns);

            //TODO: Calculate in PrecomputedData
            ulong isolationMask = 0;
            int file = BoardHelper.IndexToFile(pawnSquare);

            if (file < 7) isolationMask |= PrecomputedData.fileMasks[file + 1];
            if (file > 0) isolationMask |= PrecomputedData.fileMasks[file - 1];

            if ((whitePawnBoard & isolationMask) == 0) isolatedPawnDifference++;
        }

        ulong blackPawns = blackPawnBoard;

        while (blackPawns != 0)
        {
            int pawnSquare = BitBoardHelper.PopFirstBit(ref blackPawns);

            //TODO: Calculate in PrecomputedData
            ulong isolationMask = 0;
            int file = BoardHelper.IndexToFile(pawnSquare);

            if (file < 7) isolationMask |= PrecomputedData.fileMasks[file + 1];
            if (file > 0) isolationMask |= PrecomputedData.fileMasks[file - 1];

            if ((blackPawnBoard & isolationMask) == 0) isolatedPawnDifference--;
        }

        Features[775] = isolatedPawnDifference;
        //Console.WriteLine("isolatedPawnDifference: " + isolatedPawnDifference);


        //TODO: Try backward pawns


        //TODO: Combine with isolated pawn check
        //(Connected) Passed Pawns
        int connectedPassedPawnDifference = 0;

        whitePawns = whitePawnBoard;

        while (whitePawns != 0)
        {
            int pawnSquare = BitBoardHelper.PopFirstBit(ref whitePawns);

            ulong opposingPawnBoard = PrecomputedData.passedPawnMasks[pawnSquare] & blackPawnBoard;

            int opposingPawnCount = BitBoardHelper.BitCount(opposingPawnBoard);

            //Is passed pawn
            if (opposingPawnCount == 0)
            {
                //Console.WriteLine("white has passed pawn");

                int rank = BoardHelper.IndexToRank(pawnSquare);
                Features[776 - 1 + rank] = Features[776 - 1 + rank] + 1;

                ulong isolationMask = 0;
                int file = BoardHelper.IndexToFile(pawnSquare);

                if (file < 7) isolationMask |= PrecomputedData.fileMasks[file + 1];
                if (file > 0) isolationMask |= PrecomputedData.fileMasks[file - 1];

                if ((whitePawnBoard & isolationMask) != 0) connectedPassedPawnDifference++;
            }
        }

        blackPawns = blackPawnBoard;

        while (blackPawns != 0)
        {
            int pawnSquare = BitBoardHelper.PopFirstBit(ref blackPawns);

            ulong opposingPawnBoard = PrecomputedData.passedPawnMasks[pawnSquare + 64] & whitePawnBoard;

            int opposingPawnCount = BitBoardHelper.BitCount(opposingPawnBoard);

            //Is passed pawn
            if (opposingPawnCount == 0)
            {
                //Console.WriteLine("Black has passed pawn");

                int rank = 7 - BoardHelper.IndexToRank(pawnSquare);
                Features[776 - 1 + rank] = Features[776 - 1 + rank] - 1;

                ulong isolationMask = 0;
                int file = BoardHelper.IndexToFile(pawnSquare);

                if (file < 7) isolationMask |= PrecomputedData.fileMasks[file + 1];
                if (file > 0) isolationMask |= PrecomputedData.fileMasks[file - 1];

                if ((blackPawnBoard & isolationMask) != 0) connectedPassedPawnDifference--;
            }
        }

        Features[782] = connectedPassedPawnDifference;
        //Console.WriteLine("connectedPassedPawnDifference: " + connectedPassedPawnDifference);
    }
    #endregion

    #region King Safety
    //TODO: Add way more features
    //1 missing pawns on top of king difference = 1 feature
    private void CalculateKingSafety(Board board)
    {
        //TODO: Calculate in PrecomputedData
        int missingPawnDefenseDifference = 0;

        int kingFile = BoardHelper.IndexToFile(board.whiteKingSquare);

        if (kingFile > 0 && !BitBoardHelper.ContainsSquare(board.GetPieceList(Piece.Pawn, 0).bitboard, board.whiteKingSquare + PrecomputedData.UpLeft)) missingPawnDefenseDifference++;

        if (kingFile < 7 && !BitBoardHelper.ContainsSquare(board.GetPieceList(Piece.Pawn, 0).bitboard, board.whiteKingSquare + PrecomputedData.UpRight)) missingPawnDefenseDifference++;

        if (!BitBoardHelper.ContainsSquare(board.GetPieceList(Piece.Pawn, 0).bitboard, board.whiteKingSquare + PrecomputedData.Up)) missingPawnDefenseDifference++;



        kingFile = BoardHelper.IndexToFile(board.blackKingSquare);

        if (kingFile > 0 && !BitBoardHelper.ContainsSquare(board.GetPieceList(Piece.Pawn, 1).bitboard, board.blackKingSquare + PrecomputedData.UpLeft)) missingPawnDefenseDifference--;

        if (kingFile < 7 && !BitBoardHelper.ContainsSquare(board.GetPieceList(Piece.Pawn, 1).bitboard, board.blackKingSquare + PrecomputedData.UpRight)) missingPawnDefenseDifference--;

        if (!BitBoardHelper.ContainsSquare(board.GetPieceList(Piece.Pawn, 1).bitboard, board.blackKingSquare + PrecomputedData.Up)) missingPawnDefenseDifference--;

        Features[783] = missingPawnDefenseDifference * (1 - phase / 100);
    }
    #endregion

    #endregion
}