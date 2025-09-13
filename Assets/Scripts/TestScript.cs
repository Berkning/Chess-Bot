using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    [Space, Header("Move Generation")]
    [SerializeField] private bool testGen;
    [SerializeField] private bool testOrder;
    [SerializeField] private List<ushort> moves1 = new List<ushort>();
    [SerializeField] private List<ushort> moves2 = new List<ushort>();
    private int currentGenIndex = 0;
    [Space, Header("Zobrist")]
    [SerializeField] private bool hashPosition;
    [Space, Header("Benchmarking"), SerializeField] private bool bench;

    void Awake()
    {
        if (customFen == "") FenUtility.LoadPositionFromFen(FenUtility.StartPosFen);
        else FenUtility.LoadPositionFromFen(customFen);

        //Board.MakeMove(MoveGenerator.GenerateMoves()[0]);
        //if (runPerft) Perft.RunForeachDepth(perftDepth, true);
        if (runPerft)
        {
            string result = Perft.RunDetailed(perftDepth);
            //Debug.Log(result);
            if (compareToFish) CompareToStockfish(result);
        }

        //Debug.Log("11: " + BoardHelper.FlipIndex(11));

        // int piece1 = Piece.White | Piece.Bishop;
        // int piece2 = Piece.White | Piece.Rook;
        // int piece3 = Piece.Black | Piece.Queen;
        // int piece4 = Piece.Black | Piece.King;
        // int piece5 = Piece.White | Piece.Pawn;

        // System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        // sw.Start();
        // for (int i = 0; i < 100000000; i++)
        // {
        //     Zobrist.PieceToPolyGlot(piece1);
        //     Zobrist.PieceToPolyGlot(piece2);
        //     Zobrist.PieceToPolyGlot(piece3);
        //     Zobrist.PieceToPolyGlot(piece4);
        //     Zobrist.PieceToPolyGlot(piece5);
        // }
        // sw.Stop();
        // Debug.Log("Conversion with array took: " + sw.ElapsedMilliseconds + "ms");

        if (bench)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            Perft.RunFullSuite();
            sw.Stop();
            Debug.Log("Benc took: " + sw.ElapsedMilliseconds + "ms");
        }


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

        Debug.Log("Our Count: " + moves.Length);
        Debug.Log("Fish Count: " + stockfishResults.Length);

        for (int i = 0; i < moves.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(moves[i]) || moves[i] == "") continue;
            string[] parts = moves[i].Split(' ');


            for (int j = 0; j < stockfishResults.Length / 2; j++)
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
            Debug.Log(BoardHelper.GetMoveNameUCI(Search.StartSearch(depth, -2)));
            stopwatch.Stop();
            Debug.Log("Search Took: " + stopwatch.ElapsedMilliseconds + "ms");
        }


        if (testGen)
        {
            testGen = false;

            bool shouldGenInFirstList = currentGenIndex % 2 == 0;

            Span<Move> moves = MoveGenerator.GenerateMovesSlow();

            for (int i = 0; i < moves.Length; i++)
            {
                if (shouldGenInFirstList) moves1.Add(moves[i].data);
                else moves2.Add(moves[i].data);
            }

            currentGenIndex++;
        }

        if (testOrder)
        {
            testOrder = false;
            if (moves1.Count < 1 || moves2.Count < 1) return;

            for (int i = 0; i < moves1.Count; i++)
            {
                if (moves1[i] != moves2[i])
                {
                    Debug.Log("Difference at i=" + i);
                }
            }
        }


        if (hashPosition)
        {
            hashPosition = false;
            Debug.Log(string.Format("0x{0:X}", Zobrist.Hash()));
            Debug.Log("Internal Hash: " + string.Format("0x{0:X}", Board.currentZobrist));

            ulong difference = Zobrist.Hash() ^ Board.currentZobrist;
            Debug.Log("Difference: " + string.Format("0x{0:X}", difference));
            if (difference == 0) Debug.Log("Hash Matches Internal State");
            else Debug.Log("Hashes don't align!!!");
        }
    }
}