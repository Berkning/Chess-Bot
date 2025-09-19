
using System;

public static class Positioning //TODOne: endgame tables
{
    //Pawns
    private static int[] PawnEarlyGame = { 0, 0, 0, 0, 0, 0, 0, 0, 5, 15, 10, -10, -10, 10, 15, 5, 5, 0, 15, 15, 15, 15, 0, 5, -5, -10, 25, 30, 30, 25, -10, -5, -15, -15, 15, 20, 20, 15, -15, -15, -20, -15, -10, -5, -5, -10, -15, -20, -25, -25, -25, -25, -25, -25, -25, -25, 0, 0, 0, 0, 0, 0, 0, 0 };

    private static int[] PawnLateGame = { 0, 0, 0, 0, 0, 0, 0, 0, -30, -30, -30, -30, -30, -30, -30, -30, -20, -20, -20, -20, -20, -20, -20, -20, -10, -10, -10, -10, -10, -10, -10, -10, 0, 0, 0, 0, 0, 0, 0, 0, 25, 25, 25, 25, 25, 25, 25, 25, 75, 75, 75, 75, 75, 75, 75, 75, 0, 0, 0, 0, 0, 0, 0, 0 };


    private static int[] KnightScores = {
    -50,-40,-30,-30,-30,-30,-40,-50,
    -40,-20,  0,  0,  0,  0,-20,-40,
    -30,  0, 10, 15, 15, 10,  0,-30,
    -30,  5, 15, 20, 20, 15,  5,-30,
    -30,  0, 15, 20, 20, 15,  0,-30,
    -30,  5, 10, 15, 15, 10,  5,-30,
    -40,-20,  0,  5,  5,  0,-20,-40,
    -50,-40,-30,-30,-30,-30,-40,-50
};

    private static int[] BishopScores = { -10, -10, -10, -10, -10, -10, -10, -10, -10, 0, 0, 0, 0, 0, 0, -10, -10, 10, 10, 10, 10, 10, 10, -10, -10, 5, 10, 10, 10, 10, 5, -10, -10, 0, 5, 10, 10, 5, 0, -10, -10, 0, 5, 10, 10, 5, 0, -10, -10, 0, 0, 0, 0, 0, 0, -10, -10, -10, -10, -10, -10, -10, -10, -10 };

    private static int[] RookScores = { 0, 0, 0, 5, 5, 0, 0, 0, -5, 0, 0, 0, 0, 0, 0, -5, -5, 0, 0, 0, 0, 0, 0, -5, -5, 0, 0, 0, 0, 0, 0, -5, -5, 0, 0, 0, 0, 0, 0, -5, -5, 0, 0, 0, 0, 0, 0, -5, 5, 10, 10, 10, 10, 10, 10, 5, 0, 0, 0, 0, 0, 0, 0, 0 };

    private static int[] QueenScores = { -20, -10, -10, -5, -5, -10, -10, -20, -10, 0, 0, 0, 0, 0, 0, -10, -10, 0, 5, 5, 5, 5, 0, -10, -5, 0, 5, 5, 5, 5, 0, -5, -5, 0, 5, 5, 5, 5, 0, -5, -10, 0, 5, 5, 5, 5, 0, -10, -10, 0, 0, 0, 0, 0, 0, -10, -20, -10, -10, -5, -5, -10, -10, -20 };


    //King
    private static int[] KingEarlyGame = { 20, 30, 10, 0, 0, 10, 30, 20, 20, 20, 0, 0, 0, 0, 20, 20, -10, -20, -20, -20, -20, -20, -20, -10, -20, -30, -30, -40, -40, -30, -30, -20, -30, -40, -40, -50, -50, -40, -40, -30, -30, -40, -40, -50, -50, -40, -40, -30, -30, -40, -40, -50, -50, -40, -40, -30, -30, -40, -40, -50, -50, -40, -40, -30 };

    private static int[] KingEndgame = { -20, -10, -10, -10, -10, -10, -10, -20, -10, 5, 5, 5, 5, 5, 5, -10, -10, 5, 15, 15, 15, 15, 5, -10, -10, 5, 15, 15, 15, 15, 5, -10, -10, 5, 15, 15, 15, 15, 5, -10, -10, 5, 15, 15, 15, 15, 5, -10, -10, 5, 5, 5, 5, 5, 5, -10, -10, 0, 0, 0, 0, 0, 0, -10 };


    public static int GetPositioningScore(int colorBit)
    {
        int score = 0;

        //Score pawn positions
        for (int i = 0; i < Board.pawnList[colorBit].Count; i++)
        {
            int square = Board.pawnList[colorBit][i];

            int index = colorBit == 0 ? square : BoardHelper.FlipIndex(square); //TODO: Check if performant to do if every single time; pretty easy to optimize prob
            score += Blend(PawnEarlyGame[index], PawnLateGame[index], Evaluation.endgameMultiplier);
        }

        //Score knight positions
        for (int i = 0; i < Board.knightList[colorBit].Count; i++)
        {
            int square = Board.knightList[colorBit][i];

            int index = colorBit == 0 ? square : BoardHelper.FlipIndex(square);
            score += KnightScores[index];
        }

        //Score bishop positions
        for (int i = 0; i < Board.bishopList[colorBit].Count; i++)
        {
            int square = Board.bishopList[colorBit][i];

            int index = colorBit == 0 ? square : BoardHelper.FlipIndex(square);
            score += BishopScores[index];
        }

        //Score rook positions
        for (int i = 0; i < Board.rookList[colorBit].Count; i++)
        {
            int square = Board.rookList[colorBit][i];

            int index = colorBit == 0 ? square : BoardHelper.FlipIndex(square);
            score += RookScores[index];
        }

        //Score queen positions
        for (int i = 0; i < Board.queenList[colorBit].Count; i++)
        {
            int square = Board.queenList[colorBit][i];

            int index = colorBit == 0 ? square : BoardHelper.FlipIndex(square);
            score += QueenScores[index];
        }

        //Score king position
        int kingSquare = colorBit == 0 ? Board.whiteKingSquare : BoardHelper.FlipIndex(Board.blackKingSquare);
        score += Blend(KingEarlyGame[kingSquare], KingEndgame[kingSquare], Evaluation.endgameMultiplier);


        //Mopup
        int enemyKingSquare = colorBit == 0 ? Board.blackKingSquare : Board.whiteKingSquare;

        score += (int)Math.Ceiling(10f * PrecomputedData.manhattanDistanceFromCenter[enemyKingSquare] * Evaluation.endgameMultiplier);

        //TODO: Move king closer to enemy king in endgame as well
        //score += Mathf.CeilToInt(10f * (7f - PrecomputedData.kingDistanceLookup[kingSquare][enemyKingSquare]) * endgameMultiplier);

        return score;
    }


    private static int Blend(int early, int late, float endgameMultiplier)
    {
        return (int)Math.Round(early + (late - early) * endgameMultiplier);
    }

    private static int Blend(int early, int middle, int late, float gameStage)
    {
        return int.MaxValue;
    }
}