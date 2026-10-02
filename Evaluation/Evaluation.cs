
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public class Evaluation
{
    //TODO: Maybe make non-static for multithreaded performance

    private static readonly int[] Weights = {
        -23,32,26,-37,28,-23,50,23,-5,5,-2,-46,-36,-11,36,22,-6,3,-5,-13,-18,-13,3,-16,-6,3,5,-4,-7,-8,-7,-19,-4,2,6,2,0,2,4,-10,0,9,7,5,3,9,13,0,-2,5,5,3,3,7,4,-2,-2,0,1,0,-1,1,1,-1,0,0,0,0,0,0,0,0,-41,-15,-26,-28,-16,4,28,-33,-41,-28,-9,-18,1,-15,13,-32,-52,-25,-15,0,3,-8,-17,-62,-37,-11,-11,-7,7,-8,-10,-51,-41,-29,-10,-55,-12,37,-8,-36,-8,-29,-14,-11,-7,-8,-16,-20,0,0,0,0,0,0,0,0,-16,14,-20,-4,6,2,11,-13,-7,-12,19,52,49,36,4,16,3,24,56,49,63,59,53,3,12,9,54,53,70,59,23,7,6,51,41,91,60,67,43,19,-11,20,41,55,55,27,34,-1,-36,-15,19,12,0,29,-2,-10,-61,-8,-11,-10,-3,-23,-5,-27,-3,-1,21,6,3,5,-5,3,-1,34,10,8,13,20,51,8,21,16,14,-4,6,20,7,20,-3,4,-8,11,18,-10,1,-8,-10,-2,-2,6,6,-7,8,1,-10,0,0,-2,5,16,9,22,-25,-3,-16,-8,1,-3,-5,-32,-7,-12,-9,-7,-5,-7,-3,-8,-14,-12,10,14,20,8,-23,0,-38,-13,-13,-4,0,8,-1,-45,-38,-19,-12,-5,1,-2,-3,-16,-31,-17,-19,-10,-7,-5,-2,-14,-21,-12,3,8,-2,5,-2,-1,-4,1,2,8,7,11,8,5,-1,-6,15,19,14,17,7,11,4,8,3,6,7,3,5,3,10,3,16,29,12,-6,-6,-4,-4,5,13,17,23,19,10,8,-14,4,-4,1,1,7,14,5,-10,-13,-13,-14,1,0,14,4,-16,-19,-18,-18,-6,7,2,22,-14,-13,-17,7,15,26,23,55,-8,-34,-12,-3,-5,13,-1,21,-25,-7,-3,0,7,5,-2,4,-38,-35,-20,-11,-27,-10,-42,-71,-29,-6,9,22,23,15,-7,-29,-31,-3,13,23,26,22,7,-16,-33,-8,15,22,25,22,8,-20,-25,4,16,19,15,25,21,-8,-9,13,16,9,13,33,41,4,-14,7,5,3,6,24,23,1,-17,-11,-9,-9,-7,1,0,-9,0,0,0,0,0,0,0,0,24,9,15,7,13,10,-7,-1,16,10,0,5,5,5,-6,1,28,16,1,-4,-4,-4,3,10,43,26,12,-3,-3,4,15,23,77,64,40,16,3,13,39,50,63,54,33,8,8,13,35,40,0,0,0,0,0,0,0,0,-21,-50,-27,-19,-28,-21,-46,-18,-22,-24,-22,-25,-25,-18,-20,-24,-27,-15,-21,2,-2,-18,-25,-28,-23,-10,3,11,4,-1,-6,-20,-21,-7,10,8,10,7,-4,-20,-33,-16,7,7,-7,1,-15,-30,-39,-26,-17,-10,-20,-25,-27,-33,-42,-29,-23,-28,-23,-35,-21,-37,-15,-3,-15,-1,-2,-6,-10,-11,-4,-17,-5,0,3,-3,-12,-10,-6,4,6,15,12,0,-2,-7,-1,4,14,9,-2,10,-5,-5,6,11,7,6,5,2,-1,-1,5,3,5,5,-5,5,2,1,-10,-3,-1,-12,-2,-5,-3,-15,-7,-13,-11,-8,-8,-6,-8,-10,1,5,1,2,-5,-2,3,-25,2,-3,3,3,-4,-7,-6,-5,1,1,-5,-4,-7,-11,-10,-10,7,4,7,3,0,-5,-6,-6,11,7,9,2,2,6,-2,5,10,10,7,8,0,1,6,1,14,20,16,17,6,10,10,8,18,15,16,13,13,10,10,10,0,-6,-8,-24,1,-7,-4,-5,-1,-4,-11,-8,-9,-7,-6,-1,-4,-17,0,-8,0,5,7,7,2,-1,-3,6,1,3,9,11,-4,-1,-12,-3,9,13,12,20,-10,-8,-4,3,15,13,8,18,-4,-4,1,9,11,13,3,9,-17,-4,0,5,8,5,-7,4,99,356,337,541,1032,51,-8,-10,-1,1,17,39,89,141,4,-3,-8,-21,110,-13,-7,1,19,3,11,11,6,15
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
        return CalculatePieceSquareTables(board) + CalculateMaterial(board) + CalculatePawnStructure(board) + CalculateKingSafety(board) + CalculateMobility(board);
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
            int file = BoardHelper.IndexToFile(pawnSquare);

            if ((whitePawnBoard & PrecomputedData.isolationFileMasks[file]) == 0) isolatedPawnDifference++;
        }

        ulong blackPawns = blackPawnBoard;

        while (blackPawns != 0)
        {
            int pawnSquare = BitBoardHelper.PopFirstBit(ref blackPawns);
            int file = BoardHelper.IndexToFile(pawnSquare);

            if ((blackPawnBoard & PrecomputedData.isolationFileMasks[file]) == 0) isolatedPawnDifference--;
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

                int file = BoardHelper.IndexToFile(pawnSquare);

                if ((whitePawnBoard & PrecomputedData.isolationFileMasks[file]) != 0) connectedPassedPawnDifference++;
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

                int file = BoardHelper.IndexToFile(pawnSquare);

                if ((blackPawnBoard & PrecomputedData.isolationFileMasks[file]) != 0) connectedPassedPawnDifference--;
            }
        }

        result += Weights[782] * connectedPassedPawnDifference;
        //Console.WriteLine("connectedPassedPawnDifference: " + connectedPassedPawnDifference);
        return result;
    }
    #endregion

    #region King Safety
    //TODO: Add way more features
    //1 missing pawns on top of king difference + 1 hole in kings pawnshield difference + 1 complete open file above king difference + 4 pawn storm rank differences above king = 7 feature
    private int CalculateKingSafety(Board board)
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




        int missingPawnShieldDifference = 0;
        int openFileAboveKingDifference = 0;

        ulong whitePawnShield = PrecomputedData.kingPawnDoubleCoverMasks[board.whiteKingSquare] & whitePawns;
        ulong blackPawnShield = PrecomputedData.kingPawnDoubleCoverMasks[board.blackKingSquare + 64] & blackPawns;

        if (whiteKingFile == 0)
        {
            if ((PrecomputedData.fileMasks[whiteKingFile] & whitePawnShield) == 0) missingPawnShieldDifference++;
            if ((PrecomputedData.fileMasks[whiteKingFile + 1] & whitePawnShield) == 0) missingPawnShieldDifference++;

            if ((PrecomputedData.fileMasks[whiteKingFile + 1] & whitePawns) == 0) openFileAboveKingDifference++;
            if ((PrecomputedData.fileMasks[whiteKingFile] & whitePawns) == 0) openFileAboveKingDifference++;
        }
        else if (whiteKingFile == 7)
        {
            if ((PrecomputedData.fileMasks[whiteKingFile] & whitePawnShield) == 0) missingPawnShieldDifference++;
            if ((PrecomputedData.fileMasks[whiteKingFile - 1] & whitePawnShield) == 0) missingPawnShieldDifference++;

            if ((PrecomputedData.fileMasks[whiteKingFile - 1] & whitePawns) == 0) openFileAboveKingDifference++;
            if ((PrecomputedData.fileMasks[whiteKingFile] & whitePawns) == 0) openFileAboveKingDifference++;
        }
        else
        {
            if ((PrecomputedData.fileMasks[whiteKingFile] & whitePawnShield) == 0) missingPawnShieldDifference++;
            if ((PrecomputedData.fileMasks[whiteKingFile + 1] & whitePawnShield) == 0) missingPawnShieldDifference++;
            if ((PrecomputedData.fileMasks[whiteKingFile - 1] & whitePawnShield) == 0) missingPawnShieldDifference++;

            if ((PrecomputedData.fileMasks[whiteKingFile - 1] & whitePawns) == 0) openFileAboveKingDifference++;
            if ((PrecomputedData.fileMasks[whiteKingFile + 1] & whitePawns) == 0) openFileAboveKingDifference++;
            if ((PrecomputedData.fileMasks[whiteKingFile] & whitePawns) == 0) openFileAboveKingDifference++;
        }



        if (blackKingFile == 0)
        {
            if ((PrecomputedData.fileMasks[blackKingFile] & blackPawnShield) == 0) missingPawnShieldDifference--;
            if ((PrecomputedData.fileMasks[blackKingFile + 1] & blackPawnShield) == 0) missingPawnShieldDifference--;

            if ((PrecomputedData.fileMasks[blackKingFile + 1] & blackPawns) == 0) openFileAboveKingDifference--;
            if ((PrecomputedData.fileMasks[blackKingFile] & blackPawns) == 0) openFileAboveKingDifference--;
        }
        else if (blackKingFile == 7)
        {
            if ((PrecomputedData.fileMasks[blackKingFile] & blackPawnShield) == 0) missingPawnShieldDifference--;
            if ((PrecomputedData.fileMasks[blackKingFile - 1] & blackPawnShield) == 0) missingPawnShieldDifference--;

            if ((PrecomputedData.fileMasks[blackKingFile - 1] & blackPawns) == 0) openFileAboveKingDifference--;
            if ((PrecomputedData.fileMasks[blackKingFile] & blackPawns) == 0) openFileAboveKingDifference--;
        }
        else
        {
            if ((PrecomputedData.fileMasks[blackKingFile] & blackPawnShield) == 0) missingPawnShieldDifference--;
            if ((PrecomputedData.fileMasks[blackKingFile + 1] & blackPawnShield) == 0) missingPawnShieldDifference--;
            if ((PrecomputedData.fileMasks[blackKingFile - 1] & blackPawnShield) == 0) missingPawnShieldDifference--;

            if ((PrecomputedData.fileMasks[blackKingFile - 1] & blackPawns) == 0) openFileAboveKingDifference--;
            if ((PrecomputedData.fileMasks[blackKingFile + 1] & blackPawns) == 0) openFileAboveKingDifference--;
            if ((PrecomputedData.fileMasks[blackKingFile] & blackPawns) == 0) openFileAboveKingDifference--;
        }

        result += (Weights[784] * missingPawnShieldDifference * mgWeight) >> 8;

        result += (Weights[785] * openFileAboveKingDifference * mgWeight) >> 8;




        for (int i = 0; i < 4; i++)
        {
            //int pawnStormRankDifference = BitBoardHelper.BitCount(PrecomputedData.kingPawnCoverMasks[board.whiteKingSquare + PrecomputedData.Up * i] & blackPawns) - BitBoardHelper.BitCount(PrecomputedData.kingPawnCoverMasks[board.blackKingSquare + 64 + PrecomputedData.Down * i] & whitePawns);

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




    #region Mobility

    //Mobility for 3(*2) piece types in both game phases = 6 features
    private int CalculateMobility(Board board) //TODO: Account for mobility to squares that are attacked
    {
        int result = 0;

        PieceList whiteBishopList = board.GetPieceList(Piece.Bishop, 0);
        PieceList blackBishopList = board.GetPieceList(Piece.Bishop, 1);
        PieceList whiteRookList = board.GetPieceList(Piece.Rook, 0);
        PieceList blackRookList = board.GetPieceList(Piece.Rook, 1);
        PieceList whiteQueenList = board.GetPieceList(Piece.Queen, 0);
        PieceList blackQueenList = board.GetPieceList(Piece.Queen, 1);


        ulong otherPieces = board.GetPieceList(Piece.Pawn, 0).bitboard | board.GetPieceList(Piece.Knight, 0).bitboard | board.GetPieceList(Piece.Pawn, 1).bitboard | board.GetPieceList(Piece.Knight, 1).bitboard;

        ulong allPiecesNoKings = whiteBishopList.bitboard | blackBishopList.bitboard | whiteRookList.bitboard | blackRookList.bitboard | whiteQueenList.bitboard | blackQueenList.bitboard | otherPieces;

        //We exclude the opponent king bc he can't be on check rays - will be irrelevant when we account for checks in quiescence
        ulong whiteAllPieces = allPiecesNoKings | (1UL << board.whiteKingSquare);
        ulong blackAllPieces = allPiecesNoKings | (1UL << board.blackKingSquare);



        int bishopDifference = 0;

        for (int i = 0; i < whiteBishopList.Count; i++)
        {
            bishopDifference += BitBoardHelper.BitCount(MagicData.GetBishopMoveBoard(whiteAllPieces, whiteBishopList[i])) >> 1;
        }

        for (int i = 0; i < blackBishopList.Count; i++)
        {
            bishopDifference -= BitBoardHelper.BitCount(MagicData.GetBishopMoveBoard(blackAllPieces, blackBishopList[i])) >> 1;
        }

        result += (Weights[790] * bishopDifference * mgWeight) >> 8;
        result += (Weights[791] * bishopDifference * egWeight) >> 8;



        int rookDifference = 0;

        for (int i = 0; i < whiteRookList.Count; i++) //TODO: Split into horizontal and vertical mobility
        {
            rookDifference += BitBoardHelper.BitCount(MagicData.GetRookMoveBoard(whiteAllPieces, whiteRookList[i])) >> 1;
        }

        for (int i = 0; i < blackRookList.Count; i++)
        {
            rookDifference -= BitBoardHelper.BitCount(MagicData.GetRookMoveBoard(blackAllPieces, blackRookList[i])) >> 1;
        }

        result += (Weights[792] * rookDifference * mgWeight) >> 8;
        result += (Weights[793] * rookDifference * egWeight) >> 8;



        int queenDifference = 0;

        for (int i = 0; i < whiteQueenList.Count; i++)
        {
            queenDifference += BitBoardHelper.BitCount(MagicData.GetRookMoveBoard(whiteAllPieces, whiteQueenList[i]) | MagicData.GetBishopMoveBoard(whiteAllPieces, whiteQueenList[i])) >> 1;
        }

        for (int i = 0; i < blackQueenList.Count; i++)
        {
            queenDifference -= BitBoardHelper.BitCount(MagicData.GetRookMoveBoard(blackAllPieces, blackQueenList[i]) | MagicData.GetBishopMoveBoard(blackAllPieces, blackQueenList[i])) >> 1;
        }

        result += (Weights[792] * queenDifference * mgWeight) >> 8;
        result += (Weights[793] * queenDifference * egWeight) >> 8;

        return result;
    }

    #endregion

    #endregion
}