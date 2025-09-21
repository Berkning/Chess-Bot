using System;
using System.Collections.Generic;

public static class MoveOrdering
{
    private static int[] moveScores = new int[218];

    const int prevBestBias = 2000000;
    const int killerBias = 500000;
    const int goodCaptureBias = 8000;
    const int badCaptureBias = 1100;
    const int kingAttackBias = -250;

    public const int MaxKillerPlys = 32;

    public static KillerMove[] killerMoves = new KillerMove[MaxKillerPlys];



    public static void OrderMoves(ref Span<Move> moves, int moveCount, Move prevBestMove, int ply) //TODO: maybe prioritize checks in endgame - TODO: Optimize for q-search
    {
        for (int i = 0; i < moveCount; i++) //TODO: Pretty sure we could just sort the moves in this loop by scoring the current move, and then checking if the previous move had a lower score, in which case we swap and check if the previous move after that also had a lower score and so on - should be faster?
        {
            int moveScore = 0;
            int movedPieceType = Piece.Type(Board.Squares[moves[i].startSquare]);
            int capturedPieceType = Piece.Type(Board.Squares[moves[i].targetSquare]);

            int movedPieceValue = Evaluation.GetPieceTypeValue(movedPieceType);
            int flag = moves[i].flag; //TODO: try having ref to current move even though prob done by compiler anyway

            //TODOne: guess if opponent cant recapture

            if (moves[i].data == prevBestMove.data) moveScore += prevBestBias; //TODO: Could optimize checking through all moves to find this one prob

            if (capturedPieceType != Piece.None)
            {
                //moveScore += 10 * Evaluation.GetPieceTypeValue(capturedPieceType) - movedPieceValue;
                int valueDelta = Evaluation.GetPieceTypeValue(capturedPieceType) - movedPieceValue;

                bool canRecaptureGuess = BitBoardHelper.ContainsSquare(MoveGenerator.opponentAttackMap, moves[i].targetSquare);
                if (canRecaptureGuess)
                {
                    moveScore += valueDelta >= 0 ? goodCaptureBias : badCaptureBias;
                }
                else
                {
                    moveScore += goodCaptureBias + valueDelta;
                }
            }
            else if (moves[i].flag != Move.Flag.EnPassantCapture) //If not a capture
            {
                // if (BitBoardHelper.ContainsSquare(MoveGenerator.opponentKingAttackMap, moves[i].targetSquare))
                // {
                //     moveScore += kingAttackBias;
                // }

                if (ply < MaxKillerPlys && killerMoves[ply].Contains(moves[i])) moveScore += killerBias;
            }

            if (movedPieceType == Piece.Pawn)
            {

                if (flag == Move.Flag.PromoteToQueen) //TODO: Maybe account for king attack squares here
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

            SwapSortMove(ref moves, i, moveScore);
        }

        //SortMoves(ref moves, moveCount);
    }

    private static void SwapSortMove(ref Span<Move> moves, int i, int score)
    {
        if (i == 0) return;

        int j = i - 1;
        Move move = moves[i];

        while (moveScores[j] < score)
        {
            //Swap Scores
            moveScores[i] = moveScores[j];
            moveScores[j] = score;
            //Swap Moves
            moves[i] = moves[j];
            moves[j] = move;


            i--;
            if (i == 0) return;

            j--;
        }
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


    public struct KillerMove
    {
        public Move moveA; //TODO: test adding more than 1 per ply

        public void Add(Move move)
        {
            moveA = move;
        }

        public bool Contains(Move move)
        {
            return moveA.data == move.data;
        }
    }
}
