
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public class Evaluation
{
    //TODO: Maybe make non-static for multithreaded performance

    private const int WeigthCount = 768 + 12 + 18 + 14 + 6;
    private const int EndGameIndexOffset = WeigthCount / 2;

    private static readonly int[] Weights = {
        -4,0,9,5,1,7,2,6,-9,0,2,4,2,4,3,5,2,8,5,4,-1,-8,-1,2,0,3,2,1,0,-1,-5,-9,-1,4,2,-2,-1,5,6,6,-5,-1,2,-4,1,-5,-3,0,1,0,-1,0,0,-3,0,1,0,-1,-3,1,0,-1,-2,0,0,0,0,0,0,0,0,0,1,1,-1,-1,0,-6,-2,-2,1,0,-1,-1,-2,2,4,4,0,-4,-1,-2,-1,-1,-1,-3,1,0,0,-1,0,-1,6,2,1,1,6,-5,8,10,3,2,-3,11,0,-4,3,3,5,-9,0,0,0,0,0,0,0,0,5,4,10,-4,-1,1,5,-1,-3,12,6,1,1,1,0,2,4,2,0,4,-1,0,-3,3,2,-2,4,2,1,-1,4,-1,5,2,1,3,1,-3,2,-7,0,2,1,12,-2,-6,6,-4,8,-1,0,-13,-1,-4,-3,-3,-6,-1,-1,-3,-3,3,2,-12,-2,1,4,3,-6,3,-2,-5,-5,3,0,0,-1,1,-3,5,7,1,1,1,-3,-3,4,-2,-1,-1,-4,1,-2,-3,4,3,-7,-2,-11,4,-4,0,-7,-2,0,-5,0,-2,-4,5,5,-5,1,-3,13,4,8,10,-2,-15,11,1,-2,5,2,1,-1,-5,2,2,-1,1,0,2,-5,2,0,-11,-1,-5,7,4,2,-5,-3,-6,8,9,3,2,-2,-2,-6,-3,2,4,-5,1,-1,5,3,-2,2,-12,4,3,-5,10,-4,-1,-1,5,1,-1,1,1,3,4,-10,5,5,6,-2,-5,-4,6,-1,4,1,2,-4,-1,4,0,2,-1,1,5,1,-5,6,5,0,3,4,-4,-5,5,10,6,-2,3,2,8,3,-4,0,3,1,-2,6,-2,-2,3,4,-2,12,-5,1,-1,1,7,4,0,7,4,4,7,10,6,6,10,7,1,4,-6,-2,-6,-2,0,-3,4,0,-2,-6,-4,81,461,424,547,1170,36,12,-20,-9,-11,-15,-1,-1,39,13,-12,-15,-29,-14,-67,-23,-7,12,14,1,-4,-2,-2,-1,-1,-4,-1,-4,4,2,-3,-1,0,-1,-1,-2,-6,3,-1,-2,2,-1,0,1,6,-2,2,-1,0,1,0,3,2,3,2,0,1,-1,1,-2,-1,0,0,1,1,-4,1,3,-6,9,-1,1,0,-4,0,5,-5,3,-4,4,3,2,1,-2,0,0,0,0,0,0,0,0,0,0,-1,1,4,2,2,0,1,1,2,-1,1,1,0,-2,1,1,0,-1,1,1,0,0,1,0,-1,0,-1,3,-1,-1,-3,-1,-5,-2,-3,-1,0,-2,-2,-5,-6,-6,-4,-7,-11,-4,0,0,0,0,0,0,0,0,4,-3,-1,1,-1,2,0,8,-4,2,1,-3,-3,-6,-2,-2,0,-9,0,2,1,-1,8,-4,-1,-1,-3,-2,-2,2,1,-1,-4,2,-1,-2,-2,2,0,1,-6,2,3,-3,2,0,-3,-7,-3,3,-3,-3,-4,-5,-7,-6,-2,-1,-6,-5,-9,0,1,-12,3,2,1,-1,4,-1,4,6,-1,1,-1,2,3,1,-1,-1,-6,-3,-2,-3,0,1,-6,5,5,-2,2,-1,2,2,-6,-6,3,2,2,1,-3,1,9,5,-2,-3,-2,-1,-3,-2,-2,6,1,1,0,-14,-2,-6,-4,-1,-5,-1,-4,2,3,-3,-8,-12,-1,-2,1,-2,2,-2,6,2,1,7,4,3,-1,0,2,1,3,0,-1,-6,-1,3,5,4,0,0,0,-1,0,-1,1,-5,2,-2,-2,1,-2,-5,4,-3,4,4,1,-1,-1,3,4,1,0,2,5,-1,-1,-1,0,-2,3,-2,3,-3,1,-2,-3,2,-3,4,6,2,2,4,5,2,0,-4,4,-6,-4,3,-3,-5,-3,-5,4,0,5,-7,3,0,1,-2,-1,3,-1,-4,1,3,1,-1,-7,8,-3,1,-8,-5,5,2,2,-6,6,-3,8,-4,-7,-4,-3,-5,1,3,9,-11,-1,4,-1,2,0,-4,-7,-6,109,290,258,527,907,60,-25,-3,-2,0,25,64,147,197,6,8,13,4,75,30,15,11,13,9,20
        };

    private Vector<int>[] weightVectors;

    private const int Bias = 0;

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


    private int rawMGEval = 0;
    private int rawEGEval = 0;


    public int Evaluate(Board board)
    {
        //ApplyPhase(board);

        int result = CalculateResult(board) + Bias;

        int perspective = board.colorToMove == Piece.White ? 1 : -1;

        return result * perspective;
    }

    private int CalculateResult(Board board)
    {
        rawMGEval = 0;
        rawEGEval = 0;

        //TODO: Try making each of these return a Vector64<int>, where the first int holds the MG eval and the second contains the EG eval
        //TODO: We also do two weight multiplications per feature, for its MG and EG counterparts, which could potentially be done in a single instruction with Vector64
        CalculatePieceSquareTables(board);
        CalculateMaterial(board);
        CalculatePawnStructure(board);
        CalculateKingSafety(board);
        CalculateMobility(board);

        return ApplyPhase(board);
    }

    public static int GetPieceTypeValue(int piece)
    {
        Piece.Type(piece);

        //This doesn't take into account the game phase, which i will just assume is unnecessary
        return Weights[384 + piece - 2];
    }

    #region Phase

    private const int KnightPhase = 1;
    private const int BishopPhase = 1;
    private const int RookPhase = 2;
    private const int QueenPhase = 4;

    private const int MaxPhase = KnightPhase * 4 + BishopPhase * 4 + RookPhase * 4 + QueenPhase * 2;

    private int ApplyPhase(Board board)
    {
        int phase = (GetRawPhase(board) * 256 + (MaxPhase / 2)) / MaxPhase;  //Phase is between 0 (MG) and 256 (EG)

        int mgWeight = 256 - phase;
        int egWeight = phase;

        return (rawMGEval * mgWeight + rawEGEval * egWeight) >> 8;
    }

    public int GetRawPhase(Board board) //0 -> MaxPhase
    {
        int phase = MaxPhase;

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
    private void CalculatePieceSquareTables(Board board)
    {
        //TODO: find more efficient way to do this?

        //TODO: Use the allPieceList array instead of calling GetPiecelist

        rawMGEval += Weights[board.whiteKingSquare];
        rawEGEval += Weights[board.whiteKingSquare + EndGameIndexOffset];

        SetPSQTFeaturesWhite(board.GetPieceList(Piece.Pawn, 0), 64 * 1);
        SetPSQTFeaturesWhite(board.GetPieceList(Piece.Knight, 0), 64 * 2);
        SetPSQTFeaturesWhite(board.GetPieceList(Piece.Bishop, 0), 64 * 3);
        SetPSQTFeaturesWhite(board.GetPieceList(Piece.Rook, 0), 64 * 4);
        SetPSQTFeaturesWhite(board.GetPieceList(Piece.Queen, 0), 64 * 5);

        int blackKingFlipped = BoardHelper.FlipIndex(board.blackKingSquare);

        rawMGEval -= Weights[blackKingFlipped];
        rawEGEval -= Weights[blackKingFlipped + EndGameIndexOffset];

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
            rawMGEval += Weights[list[i] + PSQTOffset]; //Activate middlegame PSQT at this square with intensity equal to how "much" we are in the middlegame
            rawEGEval += Weights[list[i] + PSQTOffset + EndGameIndexOffset]; //Activate endgame PSQT at this square with intensity equal to how "much" we are in the endgame 
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetPSQTFeaturesBlack(PieceList list, int PSQTOffset)
    {
        for (int i = 0; i < list.Count; i++)
        {
            //TODO: Rename "flipindex" to "mirrorindex"
            int mirroredSquare = BoardHelper.FlipIndex(list[i]);

            rawMGEval -= Weights[mirroredSquare + PSQTOffset]; //Activate middlegame PSQT at this square with intensity equal to how "much" we are in the middlegame
            rawEGEval -= Weights[mirroredSquare + PSQTOffset + EndGameIndexOffset]; //Activate endgame PSQT at this square with intensity equal to how "much" we are in the endgame
        }
    }
    #endregion


    #region Material
    //5 material differences + 1 bishop pair difference = 6 features
    private void CalculateMaterial(Board board)
    {
        //TODO: Use the allPieceList array instead of calling GetPiecelist
        int pawnDiff = board.GetPieceList(Piece.Pawn, 0).Count - board.GetPieceList(Piece.Pawn, 1).Count;
        int knightDiff = board.GetPieceList(Piece.Knight, 0).Count - board.GetPieceList(Piece.Knight, 1).Count;
        int bishopDiff = board.GetPieceList(Piece.Bishop, 0).Count - board.GetPieceList(Piece.Bishop, 1).Count;
        int rookDiff = board.GetPieceList(Piece.Rook, 0).Count - board.GetPieceList(Piece.Rook, 1).Count;
        int queenDiff = board.GetPieceList(Piece.Queen, 0).Count - board.GetPieceList(Piece.Queen, 1).Count;

        rawMGEval += pawnDiff * Weights[384];
        rawMGEval += knightDiff * Weights[385];
        rawMGEval += bishopDiff * Weights[386];
        rawMGEval += rookDiff * Weights[387];
        rawMGEval += queenDiff * Weights[388];
        rawEGEval += pawnDiff * Weights[384 + EndGameIndexOffset];
        rawEGEval += knightDiff * Weights[385 + EndGameIndexOffset];
        rawEGEval += bishopDiff * Weights[386 + EndGameIndexOffset];
        rawEGEval += rookDiff * Weights[387 + EndGameIndexOffset];
        rawEGEval += queenDiff * Weights[388 + EndGameIndexOffset];


        //TODO: Could just do board.GetPieceList(Piece.Bishop, ?).Count >> 1 instead of the if
        int whiteBishopPair = board.GetPieceList(Piece.Bishop, 0).Count > 1 ? 1 : 0; //TODO: Obv don't call this again
        int blackBishopPair = board.GetPieceList(Piece.Bishop, 1).Count > 1 ? 1 : 0; //TODO: Obv don't call this again

        int bishopPairDiff = whiteBishopPair - blackBishopPair;

        rawMGEval += bishopPairDiff * Weights[389];
        rawEGEval += bishopPairDiff * Weights[389 + EndGameIndexOffset];
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

        rawMGEval += Weights[390] * doubledPawnDifference;
        rawEGEval += Weights[390 + EndGameIndexOffset] * doubledPawnDifference;


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

        rawMGEval += Weights[391] * isolatedPawnDifference;
        rawEGEval += Weights[391 + EndGameIndexOffset] * isolatedPawnDifference;


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
                int rank = BoardHelper.IndexToRank(pawnSquare);

                rawMGEval += Weights[392 + rank - 1]; //Rank will be 1-2-3-4-5-6 because we are working with pawns
                rawEGEval += Weights[392 + rank - 1 + EndGameIndexOffset]; //Rank will be 1-2-3-4-5-6 because we are working with pawns

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
                int rank = 7 - BoardHelper.IndexToRank(pawnSquare);

                rawMGEval -= Weights[392 + rank - 1]; //Rank will be 1-2-3-4-5-6 because we are working with pawns
                rawEGEval -= Weights[392 + rank - 1 + EndGameIndexOffset]; //Rank will be 1-2-3-4-5-6 because we are working with pawns

                int file = BoardHelper.IndexToFile(pawnSquare);

                if ((blackPawnBoard & PrecomputedData.isolationFileMasks[file]) != 0) connectedPassedPawnDifference--;
            }
        }

        rawMGEval += Weights[398] * connectedPassedPawnDifference;
        rawEGEval += Weights[398 + EndGameIndexOffset] * connectedPassedPawnDifference;
    }
    #endregion

    #region King Safety
    //TODO: Add way more features
    //1 missing pawns on top of king difference + 1 hole in kings pawnshield difference + 1 complete open file above king difference + 4 pawn storm rank differences above king = 7 feature
    private void CalculateKingSafety(Board board)
    {
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

        rawMGEval += Weights[399] * missingPawnDefenseDifference;
        rawEGEval += Weights[399 + EndGameIndexOffset] * missingPawnDefenseDifference;




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

        rawMGEval += Weights[400] * missingPawnShieldDifference;
        rawEGEval += Weights[400 + EndGameIndexOffset] * missingPawnShieldDifference;

        rawMGEval += Weights[401] * openFileAboveKingDifference;
        rawMGEval += Weights[401 + EndGameIndexOffset] * openFileAboveKingDifference;




        for (int i = 0; i < 4; i++)
        {
            int pawnStormRankDifference = BitBoardHelper.BitCount(PrecomputedData.kingPawnCoverMasks[board.whiteKingSquare + PrecomputedData.Up * i] & blackPawns) - BitBoardHelper.BitCount(PrecomputedData.kingPawnCoverMasks[board.blackKingSquare + 64 + PrecomputedData.Down * i] & whitePawns);

            rawMGEval += Weights[402 + i] * pawnStormRankDifference;
            rawEGEval += Weights[402 + i + EndGameIndexOffset] * pawnStormRankDifference;
        }
    }
    #endregion




    #region Mobility

    //Mobility for 3(*2) piece types in both game phases = 6 features
    private void CalculateMobility(Board board) //TODO: Account for mobility to squares that are attacked
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

        rawMGEval += Weights[406] * bishopDifference;
        rawEGEval += Weights[406 + EndGameIndexOffset] * bishopDifference;



        int rookDifference = 0;

        for (int i = 0; i < whiteRookList.Count; i++) //TODO: Split into horizontal and vertical mobility
        {
            rookDifference += BitBoardHelper.BitCount(MagicData.GetRookMoveBoard(whiteAllPieces, whiteRookList[i])) >> 1;
        }

        for (int i = 0; i < blackRookList.Count; i++)
        {
            rookDifference -= BitBoardHelper.BitCount(MagicData.GetRookMoveBoard(blackAllPieces, blackRookList[i])) >> 1;
        }

        rawMGEval += Weights[407] * rookDifference;
        rawEGEval += Weights[407 + EndGameIndexOffset] * rookDifference;



        int queenDifference = 0;

        for (int i = 0; i < whiteQueenList.Count; i++)
        {
            queenDifference += BitBoardHelper.BitCount(MagicData.GetRookMoveBoard(whiteAllPieces, whiteQueenList[i]) | MagicData.GetBishopMoveBoard(whiteAllPieces, whiteQueenList[i])) >> 1;
        }

        for (int i = 0; i < blackQueenList.Count; i++)
        {
            queenDifference -= BitBoardHelper.BitCount(MagicData.GetRookMoveBoard(blackAllPieces, blackQueenList[i]) | MagicData.GetBishopMoveBoard(blackAllPieces, blackQueenList[i])) >> 1;
        }

        rawMGEval += Weights[408] * queenDifference;
        rawEGEval += Weights[408 + EndGameIndexOffset] * queenDifference;
    }

    #endregion

    #endregion
}