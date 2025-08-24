using UnityEngine;

public static class BitBoardHelper
{
    public static bool ContainsSquare(ulong bitboard, int square)
    {
        return (bitboard & (1UL << square)) != 0;
    }

    public static void AddSquare(ref ulong bitboard, int square)
    {
        bitboard |= 1UL << square;
    }
}