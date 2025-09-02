using System;
using UnityEngine;

public static class Positioning
{
    private static int[] PawnScores = { -10, -10, -10, -10, -10, -10, -10, -10, -5, -5, -5, -5, -5, -5, -5, -5, 5, 10, 15, 15, 15, 15, 10, 5, 5, 10, 10, 30, 30, -999, 10, 5, 5, 10, 10, 10, 10, 10, 10, 5, 20, 20, 20, 20, 20, 20, 20, 20, 30, 30, 30, 30, 30, 30, 30, 30, -10, -10, -10, -10, -10, -10, -10, -10 };
    private static int[] KnightScores = {
-50,-40,-30,-30,-30,-30,-40,-50,
-40,-20,  0,  0,  0,  0,-20,-40,
-30,  0, 10, 15, 15, 10,  0,-30,
-30,  5, 15, 20, 20, 15,  5,-30,
-30,  0, 15, 20, 20, 15,  0,-30,
-30,  5, 10, 15, 15, 10,  5,-30,
-40,-20,  0,  5,  5,  0,-20,-40,
-50,-40,-30,-30,-30,-30,-40,-50};

    private static int[] BishopScores = { -10, -10, -10, -10, -10, -10, -10, -10, -10, 0, 0, 0, 0, 0, 0, -10, -10, 10, 10, 10, 10, 10, 10, -10, -10, 5, 10, 10, 10, 10, 5, -10, -10, 0, 5, 10, 10, 5, 0, -10, -10, 0, 5, 10, 10, 5, 0, -10, -10, 0, 0, 0, 0, 0, 0, -10, -10, -10, -10, -10, -10, -10, -10, -10 };

    private static int[] RookScores = { 0, 0, 0, 5, 5, 0, 0, 0, -5, 0, 0, 0, 0, 0, 0, -5, -5, 0, 0, 0, 0, 0, 0, -5, -5, 0, 0, 0, 0, 0, 0, -5, -5, 0, 0, 0, 0, 0, 0, -5, -5, 0, 0, 0, 0, 0, 0, -5, 5, 10, 10, 10, 10, 10, 10, 5, 0, 0, 0, 0, 0, 0, 0, 0 };

    private static int[] QueenScores = { -20, -10, -10, -5, -5, -10, -10, -20, -10, 0, 0, 0, 0, 0, 0, -10, -10, 0, 5, 5, 5, 5, 0, -10, -5, 0, 5, 5, 5, 5, 0, -5, -5, 0, 5, 5, 5, 5, 0, -5, -10, 0, 5, 5, 5, 5, 0, -10, -10, 0, 0, 0, 0, 0, 0, -10, -20, -10, -10, -5, -5, -10, -10, -20 };

    private static int[] KingScores = { 20, 30, 10, 0, 0, 10, 30, 20, 20, 20, 0, 0, 0, 0, 20, 20, -10, -20, -20, -20, -20, -20, -20, -10, -20, -30, -30, -40, -40, -30, -30, -20, -30, -40, -40, -50, -50, -40, -40, -30, -30, -40, -40, -50, -50, -40, -40, -30, -30, -40, -40, -50, -50, -40, -40, -30, -30, -40, -40, -50, -50, -40, -40, -30 };



    public static int GetPositioningScore(int colorBit)
    {
        int score = 0;

        //Score pawn positions
        for (int i = 0; i < Board.pawnList[colorBit].Count; i++)
        {
            int square = Board.pawnList[colorBit][i];

            int index = colorBit == 0 ? square : BoardHelper.FlipIndex(square); //TODO: Check if performant to do if every single time; pretty easy to optimize prob
            score += PawnScores[index];
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
        int kingIndex = colorBit == 0 ? Board.whiteKingSquare : BoardHelper.FlipIndex(Board.blackKingSquare);
        score += KingScores[kingIndex];


        //Mopup
        int enemyKingSquare = colorBit == 0 ? Board.blackKingSquare : Board.whiteKingSquare;

        score += Mathf.CeilToInt(10f * PrecomputedData.manhattanDistanceFromCenter[enemyKingSquare] * Mathf.Max(Evaluation.gameStage - 1f, 0f));

        //TODO: Move king closer to enemy king in endgame as well

        return score;
    }
}