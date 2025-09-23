using System;
using System.Collections.Generic;

public static class PrecomputedData
{
    //TODO: check if 2d arrays are more performant than array of array
    // First 4 are orthogonal, last 4 are diagonals (Up, Down, Left, Right, UpRight, DownLeft, UpLeft, DownRight)
    public static readonly int[] DirectionOffsets = { 8, -8, -1, 1, 7, -7, 9, -9 };
    public static readonly int[][] NumSquaresToEdge = new int[64][];

    public static readonly int[][] PawnAttackSquares = new int[128][]; //0-63 for white, 64-127 for black
    public static readonly ulong[] pawnAttackBitboards = new ulong[128]; //0-63 for white, 64-127 for black

    public static readonly int[][] KnightMoves = new int[64][];
    public static readonly ulong[] knightAttackBitboards = new ulong[64];

    private static readonly int[] allKnightJumps = { 15, 17, -17, -15, 10, -6, 6, -10 };

    public static readonly int[][] KingMoves = new int[64][];
    public static readonly ulong[] kingAttackBitboards = new ulong[64];
    public static readonly ulong[] castleMasks; //0 wShort, 1 bShort, 2 wLong, 3 bLong, 4 wLongExtraSquare, 5 bLongExtraSquare

    public static readonly int[] directionLookup = new int[127];
    public static readonly ulong[][] directionalMasks = new ulong[64][]; //King Square , Piece Square

    public static readonly int[][] kingDistanceLookup = new int[64][];


    //Mopup
    public static readonly int[] manhattanDistanceFromCenter = new int[64];
    //public static readonly int[] 

    //Pawn Masks
    public static readonly ulong[] fileMasks = new ulong[8];
    public static readonly ulong[] passedPawnMasks = new ulong[128];

    //Square Colors
    public const ulong DarkSquareMask = 0b1010101001010101101010100101010110101010010101011010101001010101; //Remember these masks are reverse order - bit furthest to right is square 0
    public const ulong LightSquareMask = ~DarkSquareMask;


    public const int Up = 8;
    public const int Down = -8;
    public const int Left = -1;
    public const int Right = 1;
    public const int UpLeft = 7;
    public const int DownRight = -7;
    public const int UpRight = 9;
    public const int DownLeft = -9;


    static PrecomputedData()
    {
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                int numUp = 7 - rank;
                int numDown = rank;
                int numLeft = file;
                int numRight = 7 - file;

                int squareIndex = BoardHelper.CoordToIndex(file, rank);

                NumSquaresToEdge[squareIndex] = new int[8]{
                    numUp,
                    numDown,
                    numLeft,
                    numRight,
                    Math.Min(numUp, numLeft),
                    Math.Min(numDown, numRight),
                    Math.Min(numUp, numRight),
                    Math.Min(numDown, numLeft)
                };


                //Pawn Attacks
                if (numLeft == 0)
                {
                    BitBoardHelper.AddSquare(ref pawnAttackBitboards[squareIndex], squareIndex + UpRight);
                    PawnAttackSquares[squareIndex] = new int[1] { squareIndex + UpRight }; //White
                    BitBoardHelper.AddSquare(ref pawnAttackBitboards[squareIndex + 64], squareIndex + DownRight);
                    PawnAttackSquares[squareIndex + 64] = new int[1] { squareIndex + DownRight }; //Black
                }
                else if (numRight == 0)
                {
                    BitBoardHelper.AddSquare(ref pawnAttackBitboards[squareIndex], squareIndex + UpLeft);
                    PawnAttackSquares[squareIndex] = new int[1] { squareIndex + UpLeft }; //White
                    BitBoardHelper.AddSquare(ref pawnAttackBitboards[squareIndex + 64], squareIndex + DownLeft);
                    PawnAttackSquares[squareIndex + 64] = new int[1] { squareIndex + DownLeft }; //Black
                }
                else
                {
                    BitBoardHelper.AddSquare(ref pawnAttackBitboards[squareIndex], squareIndex + UpRight);
                    BitBoardHelper.AddSquare(ref pawnAttackBitboards[squareIndex], squareIndex + UpLeft);
                    PawnAttackSquares[squareIndex] = new int[2] { squareIndex + UpRight, squareIndex + UpLeft }; //White

                    BitBoardHelper.AddSquare(ref pawnAttackBitboards[squareIndex + 64], squareIndex + DownRight);
                    BitBoardHelper.AddSquare(ref pawnAttackBitboards[squareIndex + 64], squareIndex + DownLeft);
                    PawnAttackSquares[squareIndex + 64] = new int[2] { squareIndex + DownRight, squareIndex + DownLeft }; //Black
                }


                //Knight Moves
                List<int> knightMoves = new List<int>(); //Really bad for gc and performance but precomputed so doesn't matter
                ulong knightAttackBitboard = 0;

                foreach (int jump in allKnightJumps)
                {
                    int targetSquare = squareIndex + jump;

                    if (targetSquare < 0 || targetSquare > 63) continue;

                    int targetFile = BoardHelper.IndexToFile(targetSquare);

                    if (Math.Abs(targetFile - file) > 2) continue; //Detects whether the move wrapped around the board

                    knightMoves.Add(targetSquare);
                    BitBoardHelper.AddSquare(ref knightAttackBitboard, targetSquare);
                }

                KnightMoves[squareIndex] = knightMoves.ToArray();
                knightAttackBitboards[squareIndex] = knightAttackBitboard;





                //King Moves
                List<int> kingMoves = new List<int>();
                ulong kingAttackBitboard = 0;

                for (int directionIndex = 0; directionIndex < 8; directionIndex++)
                {
                    int targetSquare = squareIndex + DirectionOffsets[directionIndex];

                    if (targetSquare < 0 || targetSquare > 63) continue;

                    int targetFile = BoardHelper.IndexToFile(targetSquare);

                    if (Math.Abs(targetFile - file) > 1) continue; //Detects if move wraps around the board

                    kingMoves.Add(targetSquare);
                    BitBoardHelper.AddSquare(ref kingAttackBitboard, targetSquare);
                }

                KingMoves[squareIndex] = kingMoves.ToArray();
                kingAttackBitboards[squareIndex] = kingAttackBitboard;

                castleMasks = new ulong[6];
                //Short
                castleMasks[0] = (1UL << BoardHelper.f1) | (1UL << BoardHelper.g1);
                castleMasks[1] = (1UL << BoardHelper.f8) | (1UL << BoardHelper.g8);
                //Long
                castleMasks[2] = (1UL << BoardHelper.d1) | (1UL << BoardHelper.c1);
                castleMasks[3] = (1UL << BoardHelper.d8) | (1UL << BoardHelper.c8);
                //Extra
                castleMasks[4] = 1UL << BoardHelper.b1;
                castleMasks[5] = 1UL << BoardHelper.b8;







                //DirectionLookup //TODO: Move this out of all for loops asap bc literally doing this over on every square for no reason
                for (int i = 0; i < 127; i++) //pieceSquare - friendlyKingSquare + 63
                {
                    int offset = i - 63;
                    int absOffset = System.Math.Abs(offset); //TODO: Dont think we need this - can just use offset i think
                    int absDir = 1;
                    if (absOffset % 9 == 0)
                    {
                        absDir = 9;
                    }
                    else if (absOffset % 8 == 0)
                    {
                        absDir = 8;
                    }
                    else if (absOffset % 7 == 0)
                    {
                        absDir = 7;
                    }

                    int direction = absDir * System.Math.Sign(offset);
                    directionLookup[i] = direction;
                }

                //Direction Mask Lookup
                //TODOnt: Could theoretically be optimized since we only need a direction, and not the actual square the piece is on
                directionalMasks[squareIndex] = new ulong[64]; //King is on squareIndex
                for (int pieceSquare = 0; pieceSquare < 64; pieceSquare++)
                {
                    //if (squareIndex == pieceSquare) continue; //If king- and pieceSquare are the same we skip this square

                    //int direction = directionLookup[pieceSquare - squareIndex + 63]; //59-60+63 = 62     Can't fucking do this for some reason so have to do it manually ig
                    ulong mask = 0;
                    int cap = 7;


                    int direction;

                    int fileDelta = BoardHelper.IndexToFile(pieceSquare) - BoardHelper.IndexToFile(squareIndex);
                    int rankDelta = BoardHelper.IndexToRank(pieceSquare) - BoardHelper.IndexToRank(squareIndex);

                    if (fileDelta == 0)
                    {
                        direction = Math.Sign(rankDelta) * 8;
                    }
                    else if (rankDelta == 0)
                    {
                        direction = Math.Sign(fileDelta);
                        if (direction == Right) cap = 7 - BoardHelper.IndexToFile(squareIndex);
                        else cap = BoardHelper.IndexToFile(squareIndex);
                    }
                    else if (fileDelta == rankDelta || -fileDelta == rankDelta)
                    {
                        bool up = Math.Sign(rankDelta) > 0;
                        bool right = fileDelta > 0;

                        if (right)
                        {
                            cap = 7 - BoardHelper.IndexToFile(squareIndex);
                        }
                        else cap = BoardHelper.IndexToFile(squareIndex);

                        if (up)
                        {
                            direction = right ? UpRight : UpLeft;
                        }
                        else
                        {
                            direction = right ? DownRight : DownLeft;
                        }
                    }
                    else continue; //Invalid Direction




                    //if (direction == Left || direction == UpLeft || direction == DownLeft) cap = BoardHelper.IndexToFile(squareIndex);
                    //else if (direction == Right || direction == UpRight || direction == DownRight) cap = 7 - BoardHelper.IndexToFile(squareIndex);


                    for (int i = 1; i <= cap; i++) //Start at king square and move in the direction of the piece
                    {
                        int targetSquare = squareIndex + direction * i;
                        if (targetSquare < 64 && targetSquare >= 0) mask = BitBoardHelper.AddSquare(mask, targetSquare);

                        //if (targetSquare == pieceSquare) break;
                    }

                    directionalMasks[squareIndex][pieceSquare] = mask;
                }



                //Distance Lookup
                int fileDstFromCentre = Math.Max(3 - file, file - 4);
                int rankDstFromCentre = Math.Max(3 - rank, rank - 4);
                manhattanDistanceFromCenter[squareIndex] = fileDstFromCentre + rankDstFromCentre;

                //King Distance Lookup
                kingDistanceLookup[squareIndex] = new int[64];

                for (int targetRank = 0; targetRank < 8; targetRank++)
                {
                    for (int targetFile = 0; targetFile < 8; targetFile++)
                    {
                        int targetSquare = BoardHelper.CoordToIndex(targetFile, targetRank);
                        int chebyshevDist = Math.Max(Math.Abs(targetFile - file), Math.Abs(targetRank - rank));

                        kingDistanceLookup[squareIndex][targetSquare] = chebyshevDist;
                    }
                }



                //Pawn masks
                fileMasks[file] = BitBoardHelper.AddSquare(fileMasks[file], squareIndex);

                if (file == 0) //On left edge
                {
                    //White pawn
                    for (int rankIndex = rank + 1; rankIndex < 7; rankIndex++) //Every rank above pawn
                    {
                        passedPawnMasks[squareIndex] = BitBoardHelper.AddSquare(passedPawnMasks[squareIndex], BoardHelper.CoordToIndex(0, rankIndex));
                        passedPawnMasks[squareIndex] = BitBoardHelper.AddSquare(passedPawnMasks[squareIndex], BoardHelper.CoordToIndex(1, rankIndex));
                    }

                    //Black pawn
                    for (int rankIndex = rank - 1; rankIndex > 0; rankIndex--) //Every rank below pawn
                    {
                        passedPawnMasks[squareIndex + 64] = BitBoardHelper.AddSquare(passedPawnMasks[squareIndex + 64], BoardHelper.CoordToIndex(0, rankIndex));
                        passedPawnMasks[squareIndex + 64] = BitBoardHelper.AddSquare(passedPawnMasks[squareIndex + 64], BoardHelper.CoordToIndex(1, rankIndex));
                    }
                }
                else if (file == 7) //On right edge
                {
                    //White pawn
                    for (int rankIndex = rank + 1; rankIndex < 7; rankIndex++) //Every rank above pawn
                    {
                        passedPawnMasks[squareIndex] = BitBoardHelper.AddSquare(passedPawnMasks[squareIndex], BoardHelper.CoordToIndex(6, rankIndex));
                        passedPawnMasks[squareIndex] = BitBoardHelper.AddSquare(passedPawnMasks[squareIndex], BoardHelper.CoordToIndex(7, rankIndex));
                    }

                    //Black pawn
                    for (int rankIndex = rank - 1; rankIndex > 0; rankIndex--) //Every rank below pawn
                    {
                        passedPawnMasks[squareIndex + 64] = BitBoardHelper.AddSquare(passedPawnMasks[squareIndex + 64], BoardHelper.CoordToIndex(6, rankIndex));
                        passedPawnMasks[squareIndex + 64] = BitBoardHelper.AddSquare(passedPawnMasks[squareIndex + 64], BoardHelper.CoordToIndex(7, rankIndex));
                    }
                }
                else //In middle of board
                {
                    //White pawn
                    for (int rankIndex = rank + 1; rankIndex < 7; rankIndex++) //Every rank above pawn
                    {
                        passedPawnMasks[squareIndex] = BitBoardHelper.AddSquare(passedPawnMasks[squareIndex], BoardHelper.CoordToIndex(file - 1, rankIndex));
                        passedPawnMasks[squareIndex] = BitBoardHelper.AddSquare(passedPawnMasks[squareIndex], BoardHelper.CoordToIndex(file, rankIndex));
                        passedPawnMasks[squareIndex] = BitBoardHelper.AddSquare(passedPawnMasks[squareIndex], BoardHelper.CoordToIndex(file + 1, rankIndex));
                    }

                    //Black pawn
                    for (int rankIndex = rank - 1; rankIndex > 0; rankIndex--) //Every rank below pawn
                    {
                        passedPawnMasks[squareIndex + 64] = BitBoardHelper.AddSquare(passedPawnMasks[squareIndex + 64], BoardHelper.CoordToIndex(file - 1, rankIndex));
                        passedPawnMasks[squareIndex + 64] = BitBoardHelper.AddSquare(passedPawnMasks[squareIndex + 64], BoardHelper.CoordToIndex(file, rankIndex));
                        passedPawnMasks[squareIndex + 64] = BitBoardHelper.AddSquare(passedPawnMasks[squareIndex + 64], BoardHelper.CoordToIndex(file + 1, rankIndex));
                    }
                }
            }
        }
    }
}