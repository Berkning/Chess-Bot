
using System;

public class TranspositionTable
{
    public static int SizeMB = 16; //TODO: adjust based on game speed - 16mb is WAY too small for 10s think

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



    private Transposition[] table;
    private ulong entryCount;

    public TranspositionTable()
    {
        entryCount = (ulong)(SizeMB * 1000 * 1000 / Transposition.GetSize());

        table = new Transposition[entryCount];
    }

    public float GetFilledPercent()
    {
        int entries = 0;

        for (int i = 0; i < table.Length; i++)
        {
            if (table[i].key != 0) entries++;
        }

        return (float)entries/entryCount * 100f;
    }

    private ulong Index(ulong zobrist)
    {
        return zobrist % entryCount;
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
        return table[Index(zobrist)].move; //TODOnt anymore: Could just return whole entry to avoid having to lock twice waiting for both eval and move if eval is good
    }

    public int LookupEvaluation(ulong zobrist, uint depth, int plyFromRoot, int alpha, int beta)
    {
        ulong index = Index(zobrist);

        Transposition transposition = table[index];
        //TODO: Test maybe with waiting a tiny bit here if key and data is corrupted instead of just failing the probe?

        if ((transposition.key ^ transposition.data) == zobrist)
        {
            // Only use stored evaluation if it has been searched to at least the same depth as would be searched now
            if (transposition.depth >= depth)
            {
                int correctedScore = CorrectRetrievedMateScore(transposition.value, plyFromRoot);
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
        }
        return LookupFailed;
    }



    public void StoreEvaluation(ulong zobrist, uint depth, int numPlySearched, int eval, ulong evalType, Move move)
    {
        Transposition transposition = new Transposition(zobrist, CorrectMateScoreForStorage(eval, numPlySearched), (byte)depth, (byte)evalType, move); //TODO: try changing casts/removing?

        table[Index(zobrist)] = transposition;
    }

    int CorrectMateScoreForStorage(int score, int numPlySearched)
    {
        if (Search.IsMateScore(score))
        {
            int sign = System.Math.Sign(score);
            return (score * sign + numPlySearched) * sign;
        }
        return score;
    }

    int CorrectRetrievedMateScore(int score, int numPlySearched)
    {
        if (Search.IsMateScore(score))
        {
            int sign = System.Math.Sign(score);
            return (score * sign - numPlySearched) * sign;
        }
        return score;
    }






    public struct Transposition
    {
        public readonly ulong key; //TODO: try atomic writes - move everything except key into a ulong - if were overwriting an entry the key might be the same? dont know how to solve - try just removing key and key check

        //32 bits eval (value)
        //16 bits move
        //8 bits depth
        //8 bits nodetype
        public readonly ulong data;
        //public readonly int value;
        //public readonly Move move;
        //public readonly byte depth;
        //public readonly byte nodeType;

        private const ulong valueMask = 0b11111111111111111111111111111111;
        private const ulong moveMask = 0b111111111111111100000000000000000000000000000000;
        private const ulong depthMask = 0b11111111000000000000000000000000000000000000000000000000;
        private const ulong nodeTypeMask = 0b1111111100000000000000000000000000000000000000000000000000000000;

        public int value { get { return (int)(data & valueMask); } } //Could maybe be reduced?
        public Move move { get { return new Move((ushort)((data & moveMask) >> 32)); } }
        public ulong depth { get { return (data & depthMask) >> 48; } } //Could be significantly reduced
        public ulong nodeType { get { return (data & nodeTypeMask) >> 56; } } //Could also be significantly reduced

        public Transposition(ulong zobrist, int value, byte depth, byte nodeType, Move move) //TODO: make all ulong here in params?
        {
            data = (ulong)value | (((ulong)move.data) << 32) | (((ulong)depth) << 48) | (((ulong)nodeType) << 56);
            key = zobrist ^ data;
            //this.value = value;
            //this.depth = depth; // depth is how many ply were searched ahead from this position
            //this.nodeType = nodeType;
            //this.move = move;
        }

        public static int GetSize()
        {
            return System.Runtime.InteropServices.Marshal.SizeOf<Transposition>();
        }
    }
}