

public class MoveGenerator
{
    public enum PromotionMode { All, KnightAndQueen };
    public static PromotionMode promotionMode = PromotionMode.KnightAndQueen;

    private ulong allPieces;

    private ulong friendlyPieces;
    private ulong friendlyOrthos;
    private ulong friendlyDiags;

    private ulong enemyPieces;
    private ulong enemyOrthos;
    private ulong enemyDiags;

    private int friendlyKingSquare;
    private int enemyKingSquare;
    private int friendlyIndexOffset;




    private Board board;

    public MoveGenerator(Board _board)
    {
        board = _board;
    }

    private void GeneratePieceBoards() //TODOne: would be more performant to keep track of these and update them in board on make an unmake move
    {
        //TODO: Don't use GetPieceList because we can just access the piecelist we want directly - should just give a tiny free speedup

        int friendlyBit = board.friendlyColorBit;
        int enemyBit = board.opponentColorBit;

        // ulong friendlyQueens = board.queenList[friendlyBit].bitboard;
        // ulong enemyQueens = board.queenList[enemyBit].bitboard;


        // friendlyOrthos = board.rookList[friendlyBit].bitboard | friendlyQueens;
        // friendlyDiags = board.bishopList[friendlyBit].bitboard | friendlyQueens;
        // enemyOrthos = board.rookList[enemyBit].bitboard | enemyQueens;
        // enemyDiags = board.bishopList[enemyBit].bitboard | enemyQueens;

        // friendlyPieces = board.pawnList[friendlyBit].bitboard | board.knightList[friendlyBit].bitboard | friendlyDiags | friendlyOrthos | (1UL << friendlyKingSquare);
        // enemyPieces = board.pawnList[enemyBit].bitboard | board.knightList[enemyBit].bitboard | enemyDiags | enemyOrthos | (1UL << enemyKingSquare);

        // allPieces = friendlyPieces | enemyPieces;



        ulong friendlyQueens = board.GetPieceList(Piece.Queen, friendlyBit).bitboard;
        ulong enemyQueens = board.GetPieceList(Piece.Queen, enemyBit).bitboard;


        friendlyOrthos = board.GetPieceList(Piece.Rook, friendlyBit).bitboard | friendlyQueens;
        friendlyDiags = board.GetPieceList(Piece.Bishop, friendlyBit).bitboard | friendlyQueens;
        enemyOrthos = board.GetPieceList(Piece.Rook, enemyBit).bitboard | enemyQueens;
        enemyDiags = board.GetPieceList(Piece.Bishop, enemyBit).bitboard | enemyQueens;

        friendlyPieces = board.GetPieceList(Piece.Pawn, friendlyBit).bitboard | board.GetPieceList(Piece.Knight, friendlyBit).bitboard | friendlyDiags | friendlyOrthos | (1UL << friendlyKingSquare);
        enemyPieces = board.GetPieceList(Piece.Pawn, enemyBit).bitboard | board.GetPieceList(Piece.Knight, enemyBit).bitboard | enemyDiags | enemyOrthos | (1UL << enemyKingSquare);

        allPieces = friendlyPieces | enemyPieces;
    }







    private int moveCount = 0;

    public int GenerateMoves(ref Span<Move> moves, bool genOnlyCaptures = false) //Returns move count
    {
        moveCount = 0;

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


        friendlyIndexOffset = board.friendlyColorBit * 64;

        GeneratePieceBoards();


        GenerateKingMoves(ref moves, genOnlyCaptures);


        for (int i = 0; i < board.GetPieceList(Piece.Pawn, board.friendlyColorBit).Count; i++)
        {
            GeneratePawnMoves(ref moves, board.GetPieceList(Piece.Pawn, board.friendlyColorBit)[i], genOnlyCaptures); //TODO: Cache piecelist ref ofc!!!
        }

        for (int i = 0; i < board.GetPieceList(Piece.Knight, board.friendlyColorBit).Count; i++)
        {
            GenerateKnightMoves(ref moves, board.GetPieceList(Piece.Knight, board.friendlyColorBit)[i], genOnlyCaptures); //TODO: Cache piecelist ref ofc!!!
        }

        GenerateSlidingMoves(ref moves, genOnlyCaptures);

        moves = moves.Slice(0, moveCount);
        return moveCount;
    }

    private void GenerateSlidingMoves(ref Span<Move> moves, bool genOnlyCaptures)
    {
        ulong moveMask = ulong.MaxValue; //TODO: Make moveMask &= ~friendlyPieces by default and remove else statement

        if (genOnlyCaptures) moveMask &= enemyPieces; //This is correct don't bother thinking about it
        else moveMask &= ~friendlyPieces;//Only empty or enemy squares

        ulong orthos = friendlyOrthos;
        ulong diags = friendlyDiags;


        while (orthos != 0)
        {
            int startSquare = BitBoardHelper.PopFirstBit(ref orthos);

            ulong blockers = MagicData.rookMasks[startSquare] & allPieces;
            ulong index = (blockers * MagicData.rookMagics[startSquare]) >> MagicData.rookShifts[startSquare];

            ulong moveBoard = MagicData.rookMoveBitboards[startSquare][index] & moveMask;

            while (moveBoard != 0)
            {
                int targetSquare = BitBoardHelper.PopFirstBit(ref moveBoard);
                moves[moveCount++] = new Move(startSquare, targetSquare);
            }
        }


        while (diags != 0)
        {
            int startSquare = BitBoardHelper.PopFirstBit(ref diags);

            ulong blockers = MagicData.bishopMasks[startSquare] & allPieces;
            ulong index = (blockers * MagicData.bishopMagics[startSquare]) >> MagicData.bishopShifts[startSquare];

            //TODO: Could maybe store all moves for one piece with just a startsquare and a bitboard with possible move squares highlighted - could then turn into Move struct when we need to play them?

            ulong moveBoard = MagicData.bishopMoveBitboards[startSquare][index] & moveMask;

            while (moveBoard != 0)
            {
                int targetSquare = BitBoardHelper.PopFirstBit(ref moveBoard);
                moves[moveCount++] = new Move(startSquare, targetSquare);
            }
        }
    }

    private void GenerateKnightMoves(ref Span<Move> moves, int startSquare, bool genOnlyCaptures)
    {
        //TODO: Bitboards
        for (int i = 0; i < PrecomputedData.KnightMoves[startSquare].Length; i++)
        {
            int targetSquare = PrecomputedData.KnightMoves[startSquare][i];
            int pieceOnTarget = board.Squares[targetSquare];

            if (Piece.Color(pieceOnTarget) == board.friendlyColor) continue;

            bool isCapture = !Piece.IsNone(pieceOnTarget);

            if (isCapture || !genOnlyCaptures) moves[moveCount++] = new Move(startSquare, targetSquare);
        }
    }

    private void GeneratePawnMoves(ref Span<Move> moves, int startSquare, bool genOnlyCaptures) //TODO: bitboards
    {
        int moveDir = board.friendlyColor == Piece.White ? PrecomputedData.Up : PrecomputedData.Down;
        int targetSquare = startSquare + moveDir;//One move up/down

        int rank = BoardHelper.IndexToRank(startSquare);
        bool oneStepFromPromotion = rank == (board.friendlyColor == Piece.White ? 6 : 1);

        if (!genOnlyCaptures) //Only run this code if were not generating captures only
        {
            int startRank = board.friendlyColor == Piece.White ? 1 : 6;

            if (Piece.IsNone(board.Squares[targetSquare]))
            {
                if (oneStepFromPromotion) AddPromotionMoves(ref moves, startSquare, targetSquare);
                else moves[moveCount++] = new Move(startSquare, targetSquare);



                if (rank == startRank) //If on start rank
                {
                    int squareTwoForward = targetSquare + moveDir; //One additional move up/down

                    if (Piece.IsNone(board.Squares[squareTwoForward])) moves[moveCount++] = new Move(startSquare, squareTwoForward, Move.Flag.PawnTwoForward); //If no pieces on target square, add move
                }
            }
        }



        int attackIndex = startSquare + friendlyIndexOffset;
        int epFile = (int)((board.currentGameState & Board.epFileMask) >> 5) - 1;
        int epAttackRank = board.friendlyColor == Piece.White ? 5 : 2;
        int epAttackSquare = epFile != -1 ? BoardHelper.CoordToIndex(epFile, epAttackRank) : -1;

        for (int i = 0; i < PrecomputedData.PawnAttackSquares[attackIndex].Length; i++)
        {
            targetSquare = PrecomputedData.PawnAttackSquares[attackIndex][i];


            int targetPiece = board.Squares[targetSquare];

            if (Piece.Color(targetPiece) == board.enemyColor)
            {
                if (oneStepFromPromotion) AddPromotionMoves(ref moves, startSquare, targetSquare);
                else moves[moveCount++] = new Move(startSquare, targetSquare);
            }

            //En passant
            if (targetSquare == epAttackSquare)
            {
                moves[moveCount++] = new Move(startSquare, targetSquare, Move.Flag.EnPassantCapture);
            }
        }
    }

    private void AddPromotionMoves(ref Span<Move> moves, int startSquare, int targetSquare)
    {
        moves[moveCount++] = new Move(startSquare, targetSquare, Move.Flag.PromoteToQueen);
        moves[moveCount++] = new Move(startSquare, targetSquare, Move.Flag.PromoteToKnight);

        if (promotionMode == PromotionMode.KnightAndQueen) return;

        moves[moveCount++] = new Move(startSquare, targetSquare, Move.Flag.PromoteToRook);
        moves[moveCount++] = new Move(startSquare, targetSquare, Move.Flag.PromoteToBishop);
    }

    private void GenerateKingMoves(ref Span<Move> moves, bool genOnlyCaptures)
    {
        ulong moveBoard = PrecomputedData.kingAttackBitboards[friendlyKingSquare] & (~friendlyPieces);


        if (!genOnlyCaptures)
        {
            //Castling TODO: Can check if castling is allowed before computing all the boards
            ulong castleSquares = ~allPieces; //All empty squares

            ulong shortCastleBoard = PrecomputedData.castleMasks[board.friendlyColorBit] & castleSquares;

            if (ShortCastleAllowed() && BitBoardHelper.BitCount(shortCastleBoard) == 2) //If short allowed and both castle squares are empty
            {
                moves[moveCount++] = new Move(friendlyKingSquare, board.colorToMove == Piece.White ? BoardHelper.g1 : BoardHelper.g8, Move.Flag.Castling);
            }

            ulong longCastleBoard = PrecomputedData.castleMasks[2 + board.friendlyColorBit] & castleSquares;

            if (LongCastleAllowed() && BitBoardHelper.BitCount(longCastleBoard) == 2 && (allPieces & PrecomputedData.castleMasks[4 + board.friendlyColorBit]) == 0) //If long allowed and both castle squares are empty and the last one is empty
            {
                moves[moveCount++] = new Move(friendlyKingSquare, board.colorToMove == Piece.White ? BoardHelper.c1 : BoardHelper.c8, Move.Flag.Castling);
            }
        }
        else //Keep only capture squares
        {
            moveBoard &= enemyPieces;
        }



        while (moveBoard != 0) //TODOne: test splitting this into two loops - one only worries about capture moves (no castle checks) - other does quiet (with castle checks) - just seems slightly slower
        {
            int targetSquare = BitBoardHelper.PopFirstBit(ref moveBoard);

            moves[moveCount++] = new Move(friendlyKingSquare, targetSquare);
        }
    }





    private bool ShortCastleAllowed() //TODO: Move to board class
    {
        return (board.currentGameState & (1U << (9 + board.friendlyColorBit))) > 0;
    }

    private bool LongCastleAllowed() //TODO: Move to board class
    {
        return (board.currentGameState & (1U << (11 + board.friendlyColorBit))) > 0;
    }
}