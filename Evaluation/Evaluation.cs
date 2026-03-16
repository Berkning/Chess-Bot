
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public class Evaluation
{
    //TODO: Maybe make non-static for multithreaded performance

    //ALWAYS make sure the length of this array is divisible by 2, 4, 8 and 16 to support all possible vector sizes
    private static readonly int[] Weights = {
        -6,9,7,-27,17,-23,34,11,-1,-1,-1,-26,-25,-4,22,4,-2,1,-2,-4,-4,-1,2,-7,-2,0,2,0,2,1,0,-5,-1,0,2,2,2,3,2,-1,0,3,2,2,1,3,4,0,0,1,1,1,1,2,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,-36,-12,-20,-31,-16,10,33,-14,-34,-22,-5,-13,5,-8,25,-9,-41,-18,-13,7,4,-1,-5,-41,-19,-7,-6,2,18,7,0,-17,-11,-7,0,-22,-3,19,0,-3,4,-5,-2,-1,-1,-3,-2,-3,0,0,0,0,0,0,0,0,-5,-15,-15,-15,-20,-4,-18,-5,-7,-7,-2,10,10,5,-2,-2,-14,1,24,13,19,20,19,-17,-7,0,21,21,30,24,3,-15,1,20,13,50,31,24,10,3,-3,1,11,21,17,14,8,-2,-18,-9,17,1,7,11,-2,-4,-18,-2,-2,-3,-1,-7,-1,-6,-4,-8,-7,-10,-12,-19,-2,-6,-4,20,3,0,4,9,35,-1,6,11,12,10,13,16,7,6,-7,1,7,17,23,8,1,-15,-9,-5,6,15,16,1,2,-6,-6,0,16,5,8,7,4,12,-16,-1,-8,-2,0,7,-2,-1,-6,-3,-2,-3,-3,-3,-1,-3,-17,-12,4,7,7,-5,-26,-17,-22,-6,-6,-5,-4,-2,0,-15,-16,-5,-4,0,-1,-4,0,-3,-10,-4,-6,-1,-1,-2,-1,-4,-5,-1,5,5,2,2,1,4,2,2,3,6,5,5,2,2,3,3,9,9,5,7,5,3,4,4,4,4,4,2,1,1,0,-15,-9,7,-10,-10,-5,-3,-11,2,4,6,10,7,4,-1,-12,0,1,2,3,9,8,-3,-17,-2,-5,1,10,4,9,-3,-11,-7,-2,0,7,8,1,12,-11,-5,5,9,13,11,12,20,-15,-27,-6,-1,-2,14,7,10,-6,-1,1,3,2,2,-1,-1,-11,-10,-10,-18,-27,-13,-30,-42,-11,-6,3,11,15,11,0,-16,-14,-4,5,14,17,16,6,-13,-14,-4,7,13,18,16,6,-11,-9,-1,7,11,11,16,12,-1,-4,5,5,6,7,14,17,2,-3,2,1,0,1,7,5,0,-3,-2,-3,-2,-2,0,-1,-2,0,0,0,0,0,0,0,0,11,4,13,1,6,6,-10,-8,4,3,-2,1,0,2,-9,-6,13,7,-4,-9,-8,-7,-3,1,23,15,5,-11,-11,-3,7,7,42,29,18,-2,-3,8,16,24,36,24,14,6,4,8,17,21,0,0,0,0,0,0,0,0,-6,-21,-10,-11,-12,-6,-17,-4,-6,-8,-8,-5,-7,-4,-5,-6,-9,-4,-6,5,6,-2,-4,-9,-6,-2,9,9,11,3,-3,-7,-5,5,7,16,11,9,1,-6,-8,-6,5,8,2,4,-4,-8,-14,-10,-2,-4,-4,-4,-8,-9,-12,-6,-6,-6,-6,-8,-4,-10,-8,-7,-23,-8,-8,-11,-6,-7,-5,-11,-6,1,5,0,-7,-6,-5,1,5,10,12,4,-5,-6,-4,0,8,5,8,9,-3,-7,-3,6,4,11,8,0,0,-7,-3,0,4,5,3,5,1,-2,-11,-3,-4,-5,-1,-2,-2,-9,-5,-6,-6,-5,-6,-4,-4,-3,-8,2,2,9,-2,-7,-4,-28,-7,-3,0,-1,-3,-3,-1,-6,-8,-3,-3,-2,-2,-5,-4,-6,-2,-1,1,3,0,-2,-3,-7,2,1,6,4,0,1,-1,-4,5,6,5,5,2,2,2,-3,10,16,15,12,6,8,5,1,15,11,12,9,7,3,2,3,-2,-5,-4,-27,-2,-7,-3,-3,-4,0,-2,2,0,-2,-2,-1,-5,-6,3,2,4,5,3,-2,-2,0,1,8,7,3,4,-1,-6,0,0,6,10,8,3,3,-7,-3,3,4,10,7,5,5,-6,-5,-1,3,2,11,4,1,-4,-1,1,4,3,3,-2,1,85,304,311,489,949,45,-10,-9,-2,0,12,31,72,117,2,-13
        };

    private Vector<int>[] weightVectors;

    private const int Bias = 2;

    public Evaluation()
    {
        weightVectors = new Vector<int>[(int)Math.Ceiling(Weights.Length / ((float)Vector<int>.Count))];

        int FullVectorCount = Weights.Length / Vector<int>.Count; //Floors Weights.Length to a multiple of the vector length, giving us the amount of vectors that can be fully filled before we run out of weights to fill them with

        for (int i = 0; i < FullVectorCount; i++)
        {
            weightVectors[i] = new Vector<int>(Weights, i * Vector<int>.Count);
        }

        /*if (weightVectors.Length != FullVectorCount) //If all vectors cant be fully filled, we fill the last one partially
        {

            int[] lastVectorContents = new int[Vector<int>.Count];
            int startIndex = FullVectorCount * Vector<int>.Count;

            for (int i = startIndex; i < Weights.Length; i++)
            {
                lastVectorContents[i - startIndex] = Weights[i];
            }

            weightVectors[FullVectorCount] = new Vector<int>(lastVectorContents);
        }*/


        /*for (int i = 0; i < weightVectors.Length; i++)
        {
            Console.WriteLine("Vector #" + i + " == " + weightVectors[i]);
        }*/
    }




    public int Evaluate(Board board)
    {
        CalculatePhase(board);

        int result = CalculateResult(board) + Bias;

        int perspective = board.colorToMove == Piece.White ? 1 : -1;

        return result * perspective;
    }

    private int CalculateResult(Board board)
    {
        return CalculatePieceSquareTables(board) + CalculateMaterial(board) + CalculatePawnStructure(board) + CalculateKingSafety(board);
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
    private int CalculatePieceSquareTables(Board board)
    {
        int result = 0;

        //TODO: find more efficient way to do this?

        //TODO: Use the allPieceList array instead of calling GetPiecelist

        result += Weights[board.whiteKingSquare] * (1 - (phase / 100));
        result += Weights[board.whiteKingSquare + 384] * (phase / 100);

        result += SetPSQTFeaturesWhite(board.GetPieceList(Piece.Pawn, 0), 64 * 1);
        result += SetPSQTFeaturesWhite(board.GetPieceList(Piece.Knight, 0), 64 * 2);
        result += SetPSQTFeaturesWhite(board.GetPieceList(Piece.Bishop, 0), 64 * 3);
        result += SetPSQTFeaturesWhite(board.GetPieceList(Piece.Rook, 0), 64 * 4);
        result += SetPSQTFeaturesWhite(board.GetPieceList(Piece.Queen, 0), 64 * 5);

        int blackKingFlipped = BoardHelper.FlipIndex(board.blackKingSquare);

        result -= Weights[blackKingFlipped] * (1 - (phase / 100));
        result -= Weights[blackKingFlipped + 384] * (phase / 100);

        result += SetPSQTFeaturesBlack(board.GetPieceList(Piece.Pawn, 1), 64 * 1);
        result += SetPSQTFeaturesBlack(board.GetPieceList(Piece.Knight, 1), 64 * 2);
        result += SetPSQTFeaturesBlack(board.GetPieceList(Piece.Bishop, 1), 64 * 3);
        result += SetPSQTFeaturesBlack(board.GetPieceList(Piece.Rook, 1), 64 * 4);
        result += SetPSQTFeaturesBlack(board.GetPieceList(Piece.Queen, 1), 64 * 5);


        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int SetPSQTFeaturesWhite(PieceList list, int PSQTOffset)
    {
        int result = 0;

        for (int i = 0; i < list.Count; i++)
        {
            result += Weights[list[i] + PSQTOffset] * (1 - (phase / 100)); //Activate middlegame PSQT at this square with intensity equal to how "much" we are in the middlegame
            result += Weights[list[i] + PSQTOffset + 384] * (phase / 100); //Activate endgame PSQT at this square with intensity equal to how "much" we are in the endgame 
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int SetPSQTFeaturesBlack(PieceList list, int PSQTOffset)
    {
        int result = 0;

        for (int i = 0; i < list.Count; i++)
        {
            //TODO: Rename "flipindex" to "mirrorindex"
            int mirroredSquare = BoardHelper.FlipIndex(list[i]);

            result -= Weights[mirroredSquare + PSQTOffset] * (1 - (phase / 100)); //Activate middlegame PSQT at this square with intensity equal to how "much" we are in the middlegame
            result -= Weights[mirroredSquare + PSQTOffset + 384] * (phase / 100); //Activate endgame PSQT at this square with intensity equal to how "much" we are in the endgame
        }

        return result;
    }
    #endregion


    #region Material
    //5 material differences + 1 bishop pair difference = 6 features
    private int CalculateMaterial(Board board)
    {
        int result = 0;

        //TODO: Use the allPieceList array instead of calling GetPiecelist
        result += Weights[768] * (board.GetPieceList(Piece.Pawn, 0).Count - board.GetPieceList(Piece.Pawn, 1).Count);
        result += Weights[769] * (board.GetPieceList(Piece.Knight, 0).Count - board.GetPieceList(Piece.Knight, 1).Count);
        result += Weights[770] * (board.GetPieceList(Piece.Bishop, 0).Count - board.GetPieceList(Piece.Bishop, 1).Count);
        result += Weights[771] * (board.GetPieceList(Piece.Rook, 0).Count - board.GetPieceList(Piece.Rook, 1).Count);
        result += Weights[772] * (board.GetPieceList(Piece.Queen, 0).Count - board.GetPieceList(Piece.Queen, 1).Count);

        int whiteBishopPair = board.GetPieceList(Piece.Bishop, 0).Count > 1 ? 1 : 0; //TODO: Obv don't call this again
        int blackBishopPair = board.GetPieceList(Piece.Bishop, 1).Count > 1 ? 1 : 0; //TODO: Obv don't call this again

        result += Weights[773] * (whiteBishopPair - blackBishopPair);

        return result;
    }
    #endregion

    #region Pawn Structure
    //1 doubled pawn difference + 1 isolated pawn difference + (0) backward pawn difference + 6 passed pawn buckets + 1 connected passed pawn difference = 9 features
    private int CalculatePawnStructure(Board board)
    {
        int result = 0;

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

        result += Weights[774] * doubledPawnDifference;
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

        result += Weights[775] * isolatedPawnDifference;
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
                result += Weights[776 - 1 + rank];

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
                result -= Weights[776 - 1 + rank];

                ulong isolationMask = 0;
                int file = BoardHelper.IndexToFile(pawnSquare);

                if (file < 7) isolationMask |= PrecomputedData.fileMasks[file + 1];
                if (file > 0) isolationMask |= PrecomputedData.fileMasks[file - 1];

                if ((blackPawnBoard & isolationMask) != 0) connectedPassedPawnDifference--;
            }
        }

        result += Weights[782] * connectedPassedPawnDifference;
        //Console.WriteLine("connectedPassedPawnDifference: " + connectedPassedPawnDifference);
        return result;
    }
    #endregion

    #region King Safety
    //TODO: Add way more features
    //1 missing pawns on top of king difference = 1 feature
    private int CalculateKingSafety(Board board)
    {
        int result = 0;

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

        result += Weights[783] * missingPawnDefenseDifference * (1 - phase / 100);

        return result;
    }
    #endregion

    #endregion
}