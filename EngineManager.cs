using System;
using System.Diagnostics;


public class Engine
{
    public static Board board = new Board();


    public int threadCount => searchThreads.Length;

    private Thread[] searchThreads = new Thread[1];
    private Board[] threadBoards = new Board[1];




    Stopwatch searchTimer = new Stopwatch();

    public void SetThreadCount(int count) //TODO: Could technically be optimized by keeping the threadBoards with the already correct state, and only loading the newly created ones
    {
        searchThreads = new Thread[count];
        threadBoards = new Board[count];

        string fen = FenUtility.GetCurrentFen(board);

        for (int i = 0; i < threadBoards.Length; i++)
        {
            threadBoards[i] = new Board();

            FenUtility.LoadPositionFromFen(threadBoards[i], fen);
        }
    }

    public void LoadFen(string fen)
    {
        FenUtility.LoadPositionFromFen(board, fen);

        for (int i = 0; i < threadBoards.Length; i++)
        {
            if (threadBoards[i] == null) threadBoards[i] = new Board();

            FenUtility.LoadPositionFromFen(threadBoards[i], fen);
        }
    }

    public void PlayMove(Move move)
    {
        board.MakeMove(move);

        foreach (Board threadBoard in threadBoards)
        {
            threadBoard.MakeMove(move);
        }
    }





    public void InitializeSearch(int depth, int time)
    {
        Search.cancelSearch = false;

        Action<Move, int> callback = (result, id) => OnSearchCompleted(result, id);

        for (int i = 0; i < searchThreads.Length; i++)
        {
            Console.WriteLine("Thread " + i + " Searching...");

            Search searcher = new Search();

            Board threadBoard = threadBoards[i];

            searchThreads[i] = new Thread(() => searcher.StartSearch(callback, depth, i, threadBoard, time)); //TODO: Keep threads persistent?

            searchThreads[i].Start();
        }

        searchTimer.Restart();
    }

    private void OnSearchCompleted(Move move, int id)
    {
        Console.WriteLine("Thread " + id + " Finished");

        if (!searchTimer.IsRunning)
        {
            return;
        }

        searchTimer.Stop();
        Console.WriteLine("bestmove " + BoardHelper.GetMoveNameUCI(move));
        Console.WriteLine("info string Search Finished in: " + searchTimer.ElapsedMilliseconds + "ms");

        //AdjustTT(board.colorToMove == Piece.White ? TimeManagement.whiteTime : TimeManagement.blackTime);
    }
}
