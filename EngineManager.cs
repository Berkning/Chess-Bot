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

    Stopwatch searchTimer = new Stopwatch();

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
            searchThreads[i] = new EngineThread(i, callback);

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
        Search.cancelSearch = false;

        Action<Move, int> callback = (result, id) => OnSearchCompleted(result, id);

        searchTimer.Restart();

        //                                     Janky
        MateSearch mateSearch = new MateSearch(searchThreads[0].board, callback, 0);

        availableThreads--;

        mateSearch.StartSearch();
    }

    public void InitializeSearch(int depth, int time)
    {
        if (!outOfBook) //If we aren't yet out of book, check if the position is present in our opening book. If not, mark us as out of book
        {
            Move bookMove = OpeningBook.GetMove(mainBoard.currentZobrist);

            if (bookMove.data == 0)
            {
                Console.WriteLine("Book move was empty");
                outOfBook = true;
            }
            else
            {
                Console.WriteLine("info string Found book move");
                Console.WriteLine("bestmove " + BoardHelper.GetMoveNameUCI(bookMove));
                return;
            }
        }


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
            thread.Start(); //TODO: Try unsafestart
        }
    }
}
