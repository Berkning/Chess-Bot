using System.Collections.Generic;
using UnityEngine;

public static class MoveOrdering
{
    private static int[] moveScores = new int[218];

    public static List<Move> OrderMoves(List<Move> moves)
    {
        for (int i = 0; i < moves.Count; i++)
        {
            int moveScore = 0;
            int movedPieceType = Piece.Type(Board.Squares[moves[i].startSquare]);
            int capturedPieceType = Piece.Type(Board.Squares[moves[i].targetSquare]);

            int movedPieceValue = Evaluation.GetPieceTypeValue(movedPieceType);
            int flag = moves[i].flag;

            if (capturedPieceType != Piece.None)
            {
                moveScore += 10 * Evaluation.GetPieceTypeValue(capturedPieceType) - movedPieceValue;
            }

            if (movedPieceType == Piece.Pawn)
            {

                if (flag == Move.Flag.PromoteToQueen)
                {
                    moveScore += Evaluation.QueenValue;
                }
                else if (flag == Move.Flag.PromoteToKnight)
                {
                    moveScore += Evaluation.KnightValue;
                }
                else if (flag == Move.Flag.PromoteToRook)
                {
                    moveScore += Evaluation.RookValue;
                }
                else if (flag == Move.Flag.PromoteToBishop)
                {
                    moveScore += Evaluation.BishopValue;
                }
            }
            else
            {
                // Penalize moving piece to a square attacked by opponent pawn
                if (BitBoardHelper.ContainsSquare(MoveGenerator.oponnentPawnAttackMap, moves[i].targetSquare))
                {
                    moveScore -= 350;
                }
            }

            moveScores[i] = moveScore;
        }

        return SortMoves(moves);
    }

    private static List<Move> SortMoves(List<Move> moves)
    {
        for (int i = 0; i < moves.Count - 1; i++)
        {
            for (int j = i + 1; j > 0; j--)
            {
                int swapIndex = j - 1;
                if (moveScores[swapIndex] < moveScores[j])
                {
                    (moves[j], moves[swapIndex]) = (moves[swapIndex], moves[j]);
                    (moveScores[j], moveScores[swapIndex]) = (moveScores[swapIndex], moveScores[j]);
                }
            }
        }

        return moves;
    }
}