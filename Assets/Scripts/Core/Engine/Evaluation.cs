using UnityEngine;

public static class Evaluation
{
    public const int PawnValue = 100;
    public const int KnightValue = 310;
    public const int BishopValue = 330;
    public const int RookValue = 500;
    public const int QueenValue = 900;

    public static int Evaluate()
    {

        int whiteEval = EvaluateMaterial(0) + Positioning.GetPositioningScore(0);
        int blackEval = EvaluateMaterial(1) + Positioning.GetPositioningScore(1);

        int evaluation = whiteEval - blackEval;

        int perspective = Board.colorToMove == Piece.White ? 1 : -1;

        return evaluation * perspective;
    }


    private static int EvaluateMaterial(int colorBit)
    {
        int score = 0;
        score += Board.pawnList[colorBit].Count * PawnValue;
        score += Board.knightList[colorBit].Count * KnightValue;
        score += Board.bishopList[colorBit].Count * BishopValue;
        score += Board.rookList[colorBit].Count * RookValue;
        score += Board.queenList[colorBit].Count * QueenValue;
        return score;
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

        Debug.LogError("Invalid Piece Type: " + type);
        return -1234567;
    }
}