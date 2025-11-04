using System;

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


    public ulong opponentKingAttackMap;
    public ulong oponnentPawnAttackMap;
    public ulong opponentKnightAttackMap;
    public ulong opponentSlidingAttackMap;
    public ulong opponentAttackMapNoPawns;
    public ulong opponentAttackMap;

    public ulong checkRayBitMap;
    public ulong pinRayBitMap;
    public bool inCheck;
    public bool inDoubleCheck;

    private int friendlyKingSquare;
    private int enemyKingSquare;
    private int friendlyIndexOffset;
    private int opponentIndexOffset;

    private Board board;

    public MoveGenerator(Board _board)
    {
        board = _board;
    }


    #region LegalityMaps

    private void GenerateAttackMaps()
    {
        GenerateSlidingAttackMap();

        ulong kingOrthoMask = MagicData.rookMasks[friendlyKingSquare] & enemyPieces; //Bitboard of enemy ortho attack blcoks by enemy's own pieces

        ulong kingOrthoIndex = (kingOrthoMask * MagicData.rookMagics[friendlyKingSquare]) >> MagicData.rookShifts[friendlyKingSquare];

        ulong kingOrthoAttackMask = MagicData.rookMoveBitboards[friendlyKingSquare][kingOrthoIndex]; //Bitboard of potential ortho attack directions

        ulong kingDiagMask = MagicData.bishopMasks[friendlyKingSquare] & enemyPieces; //Bitboard of enemy diags attack blcoks by enemy's own pieces
        ulong kingDiagIndex = (kingDiagMask * MagicData.bishopMagics[friendlyKingSquare]) >> MagicData.bishopShifts[friendlyKingSquare];

        ulong kingDiagAttackMask = MagicData.bishopMoveBitboards[friendlyKingSquare][kingDiagIndex]; //Bitboard of potential ortho attack directions

        ulong kingAttackMask = kingOrthoAttackMask | kingDiagAttackMask; //Bitboard of enemy slider attack blcoks - ignoring slider behind slider
        ulong potentialKingAttackers = (kingOrthoAttackMask & enemyOrthos) | (kingDiagAttackMask & enemyDiags); //Bitboard of all enemy sliders that could be checking or pinning

        //TODO: have precomputed moveBitboard array in MagicData for queen moves - prevents having to do all this for both orthos and diags at runtime

        while (potentialKingAttackers != 0) //TODOnt: Massive optimisation - could just precompute a directionMask array that doesn't go to board edge - just to piece - no need to do magic stuff above -> we need attack mask - look above and think
        {
            int startSquare = BitBoardHelper.PopFirstBit(ref potentialKingAttackers);
            ulong directionMask = PrecomputedData.directionalMasks[friendlyKingSquare][startSquare]; //Mask of line from king through piece to board edge
            ulong pinMask = kingAttackMask & directionMask; //Bitboard of line from king to attacking slider - includes slider itself

            ulong pinBoard = pinMask & friendlyPieces; //Bitboard of all potentially pinned pieces between this slider and the king - if none; were in check

            int pinCount = BitBoardHelper.BitCount(pinBoard); //Number of pieces in pinboard

            if (pinCount > 1) continue; //More than 1 piece means no pin and no check
            else if (pinCount == 1)
            {
                pinRayBitMap |= pinMask;
            }
            else
            {
                inDoubleCheck = inCheck;
                inCheck = true;
                checkRayBitMap |= pinMask;
            }
        }


        //Knight attacks
        PieceList enemyKnights = board.knightList[board.opponentColorBit];
        opponentKnightAttackMap = 0;
        bool isKnightCheck = false;

        for (int knightIndex = 0; knightIndex < enemyKnights.Count; knightIndex++)
        {
            int startSquare = enemyKnights[knightIndex];
            opponentKnightAttackMap |= PrecomputedData.knightAttackBitboards[startSquare];

            if (!isKnightCheck && BitBoardHelper.ContainsSquare(opponentKnightAttackMap, friendlyKingSquare))
            {
                isKnightCheck = true;
                inDoubleCheck = inCheck;
                inCheck = true;
                checkRayBitMap = BitBoardHelper.AddSquare(checkRayBitMap, startSquare);
            }
        }


        //Pawn attacks
        PieceList enemyPawns = board.pawnList[board.opponentColorBit];
        oponnentPawnAttackMap = 0;
        bool isPawnCheck = false;

        for (int pawnIndex = 0; pawnIndex < enemyPawns.Count; pawnIndex++)
        {
            int startSquare = enemyPawns[pawnIndex];
            oponnentPawnAttackMap |= PrecomputedData.pawnAttackBitboards[startSquare + opponentIndexOffset];

            if (!isPawnCheck && BitBoardHelper.ContainsSquare(oponnentPawnAttackMap, friendlyKingSquare))
            {
                isPawnCheck = true;
                inDoubleCheck = inCheck;
                inCheck = true;
                checkRayBitMap = BitBoardHelper.AddSquare(checkRayBitMap, startSquare);
            }
        }

        //King attacks
        opponentKingAttackMap = PrecomputedData.kingAttackBitboards[enemyKingSquare];

        opponentAttackMapNoPawns = opponentSlidingAttackMap | opponentKnightAttackMap | opponentKingAttackMap;
        opponentAttackMap = opponentAttackMapNoPawns | oponnentPawnAttackMap;

        if (!inCheck) checkRayBitMap = ulong.MaxValue; //Make all squares available to move to if not in check
    }

    private void GenerateSlidingAttackMap()
    {
        opponentSlidingAttackMap = 0;

        ulong orthos = enemyOrthos;
        ulong diags = enemyDiags;

        while (orthos != 0)
        {
            int startSquare = BitBoardHelper.PopFirstBit(ref orthos);

            ulong blockers = MagicData.rookMasks[startSquare] & (allPieces ^ (1UL << friendlyKingSquare)); //Remove friendly king square from blockers so attack ray will continue through it - prevents king from just moving backwards and still being in the ray

            ulong index = (blockers * MagicData.rookMagics[startSquare]) >> MagicData.rookShifts[startSquare];

            ulong moveBoard = MagicData.rookMoveBitboards[startSquare][index];

            opponentSlidingAttackMap |= moveBoard;
        }

        while (diags != 0)
        {
            int startSquare = BitBoardHelper.PopFirstBit(ref diags);

            ulong blockers = MagicData.bishopMasks[startSquare] & (allPieces ^ (1UL << friendlyKingSquare)); //Remove friendly king square from blockers so attack ray will continue through it - prevents king from just moving backwards and still being in the ray

            ulong index = (blockers * MagicData.bishopMagics[startSquare]) >> MagicData.bishopShifts[startSquare];

            ulong moveBoard = MagicData.bishopMoveBitboards[startSquare][index];

            opponentSlidingAttackMap |= moveBoard;
        }
    }

    #endregion

    #region PieceBoards

    private void GeneratePieceBoards() //TODOne: would be more performant to keep track of these and update them in board on make an unmake move
    {
        int friendlyBit = board.friendlyColorBit;
        int enemyBit = board.opponentColorBit;

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
        opponentIndexOffset = board.opponentColorBit * 64;

        pinRayBitMap = 0;
        checkRayBitMap = 0;
        inCheck = false;
        inDoubleCheck = false;

        GeneratePieceBoards();

        GenerateAttackMaps();

        GenerateKingMoves(ref moves, genOnlyCaptures);

        if (inDoubleCheck) return moveCount; //Only king moves valid when in double check

        for (int i = 0; i < board.pawnList[board.friendlyColorBit].Count; i++)
        {
            GeneratePawnMoves(ref moves, board.pawnList[board.friendlyColorBit][i], genOnlyCaptures);
        }

        for (int i = 0; i < board.knightList[board.friendlyColorBit].Count; i++)
        {
            GenerateKnightMoves(ref moves, board.knightList[board.friendlyColorBit][i], genOnlyCaptures);
        }

        GenerateSlidingMoves(ref moves, genOnlyCaptures);

        moves = moves.Slice(0, moveCount);
        return moveCount;
    }

    private void GenerateSlidingMoves(ref Span<Move> moves, bool genOnlyCaptures)
    {
        //            Only if blocks check
        ulong moveMask = checkRayBitMap;

        if (genOnlyCaptures) moveMask &= enemyPieces; //This is correct don't bother thinking about it
        else moveMask &= ~friendlyPieces;//Only empty or enemy squares

        ulong orthos = friendlyOrthos;
        ulong diags = friendlyDiags;

        //Pinned pieces cannot move if king is in check
        //Credit to seb lague for this if statement
        if (inCheck)
        {
            orthos &= ~pinRayBitMap;
            diags &= ~pinRayBitMap;
        }


        while (orthos != 0)
        {
            int startSquare = BitBoardHelper.PopFirstBit(ref orthos);

            ulong blockers = MagicData.rookMasks[startSquare] & allPieces;
            ulong index = (blockers * MagicData.rookMagics[startSquare]) >> MagicData.rookShifts[startSquare];

            ulong moveBoard = MagicData.rookMoveBitboards[startSquare][index] & moveMask;

            if (IsPinned(startSquare))
            {
                moveBoard &= PrecomputedData.directionalMasks[friendlyKingSquare][startSquare] & pinRayBitMap;
            }

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

            ulong moveBoard = MagicData.bishopMoveBitboards[startSquare][index] & moveMask;

            if (IsPinned(startSquare))
            {
                moveBoard &= PrecomputedData.directionalMasks[friendlyKingSquare][startSquare] & pinRayBitMap;
            }

            while (moveBoard != 0)
            {
                int targetSquare = BitBoardHelper.PopFirstBit(ref moveBoard);
                moves[moveCount++] = new Move(startSquare, targetSquare);
            }
        }
    }

    private void GenerateKingMoves(ref Span<Move> moves, bool genOnlyCaptures)
    {
        ulong moveBoard = PrecomputedData.kingAttackBitboards[friendlyKingSquare] & (~friendlyPieces);
        ulong safetyMap = ~opponentAttackMap; //All squares not attacked by opponent

        moveBoard &= safetyMap; //Remove attacked squares from moveboard

        if (genOnlyCaptures) moveBoard &= enemyPieces; //Keep only capture squares
        else if (!inCheck)
        {
            ulong castleSquares = safetyMap & (~allPieces); //All safe and empty squares

            ulong shortCastleBoard = PrecomputedData.castleMasks[board.friendlyColorBit] & castleSquares;

            if (ShortCastleAllowed() && BitBoardHelper.BitCount(shortCastleBoard) == 2) //If short allowed and both castle squares are safe and empty
            {
                moves[moveCount++] = new Move(friendlyKingSquare, board.colorToMove == Piece.White ? BoardHelper.g1 : BoardHelper.g8, Move.Flag.Castling);
            }

            ulong longCastleBoard = PrecomputedData.castleMasks[2 + board.friendlyColorBit] & castleSquares;

            if (LongCastleAllowed() && BitBoardHelper.BitCount(longCastleBoard) == 2 && (allPieces & PrecomputedData.castleMasks[4 + board.friendlyColorBit]) == 0) //If long allowed and both castle squares are safe and empty and the last one is empty
            {
                moves[moveCount++] = new Move(friendlyKingSquare, board.colorToMove == Piece.White ? BoardHelper.c1 : BoardHelper.c8, Move.Flag.Castling);
            }
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
                if (!IsPinned(startSquare) || IsMovingAlongRay(friendlyKingSquare, startSquare, moveDir))
                {
                    if (!inCheck || SquareIsInCheckRay(targetSquare))
                    {
                        if (oneStepFromPromotion) AddPromotionMoves(ref moves, startSquare, targetSquare);
                        else moves[moveCount++] = new Move(startSquare, targetSquare);
                    }



                    if (rank == startRank) //If on start rank
                    {
                        int squareTwoForward = targetSquare + moveDir; //One additional move up/down

                        if (Piece.IsNone(board.Squares[squareTwoForward]) && (!inCheck || SquareIsInCheckRay(squareTwoForward))) moves[moveCount++] = new Move(startSquare, squareTwoForward, Move.Flag.PawnTwoForward); //If no pieces on target square, add move
                    }
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
            int captureDirection = targetSquare - startSquare;

            //TODO:                      Can replace this with a simple check for the square being in pinRayBoard bc that will always be true if moving along ray with pawns?
            if (IsPinned(startSquare) && !IsMovingAlongRay(friendlyKingSquare, startSquare, captureDirection)) continue; //Pawn is pinned and cant move in this direction

            int targetPiece = board.Squares[targetSquare];

            if (Piece.Color(targetPiece) == board.enemyColor)
            {
                if (inCheck && !SquareIsInCheckRay(targetSquare)) continue; //Skip direction if were in check and this move doesn't block it

                if (oneStepFromPromotion) AddPromotionMoves(ref moves, startSquare, targetSquare);
                else moves[moveCount++] = new Move(startSquare, targetSquare);
            }

            //En passant
            if (targetSquare == epAttackSquare)
            {
                int capturedPawnSquare = targetSquare - moveDir;

                if (inCheck && !SquareIsInCheckRay(targetSquare) && !SquareIsInCheckRay(capturedPawnSquare)) continue;

                int epStartRank = board.friendlyColor == Piece.White ? 4 : 3;


                if (!InCheckAfterEnPassant(startSquare, epStartRank, capturedPawnSquare)) moves[moveCount++] = new Move(startSquare, targetSquare, Move.Flag.EnPassantCapture);
            }
        }
    }

    private void GenerateKnightMoves(ref Span<Move> moves, int startSquare, bool genOnlyCaptures)
    {
        //TODO: Bitboards
        for (int i = 0; i < PrecomputedData.KnightMoves[startSquare].Length; i++)
        {
            if (IsPinned(startSquare)) return; //Knight cant move at all if pinned

            int targetSquare = PrecomputedData.KnightMoves[startSquare][i];
            int pieceOnTarget = board.Squares[targetSquare];

            if (Piece.Color(pieceOnTarget) == board.friendlyColor) continue;

            bool isCapture = !Piece.IsNone(pieceOnTarget);

            if ((isCapture || !genOnlyCaptures) && (!inCheck || SquareIsInCheckRay(targetSquare))) moves[moveCount++] = new Move(startSquare, targetSquare);
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

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsPinned(int square)
    {
        return BitBoardHelper.ContainsSquare(pinRayBitMap, square);
        //return (pinRayBitMap & (1UL << square)) != 0;
    }

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsMovingAlongRay(int startSquare, int targetSquare, int directionOffset)
    {
        int rayDirection = PrecomputedData.directionLookup[targetSquare - startSquare + 63];
        return directionOffset == rayDirection || -directionOffset == rayDirection;
    }

    private bool SquareIsInCheckRay(int square)
    {
        return BitBoardHelper.ContainsSquare(checkRayBitMap, square);
        //return (checkRayBitMap & (1UL << square)) != 0; //&& inCheck - Included in SebLague's code but don't see why it would be necessary as the bitmaps are reset when we start generating moves
    }

    private bool SquareIsAttacked(int square)
    {
        return BitBoardHelper.ContainsSquare(opponentAttackMap, square);
    }

    private bool ShortCastleAllowed() //TODO: Move to board class
    {
        return (board.currentGameState & (1U << (9 + board.friendlyColorBit))) > 0;
    }

    private bool LongCastleAllowed() //TODO: Move to board class
    {
        return (board.currentGameState & (1U << (11 + board.friendlyColorBit))) > 0;
    }

    private bool InCheckAfterEnPassant(int square, int startRank, int capturedPawnSquare)
    {
        int kingRank = BoardHelper.IndexToRank(friendlyKingSquare);

        if (kingRank != startRank) return false; //If king is on the same rank as the en passant, a discovered king attack is possible when capturing ep

        //Check horizontally for rooks and queens
        int directionIncrement = (square - friendlyKingSquare) > 0 ? PrecomputedData.Right : PrecomputedData.Left;

        int startFile = BoardHelper.IndexToFile(friendlyKingSquare);
        int fileCount = directionIncrement == 1 ? 8 - startFile : startFile; //number of files to check

        int startSquare = friendlyKingSquare + directionIncrement; //We start at the friendly kings square and move one square away from the king; this is the first square where a piece could be blocking a potential check

        for (int i = 0; i < fileCount; i++)
        {
            int index = startSquare + i * directionIncrement;

            int pieceOnSquare = board.Squares[index];

            if (pieceOnSquare != Piece.None && index != capturedPawnSquare && index != square)
            {
                if (Piece.Color(pieceOnSquare) == board.enemyColor) return Piece.IsRookOrQueen(pieceOnSquare); //Will put us in check if it's a rook or a queen
                else return false; //Ran into a friendly piece which will be blocking any attack
            }
        }

        return false; //Empty rank - no attackers


        //Thought all the following was correct, but realised that it is impossible to be in a position where it actually holds true, and therefore the only necessary check is the horizontal one above ^
        /*else
        {
            //Here we already know (from code calling this function) that the pawn is either not pinned, or moving along the pin ray - Means no discovered attack through friendly pawn.
            //Only way a discovered attack is possible, is through the captured pawn - This can also only happen through a single diagonal in this case, as the friendly pawn will block any vertical-
            //attacks, and we have already ruled out horizontal attacks in the enclosing if-statement. The diagonal where this is possible, is the passing through both the friendly king and the-
            //pawn being captured. If such a diagonal exists.

            int kingFile = BoardHelper.IndexToFile(friendlyKingSquare);

            //For the diagonal to be valid, ΔFile has to be equal to ΔRank - As in if the king is 2 down, and 2 right from the pawn,

            int fileDelta = file - kingFile;
            int rankDelta = rank - kingRank;

            if (fileDelta == rankDelta || -fileDelta == rankDelta) //If valid diagonal exists between king an pawn being captured
            {

            }
        }*/
    }

    #endregion
}

public struct Move //FFFFTTTTTTSSSSSS - F = Flag bit - T = Target square bit - S = Start square bit
{
    public readonly struct Flag
    {
        public const int None = 0;
        public const int EnPassantCapture = 1;
        public const int Castling = 2;
        public const int PromoteToQueen = 3;
        public const int PromoteToKnight = 4;
        public const int PromoteToRook = 5;
        public const int PromoteToBishop = 6;
        public const int PawnTwoForward = 7;
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

    public Move (ushort data)
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
}