
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public class Evaluation
{
    //TODO: Maybe make non-static for multithreaded performance

    private static readonly int[] Weights = {
        -17,25,19,-36,24,-29,42,21,-4,4,1,-41,-35,-6,35,19,-5,2,-2,-9,-12,-8,5,-14,-5,2,4,-3,-4,-4,-4,-15,-3,1,4,1,0,2,3,-7,0,7,4,3,2,7,10,0,-1,3,3,2,2,5,3,-1,-1,0,1,0,-1,1,1,-1,0,0,0,0,0,0,0,0,-45,-17,-26,-44,-28,-2,23,-33,-42,-25,-10,-25,-8,-21,14,-31,-47,-21,-17,-1,-1,-13,-14,-55,-31,-10,-4,-4,10,-1,-8,-42,-27,-19,-5,-38,-4,34,-4,-23,-1,-18,-9,-5,-3,-5,-11,-12,0,0,0,0,0,0,0,0,-11,3,-24,-12,-11,-4,-1,-9,-9,-12,8,26,26,21,0,9,-2,15,43,29,38,39,41,-2,8,6,39,39,52,44,16,2,3,41,35,77,52,55,31,12,-8,16,32,45,43,20,25,-1,-26,-11,14,10,0,22,-1,-6,-47,-6,-7,-7,-2,-17,-3,-20,-12,-8,-2,-11,-15,-10,-8,-11,-5,29,14,8,14,17,45,2,12,20,22,20,21,27,17,14,-8,12,17,29,40,18,10,-14,-9,8,17,30,31,14,12,0,-7,4,14,14,11,20,10,18,-20,9,-5,-4,5,1,1,-23,-11,-9,-7,-6,-5,-7,-3,-9,-27,-23,-4,-1,2,-11,-35,-19,-48,-14,-17,-12,-9,-3,-5,-48,-38,-16,-10,-8,-3,-8,-2,-17,-26,-12,-13,-4,-4,-4,-1,-11,-16,-6,8,12,5,7,-1,1,-2,4,7,12,9,10,6,3,2,3,16,18,15,15,6,8,6,9,6,9,8,3,4,3,-1,-17,-8,13,-12,-19,-10,-10,-19,1,10,7,12,11,4,-1,-16,4,2,3,3,10,15,0,-12,-4,-3,2,14,9,19,1,-17,-11,-1,2,13,21,4,16,-16,-7,-5,20,20,26,18,36,-15,-26,-5,2,1,11,0,9,-30,-7,-2,1,4,2,-4,-4,-30,-29,-18,-12,-30,-9,-36,-60,-24,-5,8,19,22,13,-6,-25,-25,-3,10,20,22,19,7,-14,-27,-7,14,19,22,19,7,-18,-20,3,14,17,13,22,19,-6,-6,10,12,7,10,27,34,4,-9,5,3,1,4,19,17,1,-11,-8,-6,-7,-5,0,0,-6,0,0,0,0,0,0,0,0,23,10,16,11,16,10,-4,1,15,10,0,9,9,7,-4,2,25,14,3,-3,-2,-1,3,9,38,24,9,-3,-3,3,14,21,65,53,32,8,-1,10,32,41,51,42,25,5,5,8,26,31,0,0,0,0,0,0,0,0,-15,-46,-24,-18,-23,-17,-38,-12,-17,-18,-17,-13,-13,-13,-15,-18,-19,-10,-14,6,4,-8,-17,-20,-16,-7,7,13,8,3,-3,-15,-15,-2,11,11,11,9,-1,-14,-24,-11,8,9,-3,2,-10,-22,-29,-20,-12,-7,-15,-18,-19,-24,-32,-20,-15,-20,-17,-26,-14,-27,-22,-11,-31,-12,-13,-19,-15,-16,-10,-21,-9,-3,1,-6,-16,-15,-12,-1,5,10,11,1,-8,-14,-9,0,11,12,3,8,-7,-11,-3,6,7,10,10,3,-2,-9,-5,-1,6,6,-2,7,-1,-6,-17,-6,-5,-12,-3,-7,-5,-19,-13,-15,-15,-12,-13,-11,-11,-12,-3,8,8,11,3,-5,4,-27,-1,-3,3,4,-3,-5,-6,-10,-3,0,-3,-2,-4,-9,-9,-12,3,4,8,7,2,-4,-6,-10,8,8,11,7,5,6,-3,0,9,12,10,12,4,3,6,0,14,20,20,21,10,12,10,7,23,21,23,21,18,10,10,11,-6,-10,-8,-42,-2,-14,-7,-8,-9,-5,-10,0,-2,-6,-6,-5,-8,-12,8,6,9,9,5,0,-3,2,7,20,17,10,8,2,-10,2,0,13,23,18,9,8,-14,-5,5,11,20,16,6,7,-12,-4,3,9,10,11,2,1,-18,-5,0,4,5,1,-8,-2,86,318,327,530,1007,55,-6,-9,0,2,16,36,81,130,4,-4,-5,-19,86,-12,-6,1
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

        ulong whitePawns = board.GetPieceList(Piece.Pawn, 0).bitboard;
        ulong blackPawns = board.GetPieceList(Piece.Pawn, 1).bitboard;

        int whiteKingFile = BoardHelper.IndexToFile(board.whiteKingSquare);
        int blackKingFile = BoardHelper.IndexToFile(board.blackKingSquare);



        //TODO: Currently counts pawns above king no matter where he is - if king moves up with the pawn it still sees no pawns missing - we should prob also only apply this when castled to not get weird results in the opening where the king is in the middle
        int missingPawnDefenseDifference = 0;

        //                                                      First find pawns in the cover area/mask                             then invert all bits inside the mask to show holes in cover
        ulong whiteCoverHoles = (PrecomputedData.kingPawnCoverMasks[board.whiteKingSquare] & whitePawns) ^ PrecomputedData.kingPawnCoverMasks[board.whiteKingSquare];

        missingPawnDefenseDifference += BitBoardHelper.BitCount(whiteCoverHoles);



        ulong blackCoverHoles = (PrecomputedData.kingPawnCoverMasks[board.blackKingSquare + 64] & blackPawns) ^ PrecomputedData.kingPawnCoverMasks[board.blackKingSquare + 64];

        missingPawnDefenseDifference -= BitBoardHelper.BitCount(blackCoverHoles);

        result += (Weights[783] * missingPawnDefenseDifference * mgWeight) >> 8;



        //TODO: Obv optimize everything below this comment with precomputed bitboards instead of all these ugly if's -------------------------------------------------------------------------

        int missingPawnShieldDifference = 0;

        if (whiteKingFile > 0 && !(BitBoardHelper.ContainsSquare(whitePawns, board.whiteKingSquare + PrecomputedData.UpLeft) || BitBoardHelper.ContainsSquare(whitePawns, board.whiteKingSquare + PrecomputedData.UpLeft + PrecomputedData.Up))) missingPawnShieldDifference++;

        if (whiteKingFile < 7 && !(BitBoardHelper.ContainsSquare(whitePawns, board.whiteKingSquare + PrecomputedData.UpRight) || BitBoardHelper.ContainsSquare(whitePawns, board.whiteKingSquare + PrecomputedData.UpRight + PrecomputedData.Up))) missingPawnShieldDifference++;

        if (!(BitBoardHelper.ContainsSquare(whitePawns, board.whiteKingSquare + PrecomputedData.Up) || BitBoardHelper.ContainsSquare(whitePawns, board.whiteKingSquare + PrecomputedData.Up + PrecomputedData.Up))) missingPawnShieldDifference++;


        if (blackKingFile > 0 && !(BitBoardHelper.ContainsSquare(blackPawns, board.blackKingSquare + PrecomputedData.DownLeft) || BitBoardHelper.ContainsSquare(blackPawns, board.blackKingSquare + PrecomputedData.DownLeft + PrecomputedData.Down))) missingPawnShieldDifference--;

        if (blackKingFile < 7 && !(BitBoardHelper.ContainsSquare(blackPawns, board.blackKingSquare + PrecomputedData.DownRight) || BitBoardHelper.ContainsSquare(blackPawns, board.blackKingSquare + PrecomputedData.DownRight + PrecomputedData.Down))) missingPawnShieldDifference--;

        if (!(BitBoardHelper.ContainsSquare(blackPawns, board.blackKingSquare + PrecomputedData.Down) || BitBoardHelper.ContainsSquare(blackPawns, board.blackKingSquare + PrecomputedData.Down + PrecomputedData.Down))) missingPawnShieldDifference--;

        result += (Weights[784] * missingPawnShieldDifference * mgWeight) >> 8;





        int openFileAboveKingDifference = 0;

        if (whiteKingFile > 0 && ((PrecomputedData.fileMasks[whiteKingFile - 1] & whitePawns) == 0)) openFileAboveKingDifference++;

        if (whiteKingFile < 7 && ((PrecomputedData.fileMasks[whiteKingFile + 1] & whitePawns) == 0)) openFileAboveKingDifference++;

        if ((PrecomputedData.fileMasks[whiteKingFile] & whitePawns) == 0) openFileAboveKingDifference++;


        if (blackKingFile > 0 && ((PrecomputedData.fileMasks[blackKingFile - 1] & blackPawns) == 0)) openFileAboveKingDifference--;

        if (blackKingFile < 7 && ((PrecomputedData.fileMasks[blackKingFile + 1] & blackPawns) == 0)) openFileAboveKingDifference--;

        if ((PrecomputedData.fileMasks[blackKingFile] & blackPawns) == 0) openFileAboveKingDifference--;

        result += (Weights[785] * openFileAboveKingDifference * mgWeight) >> 8;





        for (int i = 0; i < 4; i++)
        {
            int pawnStormRankDifference = 0;

            if (whiteKingFile > 0 && BitBoardHelper.ContainsSquare(blackPawns, board.whiteKingSquare + PrecomputedData.UpLeft + PrecomputedData.Up * i)) pawnStormRankDifference++;
            if (whiteKingFile < 7 && BitBoardHelper.ContainsSquare(blackPawns, board.whiteKingSquare + PrecomputedData.UpRight + PrecomputedData.Up * i)) pawnStormRankDifference++;
            if (BitBoardHelper.ContainsSquare(blackPawns, board.whiteKingSquare + PrecomputedData.Up + PrecomputedData.Up * i)) pawnStormRankDifference++;

            if (blackKingFile > 0 && BitBoardHelper.ContainsSquare(whitePawns, board.blackKingSquare + PrecomputedData.DownLeft + PrecomputedData.Down * i)) pawnStormRankDifference--;
            if (blackKingFile < 7 && BitBoardHelper.ContainsSquare(whitePawns, board.blackKingSquare + PrecomputedData.DownRight + PrecomputedData.Down * i)) pawnStormRankDifference--;
            if (BitBoardHelper.ContainsSquare(whitePawns, board.blackKingSquare + PrecomputedData.Down + PrecomputedData.Down * i)) pawnStormRankDifference--;


            result += (Weights[786 + i] * pawnStormRankDifference * mgWeight) >> 8;
        }

        return result;
    }
    #endregion

    #endregion
}