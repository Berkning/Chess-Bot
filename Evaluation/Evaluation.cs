
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public class Evaluation
{
    //TODO: Maybe make non-static for multithreaded performance

    private static readonly int[] Weights = {
        -33,49,37,-35,36,-15,61,33,3,17,-2,-46,-32,-8,44,32,-7,10,-16,-37,-45,-29,6,-26,-10,12,9,-22,-30,-32,-24,-51,-6,5,14,-1,-5,-2,0,-32,3,26,17,13,4,19,27,-2,-2,16,15,10,9,15,5,-7,-5,3,6,3,-2,5,4,-4,0,0,0,0,0,0,0,0,-38,-13,-22,-26,-13,8,29,-32,-37,-26,-6,-16,3,-12,14,-31,-50,-24,-12,2,5,-6,-17,-62,-36,-9,-10,-6,8,-8,-9,-52,-56,-49,-18,-63,-15,50,-22,-57,-39,-71,-47,-32,-17,-24,-37,-63,0,0,0,0,0,0,0,0,-32,45,2,29,44,36,43,-24,24,4,54,82,80,77,39,53,35,55,86,83,99,89,87,36,44,41,85,82,99,92,63,39,41,82,75,122,88,112,73,64,-4,66,81,109,127,79,94,19,-41,1,62,45,19,86,12,3,-131,-20,-23,-17,-1,-51,-12,-67,10,9,37,21,17,20,-6,19,7,48,23,21,27,37,65,24,37,30,28,8,19,33,19,36,6,14,3,21,28,2,13,4,-5,11,5,15,15,-6,21,13,-5,5,3,1,18,36,24,38,-26,5,-18,-17,7,-2,0,-40,-10,-27,-24,-22,-14,-18,-9,-20,-17,-16,6,9,16,6,-24,-1,-45,-24,-24,-13,-4,5,-1,-57,-51,-36,-21,-13,-2,-3,-1,-24,-49,-41,-40,-23,-17,-10,-2,-26,-45,-29,-4,12,-10,5,-4,-8,-15,-5,-4,10,13,26,21,11,-8,-20,21,35,28,38,13,24,2,14,-1,11,13,3,10,3,14,13,27,38,20,-5,-12,-9,0,12,20,24,29,31,20,16,-12,12,-2,5,3,11,17,7,-4,-16,-12,-15,1,1,13,3,-20,-20,-18,-20,-13,5,-1,21,-15,-17,-24,10,26,50,47,71,-6,-34,-21,-13,-21,23,-2,41,-44,-19,-11,-3,14,8,-4,4,-46,-43,-22,-7,-28,-10,-43,-72,-33,-6,13,25,25,17,-7,-30,-34,-3,17,30,33,27,8,-11,-39,-8,16,27,31,27,13,-13,-30,5,17,22,18,28,23,-3,-14,13,17,10,15,34,43,7,-26,10,7,5,10,31,33,5,-46,-26,-17,-18,-11,4,1,-21,0,0,0,0,0,0,0,0,24,9,14,7,12,8,-8,-2,15,9,-2,4,4,3,-6,0,27,15,0,-5,-5,-5,2,9,42,25,11,-4,-4,3,14,23,85,75,49,27,10,12,46,59,90,89,63,30,26,31,64,67,0,0,0,0,0,0,0,0,-51,-80,-45,-39,-54,-43,-80,-46,-51,-43,-45,-46,-45,-45,-44,-61,-56,-36,-40,-17,-23,-38,-51,-60,-48,-27,-15,-6,-13,-20,-25,-43,-41,-27,-9,-11,-7,-17,-24,-45,-56,-40,-14,-20,-41,-18,-45,-57,-59,-41,-42,-27,-38,-58,-51,-68,-77,-69,-45,-54,-48,-70,-55,-93,-30,-11,-31,-13,-13,-20,-21,-27,-12,-28,-14,-9,-6,-13,-23,-26,-21,-5,1,11,7,-6,-10,-21,-9,-1,10,9,-2,7,-11,-14,1,5,6,7,6,4,-8,-11,0,-1,5,6,-8,1,-4,-13,-15,-9,-1,-15,-6,-9,-7,-20,-16,-24,-21,-15,-16,-14,-20,-22,5,9,5,7,-1,0,5,-24,8,4,10,8,-2,-5,-6,4,10,10,1,1,-5,-10,-11,-7,17,15,18,12,6,-2,-5,-1,21,15,13,3,8,8,0,8,15,14,11,9,0,-2,5,1,18,27,16,13,3,5,10,5,21,16,21,15,15,13,11,13,-12,-25,-30,-47,-11,-18,-13,-16,-9,-16,-25,-22,-25,-32,-26,-10,-9,-38,4,-11,1,5,7,11,-4,6,2,21,8,8,13,19,-6,5,-12,8,30,28,28,30,-20,-11,5,6,28,15,5,5,-13,-3,11,24,31,23,7,6,-24,-5,2,10,10,6,-21,2,100,356,344,550,1062,52,-8,-10,-2,1,17,39,84,129,4,-3,-7,-23,160,-11,-5,2,20,0,13,8,7,10
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

        return result * perspective + TunableConstants.TempoBonus;
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
        //TODO: Skip all of this if mgWeight == 0

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
            int pawnStormRankDifference = BitBoardHelper.BitCount(PrecomputedData.kingPawnCoverMasks[board.whiteKingSquare + PrecomputedData.Up * i] & blackPawns) - BitBoardHelper.BitCount(PrecomputedData.kingPawnCoverMasks[board.blackKingSquare + 64 + PrecomputedData.Down * i] & whitePawns);

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