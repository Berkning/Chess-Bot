
using System;

public static class Positioning //TODOne: endgame tables
{
    //Pawns
    private static int[] PawnEarlyGame = { 0, 0, 0, 0, 0, 0, 0, 0, 5, 15, 10, -10, -10, 10, 15, 5, 5, 0, 15, 15, 15, 15, 0, 5, -5, -10, 25, 30, 30, 25, -10, -5, -15, -15, 15, 20, 20, 15, -15, -15, -20, -15, -10, -5, -5, -10, -15, -20, -25, -25, -25, -25, -25, -25, -25, -25, 0, 0, 0, 0, 0, 0, 0, 0 };

    //TODO: think about maybe just using this array for passed pawns with a value multiplier - have to offset array values so no negatives
    private static int[] PawnLateGame = { 0, 0, 0, 0, 0, 0, 0, 0, -30, -30, -30, -30, -30, -30, -30, -30, -20, -20, -20, -20, -20, -20, -20, -20, -10, -10, -10, -10, -10, -10, -10, -10, 0, 0, 0, 0, 0, 0, 0, 0, 25, 25, 25, 25, 25, 25, 25, 25, 75, 75, 75, 75, 75, 75, 75, 75, 0, 0, 0, 0, 0, 0, 0, 0 };

    private static int[] PassedPawnLateGame = { 0, -30, -15, 0, 25, 50, 150 };

    //private const int PassedPawnMultiplier = 2;
    private const int PassedPawnConnectionValue = 10; //Could be weighted base on the rank as well - maybe


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

    //Bishops
    private static int[] BishopScores = { -10, -10, -10, -10, -10, -10, -10, -10, -10, 0, 0, 0, 0, 0, 0, -10, -10, 10, 10, 10, 10, 10, 10, -10, -10, 5, 10, 10, 10, 10, 5, -10, -10, 0, 5, 10, 10, 5, 0, -10, -10, 0, 5, 10, 10, 5, 0, -10, -10, 0, 0, 0, 0, 0, 0, -10, -10, -10, -10, -10, -10, -10, -10, -10 };

    //private const int OppositeColorPawnScore = 6; //Value increase of bishop for each pawn on an opposite colored square - bishop is more open
    private const int BishopPairValue = 25; //Extra value added for bishop pair



    private static int[] RookScores = { 0, 0, 0, 5, 5, 0, 0, 0, -5, 0, 0, 0, 0, 0, 0, -5, -5, 0, 0, 0, 0, 0, 0, -5, -5, 0, 0, 0, 0, 0, 0, -5, -5, 0, 0, 0, 0, 0, 0, -5, -5, 0, 0, 0, 0, 0, 0, -5, 5, 10, 10, 10, 10, 10, 10, 5, 0, 0, 0, 0, 0, 0, 0, 0 };

    private static int[] QueenScores = { -20, -10, -10, -5, -5, -10, -10, -20, -10, 0, 0, 0, 0, 0, 0, -10, -10, 0, 5, 5, 5, 5, 0, -10, -5, 0, 5, 5, 5, 5, 0, -5, -5, 0, 5, 5, 5, 5, 0, -5, -10, 0, 5, 5, 5, 5, 0, -10, -10, 0, 0, 0, 0, 0, 0, -10, -20, -10, -10, -5, -5, -10, -10, -20 };


    //King
    private static int[] KingEarlyGame = { 20, 30, 10, 0, 0, 10, 30, 20, 20, 20, 0, 0, 0, 0, 20, 20, -10, -20, -20, -20, -20, -20, -20, -10, -20, -30, -30, -40, -40, -30, -30, -20, -30, -40, -40, -50, -50, -40, -40, -30, -30, -40, -40, -50, -50, -40, -40, -30, -30, -40, -40, -50, -50, -40, -40, -30, -30, -40, -40, -50, -50, -40, -40, -30 };

    private static int[] KingEndgame = { -20, -10, -10, -10, -10, -10, -10, -20, -10, 5, 5, 5, 5, 5, 5, -10, -10, 5, 15, 15, 15, 15, 5, -10, -10, 5, 15, 15, 15, 15, 5, -10, -10, 5, 15, 15, 15, 15, 5, -10, -10, 5, 15, 15, 15, 15, 5, -10, -10, 5, 5, 5, 5, 5, 5, -10, -10, 0, 0, 0, 0, 0, 0, -10 };


    public static int GetPositioningScore(int colorBit, int enemyColorBit)
    {
        int score = 0;

        PieceList currentPieceList = Board.pawnList[colorBit]; //Isn't very readable, but slightly more performant?

        ulong friendlyPawnBoard = currentPieceList.bitboard;
        ulong enemyPawnBoard = Board.pawnList[enemyColorBit].bitboard;

        //Score pawn positions
        for (int i = 0; i < currentPieceList.Count; i++)
        {
            int square = currentPieceList[i];

            int index = colorBit == 0 ? square : BoardHelper.FlipIndex(square); //TODO: Check if performant to do if every single time; pretty easy to optimize prob

            int endgameValue = PawnLateGame[index];



            int file = BoardHelper.IndexToFile(square);

            ulong opposingPawnBoard = PrecomputedData.passedPawnMasks[square + colorBit * 64] & enemyPawnBoard;
            int opposers = BitBoardHelper.BitCount(opposingPawnBoard); //If this is zero this is a passed pawn

            //TODO: do doubled pawn eval in own loop above bc more efficient - tried to do it here but end up counting doubled pawns twice
            //Actually can prob use the reversed passed pawn mask to also detect pawns only directly behind this one - no double counting and no additional loop needed


            if (opposers == 0) //If this is a passed pawn
            {
                //TODO: prob also beneficial to give every type of pawn a better score when they have supporters - punish isolated pawns
                ulong supportingPawnBoard = PrecomputedData.passedPawnMasks[square + enemyColorBit * 64] & (~PrecomputedData.fileMasks[file]) & friendlyPawnBoard; //Looks at pawns beside but not directly behind this one
                int supporters = BitBoardHelper.BitCount(supportingPawnBoard);

                int rankFromSide = BoardHelper.IndexToRank(index);

                endgameValue = PassedPawnLateGame[rankFromSide] + supporters * PassedPawnConnectionValue;
            }

            score += Blend(PawnEarlyGame[index], endgameValue, Evaluation.endgameMultiplier);
        }

        //Score knight positions
        currentPieceList = Board.knightList[colorBit];

        for (int i = 0; i < currentPieceList.Count; i++)
        {
            int square = currentPieceList[i]; //TODO: test if having ref to piecelist is better than accesing piecelist array

            int index = colorBit == 0 ? square : BoardHelper.FlipIndex(square);
            score += KnightScores[index];
        }

        //Score bishop positions
        //TODO: Use this same stuff for scoring pawn structure prob

        //ulong darkBishopBoard = Board.bishopList[colorBit].bitboard & PrecomputedData.DarkSquareMask;
        //ulong lightBishopBoard = Board.bishopList[colorBit].bitboard & PrecomputedData.LightSquareMask;

        //if (darkBishopBoard != 0) score += Evaluation.lightPawnCount * OppositeColorPawnScore;

        //if (lightBishopBoard != 0) score += Evaluation.darkPawnCount * OppositeColorPawnScore;
        currentPieceList = Board.bishopList[colorBit];

        if (currentPieceList.Count > 1) score += BishopPairValue;

        for (int i = 0; i < currentPieceList.Count; i++)
        {
            int square = currentPieceList[i]; //TODO: test if having ref to piecelist is better than accesing piecelist array

            //bool isDark = BitBoardHelper.ContainsSquare(PrecomputedData.DarkSquareMask, square);

            //TODOne: try to subtract score with same colored pawns maybe - otherwise will raise value of all bishops
            //Doubt we need to taper score here in endgame bc there will be less pawns anyway, and if theres many it prob matters to score anyway
            //if (isDark) score += Evaluation.pawnColorCountDifference * OppositeColorPawnScore * -1; //Difference is flipped here bc want positive when more light pawns
            //else score += Evaluation.pawnColorCountDifference * OppositeColorPawnScore;

            int index = colorBit == 0 ? square : BoardHelper.FlipIndex(square);
            score += BishopScores[index];
        }

        //Score rook positions
        currentPieceList = Board.rookList[colorBit];

        for (int i = 0; i < currentPieceList.Count; i++)
        {
            int square = currentPieceList[i]; //TODO: test if having ref to piecelist is better than accesing piecelist array

            int index = colorBit == 0 ? square : BoardHelper.FlipIndex(square);
            score += RookScores[index];
        }

        //Score queen positions
        currentPieceList = Board.queenList[colorBit];

        for (int i = 0; i < currentPieceList.Count; i++)
        {
            int square = currentPieceList[i]; //TODO: test if having ref to piecelist is better than accesing piecelist array

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