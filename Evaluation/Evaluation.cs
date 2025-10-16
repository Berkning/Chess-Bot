
using System;

public static class Evaluation
{
    public const int PawnValue = 100;
    public const int KnightValue = 310;
    public const int BishopValue = 330;
    public const int RookValue = 500;
    public const int QueenValue = 900;

    private const int TempoBonusBase = 20;
    private const int TempoBonusEndgame = 5;

    //private const int DoubledPawnValue = -20; //The value difference comparing a normal pawn to a doubled one

    //TODO: Increase as it moves up board
    //private const int PassedPawnValue = 20; //Increase in endgame - maybe wont make a difference since passed pawns are unlikely to arise in early game anyway
    //private const int PassedPawnConnectionValue = 15; //Value increase of passed pawn for every supporting pawn it has beside/behind (but not directly behind) it
    //private const int IsolatedPawnValue = -50;

    private static int totalMaterialWithoutPawns;
    private static int whiteMaterialValue;
    private static int blackMaterialValue;

    public static float gameStage;
    public static float endgameMultiplier;
    //public static float earlygameMultiplier;

    //public static ulong darkPawnBoard;
    //public static ulong lightPawnBoard;

    //private static int darkPawnCount;
    //private static int lightPawnCount;
    //public static int pawnColorCountDifference; //Darkcount - lightCount


    public static int Evaluate() //TODO: https://www.chessprogramming.org/Tempo - tempo bonus to avoid score oscillation - except in endgame
    {
        EvaluateMaterial();
        gameStage = CalculateGameStage();
        endgameMultiplier = Math.Max(gameStage - 1f, 0f);
        //earlygameMultiplier = Math.Max(1f - gameStage, 0f);

        //ulong darkPawnBoard = (Board.pawnList[0].bitboard | Board.pawnList[1].bitboard) & PrecomputedData.DarkSquareMask;
        //ulong lightPawnBoard = (Board.pawnList[0].bitboard | Board.pawnList[1].bitboard) & PrecomputedData.LightSquareMask;

        //int darkPawnCount = BitBoardHelper.BitCount(darkPawnBoard);
        //int lightPawnCount = BitBoardHelper.BitCount(lightPawnBoard);

        //pawnColorCountDifference = darkPawnCount - lightPawnCount;


        int whiteEval = whiteMaterialValue + Positioning.GetPositioningScore(0, 1);// + EvaluatePawnStructure(0, 1);
        int blackEval = blackMaterialValue + Positioning.GetPositioningScore(1, 0);// + EvaluatePawnStructure(1, 0);

        int evaluation = whiteEval - blackEval;

        //White: = (white +25)-black
        //Final: = 1*white + 1*25 - 1*black = white - black + 25
        //Black: = white - (black + 25)
        //Final: = white - (-1*black + -1*25) = white - (-black-25) = white + black+25

        int perspective = Board.colorToMove == Piece.White ? 1 : -1;

        return (evaluation) * perspective + GetTempoBonus()*perspective;
    }

    public static int Blend(int early, int late, float endgameMultiplier)
    {
        return (int)Math.Round(early + (late - early) * endgameMultiplier);
    }

    public static int Blend(int early, int middle, int late, float gameStage)
    {
        return int.MaxValue;
    }



    private static int GetTempoBonus(/*int depth*/)
    {
        return Blend(TempoBonusBase, TempoBonusEndgame, endgameMultiplier);

        if (endgameMultiplier > 0f) return TempoBonusEndgame;

        return TempoBonusBase; //TODO: Reduce base on depth and endgame
    }

    private static int EvaluatePawnStructure(int colorBit, int enemyColorBit) //TODO: try giving high score in early to midgame when king can see pawns above him
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

        //Combined pawn eval
        // for (int i = 0; i < friendlyPawns.Count; i++)
        // {
        //     int square = friendlyPawns[i];
        //     int file = BoardHelper.IndexToFile(square);

        //     ulong opposingPawnBoard = PrecomputedData.passedPawnMasks[square + colorBit * 64] & enemyPawnBoard;
        //     int opposers = BitBoardHelper.BitCount(opposingPawnBoard); //If this is zero this is a passed pawn


        //     //TODO: do doubled pawn eval in own loop above bc more efficient - tried to do it here but end up counting doubled pawns twice
        //     //Actually can prob use the reversed passed pawn mask to also detect pawns only directly behind this one - no double counting and no additional loop needed

        //     if (opposers == 0) //If this is a passed pawn
        //     {
        //         //TODO: prob also beneficial to give every type of pawn a better score when they have supporters - punish isolated pawns
        //         ulong supportingPawnBoard = PrecomputedData.passedPawnMasks[square + enemyColorBit * 64] & (~PrecomputedData.fileMasks[file]) & friendlyPawnBoard; //Looks at pawns beside but not directly behind this one
        //         int supporters = BitBoardHelper.BitCount(supportingPawnBoard);

        //         score += (int)((PassedPawnValue + supporters * PassedPawnConnectionValue) * endgameMultiplier); //TODO: Think about maybe having an int version of endgamemultiplier bc were using it in a few spots and seems quite inefficient to keep casting/rounding - Compiler converts all the constants here to floats so the cast is really the only thing to impact performance (i imagine)
        //     }
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