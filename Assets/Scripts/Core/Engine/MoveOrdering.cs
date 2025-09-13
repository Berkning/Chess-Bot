using System;
using System.Collections.Generic;

public static class MoveOrdering
{
    private static int[] moveScores = new int[218];

    const int prevBestBias = 2000000;

    public static void OrderMoves(ref Span<Move> moves, int moveCount, Move prevBestMove)
    {
        for (int i = 0; i < moveCount; i++) //TODO: Pretty sure we could just sort the moves in this loop by scoring the current move, and then checking if the previous move had a lower score, in which case we swap and check if the previous move after that also had a lower score and so on - should be faster?
        {
            int moveScore = 0;
            int movedPieceType = Piece.Type(Board.Squares[moves[i].startSquare]);
            int capturedPieceType = Piece.Type(Board.Squares[moves[i].targetSquare]);

            int movedPieceValue = Evaluation.GetPieceTypeValue(movedPieceType);
            int flag = moves[i].flag;

            //TODO: guess if opponent cant recapture

            if (moves[i].data == prevBestMove.data) moveScore += prevBestBias; //TODO: Could optimize checking through all moves to find this one prob

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

        SortMoves(ref moves, moveCount);
    }

    private static void SortMoves(ref Span<Move> moves, int moveCount)
    {
        for (int i = 0; i < moveCount - 1; i++)
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
    }
}