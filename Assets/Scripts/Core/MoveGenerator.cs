using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class MoveGenerator
{
    public enum PromotionMode { All, KnightAndQueen };
    public static PromotionMode promotionMode = PromotionMode.All;


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

        /*TODO: if (board.queens[opponentColourIndex].Count == 0) {
            startDirIndex = (board.rooks[opponentColourIndex].Count > 0) ? 0 : 4;
            endDirIndex = (board.bishops[opponentColourIndex].Count > 0) ? 8 : 4;
        }*/


        //loop over all possible attack directions from friendly king
        for (int directionIndex = 0; directionIndex < 8; directionIndex++)
        {
            bool isDiagonal = directionIndex > 3;
            bool friendlyPieceAlongRay = false;
            ulong currentRayBitMap = 0;


            for (int n = 0; n < PrecomputedData.NumSquaresToEdge[friendlyKingSquare][directionIndex]; n++)
            {
                int square = friendlyKingSquare + PrecomputedData.DirectionOffsets[directionIndex] * (n + 1);
                BitBoardHelper.AddSquare(ref currentRayBitMap, square);
                int piece = Board.Squares[square];

                if (piece != Piece.None)
                {
                    if (Piece.ColorBit(piece) == Board.friendlyColorBit) //If friendly piece
                    {
                        if (!friendlyPieceAlongRay) friendlyPieceAlongRay = true; //First in this direction so possibly pinned
                        else break; //Second friendly piece = no pin
                    }
                    else //if its an enemy piece
                    {
                        if ((Piece.IsBishopOrQueen(piece) && isDiagonal) || (Piece.IsRookOrQueen(piece) && !isDiagonal))
                        {

                            if (friendlyPieceAlongRay)
                            {
                                //Piece is pinned
                                pinRayBitMap |= currentRayBitMap;
                            }
                            else
                            {
                                //No piece blocking so were in check
                                checkRayBitMap |= currentRayBitMap;
                                inDoubleCheck = inCheck;
                                inCheck = true;
                            }

                            break;
                        }
                        else break; //Enemy piece here is blocking the check and not checking the king
                    }
                }
            }

            if (inDoubleCheck) break;
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
                BitBoardHelper.AddSquare(ref checkRayBitMap, startSquare);
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
                BitBoardHelper.AddSquare(ref checkRayBitMap, startSquare);
            }
        }

        opponentAttackMapNoPawns = opponentSlidingAttackMap | opponentKnightAttackMap | PrecomputedData.kingAttackBitboards[enemyKingSquare];
        opponentAttackMap = opponentAttackMapNoPawns | oponnentPawnAttackMap;
    }

    private static void GenerateSlidingAttackMap()
    {
        opponentSlidingAttackMap = 0;

        PieceList enemyRooks = Board.rookList[Board.opponentColorBit];
        for (int i = 0; i < enemyRooks.Count; i++)
        {
            UpdateSlidingAttackPiece(enemyRooks[i], 0, 4);
        }

        PieceList enemyQueens = Board.queenList[Board.opponentColorBit];
        for (int i = 0; i < enemyQueens.Count; i++)
        {
            UpdateSlidingAttackPiece(enemyQueens[i], 0, 8);
        }

        PieceList enemyBishops = Board.bishopList[Board.opponentColorBit];
        for (int i = 0; i < enemyBishops.Count; i++)
        {
            UpdateSlidingAttackPiece(enemyBishops[i], 4, 8);
        }
    }

    private static void UpdateSlidingAttackPiece(int startSquare, int startDirIndex, int endDirIndex)
    {

        for (int directionIndex = startDirIndex; directionIndex < endDirIndex; directionIndex++)
        {
            int currentDirOffset = PrecomputedData.DirectionOffsets[directionIndex];
            for (int n = 0; n < PrecomputedData.NumSquaresToEdge[startSquare][directionIndex]; n++)
            {
                int targetSquare = startSquare + currentDirOffset * (n + 1);
                int targetSquarePiece = Board.Squares[targetSquare];
                opponentSlidingAttackMap |= 1ul << targetSquare;
                if (targetSquare != friendlyKingSquare) //From seb lague - don't have any idea why we need to continue the ray through the king? - Found out why
                {
                    if (targetSquarePiece != Piece.None)
                    {
                        break;
                    }
                }
            }
        }
    }

    #endregion



    private static List<Move> moves;

    #region MoveGeneration

    public static List<Move> GenerateMoves()
    {
        moves = new List<Move>();

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

        GenerateKingMoves();

        if (inDoubleCheck) return moves; //Only king moves valid when in double check

        for (int i = 0; i < Board.pawnList[Board.friendlyColorBit].Count; i++)
        {
            GeneratePawnMoves(Board.pawnList[Board.friendlyColorBit][i]);
        }

        for (int i = 0; i < Board.knightList[Board.friendlyColorBit].Count; i++)
        {
            GenerateKnightMoves(Board.knightList[Board.friendlyColorBit][i]);
        }

        for (int i = 0; i < Board.bishopList[Board.friendlyColorBit].Count; i++)
        {
            GenerateSlidingMoves(Board.bishopList[Board.friendlyColorBit][i], Piece.Bishop);
        }

        for (int i = 0; i < Board.rookList[Board.friendlyColorBit].Count; i++)
        {
            GenerateSlidingMoves(Board.rookList[Board.friendlyColorBit][i], Piece.Rook);
        }

        for (int i = 0; i < Board.queenList[Board.friendlyColorBit].Count; i++)
        {
            GenerateSlidingMoves(Board.queenList[Board.friendlyColorBit][i], Piece.Queen);
        }


        return moves;
    }

    /*private static List<Move> TempGenPseudoLegalMoves()
    {
        moves = new List<Move>();
        int colorToMoveBit = Piece.ColorBit(Board.colorToMove);

        for (int i = 0; i < Board.pawnList[colorToMoveBit].Count; i++)
        {
            GeneratePawnMoves(Board.pawnList[colorToMoveBit][i]);
        }

        for (int i = 0; i < Board.knightList[colorToMoveBit].Count; i++)
        {
            GenerateKnightMoves(Board.knightList[colorToMoveBit][i]);
        }

        for (int i = 0; i < Board.bishopList[colorToMoveBit].Count; i++)
        {
            GenerateSlidingMoves(Board.bishopList[colorToMoveBit][i], Piece.Bishop);
        }

        for (int i = 0; i < Board.rookList[colorToMoveBit].Count; i++)
        {
            GenerateSlidingMoves(Board.rookList[colorToMoveBit][i], Piece.Rook);
        }

        for (int i = 0; i < Board.queenList[colorToMoveBit].Count; i++)
        {
            GenerateSlidingMoves(Board.queenList[colorToMoveBit][i], Piece.Queen);
        }

        GenerateKingMoves();

        return moves;
    }*/

    private static void GenerateSlidingMoves(int startSquare, int piece)
    {
        //Debug.Log(BoardHelper.SquareNameFromIndex(startSquare));
        //Debug.Log("Sliding Piece: " + Convert.ToString(piece, 2));

        bool isPinned = IsPinned(startSquare);

        if (inCheck && isPinned)
        {
            //Piece cannot move
            //Prob check if this if statement is even worth it performance wise as this is probably a pretty rare situation i think
            return;
        }


        int startDirIndex = (Piece.Type(piece) == Piece.Bishop) ? 4 : 0;
        int endDirIndex = (Piece.Type(piece) == Piece.Rook) ? 4 : 8;

        for (int directionIndex = startDirIndex; directionIndex < endDirIndex; directionIndex++)
        {
            int directionOffset = PrecomputedData.DirectionOffsets[directionIndex];

            //If we are pinned we can only move along the pinray - the pinray goes from the kings square to the square the pinned piece is on, before continuing ofc
            if (isPinned && !IsMovingAlongRay(friendlyKingSquare, startSquare, directionOffset))
            {
                continue; //TODOnt: could prob be slightly more beneficial to just directly calculate the two directions we could possibly move in if were pinned. We would avoid having to check all 8 directions for a pinned queen fx who is always only able to move in 2 directions when pinned (i think)
                          //Honestly dont think this would be benefitial with the extra overhead compared to how little overhead checking this if statement for 4 times as many directions is
            }


            for (int n = 0; n < PrecomputedData.NumSquaresToEdge[startSquare][directionIndex]; n++)
            {
                int targetSquare = startSquare + directionOffset * (n + 1);
                int pieceOnTarget = Board.Squares[targetSquare];

                if (Piece.Color(pieceOnTarget) == Board.friendlyColor) break;


                bool isCapture = pieceOnTarget != Piece.None;
                bool preventsCheck = SquareIsInCheckRay(targetSquare);

                if (!inCheck || preventsCheck) //If were not in check, or if this move prevents the check
                {
                    //TODO: think about implementing quiet moves    if (isCapture || genQuiets)
                    moves.Add(new Move(startSquare, targetSquare));
                }

                if (isCapture || preventsCheck) break; //If we hit an enemy piece we cant move further in this direction, if we can block the check on this square, we def wont be able to on any following squares
            }
        }
    }

    private static void GenerateKingMoves()
    {
        for (int i = 0; i < PrecomputedData.KingMoves[friendlyKingSquare].Length; i++)
        {
            int targetSquare = PrecomputedData.KingMoves[friendlyKingSquare][i];
            int pieceOnTarget = Board.Squares[targetSquare];

            if (Piece.Color(pieceOnTarget) == Board.friendlyColor) continue;

            if (!SquareIsAttacked(targetSquare))
            {
                moves.Add(new Move(friendlyKingSquare, targetSquare));

                bool isCapture = pieceOnTarget != Piece.None;

                //Castling
                if (!inCheck && !isCapture)
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
                                moves.Add(new Move(friendlyKingSquare, castleKingsideSquare, Move.Flag.Castling));
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
                                moves.Add(new Move(friendlyKingSquare, castleQueensideSquare, Move.Flag.Castling));
                            }
                        }
                    }
                }
            }
        }
    }

    private static void GeneratePawnMoves(int startSquare)
    {
        int moveDir = Board.friendlyColor == Piece.White ? PrecomputedData.Up : PrecomputedData.Down;

        int targetSquare = startSquare + moveDir; //One move up/down
        int rank = BoardHelper.IndexToRank(startSquare);

        int startRank = Board.friendlyColor == Piece.White ? 1 : 6;

        bool oneStepFromPromotion = rank == (Board.friendlyColor == Piece.White ? 6 : 1);


        if (Piece.IsNone(Board.Squares[targetSquare]))
        {
            if (!IsPinned(startSquare) || IsMovingAlongRay(friendlyKingSquare, startSquare, moveDir))
            {
                if (!inCheck || SquareIsInCheckRay(targetSquare))
                {
                    if (oneStepFromPromotion) AddPromotionMoves(startSquare, targetSquare);
                    else moves.Add(new Move(startSquare, targetSquare));
                }



                if (rank == startRank) //If on start rank
                {
                    int squareTwoForward = targetSquare + moveDir; //One additional move up/down

                    if (Piece.IsNone(Board.Squares[squareTwoForward]) && (!inCheck || SquareIsInCheckRay(squareTwoForward))) moves.Add(new Move(startSquare, squareTwoForward, Move.Flag.PawnTwoForward)); //If no pieces on target square, add move
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

            if (Piece.Color(targetPiece) == Board.enemyColor) //moves.Add(new Move(startSquare, targetSquare)); //If enemy piece on attack square
            {
                if (inCheck && !SquareIsInCheckRay(targetSquare)) continue; //Skip direction if were in check and this move doesn't block it

                if (oneStepFromPromotion) AddPromotionMoves(startSquare, targetSquare);
                else moves.Add(new Move(startSquare, targetSquare));
            }

            //En passant
            if (targetSquare == epAttackSquare)
            {
                //Debug.Log("ep possible");
                int capturedPawnSquare = targetSquare - moveDir;

                if (inCheck && !SquareIsInCheckRay(targetSquare) && !SquareIsInCheckRay(capturedPawnSquare)) continue;

                int epStartRank = Board.friendlyColor == Piece.White ? 4 : 3;


                if (!InCheckAfterEnPassant(startSquare, epStartRank, capturedPawnSquare)) moves.Add(new Move(startSquare, targetSquare, Move.Flag.EnPassantCapture));
            }
        }
    }

    private static void GenerateKnightMoves(int startSquare)
    {
        for (int i = 0; i < PrecomputedData.KnightMoves[startSquare].Length; i++)
        {
            if (IsPinned(startSquare)) return; //Knight cant move at all if pinned

            int targetSquare = PrecomputedData.KnightMoves[startSquare][i];
            int pieceOnTarget = Board.Squares[targetSquare];

            if (Piece.Color(pieceOnTarget) == Board.friendlyColor) continue;


            if (!inCheck || SquareIsInCheckRay(targetSquare)) moves.Add(new Move(startSquare, targetSquare));
        }
    }


    private static void AddPromotionMoves(int startSquare, int targetSquare)
    {
        moves.Add(new Move(startSquare, targetSquare, Move.Flag.PromoteToQueen));
        moves.Add(new Move(startSquare, targetSquare, Move.Flag.PromoteToKnight));

        if (promotionMode == PromotionMode.KnightAndQueen) return;

        moves.Add(new Move(startSquare, targetSquare, Move.Flag.PromoteToRook));
        moves.Add(new Move(startSquare, targetSquare, Move.Flag.PromoteToBishop));
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