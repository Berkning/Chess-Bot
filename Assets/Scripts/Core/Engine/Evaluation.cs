
using System;

public static class Evaluation
{
    public const int PawnValue = 100;
    public const int KnightValue = 310;
    public const int BishopValue = 330;
    public const int RookValue = 500;
    public const int QueenValue = 900;

    //private const int DoubledPawnValue = -40;

    //TODO: Increase as it moves up board
    private const int PassedPawnValue = 50; //Increase in endgame - maybe wont make a difference since passed pawns are unlikely to arise in early game anyway
    private const int PassedPawnConnectionValue = 15;
    private const int IsolatedPawnValue = -50;

    private static int totalMaterialWithoutPawns;
    private static int whiteMaterialValue;
    private static int blackMaterialValue;
    public static float gameStage;

    public static int Evaluate()
    {
        EvaluateMaterial();
        gameStage = CalculateGameStage();

        int whiteEval = whiteMaterialValue + Positioning.GetPositioningScore(0) + EvaluatePawnStructure(0, 1);
        int blackEval = blackMaterialValue + Positioning.GetPositioningScore(1) + EvaluatePawnStructure(1, 0);

        int evaluation = whiteEval - blackEval;

        int perspective = Board.colorToMove == Piece.White ? 1 : -1;

        return evaluation * perspective;
    }

    private static int EvaluatePawnStructure(int colorBit, int enemyColorBit) //TODO: give high score in early to midgame when king can see pawns above him
    {
        int score = 0;

        PieceList friendlyPawns = Board.pawnList[colorBit];
        ulong friendlyPawnBoard = friendlyPawns.bitboard;
        ulong enemyPawnBoard = Board.pawnList[enemyColorBit].bitboard;

        //Doubled Pawns
        // for (int file = 0; file < 8; file++)
        // {
        //     ulong fileMask = PrecomputedData.fileMasks[file];
        //     int pawnsOnFile = BitBoardHelper.BitCount(friendlyPawnBoard & fileMask);

        //     if (pawnsOnFile > 1) score += DoubledPawnValue * (pawnsOnFile - 1);
        // }

        //Connected Pawns

        // for (int i = 0; i < friendlyPawns.Count; i++)
        // {
        //     int pawnSquare = friendlyPawns[i];
        //     ulong attackBoard = PrecomputedData.pawnAttackBitboards[pawnSquare];

        //     ulong connectedPawnBoard = friendlyPawnBoard & attackBoard;
        //     score += BitBoardHelper.BitCount(connectedPawnBoard) * PawnConnectionValue;
        // }

        //Passed pawns
        // for (int i = 0; i < friendlyPawns.Count; i++)
        // {
        //     int square = friendlyPawns[i];
        //     ulong opposingPawnBoard = PrecomputedData.passedPawnMasks[square + colorBit * 64] & enemyPawnBoard;

        //     int opposingPawnCount = BitBoardHelper.BitCount(opposingPawnBoard);

        //     if (opposingPawnCount == 0) score += PassedPawnValue;
        // }

        return score;
    }

    private static void EvaluateMaterial()
    {
        whiteMaterialValue = 0;
        blackMaterialValue = 0;

        int whiteNonPawn = 0;
        whiteNonPawn += Board.knightList[0].Count * KnightValue;
        whiteNonPawn += Board.bishopList[0].Count * BishopValue;
        whiteNonPawn += Board.rookList[0].Count * RookValue;
        whiteNonPawn += Board.queenList[0].Count * QueenValue;

        whiteMaterialValue = whiteNonPawn + Board.pawnList[0].Count * PawnValue;

        int blackNonPawn = 0;
        blackNonPawn += Board.knightList[1].Count * KnightValue;
        blackNonPawn += Board.bishopList[1].Count * BishopValue;
        blackNonPawn += Board.rookList[1].Count * RookValue;
        blackNonPawn += Board.queenList[1].Count * QueenValue;
        blackMaterialValue = blackNonPawn + Board.pawnList[1].Count * PawnValue;

        totalMaterialWithoutPawns = whiteNonPawn + blackNonPawn;
    }

    private static float CalculateGameStage()
    {
        float material = totalMaterialWithoutPawns / 100f + 7.5f;
        if (material < 13.3) return 2f;
        //stage = -0.0006f * material * material + 0.0091f * material + 2;
        //stage = -0.035f * material + 2.645f;
        return Math.Max(-0.0008f * material * material + 0.0213f * material + 1.7824f, 0f);
    }

    public static int GetPieceTypeValue(int type)
    {
        switch (type) //TODO: Optimize
        {
            case Piece.King: return 0; //FIXME: No fucking idea why this keeps happening
            case Piece.Pawn: return PawnValue;
            case Piece.Knight: return KnightValue;
            case Piece.Bishop: return BishopValue;
            case Piece.Rook: return RookValue;
            case Piece.Queen: return QueenValue;
        }

        Console.WriteLine("Invalid Piece Type: " + type);
        return -1234567;
    }
}