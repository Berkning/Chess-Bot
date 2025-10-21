using System;
using System.Diagnostics;


public class Engine
{
    public static Board mainBoard = new Board();


    public int threadCount => searchThreads.Length;

    //private Thread[] searchThreads = new Thread[1];
    //private Board[] threadBoards = new Board[1];
    private EngineThread[] searchThreads = new EngineThread[1];




    Stopwatch searchTimer = new Stopwatch();

    public Engine()
    {
        SetThreadCount(1);
    }


    public void SetThreadCount(int count) //TODO: Could technically be optimized by keeping the threadBoards with the already correct state, and only loading the newly created ones
    {
        searchThreads = new EngineThread[count];

        Action<Move, int> callback = OnSearchCompleted;

        string fen = FenUtility.GetCurrentFen(mainBoard);

        for (int i = 0; i < searchThreads.Length; i++)
        {
            searchThreads[i] = new EngineThread(i, callback);

            FenUtility.LoadPositionFromFen(searchThreads[i].board, fen);
        }
    }

    public void LoadFen(string fen)
    {
        FenUtility.LoadPositionFromFen(mainBoard, fen);

        for (int i = 0; i < searchThreads.Length; i++)
        {
            FenUtility.LoadPositionFromFen(searchThreads[i].board, fen);
        }
    }

    public void PlayMove(Move move)
    {
        mainBoard.MakeMove(move);

        for (int i = 0; i < searchThreads.Length; i++)
        {
            searchThreads[i].board.MakeMove(move);
        }
    }





    public void InitializeSearch(int depth, int time)
    {
        Search.cancelSearch = false;

        Action<Move, int> callback = (result, id) => OnSearchCompleted(result, id);

        searchTimer.Restart();

        for (int i = 0; i < searchThreads.Length; i++)
        {
            Console.WriteLine("Thread " + i + " Searching...");

            int id = i; //Extremely weird issue where i gets incremented before being passed along to thread if not done like this - found out why. everything is passed as a reference to threads apparently

            //if (id != 0) Thread.Sleep(100);
            if (id != 0) Thread.Sleep(10 * id); //TODOne: Check if id == 0 bc Sleep(0) will yield for other threads

            //if (i % 4 == 0) depth++;

            searchThreads[i].Start(depth, time); //TODOne: Keep thread data persistent?
        }
    }



    private bool jankBool = false;

    private void OnSearchCompleted(Move move, int id)
    {
        //Console.WriteLine("Thread " + id + " Finished");

        //if (!searchTimer.IsRunning)
        //{
        //    return;
        //}

        searchTimer.Stop();
        Console.WriteLine("bestmove " + BoardHelper.GetMoveNameUCI(move));
        Console.WriteLine("info string Search Finished in: " + searchTimer.ElapsedMilliseconds + "ms");

        //AdjustTT(board.colorToMove == Piece.White ? TimeManagement.whiteTime : TimeManagement.blackTime);
        //FIXME:

        if (jankBool) return; //TODO: obv cant keep this bc if multiple threads it resets TT after first thread is done

        jankBool = true;

        TranspositionTable.SizeMB = 8;

        Console.WriteLine("info string Table adjusted to " + TranspositionTable.SizeMB + "mb");

        Search.transpositionTable = new TranspositionTable();
    }


    public struct EngineThread
    {
        public int id;
        public Search search;
        public Board board;
        private Thread thread;

        public EngineThread(int _id, Action<Move, int> callback)
        {
            id = _id;
            board = new Board();
            search = new Search(board, callback, id);
        }

        public void Start(int depth, int time)
        {
            search.searchDepth = depth;
            search.searchTime = time;

            thread = new Thread(search.StartSearch);
            thread.Start();
        }
    }
}
