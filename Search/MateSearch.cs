public class MateSearch //TODO: Should def have some special move ordering like looking at checks first
{
    //Kinda janky to have these here as well, but changing these doesn't affect how mate search behaves
    private const int ImmediateMateScore = 100000;
    private const int PositiveInfinity = 9999999;
    private const int NegativeInfinity = -PositiveInfinity;

    private Move bestMove;
    private int bestMateScore;



    private Board board;
    private MoveGenerator moveGenerator;
    private int threadID;
    private Action<Move, int> callback;

    public MateSearch(Board _board, Action<Move, int> _callback, int _threadID)
    {
        threadID = _threadID;
        callback = _callback;


        board = _board;
        moveGenerator = new MoveGenerator(board);
    }

    public void StartSearch() //I imagine it would be completely unnecessary to add repetition table bc if we ever repeat moves 3 times there is obv not a way to mate at current depth
    {
        //FIXME:
        // bestMove = Move.nullMove;
        // bestMateScore = NegativeInfinity;

        // for (uint depth = 1; depth <= 255; depth++)
        // {
        //     if (FindMate(depth, 0) != 0)
        //     {
        //         Console.WriteLine("info string Found mate");
        //         break;
        //     }

        //     if (Search.cancelSearch)
        //     {
        //         Console.WriteLine("info string Mate search interrupted");
        //         break;
        //     }
        // }
        // callback.Invoke(bestMove, threadID);
    }

    private int FindMate(uint depth, int plyFromRoot)
    {
        return -1;
        //FIXME:
        // if (Search.cancelSearch) return 0;

        // Span<Move> moves = stackalloc Move[256];

        // int moveCount = moveGenerator.GenerateMoves(ref moves);

        // //TODO: Special move ordering

        // if (moveCount == 0)
        // {
        //     if (moveGenerator.inCheck) return -1;

        //     return 0;
        // }

        // if (depth == 0) return 0;

        // //if (plyFromRoot > 0)
        // //{
        // //    if (moveCount > 7) return 0; //We have a large enough available move count to assume we wont be mated
        // //}



        // bool mateAvoidable = false;
        // Move bestMoveInPos = Move.nullMove;

        // for (int i = 0; i < moveCount; i++)
        // {
        //     board.MakeMove(moves[i], true);
        //     int mateEval = -FindMate(depth - 1, plyFromRoot + 1);
        //     board.UnMakeMove(moves[i], true);

        //     //TODO: Check for search cancelled

        //     if (mateEval == 1) //If we can force a mate with the current move
        //     {
        //         if (plyFromRoot == 0) bestMove = moves[i];

        //         return 1;
        //     }

        //     if (mateEval == 0)
        //     {
        //         mateAvoidable = true;
        //         bestMoveInPos = moves[i];

        //         if (plyFromRoot == 0) bestMove = bestMoveInPos;

        //         //if (score == -1)
        //     }
        // }

        // if (!mateAvoidable)
        // {
        //     if (plyFromRoot == 0) bestMove = moves[0];
        //     return -1;
        // }

        // return 0;
    }
}