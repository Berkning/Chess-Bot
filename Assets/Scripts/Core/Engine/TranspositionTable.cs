using UnityEngine;

public class TranspositionTable
{
    public static int SizeMB = 16;

    public const int LookupFailed = int.MinValue;

    // The value for this position is the exact evaluation
    public const int Exact = 0;
    // A move was found during the search that was too good, meaning the opponent will play a different move earlier on,
    // not allowing the position where this move was available to be reached. Because the search cuts off at
    // this point (beta cut-off), an even better move may exist. This means that the evaluation for the
    // position could be even higher, making the stored value the lower bound of the actual value.
    public const int LowerBound = 1;
    // No move during the search resulted in a position that was better than the current player could get from playing a
    // different move in an earlier position (i.e eval was <= alpha for all moves in the position).
    // Due to the way alpha-beta search works, the value we get here won't be the exact evaluation of the position,
    // but rather the upper bound of the evaluation. This means that the evaluation is, at most, equal to this value.
    public const int UpperBound = 2;



    private Transposition[] table;
    private ulong entryCount;

    public TranspositionTable()
    {
        entryCount = (ulong)(SizeMB * 1000 * 1000 / Transposition.GetSize());

        table = new Transposition[entryCount];
    }

    private ulong Index
    {
        get { return Board.currentZobrist % entryCount; }
    }


    public void Clear()
    {
        for (int i = 0; i < table.Length; i++)
        {
            table[i] = new Transposition();
        }
    }


    public Move GetStoredMove()
    {
        return table[Index].move;
    }

    public int LookupEvaluation(int depth, int plyFromRoot, int alpha, int beta)
    {
        Transposition transposition = table[Index];

        if (transposition.key == Board.currentZobrist)
        {
            // Only use stored evaluation if it has been searched to at least the same depth as would be searched now
            if (transposition.depth >= depth)
            {
                int correctedScore = CorrectRetrievedMateScore(transposition.value, plyFromRoot);
                // We have stored the exact evaluation for this position, so return it
                if (transposition.nodeType == Exact)
                {
                    return correctedScore;
                }
                // We have stored the upper bound of the eval for this position. If it's less than alpha then we don't need to
                // search the moves in this position as they won't interest us; otherwise we will have to search to find the exact value
                if (transposition.nodeType == UpperBound && correctedScore <= alpha)
                {
                    return correctedScore;
                }
                // We have stored the lower bound of the eval for this position. Only return if it causes a beta cut-off.
                if (transposition.nodeType == LowerBound && correctedScore >= beta)
                {
                    return correctedScore;
                }
            }
        }
        return LookupFailed;
    }

    public void StoreEvaluation(int depth, int numPlySearched, int eval, int evalType, Move move)
    {
        Transposition transposition = new Transposition(Board.currentZobrist, CorrectMateScoreForStorage(eval, numPlySearched), (byte)depth, (byte)evalType, move);
        table[Index] = transposition;
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
        public readonly ulong key;
        public readonly int value;
        public readonly Move move;
        public readonly byte depth;
        public readonly byte nodeType;

        public Transposition(ulong key, int value, byte depth, byte nodeType, Move move)
        {
            this.key = key;
            this.value = value;
            this.depth = depth; // depth is how many ply were searched ahead from this position
            this.nodeType = nodeType;
            this.move = move;
        }

        public static int GetSize()
        {
            return System.Runtime.InteropServices.Marshal.SizeOf<Transposition>();
        }
    }
}