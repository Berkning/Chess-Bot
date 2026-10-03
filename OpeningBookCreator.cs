

using System.Diagnostics;

public static class OpeningBookCreator
{
    private static Action<Move, int> callback = (result, id) => { };
    private static Search search;
    private static MoveGenerator moveGenerator;
    private static Board board;

    public const int MaxEvalDrop = -50; //If a move causes the eval to drop to, or below, this value, we will not add the resulting position to the book
    public const int BookEntrySearchTime = 10000; //The amount of time to spend searching to figure out the best move in a given position, before adding the result to the book
    public const int ResponseCandidateSearchTime = 100; //The amount of time to spend searching to figure out whether a move is good enough, that we should account for the possibility of our opponent playing it, as in add the resulting position (with best move) to the book
    public const int MaxDepth = 3; //The maximum depth for the book to go from the opening position
    public const int MaxCandidates = 3; //The maximum amount of candidates to assume the opponent might play in a given position. If this is 5, for example, we assume the opponent will play one of the top 5 moves in the position, and nothing else

    public static void CreateBook()
    {
        board = new Board();
        search = new Search(board, callback, 0, null);
        moveGenerator = new MoveGenerator(board);
        search.searchDepth = int.MaxValue;

        FenUtility.LoadPositionFromFen(board, FenUtility.StartPosFen);

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Restart();
        SearchCandidatesRecursive(MaxDepth);
        stopwatch.Stop();

        Console.WriteLine("Done in " + stopwatch.ElapsedMilliseconds + "ms");
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

            SearchCandidatesRecursive(depth - 1);

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
                Console.WriteLine(BoardHelper.GetMoveNameUCI(moves[i]) + " Accepted as candidate");

                candidates.Add(new CandidateMove(moves[i], eval));
            }
            else Console.WriteLine(BoardHelper.GetMoveNameUCI(moves[i]) + " rejected");


            board.UnMakeMove(moves[i], true);
        }

        SortCandidates(candidates);

        return candidates;
    }



    private static void AddBestMoveToBook()
    {
        Move bestMove = search.StartInternalSearch(BookEntrySearchTime).bestMove;

        Console.WriteLine("Best move is: " + BoardHelper.GetMoveNameUCI(bestMove));
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
}