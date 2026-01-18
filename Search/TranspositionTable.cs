
using System;
using System.Numerics;
using System.Runtime.CompilerServices;

public class TranspositionTable
{
    public static int SizeMB = 16; //TODO: adjust based on game speed - 16mb is WAY too small for 10s think


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSuccessfulLookup(int eval) { return eval > DepthFailed; }

    public const int DepthFailed = int.MinValue + 1;
    public const int LookupFailed = int.MinValue;

    // The value for this position is the exact evaluation
    public const ulong Exact = 0;
    // A move was found during the search that was too good, meaning the opponent will play a different move earlier on,
    // not allowing the position where this move was available to be reached. Because the search cuts off at
    // this point (beta cut-off), an even better move may exist. This means that the evaluation for the
    // position could be even higher, making the stored value the lower bound of the actual value.
    public const ulong LowerBound = 1;
    // No move during the search resulted in a position that was better than the current player could get from playing a
    // different move in an earlier position (i.e eval was <= alpha for all moves in the position).
    // Due to the way alpha-beta search works, the value we get here won't be the exact evaluation of the position,
    // but rather the upper bound of the evaluation. This means that the evaluation is, at most, equal to this value.
    public const ulong UpperBound = 2;


    //TODO: Try with Memory<T> - makes resizing easier? Don't really see how it would be faster tbh
    private Transposition[] table;
    private ulong entryCount;
    //private ulong writeCount = 0;
    private readonly ulong indexMask;

    //TODOne: https://www.chessprogramming.org/Transposition_Table try power of two table - ask chat - can use mask instead of modulo for index
    public TranspositionTable()
    {
        entryCount = (ulong)(SizeMB * 1024 * 1024 / Transposition.GetSize());

        if (!BitOperations.IsPow2(entryCount)) throw new NotSupportedException("Table size is not power of two");

        indexMask = entryCount - 1;

        table = new Transposition[entryCount];
    }

    public float GetFilledPercent()
    {
        int entries = 0;

        for (int i = 0; i < table.Length; i++)
        {
            if (table[i].key != 0) entries++;
        }

        return (float)entries / entryCount * 100f;
    }

    //public float GetTest() { return (float)writeCount / entryCount * 100f; }

    //TODO: Aggressive inlining
    private ulong Index(ulong zobrist)
    {
        //zobrist ^= zobrist >> 32; //Mix lower bits with upper for more randomness

        //zobrist *= 0xBF58476D1CE4E5B9UL;

        return zobrist & indexMask;
    }


    public void Clear()
    {
        for (int i = 0; i < table.Length; i++)
        {
            table[i] = new Transposition();
        }
    }


    public Move GetStoredMove(ulong zobrist)
    {
        //TODO: Should prob store whole data ulong we get from lookupevaluation in search to avoid having to do a secondary lookup for just to get the move data - could also have been changed/(corrupted? but 16 bits?)
        //Prob not bc getstoredmove is almost never called
        return table[Index(zobrist)].move; //TODOnt anymore: Could just return whole entry to avoid having to lock twice waiting for both eval and move if eval is good
    }

    public int LookupEvaluation(ulong zobrist, uint depth, int plyFromRoot, int alpha, int beta)
    {
        ulong index = Index(zobrist);

        Transposition transposition = table[index];
        //TODO: Test maybe with waiting a tiny bit here if key and data is corrupted instead of just failing the probe?

        //if ((transposition.key ^ (uint)transposition.data) == (uint)zobrist) //TODO: guarantee atomic for SMP
        if (transposition.key == zobrist >> 48)
        {
            // Only use stored evaluation if it has been searched to at least the same depth as would be searched now
            if (transposition.depth >= depth)
            {
                int correctedScore = CorrectRetrievedMateScore(transposition.eval, plyFromRoot);
                // We have stored the exact evaluation for this position, so return it
                if (transposition.nodeType == Exact) //TODO: test with caching nodetype maybe?
                {
                    return correctedScore;
                }
                // We have stored the upper bound of the eval for this position. If it's less than alpha then we don't need to
                // search the moves in this position as they won't interest us; otherwise we will have to search to find the exact value
                if (transposition.nodeType == UpperBound && correctedScore <= alpha)//TODO: test with caching nodetype maybe?
                {
                    return correctedScore;
                }
                // We have stored the lower bound of the eval for this position. Only return if it causes a beta cut-off.
                if (transposition.nodeType == LowerBound && correctedScore >= beta)//TODO: test with caching nodetype maybe?
                {
                    return correctedScore;
                }
            }

            return DepthFailed;
        }

        return LookupFailed;
    }



    public void StoreEvaluation(ulong zobrist, uint depth, int numPlySearched, int eval, ulong evalType, Move move) //TODO: Replacement strategies - if spot already filled we have to figure out whether to replace the entry or not-https://www.chessprogramming.org/Transposition_Table#Table_Entry_Types
    {
        //ulong index = Index(zobrist);

        //Transposition currentEntry = table[index];

        //TODO: Try with >= bc ig would just save having to write to the table unnecessarily
        //TODO: Test swapping the two cases --- currentEntry.depth > depth && (currentEntry.key ^ currentEntry.data) == zobrist --- bc maybe first one is more likely

        //Actually seems this is entirely wasted work bc were guarantueed to only get to store eval if there already wasnt an entry at deeper depth. We should also account for nodetypes bc cutoff nodes should also be overriden?
        //if ((currentEntry.key ^ currentEntry.data) == zobrist && currentEntry.depth > depth) return; //If the entry we are trying to override contains info about the same position at a deeper depth, we don't want to override it

        //TODO: Test perf difference with reassigning fields individually and just using new
        //currentEntry = new Transposition(zobrist, CorrectMateScoreForStorage(eval, numPlySearched), (byte)depth, (byte)evalType, move); //TODO: try changing casts/removing?

        //table[index] = currentEntry;

        //TODO: remove
        if (Math.Abs(eval) > short.MaxValue)
        {
            Console.WriteLine("Eval " + eval + " is outside of short bounds");
        }

        table[Index(zobrist)] = new Transposition(zobrist, CorrectMateScoreForStorage(eval, numPlySearched), (byte)depth, (byte)evalType, move);
    }

    short CorrectMateScoreForStorage(int score, int numPlySearched)
    {
        if (Search.IsMateScore(score))
        {
            int sign = Math.Sign(score);
            return (short)((score * sign + numPlySearched) * sign);
        }
        return (short)score;
    }

    int CorrectRetrievedMateScore(short score, int numPlySearched)
    {
        if (Search.IsMateScore(score))
        {
            int sign = Math.Sign(score);
            return (score * sign - numPlySearched) * sign;
        }
        return score;
    }





    //TODO: Try with [StructLayout(LayoutKind.Explicit, Size = 8)] - shouldn't make a difference bc should already be the case
    public struct Transposition
    {
        public readonly ulong data;

        private const ulong keyMask = 0b0000000000000000000000000000000000000000000000001111111111111111;
        private const ulong evalMask = 0b0000000000000000000000000000000011111111111111110000000000000000;
        private const ulong depthMask = 0b0000000000000000000000000011111100000000000000000000000000000000;
        private const ulong nodeTypeMask = 0b0000000000000000000000001100000000000000000000000000000000000000;
        private const ulong moveMask = 0b0000000011111111111111110000000000000000000000000000000000000000;

        public ulong key { get { return data & keyMask; } }
        public short eval { get { return (short)((data & evalMask) >> 16); } }
        public byte depth { get { return (byte)((data & depthMask) >> 32); } }
        public ulong nodeType { get { return (data & nodeTypeMask) >> 38; } }
        public Move move { get { return new Move((ushort)((data & moveMask) >> 40)); } } //TODO: Try without move entirely - not necessary unless at root - just maintain PV instead

        public Transposition(ulong zobrist, short eval, byte depth, byte nodeType, Move move) //TODO: make all ulong here in params?
        {
            //TODO: test if depth is bigger than 6 bits - prob no need to check if depth is above 63 as we would prob be winning anyway
            //TODO: Maybe try testing if eval is larger than 16 bits as this would provide very wrong answers if ever the case

            ulong key = zobrist >> 48; //Bc we are using PO2 TT we already now that fx the first 26 bits match for a 64mb table so we use the last 16 bits as the key since the last bits are the ones that will actually differ if the position is different

            data = key | ((((ulong)eval) << 16) & evalMask) | ((((ulong)depth) << 32) & depthMask) | ((((ulong)nodeType) << 38) & nodeTypeMask) | ((((ulong)move.data) << 40) & moveMask);


            //data = (uint)eval | (((ulong)move.data) << 32) | (((ulong)depth) << 48) | (((ulong)nodeType) << 56);

            //key = zobrist ^ data;
        }

        public static int GetSize()
        {
            return System.Runtime.InteropServices.Marshal.SizeOf<Transposition>();
        }
    }
}