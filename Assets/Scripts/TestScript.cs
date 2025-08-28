using System;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    [Space, Header("Debugging"), SerializeField] private string customFen;
    [SerializeField] private bool runPerft;
    [SerializeField, Range(1, 8)] private int perftDepth;
    [Space, SerializeField] private bool compareToFish;
    [SerializeField] private string stockFishCompare;
    [Space, Header("Search & Eval")]
    [SerializeField] private bool doSearch;
    [SerializeField] private int depth;
    [SerializeField] private bool useMoveOrdering;

    void Awake()
    {
        if (customFen == "") FenUtility.LoadPositionFromFen(FenUtility.StartPosFen);
        else FenUtility.LoadPositionFromFen(customFen);

        //Board.MakeMove(MoveGenerator.GenerateMoves()[0]);
        //if (runPerft) Perft.RunForeachDepth(perftDepth, true);
        if (runPerft)
        {
            string result = Perft.RunDetailed(perftDepth);
            if (compareToFish) CompareToStockfish(result);
        }

        Debug.Log("11: " + BoardHelper.FlipIndex(11));

        return;
        for (int i = 0; i < 64; i++)
        {
            int rank = BoardHelper.IndexToRank(i);
            int file = BoardHelper.IndexToFile(i);

            Debug.Log("Index: " + i + " Rank: " + rank + " File: " + file + " BackIndex: " + BoardHelper.CoordToIndex(file, rank));
        }
    }

    private void CompareToStockfish(string result)
    {
        string[] moves = result.Split('\n');
        string[] stockfishResults = stockFishCompare.Split(' ');

        for (int i = 0; i < moves.Length; i++)
        {
            string[] parts = moves[i].Split(' ');

            for (int j = 0; j < stockfishResults.Length; j++)
            {
                if (stockfishResults[j * 2] == parts[0])
                {
                    string stockfishPart0 = stockfishResults[j * 2];
                    string stockfishPart1 = stockfishResults[j * 2 + 1];

                    //Debug.Log("Found Match: " + parts[0] + " : " + stockfishPart0);
                    //break;

                    int stockCount = int.Parse(stockfishPart1);
                    int ourCount = int.Parse(parts[1]);

                    if (stockCount != ourCount)
                    {
                        Debug.Log("Discrepancy found: " + parts[0] + " Expected: " + stockCount + " Got: " + ourCount + " Error: " + Mathf.Abs(ourCount - stockCount));
                    }

                    break;
                }
            }
        }
    }

    void Update()
    {
        /*int epFile = (int)((Board.currentGameState & Board.epFileMask) >> 5) - 1;
        int epRank = Board.friendlyColor == Piece.White ? 4 : 3;
        int epSquare = epFile != -1 ? BoardHelper.CoordToIndex(epFile, epRank) : -1;

        Debug.Log("file: " + epFile);
        Debug.Log("square: " + epSquare);*/

        if (doSearch)
        {
            doSearch = false;
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            Debug.Log(BoardHelper.NameMove(Search.StartSearch(depth, useMoveOrdering)));
            stopwatch.Stop();
            Debug.Log("Search Took: " + stopwatch.ElapsedMilliseconds + "ms");
        }
    }
}