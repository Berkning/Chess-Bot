
public class RepetitionTable
{
    private ulong[] hashes = new ulong[256]; //A size of 128 for this would theoretically (i believe) be more than enough (bc of 50 move rule), but without having 50 move rule implemented this should be 256 (which could still cause crashes but odds are like 1/inf)
    private int currentIndex = 0;

    public int Count => currentIndex;

    public ulong this[int i] { get => hashes[i]; }

    public void Push(ulong hash) //TODO: optimize like sebastian so we have a reversible reset, maybe, when pawn pushes/capture
    {
        if (currentIndex < 0 || currentIndex >= hashes.Length) Console.WriteLine("bestmove repetitionTableBoundsViolation:" + currentIndex);
        hashes[currentIndex] = hash;
        currentIndex++;
    }

    public ulong Pop()
    {
        currentIndex--;
        return hashes[currentIndex];
    }

    public void PopNoRtn()
    {
        currentIndex--;
    }

    public void Clear()
    {
        currentIndex = 0;
    }

    public void Copy(RepetitionTable table)
    {
        Clear();

        for (int i = 0; i < table.Count; i++)
        {
            Push(table[i]);
        }
    }

    public bool Contains(ulong hash)
    {
        for (int i = currentIndex - 1; i > -1; i--) //We go from top of the stack to the bottom and check if the hash has been seen - imagine it would be slightly more likely the repetition occured in the most recent moves
        {
            if (hashes[i] == hash) return true;
        }

        return false;
    }
}