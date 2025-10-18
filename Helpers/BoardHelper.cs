using System;
using System.Runtime.CompilerServices;

public static class BoardHelper
{
    public const string fileNames = "abcdefgh";
    public const string rankNames = "12345678";

    public const int a1 = 0;
    public const int b1 = 1;
    public const int c1 = 2;
    public const int d1 = 3;
    public const int e1 = 4;
    public const int f1 = 5;
    public const int g1 = 6;
    public const int h1 = 7;

    public const int a8 = 56;
    public const int b8 = 57;
    public const int c8 = 58;
    public const int d8 = 59;
    public const int e8 = 60;
    public const int f8 = 61;
    public const int g8 = 62;
    public const int h8 = 63;


    public static Move GetMoveFromUCIName(Board board, string moveName)
    {
        int startSquare = IndexFromString(moveName.Substring(0, 2));
        int targetSquare = IndexFromString(moveName.Substring(2, 2));

        int movedPieceType = Piece.Type(board.Squares[startSquare]);
        int startRank = IndexToRank(startSquare);
        int startFile = IndexToFile(startSquare);
        int targetRank = IndexToRank(targetSquare);
        int targetFile = IndexToFile(targetSquare);

        // Figure out move flag
        int flag = Move.Flag.None;

        if (movedPieceType == Piece.Pawn)
        {
            // Promotion
            if (moveName.Length > 4)
            {
                flag = moveName[^1] switch
                {
                    'q' => Move.Flag.PromoteToQueen,
                    'r' => Move.Flag.PromoteToRook,
                    'n' => Move.Flag.PromoteToKnight,
                    'b' => Move.Flag.PromoteToBishop,
                    _ => Move.Flag.None
                };
            }
            // Double pawn push
            else if (Math.Abs(targetRank - startRank) == 2)
            {
                flag = Move.Flag.PawnTwoForward;
            }
            // En-passant
            else if (startFile != targetFile && board.Squares[targetSquare] == Piece.None)
            {
                flag = Move.Flag.EnPassantCapture;
            }
        }
        else if (movedPieceType == Piece.King)
        {
            if (Math.Abs(startFile - targetFile) > 1)
            {
                flag = Move.Flag.Castling;
            }
        }

        return new Move(startSquare, targetSquare, flag);
    }

    /// <summary>
    /// Get algebraic name of move (with promotion specified)
    /// Examples: "e2e4", "e7e8q"
    /// </summary>
    public static string GetMoveNameUCI(Move move)
    {
        string startSquareName = SquareNameFromIndex(move.startSquare);
        string endSquareName = SquareNameFromIndex(move.targetSquare);
        string moveName = startSquareName + endSquareName;
        if (move.IsPromotion())
        {
            switch (move.flag)
            {
                case Move.Flag.PromoteToRook:
                    moveName += 'r';
                    break;
                case Move.Flag.PromoteToKnight:
                    moveName += 'n';
                    break;
                case Move.Flag.PromoteToBishop:
                    moveName += 'b';
                    break;
                case Move.Flag.PromoteToQueen:
                    moveName += 'q';
                    break;
            }
        }
        return moveName;
    }




    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string SquareNameFromCoord(int file, int rank)
    {
        return fileNames[file] + "" + rankNames[rank];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string SquareNameFromIndex(int index)
    {
        return fileNames[IndexToFile(index)] + "" + rankNames[IndexToRank(index)];
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CoordToIndex(int file, int rank)
    {
        return rank * 8 + file;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexToFile(int index)
    {
        return index % 8;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexToRank(int index)
    {
        return index / 8;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FlipIndex(int i)
    {
        return CoordToIndex(IndexToFile(i), 7 - IndexToRank(i));
    }

    public static int IndexFromString(string s)
    {
        return CoordToIndex(FileFromString(s), RankFromString(s));
    }

    public static int FileFromString(string s)
    {
        char fileChar = s[0];

        for (int i = 0; i < 8; i++)
        {
            if (fileNames[i] == fileChar) return i;
        }

        return -1;
    }

    public static int RankFromString(string s)
    {
        char rankChar = s[1];

        for (int i = 0; i < 8; i++)
        {
            if (rankNames[i] == rankChar) return i;
        }

        return -1;
    }
}