using System;

public class MoveOrdering
{
    private int[] moveScores = new int[218]; //TODOcant: Change to span? Should be way faster in sort especially i think

    const int prevBestBias = 2000000;
    const int killerBias = 500000; //TODO: try making smaller than goodCaptureBias
    const int goodCaptureBias = 8000;
    const int badCaptureBias = 1100;
    //const int jitterBias = -100000;
    const int kingAttackBias = -250;

    public const int MaxKillerPlys = 32;

    public /*static*/ KillerMove[] killerMoves = new KillerMove[MaxKillerPlys]; //TODO: test making atomic

    //Indexed by [sideToMove][from][to] //TODO: Try with [piece][to] - would make array a LOT smaller and maybe not have that much of a negative impact either

    private const int MaxHistory = 1024; //800 seems to be exactly the same/extremely slightly better than 1024
    public int[][][] history;

    public void UpdateHistory(int bonus, int colorBit, int from, int to)
    {
        int clampedBonus = Math.Clamp(bonus, 0, MaxHistory);

        history[colorBit][from][to] += clampedBonus - history[colorBit][from][to] * clampedBonus / MaxHistory;



        //history[colorBit][from][to] += score;
        //if (history[colorBit][from][to] > HistoryUpperBound) history[colorBit][from][to] = HistoryUpperBound;
    }

    //TODO: reset on new game
    public void DecayHistory()
    {
        for (int i = 0; i < 64; i++)
        {
            for (int j = 0; j < 64; j++)
            {
                //Console.WriteLine("Before: " + history[0][i][j]);
                history[0][i][j] *= 8;
                history[0][i][j] /= 10;
                //Console.WriteLine("After: " + history[0][i][j]);
                history[1][i][j] *= 8;
                history[1][i][j] /= 10;
            }
        }
    }


    private Board board;
    private MoveGenerator moveGenerator;
    private int threadID;

    public MoveOrdering(Board _board, MoveGenerator _moveGenerator, int _threadID)
    {
        board = _board;
        moveGenerator = _moveGenerator;
        threadID = _threadID;

        history = new int[2][][] { new int[64][], new int[64][] };

        for (int i = 0; i < 64; i++)
        {
            history[0][i] = new int[64];
            history[1][i] = new int[64];
        }
    }


    //TODOne: try penalizing moving very valuable pieces into less valuable enemy attack range - penalize rook in bishop attack range
    public void OrderMoves(ref Span<Move> moves, int moveCount, Move prevBestMove, int ply) //TODO: maybe prioritize checks in endgame - TODO: Optimize for q-search
    {
        int jitterIndex = moveCount != 0 ? threadID % moveCount : 0;

        for (int i = 0; i < moveCount; i++) //TODOne: Pretty sure we could just sort the moves in this loop by scoring the current move, and then checking if the previous move had a lower score, in which case we swap and check if the previous move after that also had a lower score and so on - should be faster?
        {
            int moveScore = 0;
            int movedPieceType = Piece.Type(board.Squares[moves[i].startSquare]);
            int capturedPieceType = Piece.Type(board.Squares[moves[i].targetSquare]);

            int movedPieceValue = Evaluation.GetPieceTypeValue(movedPieceType);
            int flag = moves[i].flag; //TODO: try having ref to current move even though prob done by compiler anyway

            //TODOne: guess if opponent cant recapture //TODOne: penalize rook and queen movements in early game?

            if (moves[i].data == prevBestMove.data) moveScore += prevBestBias; //TODO: Could optimize checking through all moves to find this one prob

            //if (i == jitterIndex) moveScore += jitterBias;

            if (capturedPieceType != Piece.None)
            {
                //moveScore += 10 * Evaluation.GetPieceTypeValue(capturedPieceType) - movedPieceValue;
                int valueDelta = Evaluation.GetPieceTypeValue(capturedPieceType) - movedPieceValue;

                bool canRecaptureGuess = BitBoardHelper.ContainsSquare(moveGenerator.opponentAttackMap, moves[i].targetSquare);
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
                else moveScore += history[board.friendlyColorBit][moves[i].startSquare][moves[i].targetSquare];


                //TODO: try with else if
                //if (movedPieceValue >= Evaluation.RookValue)
                //{
                //if (BitBoardHelper.ContainsSquare(MoveGenerator.opponentKnightAttackMap, moves[i].targetSquare)) moveScore -= 150; //TODO: Tweak value and test //Cant do with bishops and rooks bc their attack boards are combined with each other - and the queen
                //}

                //if (movedPieceType == Piece.Rook) moveScore -= (int)(100f * Evaluation.earlygameMultiplier); //TODO: experiment with moving this into different if statements - also try with else if
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
                if (BitBoardHelper.ContainsSquare(moveGenerator.oponnentPawnAttackMap, moves[i].targetSquare))
                {
                    moveScore -= 350;
                }
                //else if (movedPieceType == Piece.Rook) moveScore -= (int)(100f * Evaluation.earlygameMultiplier); //Penalize moving rook in early game
            }

            moveScores[i] = moveScore;

            SwapSortMove(ref moves, i, moveScore);
        }

        //SortMoves(ref moves, moveCount);
    }

    //TODO: Agressive inlining
    private void SwapSortMove(ref Span<Move> moves, int i, int score) //TODO: Try other sorting algo
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

    public void ThreadRootShuffle(ref Span<Move> moves, int moveCount, int threadShuffle)
    {
        int shuffleIndex = threadShuffle % moveCount;

        //Swap move with first move
        Move shuffledMove = moves[shuffleIndex];
        moves[shuffleIndex] = moves[0];
        moves[0] = shuffledMove;

        shuffleIndex = (threadShuffle * 2) % moveCount;

        shuffledMove = moves[shuffleIndex];
        moves[shuffleIndex] = moves[1];
        moves[1] = shuffledMove;
    }



    private void SortMoves(ref Span<Move> moves, int moveCount)
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


    public struct KillerMove //TODO: if only using one killer per ply, try not using struct and these add and contains methods, just pure array - don't see why this would make a difference
    {
        public Move moveA; //TODOne: test adding more than 1 per ply - worse apparently
        //public Move moveB;

        public void Add(Move move)
        {
            moveA = move;
            // if (move.data != moveA.data)
            // {
            //     moveB = moveA;
            //     moveA = move;
            // }
        }

        public bool Contains(Move move)
        {
            return move.data == moveA.data;
            //return moveA.data == move.data || moveB.data == move.data;
        }
    }
}
