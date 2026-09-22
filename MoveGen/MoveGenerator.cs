using System;
using System.Runtime.CompilerServices;

public class MoveGenerator
{
    public enum PromotionMode { All, KnightAndQueen };
    public static PromotionMode promotionMode = PromotionMode.KnightAndQueen;

    private ulong friendlyPieces;
    private ulong friendlyOrthos;
    private ulong friendlyDiags;

    private ulong enemyPieces;
    private ulong enemyOrthos;
    private ulong enemyDiags;

    private int friendlyKingSquare;
    private int enemyKingSquare;
    private int friendlyIndexOffset;
    //private int opponentIndexOffset;

    private Board board;

    public MoveGenerator(Board _board)
    {
        board = _board;
    }


    #region PieceBoards

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
    }

    #endregion




    private int moveCount = 0;

    #region MoveGeneration

    public Span<Move> GenerateMovesSlow()
    {
        Span<Move> moves = new Move[256];
        GenerateMoves(ref moves);
        return moves;
    }

    //TODO: Remove ref here bc unnecessary - span is ref to array anyway so just return a span like normal
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
        //opponentIndexOffset = board.opponentColorBit * 64;

        GeneratePieceBoards();

        GenerateKingMoves(ref moves, genOnlyCaptures);

        for (int i = 0; i < board.GetPieceList(Piece.Pawn, board.friendlyColorBit).Count; i++)
        {
            GeneratePawnMoves(ref moves, board.GetPieceList(Piece.Pawn, board.friendlyColorBit)[i], genOnlyCaptures); //TODO: Cache piecelist ref ofc!!!
        }


        PieceList knightList = board.GetPieceList(Piece.Knight, board.friendlyColorBit);

        for (int i = 0; i < knightList.Count; i++)
        {
            GenerateKnightMoves(ref moves, knightList[i], genOnlyCaptures);
        }

        GenerateSlidingMoves(ref moves, genOnlyCaptures);

        moves = moves.Slice(0, moveCount);
        return moveCount;
    }

    private void GenerateSlidingMoves(ref Span<Move> moves, bool genOnlyCaptures)
    {
        PieceList bishopList = board.GetPieceList(Piece.Bishop, board.friendlyColorBit);
        PieceList rookList = board.GetPieceList(Piece.Rook, board.friendlyColorBit);
        PieceList queenList = board.GetPieceList(Piece.Queen, board.friendlyColorBit); //TODO: Prob keep track of these as a single piecelist as well, as these 3 for-loops are completely identical and could be very easily combined


        for (int i = 0; i < bishopList.Count; i++)
        {
            int startSquare = bishopList[i];
            ulong attackMap = bishopList.attackMaps[i];

            if (genOnlyCaptures) attackMap &= enemyPieces;
            else attackMap &= ~friendlyPieces;

            while (attackMap != 0)
            {
                int targetSquare = BitBoardHelper.PopFirstBit(ref attackMap);
                moves[moveCount++] = new Move(startSquare, targetSquare);
            }
        }

        for (int i = 0; i < rookList.Count; i++)
        {
            int startSquare = rookList[i];
            ulong attackMap = rookList.attackMaps[i];

            if (genOnlyCaptures) attackMap &= enemyPieces;
            else attackMap &= ~friendlyPieces;

            while (attackMap != 0)
            {
                int targetSquare = BitBoardHelper.PopFirstBit(ref attackMap);
                moves[moveCount++] = new Move(startSquare, targetSquare);
            }
        }

        for (int i = 0; i < queenList.Count; i++)
        {
            int startSquare = queenList[i];
            ulong attackMap = queenList.attackMaps[i];

            if (genOnlyCaptures) attackMap &= enemyPieces;
            else attackMap &= ~friendlyPieces;

            while (attackMap != 0)
            {
                int targetSquare = BitBoardHelper.PopFirstBit(ref attackMap);
                moves[moveCount++] = new Move(startSquare, targetSquare);
            }
        }
    }



    private void GenerateKingMoves(ref Span<Move> moves, bool genOnlyCaptures)
    {
        ulong moveBoard = PrecomputedData.kingAttackBitboards[friendlyKingSquare] & (~friendlyPieces);


        if (genOnlyCaptures) moveBoard &= enemyPieces; //Keep only capture squares

        ulong castleSquares = ~board.allPieceBoard; //All empty squares

        ulong shortCastleBoard = PrecomputedData.castleMasks[board.friendlyColorBit] & castleSquares;

        if (ShortCastleAllowed() && BitBoardHelper.BitCount(shortCastleBoard) == 2) //If short allowed and both castle squares are empty
        {
            moves[moveCount++] = new Move(friendlyKingSquare, board.colorToMove == Piece.White ? BoardHelper.g1 : BoardHelper.g8, Move.Flag.Castling);
        }

        ulong longCastleBoard = PrecomputedData.castleMasks[2 + board.friendlyColorBit] & castleSquares;

        if (LongCastleAllowed() && BitBoardHelper.BitCount(longCastleBoard) == 2 && (board.allPieceBoard & PrecomputedData.castleMasks[4 + board.friendlyColorBit]) == 0) //If long allowed and both castle squares are empty and the last one is empty
        {
            moves[moveCount++] = new Move(friendlyKingSquare, board.colorToMove == Piece.White ? BoardHelper.c1 : BoardHelper.c8, Move.Flag.Castling);
        }



        while (moveBoard != 0) //TODOne: test splitting this into two loops - one only worries about capture moves (no castle checks) - other does quiet (with castle checks) - just seems slightly slower
        {
            int targetSquare = BitBoardHelper.PopFirstBit(ref moveBoard);

            moves[moveCount++] = new Move(friendlyKingSquare, targetSquare);
        }
    }

    private void GeneratePawnMoves(ref Span<Move> moves, int startSquare, bool genOnlyCaptures)
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
            if (targetSquare == epAttackSquare) moves[moveCount++] = new Move(startSquare, targetSquare, Move.Flag.EnPassantCapture);
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


    private void AddPromotionMoves(ref Span<Move> moves, int startSquare, int targetSquare)
    {
        moves[moveCount++] = new Move(startSquare, targetSquare, Move.Flag.PromoteToQueen);
        moves[moveCount++] = new Move(startSquare, targetSquare, Move.Flag.PromoteToKnight);

        if (promotionMode == PromotionMode.KnightAndQueen) return;

        moves[moveCount++] = new Move(startSquare, targetSquare, Move.Flag.PromoteToRook);
        moves[moveCount++] = new Move(startSquare, targetSquare, Move.Flag.PromoteToBishop);
    }



    #endregion



    #region Helpers

    private bool ShortCastleAllowed() //TODO: Move to board class
    {
        return (board.currentGameState & (1U << (9 + board.friendlyColorBit))) > 0;
    }

    private bool LongCastleAllowed() //TODO: Move to board class
    {
        return (board.currentGameState & (1U << (11 + board.friendlyColorBit))) > 0;
    }

    #endregion
}

public struct Move //FFFFTTTTTTSSSSSS - F = Flag bit - T = Target square bit - S = Start square bit
{
    public readonly struct Flag //TODO: Use the last flagbit for quiet vs capture maybe - could make a lot of things easier and avoid expensive checks like whether the target square is empty in make/unmake
    {
        public const int None = 0;
        public const int EnPassantCapture = 1; //0b001
        public const int Castling = 2; //0b010
        public const int PromoteToQueen = 3; //0b011
        public const int PromoteToKnight = 4; //0b100
        public const int PromoteToRook = 5; //0b101
        public const int PromoteToBishop = 6; //0b110
        public const int PawnTwoForward = 7; //0b111
        public const int TestFlag = 8;
    }

    public readonly ushort data;
    public int startSquare { get { return data & StartMask; } }
    public int targetSquare { get { return (data & TargetMask) >> 6; } }
    public int flag { get { return (data & FlagMask) >> 12; } }

    private const ushort StartMask = 0b0000000000111111;
    private const ushort TargetMask = 0b0000111111000000;
    private const ushort FlagMask = 0b1111000000000000;

    public static Move nullMove = new Move(0, 0); //TODO: try if setting as const is more performant? Or adding function for IsNull

    public Move(ushort data)
    {
        this.data = data;
    }

    public Move(int start, int target)
    {
        data = (ushort)(start | target << 6);
    }

    public Move(int start, int target, int flag)
    {
        data = (ushort)(start | target << 6 | flag << 12);
    }

    public bool IsPromotion()
    {
        int _flag = flag;
        return _flag > 2 && _flag < 7;
    }

    public bool IsNullMove()
    {
        return data == 0;
    }
}