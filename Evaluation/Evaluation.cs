
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public class Evaluation
{
    //TODO: Maybe make non-static for multithreaded performance

    //ALWAYS make sure the length of this array is divisible by 2, 4, 8 and 16 to support all possible vector sizes
    private static readonly int[] Weights = {
        -11,19,12,-34,19,-26,39,18,-3,2,0,-35,-32,-7,26,10,-3,1,-2,-6,-7,-4,3,-9,-3,1,2,0,0,0,-1,-7,-1,1,3,2,2,4,4,-2,0,4,3,2,2,5,6,0,0,2,1,1,1,3,1,-1,-1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,-41,-15,-24,-35,-22,8,29,-22,-37,-23,-8,-15,1,-8,24,-16,-43,-19,-15,6,6,-2,-6,-42,-23,-6,-4,5,19,11,3,-24,-13,-8,-2,-23,-1,24,2,-7,4,-6,-2,0,0,-2,-5,-3,0,0,0,0,0,0,0,0,-7,-11,-19,-17,-17,-7,-13,-6,-9,-10,-4,12,12,10,-3,-3,-15,3,27,16,23,24,25,-15,-5,1,25,24,35,28,6,-11,-1,24,20,57,38,34,16,5,-5,7,17,25,23,10,12,-2,-19,-9,5,4,-1,11,-1,-4,-19,-3,-3,-4,-1,-9,-2,-8,-9,-8,-9,-11,-14,-16,-4,-9,-3,20,6,1,6,10,38,-1,4,12,13,13,14,19,10,7,-7,4,10,21,28,10,3,-14,-9,1,8,19,22,6,4,-5,-9,2,6,8,5,10,5,11,-18,2,-7,-2,1,1,-1,-18,-7,-3,-3,-3,-3,-3,-1,-3,-18,-14,3,8,9,-5,-30,-16,-35,-8,-10,-6,-5,-2,-3,-35,-25,-8,-6,-4,-1,-6,-2,-11,-14,-6,-6,-1,-1,-3,-1,-7,-8,-2,7,8,4,5,0,2,2,4,5,10,6,6,4,2,5,6,13,14,9,10,4,5,5,6,5,6,5,1,2,2,0,-14,-7,12,-9,-13,-6,-5,-15,0,9,7,12,8,2,-1,-12,3,3,4,4,11,11,-1,-12,-4,-3,2,12,6,13,-1,-12,-9,-2,1,8,13,2,11,-11,-4,-5,13,10,14,10,23,-15,-27,-4,1,0,5,-2,4,-9,-1,1,2,4,2,0,1,-21,-21,-16,-18,-31,-12,-34,-51,-16,-7,4,13,16,9,-5,-21,-17,-5,7,16,18,15,5,-15,-17,-5,12,17,21,18,6,-15,-11,4,11,16,15,22,17,-2,-3,7,9,7,9,22,23,2,-4,3,1,1,3,11,7,0,-5,-3,-4,-3,-3,0,-1,-3,0,0,0,0,0,0,0,0,15,4,13,3,10,5,-10,-6,7,4,-4,1,1,1,-10,-5,17,8,-2,-10,-7,-6,-2,3,28,16,5,-12,-10,-3,8,12,49,37,20,-5,-7,8,21,31,40,29,15,4,4,7,17,26,0,0,0,0,0,0,0,0,-8,-29,-16,-14,-16,-11,-25,-7,-10,-11,-10,-7,-7,-7,-8,-10,-11,-4,-6,8,7,-3,-9,-11,-10,-3,11,16,12,8,-1,-9,-9,4,14,19,16,13,3,-8,-13,-6,9,13,4,4,-4,-12,-18,-11,-6,-2,-7,-8,-10,-12,-16,-9,-7,-10,-7,-12,-7,-12,-13,-7,-25,-10,-10,-15,-7,-9,-6,-16,-6,0,4,-2,-11,-9,-7,1,8,12,13,4,-4,-7,-5,2,11,12,9,9,-3,-8,-3,7,8,13,11,4,0,-6,-4,-1,6,6,1,7,2,-2,-12,-3,-3,-7,-1,-3,-2,-12,-7,-8,-8,-6,-6,-5,-4,-5,-7,4,3,6,-2,-9,1,-29,-7,-4,-1,0,-5,-4,-5,-9,-8,-2,-4,-3,-4,-8,-6,-11,-1,0,4,4,0,-4,-5,-9,3,3,9,6,4,4,-3,-3,7,9,8,10,3,2,3,-2,13,18,20,19,9,11,7,5,18,15,17,15,12,6,5,7,-3,-7,-5,-31,-2,-9,-4,-4,-6,-3,-6,1,-1,-2,-3,-2,-5,-7,5,4,6,7,4,0,-3,1,4,12,11,6,7,1,-6,1,1,8,13,11,5,6,-8,-3,3,8,11,9,4,5,-8,-4,1,5,5,6,1,0,-6,0,2,4,4,2,-1,3,89,313,323,516,992,46,-10,-9,-2,1,14,34,80,127,4,-12
        };

    private Vector<int>[] weightVectors;

    private const int Bias = 1;

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
    private int phase; //Phase is between 0 (MG) and 256 (EG)
    private int mgWeight;
    private int egWeight;

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

        phase = (phase * 256 + (MaxPhase / 2)) / MaxPhase;

        mgWeight = 256 - phase;
        egWeight = phase;
    }

    public int GetRawPhase(Board board) //0 -> MaxPhase
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

        return phase;
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

        result += (Weights[board.whiteKingSquare] * mgWeight) >> 8;
        result += (Weights[board.whiteKingSquare + 384] * egWeight) >> 8;

        result += SetPSQTFeaturesWhite(board.GetPieceList(Piece.Pawn, 0), 64 * 1);
        result += SetPSQTFeaturesWhite(board.GetPieceList(Piece.Knight, 0), 64 * 2);
        result += SetPSQTFeaturesWhite(board.GetPieceList(Piece.Bishop, 0), 64 * 3);
        result += SetPSQTFeaturesWhite(board.GetPieceList(Piece.Rook, 0), 64 * 4);
        result += SetPSQTFeaturesWhite(board.GetPieceList(Piece.Queen, 0), 64 * 5);

        int blackKingFlipped = BoardHelper.FlipIndex(board.blackKingSquare);

        result -= (Weights[blackKingFlipped] * mgWeight) >> 8;
        result -= (Weights[blackKingFlipped + 384] * egWeight) >> 8;

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
            result += (Weights[list[i] + PSQTOffset] * mgWeight) >> 8; //Activate middlegame PSQT at this square with intensity equal to how "much" we are in the middlegame
            result += (Weights[list[i] + PSQTOffset + 384] * egWeight) >> 8; //Activate endgame PSQT at this square with intensity equal to how "much" we are in the endgame 
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

            result -= (Weights[mirroredSquare + PSQTOffset] * mgWeight) >> 8; //Activate middlegame PSQT at this square with intensity equal to how "much" we are in the middlegame
            result -= (Weights[mirroredSquare + PSQTOffset + 384] * egWeight) >> 8; //Activate endgame PSQT at this square with intensity equal to how "much" we are in the endgame
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
    private int CalculateKingSafety(Board board) //TODO: Extend pawn cover to include extra row above king so pawns are allowed to push - maybe add as new feature bc it's prob still bad if all pawns push but idk
    {
        int result = 0;

        //TODO: Currently counts pawns above king no matter where he is - if king moves up with the pawn it still sees no pawns missing - we should prob also only apply this when castled to not get weird results in the opening where the king is in the middle
        int missingPawnDefenseDifference = 0;

        //                                                      First find pawns in the cover area/mask                             then invert all bits inside the mask to show holes in cover
        ulong whiteCoverHoles = (PrecomputedData.kingPawnCoverMasks[board.whiteKingSquare] & board.GetPieceList(Piece.Pawn, 0).bitboard) ^ PrecomputedData.kingPawnCoverMasks[board.whiteKingSquare];

        missingPawnDefenseDifference += BitBoardHelper.BitCount(whiteCoverHoles);



        ulong blackCoverHoles = (PrecomputedData.kingPawnCoverMasks[board.blackKingSquare + 64] & board.GetPieceList(Piece.Pawn, 1).bitboard) ^ PrecomputedData.kingPawnCoverMasks[board.blackKingSquare + 64];

        missingPawnDefenseDifference -= BitBoardHelper.BitCount(blackCoverHoles);

        result += (Weights[783] * missingPawnDefenseDifference * mgWeight) >> 8;

        return result;
    }
    #endregion

    #endregion
}