using System;
using System.Diagnostics;


public class Engine
{
    public static Board mainBoard = new Board();
    private EngineUCI uci;


    public int threadCount => searchThreads.Length;

    private EngineThread[] searchThreads = new EngineThread[1];
    private int availableThreads = 1;


    private bool outOfBook = false; //Whether we have run out of book moves and have to search on our own

    Stopwatch searchTimer = new Stopwatch(); //TODO: Could just use Search.clock now

    public Engine(EngineUCI _uci)
    {
        uci = _uci;
        SetThreadCount(1);
        outOfBook = false;
    }



    public void SetThreadCount(int count) //TODO: Could technically be optimized by keeping the threadBoards with the already correct state, and only loading the newly created ones
    {
        searchThreads = new EngineThread[count];
        availableThreads = count;

        Action<Move, int> callback = OnSearchCompleted;

        string fen = FenUtility.GetCurrentFen(mainBoard);

        for (int i = 0; i < searchThreads.Length; i++)
        {
            searchThreads[i] = new EngineThread(i, callback, this);

            FenUtility.LoadPositionFromFen(searchThreads[i].board, fen);
        }
    }

    public void LoadFen(string fen)
    {
        outOfBook = false;

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





    //TODO: Remove janky shit
    public void RunMateSearch()
    {
        //Search.cancelSearch = false;

        Action<Move, int> callback = (result, id) => OnSearchCompleted(result, id);

        searchTimer.Restart();

        //                                     Janky - just use mainboard now
        MateSearch mateSearch = new MateSearch(searchThreads[0].board, callback, 0);

        availableThreads--;

        mateSearch.StartSearch();
    }

    public void InitializeSearch(int depth, int time)
    {
        //TODOne: Adjust TT on first book move
        if (!outOfBook) //If we aren't yet out of book, check if the position is present in our opening book. If not, mark us as out of book
        {
            Move bookMove = OpeningBook.GetMove(mainBoard.currentZobrist);

            if (bookMove.data == 0)
            {
                Console.WriteLine("Book move was empty");
                outOfBook = true;
                uci.AdjustTT();
            }
            else
            {
                Console.WriteLine("info string Found book move");

                string name = BoardHelper.GetMoveNameUCI(bookMove);
                Console.WriteLine("info depth 0 score 0 pv " + name + " nodes 0"); //Can be removed - just to get fastchess to shut up about not having info strings
                Console.WriteLine("bestmove " + name);

                uci.AdjustTT();
                return;
            }
        }

        Action<Move, int> callback = (result, id) => OnSearchCompleted(result, id);

        searchTimer.Restart();

        //for (int i = 0; i < searchThreads.Length; i++)
        //{
        //Console.WriteLine("info string Thread " + i + " Searching...");

        //int id = i; //Extremely weird issue where i gets incremented before being passed along to thread if not done like this - found out why. everything is passed as a reference to threads apparently

        //if (id != 0) Thread.Sleep(100);
        //if (id != 0) Thread.Sleep(10 * id); //TODOne: Check if id == 0 bc Sleep(0) will yield for other threads

        //if (i % 4 == 0) depth++;

        //if (Search.cancelSearch) break; //TODO: stop if search has been cancelled while we are launching threads still - prob use own timer for this

        Console.WriteLine("info string Main Thread Searching...");

        availableThreads--;
        searchThreads[0].Start(depth, time); //TODOne: Keep thread data persistent?
        //}
    }

    public void StartHelperThreads(int depth) //TODO: Def doesn't make sense - Maybe it doesn't make sense to have helper threads running iterative deepening but seems like TT makes it almost instant anyway so idk. Does seem stupid tho
    {
        Action<Move, int> callback = (result, id) => OnSearchCompleted(result, id);


        for (int i = 1; i < searchThreads.Length; i++) //Start all threads except the main one - these will be helper threads that will stop themselves when they are done
        {
            Console.WriteLine("info string Thread " + i + " Searching...");

            int id = i; //Extremely weird issue where i gets incremented before being passed along to thread if not done like this - found out why. everything is passed as a reference to threads apparently

            availableThreads--;

            //if (id % 2 == 1) depth++; //TODO: try starting first helper thread as same depth as main thread

            searchThreads[i].Start(depth, -2); //Time is set to -2 so that they will stop themselves when they are done searching their assigned depth
        }
    }

    public void StopHelperThreads()
    {
        Console.WriteLine("stopping helper threads");


        for (int i = 1; i < searchThreads.Length; i++)
        {
            searchThreads[i].search.searchTime = int.MinValue; //Stop helper //TODO: Use stopsearch instead of this bc other threads won't be stopping while we wait for this one to finish currently
            searchThreads[i].WaitForFinish();
        }

        //Console.WriteLine("Finished stopping helper threads");
    }

    private void OnSearchCompleted(Move move, int id) //TODO: Just have separate callback for helper threads that doesn't do all of this
    {
        availableThreads++;

        if (availableThreads == threadCount) Console.WriteLine("info string All Threads Done");

        if (id != 0) return; //This is a helper thread

        searchTimer.Stop();
        StopSearch();

        Console.WriteLine("info string Thread " + id + " Finished in: " + searchTimer.ElapsedMilliseconds + "ms");
        Console.WriteLine("bestmove " + BoardHelper.GetMoveNameUCI(move));

        //Search.cancelSearch = true;
    }

    public void StopSearch() //TODO: Wait with .Join()?
    {
        foreach (EngineThread thread in searchThreads) thread.search.searchTime = int.MinValue;
    }


    public struct EngineThread
    {
        public int id;
        public Search search;
        public Board board;
        private Thread thread;

        public EngineThread(int _id, Action<Move, int> callback, Engine _engine)
        {
            id = _id;
            board = new Board();
            search = new Search(board, callback, id, (id == 0) ? _engine : null);
        }

        public void Start(int depth, int time)
        {
            search.searchDepth = depth;
            search.searchTime = time;

            if (id == 0) thread = new Thread(search.StartSearch);
            else thread = new Thread(search.StartHelperSearch);

            thread.Start(); //TODO: Try unsafestart
        }

        public void WaitForFinish()
        {
            thread.Join();
        }
    }
}
