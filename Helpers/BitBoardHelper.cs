using System.Runtime.CompilerServices;
using System.Numerics;

public static class BitBoardHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsSquare(ulong bitboard, int square)
    {
        return (bitboard & (1UL << square)) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong AddSquare(ulong bitboard, int square)
    {
        bitboard |= 1UL << square;
        return bitboard;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddSquare(ref ulong bitboard, int square)
    {
        bitboard |= 1UL << square;
    }

    public static ulong BitboardFromPieceList(PieceList pieces)
    {
        ulong bitboard = 0;

        for (int i = 0; i < pieces.Count; i++)
        {
            bitboard = AddSquare(bitboard, pieces[i]);
        }

        return bitboard;
    }

    // public static ulong BitboardFromPieceListArray(PieceList[] pieceLists)
    // {
    //     ulong bitboard = 0;

    //     foreach (PieceList list in pieceLists)
    //     {
    //         bitboard |= BitboardFromPieceList(list);
    //     }

    //     return bitboard;
    // }

    // Get index of least significant set bit in given 64bit value. Also clears the bit to zero.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int PopFirstBit(ref ulong board)
    {
        int z = BitOperations.TrailingZeroCount(board);
        board &= board - 1UL;
        return z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int BitCount(ulong board)
    {
        //return math.countbits(board);
        return BitOperations.PopCount(board);
    }
}