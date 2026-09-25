

public class EvasionGenerator
{
    private ulong friendlyPieces;
    private int friendlyKingSquare;
    private int enemyKingSquare;
    private Board board;

    public EvasionGenerator(Board _board)
    {
        board = _board;
    }


    private int moveCount = 0;

    public int Generate(ref Span<Move> moves, CheckInfo checkInfo)
    {
        InitializeVariables();

        if (checkInfo.IsSlidingCheck()) //If sliding piece check, we don't know if we're in double check yet
        {
            GenerateKingMoves(ref moves);

            if (HasSecondAttacker(checkInfo)) return moveCount; //Double check, so only king moves possible

            ulong attackerBoard;
            int attackerSquare = int.MaxValue;

            if (checkInfo.data == 1) //Queen check
            {
                PieceList queenList = board.GetPieceList(Piece.Queen, board.opponentColorBit);
                attackerBoard = PrecomputedData.queenAttackBitboards[friendlyKingSquare] & queenList.bitboard;

                if (BitBoardHelper.BitCount(attackerBoard) == 1) attackerSquare = BitBoardHelper.GetFirstBit(attackerBoard);
                else
                {
                    ulong board = attackerBoard;

                    while (board != 0)
                    {
                        attackerSquare = BitBoardHelper.PopFirstBit(ref board);

                        if (BitBoardHelper.ContainsSquare(queenList.attackMaps[queenList.indexMap[attackerSquare]], friendlyKingSquare)) break; //Found actual attacker
                    }
                }
            }
            else if (checkInfo.data == 2) //Rook check
            {
                PieceList rookList = board.GetPieceList(Piece.Rook, board.opponentColorBit);
                attackerBoard = PrecomputedData.rookAttackBitboards[friendlyKingSquare] & rookList.bitboard;

                if (BitBoardHelper.BitCount(attackerBoard) == 1) attackerSquare = BitBoardHelper.GetFirstBit(attackerBoard);
                else
                {
                    ulong board = attackerBoard;

                    while (board != 0)
                    {
                        attackerSquare = BitBoardHelper.PopFirstBit(ref board);

                        if (BitBoardHelper.ContainsSquare(rookList.attackMaps[rookList.indexMap[attackerSquare]], friendlyKingSquare)) break; //Found actual attacker
                    }
                }
            }
            else //Bishop check
            {
                PieceList bishopList = board.GetPieceList(Piece.Bishop, board.opponentColorBit);
                attackerBoard = PrecomputedData.bishopAttackBitboards[friendlyKingSquare] & bishopList.bitboard;

                if (BitBoardHelper.BitCount(attackerBoard) == 1) attackerSquare = BitBoardHelper.GetFirstBit(attackerBoard);
                else
                {
                    ulong board = attackerBoard;

                    while (board != 0)
                    {
                        attackerSquare = BitBoardHelper.PopFirstBit(ref board);

                        if (BitBoardHelper.ContainsSquare(bishopList.attackMaps[bishopList.indexMap[attackerSquare]], friendlyKingSquare)) break; //Found actual attacker
                    }
                }
            }


            ulong blockBoard = PrecomputedData.blockMasks[friendlyKingSquare][attackerSquare];

            GenerateBlocksAndCaptures(ref moves, blockBoard, attackerBoard, attackerSquare);
        }
        else if (checkInfo.IsKnightCheck()) //Knight check
        {
            GenerateKingMoves(ref moves);

            GenerateCaptures(ref moves, checkInfo.attackerSquare);
        }
        else //Pawn check
        {
            GenerateKingMoves(ref moves);

            GenerateCaptures(ref moves, checkInfo.attackerSquare);

            //En-passant capture to capture checking pawn
            int epFile = (int)((board.currentGameState & Board.epFileMask) >> 5) - 1;

            if (epFile != -1) //EP possible
            {
                int epAttackRank = board.friendlyColor == Piece.White ? 5 : 2;
                int epAttackSquare = epFile != -1 ? BoardHelper.CoordToIndex(epFile, epAttackRank) : -1;

                ulong pawnCaptureCandidates = PrecomputedData.pawnAttackBitboards[epAttackSquare + board.opponentColorBit * 64] & board.GetPieceList(Piece.Pawn, board.friendlyColorBit).bitboard;

                while (pawnCaptureCandidates != 0)
                {
                    int startSquare = BitBoardHelper.PopFirstBit(ref pawnCaptureCandidates);
                    moves[moveCount++] = new Move(startSquare, epAttackSquare, Move.Flag.EnPassantCapture);
                }
            }
        }

        return moveCount;
    }

    private void InitializeVariables()
    {
        moveCount = 0;

        friendlyPieces = board.GetPieceList(Piece.Pawn, board.friendlyColorBit).bitboard | board.GetPieceList(Piece.Knight, board.friendlyColorBit).bitboard | board.GetPieceList(Piece.Bishop, board.friendlyColorBit).bitboard | board.GetPieceList(Piece.Rook, board.friendlyColorBit).bitboard | board.GetPieceList(Piece.Queen, board.friendlyColorBit).bitboard;

        if (board.colorToMove == Piece.White)
        {
            friendlyKingSquare = board.whiteKingSquare;
            enemyKingSquare = board.blackKingSquare;
        }
        else
        {
            friendlyKingSquare = board.blackKingSquare;
            enemyKingSquare = board.whiteKingSquare;
        }
    }

    private void GenerateKingMoves(ref Span<Move> moves)
    {
        ulong moveBoard = PrecomputedData.kingAttackBitboards[friendlyKingSquare] & (~friendlyPieces);

        ulong opponentSlidingAttackMap = board.GetPieceList(Piece.Bishop, board.opponentColorBit).attackMap | board.GetPieceList(Piece.Rook, board.opponentColorBit).attackMap | board.GetPieceList(Piece.Queen, board.opponentColorBit).attackMap;

        moveBoard &= ~opponentSlidingAttackMap;
        moveBoard &= ~PrecomputedData.kingAttackBitboards[enemyKingSquare];

        ulong enemyKnights = board.GetPieceList(Piece.Knight, board.opponentColorBit).bitboard;
        ulong enemyPawns = board.GetPieceList(Piece.Pawn, board.opponentColorBit).bitboard;

        while (moveBoard != 0)
        {
            int targetSquare = BitBoardHelper.PopFirstBit(ref moveBoard);

            if ((PrecomputedData.knightAttackBitboards[targetSquare] & enemyKnights) != 0) continue;
            else if ((PrecomputedData.pawnAttackBitboards[targetSquare + board.friendlyColorBit * 64] & enemyPawns) != 0) continue;

            moves[moveCount++] = new Move(friendlyKingSquare, targetSquare);
        }
    }

    private bool HasSecondAttacker(CheckInfo checkInfo)
    {
        switch (checkInfo.data)
        {
            case 1: //Queen check so second attacker can be a rook, knight or bishop
                if ((board.GetPieceList(Piece.Rook, board.opponentColorBit).attackMap & 1UL << friendlyKingSquare) == 0 && (board.GetPieceList(Piece.Bishop, board.opponentColorBit).attackMap & 1UL << friendlyKingSquare) == 0 && (PrecomputedData.knightAttackBitboards[friendlyKingSquare] & board.GetPieceList(Piece.Knight, board.opponentColorBit).bitboard) == 0) return false;

                return true;
            case 2:  //Rook check so second attacker can only be a knight or bishop
                if ((board.GetPieceList(Piece.Bishop, board.opponentColorBit).attackMap & 1UL << friendlyKingSquare) == 0 && (PrecomputedData.knightAttackBitboards[friendlyKingSquare] & board.GetPieceList(Piece.Knight, board.opponentColorBit).bitboard) == 0) return false;

                return true;
            case 3: //Bishop check so second attacker can only be a knight
                if ((PrecomputedData.knightAttackBitboards[friendlyKingSquare] & board.GetPieceList(Piece.Knight, board.opponentColorBit).bitboard) == 0) return false;

                return true;
        }

        return false;
    }

    private void GenerateBlocksAndCaptures(ref Span<Move> moves, ulong blockBoard, ulong attackerBoard, int attackerSquare)
    {
        ulong combinedBoard = blockBoard | attackerBoard;

        PieceList queenList = board.GetPieceList(Piece.Queen, board.friendlyColorBit);

        //TODO: Try testing if pieceList.attackMap & targets == 0 - could easily skip it if true (which it will be most of the time i imagine)
        for (int i = 0; i < queenList.Count; i++)
        {
            int startSquare = queenList[i];
            ulong moveBoard = queenList.attackMaps[i] & combinedBoard; //Would normally do & ~friendlyPieces, but targets would never contain a friendly piece, so doesn't matter

            while (moveBoard != 0)
            {
                int targetSquare = BitBoardHelper.PopFirstBit(ref moveBoard);
                moves[moveCount++] = new Move(startSquare, targetSquare);
            }
        }

        PieceList rookList = board.GetPieceList(Piece.Rook, board.friendlyColorBit);

        //TODO: Try testing if pieceList.attackMap & targets == 0 - could easily skip it if true (which it will be most of the time i imagine)
        for (int i = 0; i < rookList.Count; i++)
        {
            int startSquare = rookList[i];
            ulong moveBoard = rookList.attackMaps[i] & combinedBoard; //Would normally do & ~friendlyPieces, but targets would never contain a friendly piece, so doesn't matter

            while (moveBoard != 0)
            {
                int targetSquare = BitBoardHelper.PopFirstBit(ref moveBoard);
                moves[moveCount++] = new Move(startSquare, targetSquare);
            }
        }

        PieceList bishopList = board.GetPieceList(Piece.Bishop, board.friendlyColorBit);

        //TODO: Try testing if pieceList.attackMap & targets == 0 - could easily skip it if true (which it will be most of the time i imagine)
        for (int i = 0; i < bishopList.Count; i++)
        {
            int startSquare = bishopList[i];
            ulong moveBoard = bishopList.attackMaps[i] & combinedBoard; //Would normally do & ~friendlyPieces, but targets would never contain a friendly piece, so doesn't matter

            while (moveBoard != 0)
            {
                int targetSquare = BitBoardHelper.PopFirstBit(ref moveBoard);
                moves[moveCount++] = new Move(startSquare, targetSquare);
            }
        }


        PieceList knightList = board.GetPieceList(Piece.Knight, board.friendlyColorBit);

        for (int i = 0; i < knightList.Count; i++)
        {
            int startSquare = knightList[i];
            ulong moveBoard = PrecomputedData.knightAttackBitboards[startSquare] & combinedBoard;

            while (moveBoard != 0)
            {
                int targetSquare = BitBoardHelper.PopFirstBit(ref moveBoard);
                moves[moveCount++] = new Move(startSquare, targetSquare);
            }
        }


        //TODO: Could exclude this completely if vertical check, bc pawns can't block
        PieceList pawnList = board.GetPieceList(Piece.Pawn, board.friendlyColorBit);
        int upDirection = board.friendlyColor == Piece.White ? PrecomputedData.Up : PrecomputedData.Down;
        int promotionRank = board.friendlyColor == Piece.White ? 6 : 1;
        int startRank = board.friendlyColor == Piece.White ? 1 : 6;

        for (int i = 0; i < pawnList.Count; i++)
        {
            int startSquare = pawnList[i];
            int rank = BoardHelper.IndexToRank(startSquare);
            int targetSquare = startSquare + upDirection;

            if (BitBoardHelper.ContainsSquare(blockBoard, targetSquare))
            {
                bool oneStepFromPromotion = rank == promotionRank;

                if (oneStepFromPromotion) AddPromotionMoves(ref moves, startSquare, targetSquare);
                else moves[moveCount++] = new Move(startSquare, targetSquare);
            }

            if (rank == startRank && board.Squares[targetSquare] == Piece.None)
            {
                targetSquare += upDirection;

                if (BitBoardHelper.ContainsSquare(blockBoard, targetSquare)) moves[moveCount++] = new Move(startSquare, targetSquare, Move.Flag.PawnTwoForward);
            }
        }

        ulong pawnCaptureCandidates = PrecomputedData.pawnAttackBitboards[attackerSquare + board.opponentColorBit * 64] & pawnList.bitboard;
        bool promotionCapture = BoardHelper.IndexToRank(attackerSquare) == board.opponentColorBit * 7;

        while (pawnCaptureCandidates != 0)
        {
            int startSquare = BitBoardHelper.PopFirstBit(ref pawnCaptureCandidates);
            if (promotionCapture) AddPromotionMoves(ref moves, startSquare, attackerSquare);
            else moves[moveCount++] = new Move(startSquare, attackerSquare);
        }
    }

    private void GenerateCaptures(ref Span<Move> moves, int target)
    {
        ulong targetBoard = 1UL << target;


        PieceList queenList = board.GetPieceList(Piece.Queen, board.friendlyColorBit);

        //TODO: Try testing if pieceList.attackMap & targets == 0 - could easily skip it if true (which it will be most of the time i imagine)
        for (int i = 0; i < queenList.Count; i++)
        {
            int startSquare = queenList[i];
            ulong moveBoard = queenList.attackMaps[i] & targetBoard; //Would normally do & ~friendlyPieces, but targets would never contain a friendly piece, so doesn't matter

            while (moveBoard != 0)
            {
                int targetSquare = BitBoardHelper.PopFirstBit(ref moveBoard);
                moves[moveCount++] = new Move(startSquare, targetSquare);
            }
        }

        PieceList rookList = board.GetPieceList(Piece.Rook, board.friendlyColorBit);

        //TODO: Try testing if pieceList.attackMap & targets == 0 - could easily skip it if true (which it will be most of the time i imagine)
        for (int i = 0; i < rookList.Count; i++)
        {
            int startSquare = rookList[i];
            ulong moveBoard = rookList.attackMaps[i] & targetBoard; //Would normally do & ~friendlyPieces, but targets would never contain a friendly piece, so doesn't matter

            while (moveBoard != 0)
            {
                int targetSquare = BitBoardHelper.PopFirstBit(ref moveBoard);
                moves[moveCount++] = new Move(startSquare, targetSquare);
            }
        }

        PieceList bishopList = board.GetPieceList(Piece.Bishop, board.friendlyColorBit);

        //TODO: Try testing if pieceList.attackMap & targets == 0 - could easily skip it if true (which it will be most of the time i imagine)
        for (int i = 0; i < bishopList.Count; i++)
        {
            int startSquare = bishopList[i];
            ulong moveBoard = bishopList.attackMaps[i] & targetBoard; //Would normally do & ~friendlyPieces, but targets would never contain a friendly piece, so doesn't matter

            while (moveBoard != 0)
            {
                int targetSquare = BitBoardHelper.PopFirstBit(ref moveBoard);
                moves[moveCount++] = new Move(startSquare, targetSquare);
            }
        }


        ulong knightCaptureCandidates = PrecomputedData.knightAttackBitboards[target] & board.GetPieceList(Piece.Knight, board.friendlyColorBit).bitboard;
        while (knightCaptureCandidates != 0)
        {
            int startSquare = BitBoardHelper.PopFirstBit(ref knightCaptureCandidates);
            moves[moveCount++] = new Move(startSquare, target);
        }


        ulong pawnCaptureCandidates = PrecomputedData.pawnAttackBitboards[target + board.opponentColorBit * 64] & board.GetPieceList(Piece.Pawn, board.friendlyColorBit).bitboard;
        bool promotionCapture = BoardHelper.IndexToRank(target) == board.opponentColorBit * 7;

        while (pawnCaptureCandidates != 0)
        {
            int startSquare = BitBoardHelper.PopFirstBit(ref pawnCaptureCandidates);
            if (promotionCapture) AddPromotionMoves(ref moves, startSquare, target);
            else moves[moveCount++] = new Move(startSquare, target);
        }
    }


    private void AddPromotionMoves(ref Span<Move> moves, int startSquare, int targetSquare)
    {
        moves[moveCount++] = new Move(startSquare, targetSquare, Move.Flag.PromoteToQueen);
        moves[moveCount++] = new Move(startSquare, targetSquare, Move.Flag.PromoteToKnight);

        if (MoveGenerator.promotionMode == MoveGenerator.PromotionMode.KnightAndQueen) return;

        moves[moveCount++] = new Move(startSquare, targetSquare, Move.Flag.PromoteToRook);
        moves[moveCount++] = new Move(startSquare, targetSquare, Move.Flag.PromoteToBishop);
    }
}