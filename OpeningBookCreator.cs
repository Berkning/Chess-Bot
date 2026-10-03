

using System.Buffers.Binary;
using System.Diagnostics;

public static class OpeningBookCreator
{
    private static Action<Move, int> callback = (result, id) => { };
    private static Search search;
    private static MoveGenerator moveGenerator;
    private static Board board;

    public const int MaxEvalDrop = -50; //If a move causes the eval to drop to, or below, this value, we will not add the resulting position to the book
    public const int BookEntrySearchTime = 10; //The amount of time to spend searching to figure out the best move in a given position, before adding the result to the book
    public const int ResponseCandidateSearchTime = 50; //The amount of time to spend searching to figure out whether a move is good enough, that we should account for the possibility of our opponent playing it, as in add the resulting position (with best move) to the book
    //public const int MaxDepth = 1; //The maximum depth for the book to go from the opening position
    public const int MaxCandidates = 5; //The maximum amount of candidates to assume the opponent might play in a given position. If this is 5, for example, we assume the opponent will play one of the top 5 moves in the position, and nothing else

    //Essentially behaves like LMR reduction rate
    public const int AlternativeCandidateReductionRate = 1; //The amount we reduce our search depth for "alternative" candidate moves. Multiplied by the sorted index, so candidate #1 is searched to full depth, #2 is searched to full depth - ReductionRate, #3 is searched to full depth - ReductionRate*2 and so on

    private static int totalRejects;
    private static int totalAccepted;
    private static int totalEntries;

    public static List<PolyglotEntry> entries = new List<PolyglotEntry>();


    public static void CreateBook(int maxDepth)
    {
        board = new Board();
        search = new Search(board, callback, 0, null);
        moveGenerator = new MoveGenerator(board);
        search.searchDepth = int.MaxValue;

        totalAccepted = 0;
        totalRejects = 0;
        totalEntries = 0;

        FenUtility.LoadPositionFromFen(board, FenUtility.StartPosFen);

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Restart();
        SearchCandidatesRecursive(maxDepth);
        stopwatch.Stop();

        Console.WriteLine("Done in " + stopwatch.ElapsedMilliseconds + "ms");
        Console.WriteLine("Accepted Candidates: " + totalAccepted + " Rejects: " + totalRejects);
        Console.WriteLine("Book now consists of " + totalEntries + " entries");

        WriteBook("CUSTOMBOOK.bin");
    }


    public static void WriteBook(string filePath)
    {
        // Sort entries by key (lowest first).
        entries.Sort((a, b) => a.key.CompareTo(b.key));

        using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);

        Span<byte> buffer = stackalloc byte[16];

        foreach (PolyglotEntry entry in entries)
        {
            // Key: 8 bytes
            BinaryPrimitives.WriteUInt64BigEndian(
                buffer.Slice(0, 8), entry.key);

            // Move: 2 bytes
            BinaryPrimitives.WriteUInt16BigEndian(
                buffer.Slice(8, 2), entry.move);

            // Weight: 2 bytes
            BinaryPrimitives.WriteUInt16BigEndian(
                buffer.Slice(10, 2), entry.weight);

            // Learn: 4 bytes
            BinaryPrimitives.WriteUInt32BigEndian(
                buffer.Slice(12, 4), entry.learn);

            // Write the complete 16-byte entry.
            stream.Write(buffer);
        }
    }

    private static void SearchCandidatesRecursive(int depth)
    {
        AddBestMoveToBook();

        if (depth <= 0) return;

        List<CandidateMove> candidateMoves = GetCandidateMoves();
        int candidatesToCheck = int.Min(candidateMoves.Count, MaxCandidates);

        for (int i = 0; i < candidatesToCheck; i++)
        {
            board.MakeMove(candidateMoves[i].move, true);

            int reduction = 0;
            if (i >= 2)
            {
                reduction = (i - 1) * AlternativeCandidateReductionRate;
            }

            SearchCandidatesRecursive(depth - 1 - reduction);

            board.UnMakeMove(candidateMoves[i].move, true);
        }
    }




    private static List<CandidateMove> GetCandidateMoves()
    {
        Span<Move> moves = stackalloc Move[256];
        int moveCount = moveGenerator.GenerateMoves(ref moves);

        List<CandidateMove> candidates = new List<CandidateMove>();


        for (int i = 0; i < moveCount; i++)
        {
            board.MakeMove(moves[i], true);

            int eval = -search.StartInternalSearch(ResponseCandidateSearchTime).eval; //Position will be analysed from opponents perspective, so we negate the result to account for this

            if (eval > MaxEvalDrop)
            {
                //Console.WriteLine(BoardHelper.GetMoveNameUCI(moves[i]) + " Accepted as candidate");
                totalAccepted++;

                candidates.Add(new CandidateMove(moves[i], eval));
            }
            else totalRejects++;


            board.UnMakeMove(moves[i], true);
        }

        SortCandidates(candidates);

        return candidates;
    }



    private static void AddBestMoveToBook()
    {
        Move bestMove = search.StartInternalSearch(BookEntrySearchTime).bestMove;

        Console.WriteLine("Best move is: " + BoardHelper.GetMoveNameUCI(bestMove));
        totalEntries++;
        entries.Add(new PolyglotEntry(board.currentZobrist, OpeningBook.TranslateMoveToPolyglot(bestMove), ushort.MaxValue, 0));
    }




    private static void SortCandidates(List<CandidateMove> candidates)
    {
        for (int i = 0; i < candidates.Count - 1; i++)
        {
            for (int j = i + 1; j > 0; j--)
            {
                int swapIndex = j - 1;
                if (candidates[swapIndex].eval < candidates[j].eval)
                {
                    (candidates[j], candidates[swapIndex]) = (candidates[swapIndex], candidates[j]);
                }
            }
        }
    }

    private static void LogCandidates(List<CandidateMove> candidates)
    {
        for (int i = 0; i < candidates.Count; i++)
        {
            Console.WriteLine((i + 1) + "- Eval: " + candidates[i].eval + " Move: " + BoardHelper.GetMoveNameUCI(candidates[i].move));
        }
    }



    public struct CandidateMove
    {
        public Move move;
        public int eval;

        public CandidateMove(Move _move, int _eval)
        {
            move = _move;
            eval = _eval;
        }
    }

    public struct PolyglotEntry
    {
        public ulong key;
        public ushort move;
        public ushort weight;
        public uint learn;

        public PolyglotEntry(ulong key, ushort move, ushort weight, uint learn)
        {
            this.key = key;
            this.move = move;
            this.weight = weight;
            this.learn = learn;
        }
    }
}