using System;
using System.Diagnostics;


public class Engine
{
    public static Board mainBoard = new Board();
    private EngineUCI uci;


    public int threadCount => searchThreads.Length;

    private EngineThread[] searchThreads = new EngineThread[1];
    private int availableThreads = 1;



    Stopwatch searchTimer = new Stopwatch();

    public Engine(EngineUCI _uci)
    {
        uci = _uci;
        SetThreadCount(1);
    }



    public void SetThreadCount(int count) //TODO: Could technically be optimized by keeping the threadBoards with the already correct state, and only loading the newly created ones
    {
        searchThreads = new EngineThread[count];
        availableThreads = count;

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
            Console.WriteLine("info string Thread " + i + " Searching...");

            int id = i; //Extremely weird issue where i gets incremented before being passed along to thread if not done like this - found out why. everything is passed as a reference to threads apparently

            //if (id != 0) Thread.Sleep(100);
            if (id != 0) Thread.Sleep(10 * id); //TODOne: Check if id == 0 bc Sleep(0) will yield for other threads

            //if (i % 4 == 0) depth++;

            if (Search.cancelSearch) break; //If search has been cancelled while we are launching threads still

            availableThreads--;
            searchThreads[i].Start(depth, time); //TODOne: Keep thread data persistent?
        }
    }


    private void OnSearchCompleted(Move move, int id)
    {
        availableThreads++;

        if (availableThreads == threadCount) Console.WriteLine("info string All Threads Done");

        if (availableThreads > 1) return; //Another thread has already finished so no need to log stuff

        searchTimer.Stop();

        Console.WriteLine("info string Thread " + id + " Finished in: " + searchTimer.ElapsedMilliseconds + "ms");
        Console.WriteLine("bestmove " + BoardHelper.GetMoveNameUCI(move));

        Search.cancelSearch = true;
        uci.AdjustTT();
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
