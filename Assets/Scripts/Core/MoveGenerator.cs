using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class MoveGenerator
{
    public enum PromotionMode { All, KnightAndQueen };
    public static PromotionMode promotionMode = PromotionMode.KnightAndQueen;

    public static ulong oponnentPawnAttackMap;
    public static ulong opponentKnightAttackMap;
    public static ulong opponentSlidingAttackMap;
    public static ulong opponentAttackMapNoPawns;
    public static ulong opponentAttackMap;

    public static ulong checkRayBitMap;
    public static ulong pinRayBitMap;
    public static bool inCheck;
    public static bool inDoubleCheck;

    private static int friendlyKingSquare;
    private static int enemyKingSquare;
    private static int friendlyIndexOffset;
    private static int opponentIndexOffset;

    #region LegalityMaps

    private static void GenerateAttackMaps()
    {
        GenerateSlidingAttackMap();

        //int startDirIndex = 0;
        //int endDirIndex = 8;

        //TODO: try: if friendly king is not in sliding attack map dont check for sliding checks? still pins ofc

        //ulong kingOrthoMask = MagicData.rookMoveBitboards[friendlyKingSquare][0]; //All possible ortho moves ignoring other pieces
        //ulong potentialOrthoAttackers = enemyOrthos & kingOrthoMask; //Bitboard of all orthos in line of sight of the king

        ulong enemyPieces = Board.colorPieces[Board.opponentColorBit];

        ulong kingOrthoMask = MagicData.rookMasks[friendlyKingSquare] & enemyPieces; //Bitboard of enemy ortho attack blcoks by enemy's own pieces

        ulong kingOrthoIndex = (kingOrthoMask * MagicData.rookMagics[friendlyKingSquare]) >> MagicData.rookShifts[friendlyKingSquare];

        ulong kingOrthoAttackMask = MagicData.rookMoveBitboards[friendlyKingSquare][kingOrthoIndex]; //Bitboard of potential ortho attack directions

        ulong kingDiagMask = MagicData.bishopMasks[friendlyKingSquare] & enemyPieces; //Bitboard of enemy diags attack blcoks by enemy's own pieces
        ulong kingDiagIndex = (kingDiagMask * MagicData.bishopMagics[friendlyKingSquare]) >> MagicData.bishopShifts[friendlyKingSquare];

        ulong kingDiagAttackMask = MagicData.bishopMoveBitboards[friendlyKingSquare][kingDiagIndex]; //Bitboard of potential ortho attack directions

        ulong kingAttackMask = kingOrthoAttackMask | kingDiagAttackMask; //Bitboard of enemy slider attack blcoks - ignoring slider behind slider
        ulong potentialKingAttackers = (kingOrthoAttackMask & Board.orthos[Board.opponentColorBit]) | (kingDiagAttackMask & Board.diags[Board.opponentColorBit]); //Bitboard of all enemy sliders that could be checking or pinning

        //TODO: have precomputed moveBitboard array in MagicData for queen moves - prevents having to do all this for both orthos and diags at runtime

        //Could just check here if kingOrthoAttackMask & enemyOrthos != 0 bc always check, but this is done in the pin checking anyway
        //BoardGraphics.instance.ResetCummulativeBoard();

        while (potentialKingAttackers != 0) //TODOnt: Massive optimisation - could just precompute a directionMask array that doesn't go to board edge - just to piece - no need to do magic stuff above -> we need attack mask - look above and think
        {
            int startSquare = BitBoardHelper.PopFirstBit(ref potentialKingAttackers);
            ulong directionMask = PrecomputedData.directionalMasks[friendlyKingSquare][startSquare]; //Mask of line from king through piece to board edge
            ulong pinMask = kingAttackMask & directionMask; //Bitboard of line from king to attacking slider - includes slider itself
            //BoardGraphics.instance.HighlightBitBoardCummulative(directionMask);
            //Debug.Log("Direciton: " + PrecomputedData.directionLookup[startSquare - friendlyKingSquare + 63]);

            ulong pinBoard = pinMask & Board.colorPieces[Board.friendlyColorBit]; //Bitboard of all potentially pinned pieces between this slider and the king - if none; were in check

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
        PieceList enemyKnights = Board.knightList[Board.opponentColorBit];
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
        PieceList enemyPawns = Board.pawnList[Board.opponentColorBit];
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

        opponentAttackMapNoPawns = opponentSlidingAttackMap | opponentKnightAttackMap | PrecomputedData.kingAttackBitboards[enemyKingSquare];
        opponentAttackMap = opponentAttackMapNoPawns | oponnentPawnAttackMap;

        if (!inCheck) checkRayBitMap = ulong.MaxValue; //Make all squares available to move to if not in check
    }

    private static void GenerateSlidingAttackMap()
    {
        opponentSlidingAttackMap = 0;

        ulong orthos = Board.orthos[Board.opponentColorBit];
        ulong diags = Board.diags[Board.opponentColorBit];

        while (orthos != 0)
        {
            int startSquare = BitBoardHelper.PopFirstBit(ref orthos);

            ulong blockers = MagicData.rookMasks[startSquare] & (Board.allPieces ^ (1UL << friendlyKingSquare)); //Remove friendly king square from blockers so attack ray will continue through it - prevents king from just moving backwards and still being in the ray

            ulong index = (blockers * MagicData.rookMagics[startSquare]) >> MagicData.rookShifts[startSquare];

            ulong moveBoard = MagicData.rookMoveBitboards[startSquare][index];

            opponentSlidingAttackMap |= moveBoard;
        }

        while (diags != 0)
        {
            int startSquare = BitBoardHelper.PopFirstBit(ref diags);

            ulong blockers = MagicData.bishopMasks[startSquare] & (Board.allPieces ^ (1UL << friendlyKingSquare)); //Remove friendly king square from blockers so attack ray will continue through it - prevents king from just moving backwards and still being in the ray

            ulong index = (blockers * MagicData.bishopMagics[startSquare]) >> MagicData.bishopShifts[startSquare];

            ulong moveBoard = MagicData.bishopMoveBitboards[startSquare][index];

            opponentSlidingAttackMap |= moveBoard;
        }
    }

    #endregion




    //public static Move[] moves = new Move[218];
    private static int moveCount = 0;

    #region MoveGeneration

    public static Span<Move> GenerateMovesSlow()
    {
        Span<Move> moves = new Move[256];
        GenerateMoves(ref moves);
        return moves;
    }

    //TODO: Remove ref here bc unnecessary - span is ref to array anyway so just return a span like normal
    public static int GenerateMoves(ref Span<Move> moves, bool genOnlyCaptures = false) //Returns move count
    {
        moveCount = 0;

        if (Board.colorToMove == Piece.White)
        {
            friendlyKingSquare = Board.whiteKingSquare;
            enemyKingSquare = Board.blackKingSquare;
        }
        else
        {
            friendlyKingSquare = Board.blackKingSquare;
            enemyKingSquare = Board.whiteKingSquare;
        }

        friendlyIndexOffset = Board.friendlyColorBit * 64;
        opponentIndexOffset = Board.opponentColorBit * 64;

        pinRayBitMap = 0;
        checkRayBitMap = 0;
        inCheck = false;
        inDoubleCheck = false;

        GenerateAttackMaps();

        GenerateKingMoves(ref moves, genOnlyCaptures);

        if (inDoubleCheck) return moveCount; //Only king moves valid when in double check

        for (int i = 0; i < Board.pawnList[Board.friendlyColorBit].Count; i++)
        {
            GeneratePawnMoves(ref moves, Board.pawnList[Board.friendlyColorBit][i], genOnlyCaptures);
        }

        for (int i = 0; i < Board.knightList[Board.friendlyColorBit].Count; i++)
        {
            GenerateKnightMoves(ref moves, Board.knightList[Board.friendlyColorBit][i], genOnlyCaptures);
        }

        GenerateSlidingMoves(ref moves, genOnlyCaptures);

        moves = moves.Slice(0, moveCount);
        return moveCount;
    }

    private static void GenerateSlidingMoves(ref Span<Move> moves, bool genOnlyCaptures)
    {
        //            Only if blocks check
        ulong moveMask = checkRayBitMap;

        if (genOnlyCaptures) moveMask &= Board.colorPieces[Board.opponentColorBit];
        else moveMask &= ~Board.colorPieces[Board.friendlyColorBit];//Only empty or enemy squares

        ulong orthos = Board.orthos[Board.friendlyColorBit]; //Board.orthos[Board.friendlyColorBit];
        ulong diags = Board.diags[Board.friendlyColorBit];//Board.diags[Board.friendlyColorBit];

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

            ulong blockers = MagicData.rookMasks[startSquare] & Board.allPieces;
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

            ulong blockers = MagicData.bishopMasks[startSquare] & Board.allPieces;
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

    private static void GenerateKingMoves(ref Span<Move> moves, bool genOnlyCaptures)
    {
        //TODO: Could prob be easily optimized with bitboards

        for (int i = 0; i < PrecomputedData.KingMoves[friendlyKingSquare].Length; i++)
        {
            int targetSquare = PrecomputedData.KingMoves[friendlyKingSquare][i];
            int pieceOnTarget = Board.Squares[targetSquare];

            if (Piece.Color(pieceOnTarget) == Board.friendlyColor) continue;

            if (!SquareIsAttacked(targetSquare))
            {
                bool isCapture = pieceOnTarget != Piece.None;

                if (isCapture) //if it's a capture we can just add the move, and skip the rest of the code below, because castling won't be possible
                {
                    moves[moveCount++] = new Move(friendlyKingSquare, targetSquare);
                    continue;
                }
                else if (genOnlyCaptures) continue;

                moves[moveCount++] = new Move(friendlyKingSquare, targetSquare);

                //Castling
                if (!inCheck) //Should also check for not capture, but we have done that above
                {
                    //Short
                    if ((targetSquare == BoardHelper.f1 || targetSquare == BoardHelper.f8) && ShortCastleAllowed())
                    {
                        //Debug.Log("Short move added");
                        //Debug.Log(Convert.ToString(Board.currentGameState, 2));
                        int castleKingsideSquare = targetSquare + 1;
                        if (Board.Squares[castleKingsideSquare] == Piece.None)
                        {
                            if (!SquareIsAttacked(castleKingsideSquare))
                            {
                                moves[moveCount++] = new Move(friendlyKingSquare, castleKingsideSquare, Move.Flag.Castling);
                            }
                        }
                    }
                    //Long
                    else if ((targetSquare == BoardHelper.d1 || targetSquare == BoardHelper.d8) && LongCastleAllowed())
                    {
                        //Debug.Log("Long move added");
                        int castleQueensideSquare = targetSquare - 1;
                        if (Board.Squares[castleQueensideSquare] == Piece.None && Board.Squares[castleQueensideSquare - 1] == Piece.None)
                        {
                            if (!SquareIsAttacked(castleQueensideSquare))
                            {
                                moves[moveCount++] = new Move(friendlyKingSquare, castleQueensideSquare, Move.Flag.Castling);
                            }
                        }
                    }
                }
            }
        }
    }

    private static void GeneratePawnMoves(ref Span<Move> moves, int startSquare, bool genOnlyCaptures)
    {
        //TODO: use bitboards for pawn moves

        int moveDir = Board.friendlyColor == Piece.White ? PrecomputedData.Up : PrecomputedData.Down;
        int targetSquare = startSquare + moveDir;//One move up/down

        int rank = BoardHelper.IndexToRank(startSquare);
        bool oneStepFromPromotion = rank == (Board.friendlyColor == Piece.White ? 6 : 1);

        if (!genOnlyCaptures) //Only run this code if were not generating captures only
        {

            int startRank = Board.friendlyColor == Piece.White ? 1 : 6;



            if (Piece.IsNone(Board.Squares[targetSquare]))
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

                        if (Piece.IsNone(Board.Squares[squareTwoForward]) && (!inCheck || SquareIsInCheckRay(squareTwoForward))) moves[moveCount++] = new Move(startSquare, squareTwoForward, Move.Flag.PawnTwoForward); //If no pieces on target square, add move
                    }
                }
            }
        }



        int attackIndex = startSquare + friendlyIndexOffset;
        int epFile = (int)((Board.currentGameState & Board.epFileMask) >> 5) - 1;
        int epAttackRank = Board.friendlyColor == Piece.White ? 5 : 2;
        int epAttackSquare = epFile != -1 ? BoardHelper.CoordToIndex(epFile, epAttackRank) : -1;

        for (int i = 0; i < PrecomputedData.PawnAttackSquares[attackIndex].Length; i++)
        {
            targetSquare = PrecomputedData.PawnAttackSquares[attackIndex][i];
            int captureDirection = targetSquare - startSquare;

            if (IsPinned(startSquare) && !IsMovingAlongRay(friendlyKingSquare, startSquare, captureDirection)) continue; //Pawn is pinned and cant move in this direction

            int targetPiece = Board.Squares[targetSquare];

            if (Piece.Color(targetPiece) == Board.enemyColor) //AddMove(new Move(startSquare, targetSquare)); //If enemy piece on attack square
            {
                if (inCheck && !SquareIsInCheckRay(targetSquare)) continue; //Skip direction if were in check and this move doesn't block it

                if (oneStepFromPromotion) AddPromotionMoves(ref moves, startSquare, targetSquare);
                else moves[moveCount++] = new Move(startSquare, targetSquare);
            }

            //En passant
            if (targetSquare == epAttackSquare)
            {
                //Debug.Log("ep possible");
                int capturedPawnSquare = targetSquare - moveDir;

                if (inCheck && !SquareIsInCheckRay(targetSquare) && !SquareIsInCheckRay(capturedPawnSquare)) continue;

                int epStartRank = Board.friendlyColor == Piece.White ? 4 : 3;


                if (!InCheckAfterEnPassant(startSquare, epStartRank, capturedPawnSquare)) moves[moveCount++] = new Move(startSquare, targetSquare, Move.Flag.EnPassantCapture);
            }
        }
    }

    private static void GenerateKnightMoves(ref Span<Move> moves, int startSquare, bool genOnlyCaptures)
    {
        for (int i = 0; i < PrecomputedData.KnightMoves[startSquare].Length; i++)
        {
            if (IsPinned(startSquare)) return; //Knight cant move at all if pinned

            int targetSquare = PrecomputedData.KnightMoves[startSquare][i];
            int pieceOnTarget = Board.Squares[targetSquare];

            if (Piece.Color(pieceOnTarget) == Board.friendlyColor) continue;

            bool isCapture = !Piece.IsNone(pieceOnTarget);

            if ((isCapture || !genOnlyCaptures) && (!inCheck || SquareIsInCheckRay(targetSquare))) moves[moveCount++] = new Move(startSquare, targetSquare);
        }
    }


    private static void AddPromotionMoves(ref Span<Move> moves, int startSquare, int targetSquare)
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
    private static bool IsPinned(int square)
    {
        return BitBoardHelper.ContainsSquare(pinRayBitMap, square);
        //return (pinRayBitMap & (1UL << square)) != 0;
    }

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsMovingAlongRay(int startSquare, int targetSquare, int directionOffset)
    {
        int rayDirection = PrecomputedData.directionLookup[targetSquare - startSquare + 63];
        return directionOffset == rayDirection || -directionOffset == rayDirection;
    }

    private static bool SquareIsInCheckRay(int square)
    {
        return BitBoardHelper.ContainsSquare(checkRayBitMap, square);
        //return (checkRayBitMap & (1UL << square)) != 0; //&& inCheck - Included in SebLague's code but don't see why it would be necessary as the bitmaps are reset when we start generating moves
    }

    private static bool SquareIsAttacked(int square)
    {
        return BitBoardHelper.ContainsSquare(opponentAttackMap, square);
    }

    private static bool ShortCastleAllowed()
    {
        return (Board.currentGameState & (1U << (9 + Board.friendlyColorBit))) > 0;
    }

    private static bool LongCastleAllowed()
    {
        return (Board.currentGameState & (1U << (11 + Board.friendlyColorBit))) > 0;
    }

    private static bool InCheckAfterEnPassant(int square, int startRank, int capturedPawnSquare)
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

            int pieceOnSquare = Board.Squares[index];

            if (pieceOnSquare != Piece.None && index != capturedPawnSquare && index != square)
            {
                if (Piece.Color(pieceOnSquare) == Board.enemyColor) return Piece.IsRookOrQueen(pieceOnSquare); //Will put us in check if it's a rook or a queen
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

    public static Move nullMove = new Move(0, 0);

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