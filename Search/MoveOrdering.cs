using System;

public class MoveOrdering
{
    private int[] moveScores = new int[218];

    //const int jitterBias = -100000;
    const int kingAttackBias = -250;

    public const int MaxKillerPlys = 32;

    public /*static*/ KillerMove[] killerMoves = new KillerMove[MaxKillerPlys]; //TODO: test making atomic

    //Indexed by [sideToMove][from][to] //TODO: Try with [piece][to] - would make array a LOT smaller and maybe not have that much of a negative impact either
    //TODO: Could also just store ushort/short instead of int. Every entry just holds a value from 0->TunableConstants.MaxHistory, so 2 bytes are more than enough - also halves array size in memory
    public int[][][] history;

    public void UpdateHistory(int bonus, int colorBit, int from, int to)
    {
        int clampedBonus = Math.Clamp(bonus, 0, TunableConstants.MaxHistory);

        history[colorBit][from][to] += clampedBonus - history[colorBit][from][to] * clampedBonus / TunableConstants.MaxHistory;



        //history[colorBit][from][to] += score;
        //if (history[colorBit][from][to] > HistoryUpperBound) history[colorBit][from][to] = HistoryUpperBound;
    }



    //TODO: reset on new game
    public void DecayHistory() //TODO: Don't call this before search, wait till after it has returned - this could also be the cause of our time-losses in extreme STC
    {
        for (int i = 0; i < 64; i++)
        {
            for (int j = 0; j < 64; j++)
            {
                //Console.WriteLine("Before: " + history[0][i][j]);
                history[0][i][j] *= TunableConstants.HistoryDecay;
                history[0][i][j] /= 10000;
                //Console.WriteLine("After: " + history[0][i][j]);
                history[1][i][j] *= TunableConstants.HistoryDecay;
                history[1][i][j] /= 10000;
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

    //Returns false if hash move wasn't part of the current legal move set, otherwise true
    public bool OrderHashMove(ref Span<Move> moves, Move hashMove) //TODO: Do this Lazy-MoveOrdering type thing for movegen as well - always start by just generating the hash move - would also mean we dont have to do this inefficient loop to find it in the movelist
    {
        for (int i = 0; i < moves.Length; i++)
        {
            if (moves[i].data == hashMove.data)
            {
                moves[i] = moves[0];
                moves[0] = hashMove;
                return true;
            }
        }

        return false;
    }


    //skipFirstMove will be passed as true here, if OrderHashMove was already called on the movelist we are operating on,
    //meaning the move at index 0, will be the hash-move, which has already been looked at by the search, so we don't have to worry about it.
    public void OrderMoves(ref Span<Move> moves, int moveCount, int ply, bool skipFirstMove = false) //TODO: maybe prioritize checks in endgame - TODO: Optimize for q-search
    {
        int jitterIndex = moveCount != 0 ? threadID % moveCount : 0;

        for (int i = skipFirstMove ? 1 : 0; i < moveCount; i++) //TODOne: Pretty sure we could just sort the moves in this loop by scoring the current move, and then checking if the previous move had a lower score, in which case we swap and check if the previous move after that also had a lower score and so on - should be faster?
        {
            int moveScore = 0;
            int movedPieceType = Piece.Type(board.Squares[moves[i].startSquare]);
            int capturedPieceType = Piece.Type(board.Squares[moves[i].targetSquare]);

            int movedPieceValue = Evaluation.GetPieceTypeValue(movedPieceType);
            int flag = moves[i].flag;

            //if (i == jitterIndex) moveScore += jitterBias;

            if (capturedPieceType != Piece.None)
            {
                //moveScore += 10 * Evaluation.GetPieceTypeValue(capturedPieceType) - movedPieceValue;

                int valueDelta = Evaluation.GetPieceTypeValue(capturedPieceType) - movedPieceValue;

                bool canRecaptureGuess = BitBoardHelper.ContainsSquare(moveGenerator.opponentAttackMap, moves[i].targetSquare);
                if (canRecaptureGuess)
                {
                    if (valueDelta == 0) moveScore += TunableConstants.EqualCaptureBias;
                    else moveScore += valueDelta > 0 ? TunableConstants.GoodCaptureBias : TunableConstants.BadCaptureBias;
                }
                else
                {
                    moveScore += TunableConstants.GoodCaptureBias + valueDelta * TunableConstants.CaptureValueDeltaMultiplier;
                }
            }
            else if (flag != Move.Flag.EnPassantCapture) //If not a capture
            {
                if (ply < MaxKillerPlys && killerMoves[ply].Contains(moves[i])) moveScore += TunableConstants.KillerBias;
                else moveScore += history[board.friendlyColorBit][moves[i].startSquare][moves[i].targetSquare];


                //TODO: try with else if
                //if (movedPieceValue >= Evaluation.RookValue)
                //{
                //if (BitBoardHelper.ContainsSquare(MoveGenerator.opponentKnightAttackMap, moves[i].targetSquare)) moveScore -= 150; //TODO: Tweak value and test //Cant do with bishops and rooks bc their attack boards are combined with each other - and the queen
                //}
            }

            if (movedPieceType == Piece.Pawn)
            {
                if (flag == Move.Flag.PromoteToQueen) //TODO: Maybe account for king attack squares here
                {
                    moveScore += Evaluation.GetPieceTypeValue(Piece.Queen) * TunableConstants.PromotionMultiplier;
                }
                else if (flag == Move.Flag.PromoteToKnight)
                {
                    moveScore += Evaluation.GetPieceTypeValue(Piece.Knight) * TunableConstants.PromotionMultiplier;
                }
                else if (flag == Move.Flag.PromoteToRook)
                {
                    moveScore += Evaluation.GetPieceTypeValue(Piece.Rook) * TunableConstants.PromotionMultiplier;
                }
                else if (flag == Move.Flag.PromoteToBishop)
                {
                    moveScore += Evaluation.GetPieceTypeValue(Piece.Bishop) * TunableConstants.PromotionMultiplier;
                }
            }
            else
            {
                //TODOnt: This is accounted for already (as far as i can tell) - Account for the move being a capture? Mb this is done already with recapture guess? Doesn't account for it being an equal trade tho?

                // Penalize moving piece to a square attacked by enemy pawn
                if (BitBoardHelper.ContainsSquare(moveGenerator.oponnentPawnAttackMap, moves[i].targetSquare))
                {
                    moveScore += TunableConstants.AttackedByPawnBias;
                }
                else if (BitBoardHelper.ContainsSquare(moveGenerator.opponentKnightAttackMap, moves[i].targetSquare))// Penalize moving piece to a square attacked by enemy knight
                {
                    if (movedPieceValue >= TunableConstants.AttackedByKnightMinPieceValue) moveScore += TunableConstants.AttackedByKnightBias;
                }

                //TODO: Defense bonuses - if we move a piece to a square where it is protected by one of our pawns that probably deserves a bonus. Could do the same for knights and maybe king as well (although this might turn out to be a penalty instead, but still prob good heuristic)

                //Give bonus for moving a piece to a square, where it is defended by a friendly pawn
                if ((PrecomputedData.pawnAttackBitboards[moves[i].targetSquare + board.opponentColorBit * 64] & board.GetPieceList(Piece.Pawn, board.friendlyColorBit).bitboard) != 0) //TODO: try applying a bonus like this when moving pawns as well
                {
                    moveScore += TunableConstants.DefendedByPawnBias;
                }
            }

            moveScores[i] = moveScore;

            SwapSortMove(ref moves, i, moveScore, skipFirstMove);
        }

        //SortMoves(ref moves, moveCount);
    }

    //TODO: Agressive inlining
    private void SwapSortMove(ref Span<Move> moves, int i, int score, bool skipFirstMove) //TODO: Try other sorting algo
    {
        int cap = skipFirstMove ? 1 : 0;

        if (i == cap) return;

        int j = i - 1;
        Move move = moves[i];

        while (moveScores[j] < score)
        {
            //Swap ScoresTunableConstants.CaptureValueDeltaMultiplier
            moveScores[i] = moveScores[j];
            moveScores[j] = score;
            //Swap Moves
            moves[i] = moves[j];
            moves[j] = move;


            i--;
            if (i == cap) return;

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


    public struct KillerMove
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
