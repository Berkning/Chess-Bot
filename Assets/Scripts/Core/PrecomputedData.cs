using System.Collections.Generic;
using UnityEngine;

public static class PrecomputedData
{
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

    public static readonly int[] directionLookup = new int[127];
    public static readonly ulong[][] directionalMasks = new ulong[64][]; //King Square , Piece Square

    public static readonly int[][] kingDistanceLookup = new int[64][];


    //Mopup
    public static readonly int[] manhattanDistanceFromCenter = new int[64];
    //public static readonly int[] 


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
                    Mathf.Min(numUp, numLeft),
                    Mathf.Min(numDown, numRight),
                    Mathf.Min(numUp, numRight),
                    Mathf.Min(numDown, numLeft)
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

                    if (Mathf.Abs(targetFile - file) > 2) continue; //Detects whether the move wrapped around the board

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

                    if (Mathf.Abs(targetFile - file) > 1) continue; //Detects if move wraps around the board

                    kingMoves.Add(targetSquare);
                    BitBoardHelper.AddSquare(ref kingAttackBitboard, targetSquare);
                }

                KingMoves[squareIndex] = kingMoves.ToArray();
                kingAttackBitboards[squareIndex] = kingAttackBitboard;







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
                //TODO: Could theoretically be optimized since we only need a direction, and not the actual square the piece is on
                directionalMasks[squareIndex] = new ulong[64]; //King is on squareIndex
                for (int pieceSquare = 0; pieceSquare < 64; pieceSquare++)
                {
                    //if (squareIndex == pieceSquare) continue; //If king- and pieceSquare are the same we skip this square

                    int direction = directionLookup[pieceSquare - squareIndex + 63];
                    ulong mask = 0;

                    for (int i = 0; i < 8; i++) //Start at king square and move in the direction of the piece
                    {
                        int targetSquare = squareIndex + direction * i;
                        if (targetSquare < 64 && targetSquare >= 0) mask = BitBoardHelper.AddSquare(mask, targetSquare);
                    }

                    directionalMasks[squareIndex][pieceSquare] = mask;
                }



                //Distance Lookup
                int fileDstFromCentre = Mathf.Max(3 - file, file - 4);
                int rankDstFromCentre = Mathf.Max(3 - rank, rank - 4);
                manhattanDistanceFromCenter[squareIndex] = fileDstFromCentre + rankDstFromCentre;

                //King Distance Lookup
                kingDistanceLookup[squareIndex] = new int[64];

                for (int targetRank = 0; targetRank < 8; targetRank++)
                {
                    for (int targetFile = 0; targetFile < 8; targetFile++)
                    {
                        int targetSquare = BoardHelper.CoordToIndex(targetFile, targetRank);
                        int chebyshevDist = Mathf.Max(Mathf.Abs(targetFile - file), Mathf.Abs(targetRank - rank));

                        kingDistanceLookup[squareIndex][targetSquare] = chebyshevDist;
                    }
                }
            }
        }
    }
}