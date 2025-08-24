using System;
using System.Runtime.CompilerServices;
using UnityEngine;

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


    public static string NameMove(Move move)
    {
        return SquareNameFromIndex(move.startSquare) + SquareNameFromIndex(move.targetSquare);
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
    public static int IndexFromString(string s)
    {
        return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FileFromString(string s)
    {
        char fileChar = s[0];

        for (int i = 0; i < 8; i++)
        {
            if (fileNames[i] == fileChar) return i;
        }

        return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int RankFromString(string s)
    {
        return -1;
    }
}