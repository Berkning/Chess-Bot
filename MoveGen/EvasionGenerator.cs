

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

        ulong opponentSlidingAttackMap = board.GetPieceList(Piece.Bishop, board.opponentColorBit).bitboard | board.GetPieceList(Piece.Rook, board.opponentColorBit).bitboard | board.GetPieceList(Piece.Queen, board.opponentColorBit).bitboard;

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

    private void GenerateBlocks(ref Span<Move> moves, ulong targets) //DO NOT pass bitboard with enemy pieces highlighted
    {
        PieceList queenList = board.GetPieceList(Piece.Queen, board.friendlyColorBit);

        //TODO: Try testing if pieceList.attackMap & targets == 0 - could easily skip it if true (which it will be most of the time i imagine)
        for (int i = 0; i < queenList.Count; i++)
        {
            int startSquare = queenList[i];
            ulong moveBoard = queenList.attackMaps[i] & targets; //Would normally do & ~friendlyPieces, but targets would never contain a friendly piece, so doesn't matter

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
            ulong moveBoard = rookList.attackMaps[i] & targets; //Would normally do & ~friendlyPieces, but targets would never contain a friendly piece, so doesn't matter

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
            ulong moveBoard = bishopList.attackMaps[i] & targets; //Would normally do & ~friendlyPieces, but targets would never contain a friendly piece, so doesn't matter

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
            ulong moveBoard = PrecomputedData.knightAttackBitboards[startSquare] & targets;

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

            if (BitBoardHelper.ContainsSquare(targets, targetSquare))
            {
                bool oneStepFromPromotion = rank == promotionRank;

                if (oneStepFromPromotion) AddPromotionMoves(ref moves, startSquare, targetSquare);
                else moves[moveCount++] = new Move(startSquare, targetSquare);
            }

            if (rank == startRank)
            {
                targetSquare += upDirection;

                if (BitBoardHelper.ContainsSquare(targets, targetSquare)) moves[moveCount++] = new Move(startSquare, targetSquare);
            }
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
        while (pawnCaptureCandidates != 0)
        {
            int startSquare = BitBoardHelper.PopFirstBit(ref pawnCaptureCandidates);
            moves[moveCount++] = new Move(startSquare, target);
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