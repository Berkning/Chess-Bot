using UnityEngine;
using System.Collections.Generic;

namespace EngVEng
{
    public class TournamentManager : MonoBehaviour, UCIManager
    {
        public static TournamentManager instance;

        public static string Fen;
        [SerializeField] private EngineManager engineManager;
        [SerializeField] private EngineTimer engineTimer;

        [Space, Header("Tournament")]
        [SerializeField] private int thinkTimeMs = 100;
        [SerializeField] private int maxMoveCount = 150;

        void Awake()
        {
            instance = this;
        }

        public void Initialize()
        {
            ResetGame();
        }

        public void ResetGame()
        {
            playedMoves.Clear();

            Fen = PositionGenerator.instance.GetRandomFen();

            engineManager.TellEngine(1, "ucinewgame");
            engineManager.TellEngine(2, "ucinewgame");

            FenUtility.LoadPositionFromFen(Fen);

            int engineToStart = Random.Range(1, 3); //Random number from 1-2 - 3 is exluded

            engineManager.TellEngine(engineToStart, "position fen " + Fen);

            engineTimer.RestartEngineTimer();
            engineManager.TellEngine(engineToStart, "go movetime " + thinkTimeMs);
        }

        private List<string> playedMoves = new List<string>();

        public void InterpretResponse(int sender, string message)
        {
            string[] args = message.Split(' ');

            switch (args[0])
            {
                case "bestmove":
                    if (args[1] == "a1a1")
                    {
                        Debug.LogError("Invalid null move");
                        ResetGame();
                        return;
                    }
                    RecieveBestMove(sender, args[1]);
                    break;
                default:
                    break; //Ignore
            }
        }


        private bool CheckForRepetition(string repeatedMove)
        {
            int matchesFound = 0;

            for (int i = 0; i < playedMoves.Count; i++)
            {
                if (playedMoves[i] == repeatedMove) matchesFound++;
            }

            return matchesFound > 6;
        }


        private void RecieveBestMove(int sender, string move)
        {
            engineTimer.StopEngineTimer(sender);
            playedMoves.Add(move);

            Board.MakeMove(BoardHelper.GetMoveFromUCIName(move));

            if (MoveGenerator.GenerateMovesSlow().Length < 1) //TODO: kind of trash that we are just generating all moves in the current position just to check this, and then regenerating them in the search, but idk if its practical to avoid
            {
                //Either checkmate or stalemate
                if (MoveGenerator.inCheck)
                {
                    Debug.Log("Checkmate! - Engine" + sender + " Wins!");
                    engineManager.Win(sender);
                }
                else
                {
                    Debug.Log("Stalemate");
                    engineManager.Draw();
                }

                return;
            }
            else if (playedMoves.Count > maxMoveCount)
            {
                Debug.Log("Draw bc exceeded max moves");
                engineManager.Draw();
                return;
            }
            else if (playedMoves.Contains(move))
            {
                if (CheckForRepetition(move))
                {
                    Debug.Log("Draw By Repetition");
                    engineManager.Draw();
                    return;
                }
            }

            engineTimer.RestartEngineTimer();
            int otherEngine = GetOtherEngineIndex(sender);
            engineManager.TellEngine(otherEngine, GetMoveString());
            engineManager.TellEngine(otherEngine, "go movetime " + thinkTimeMs);
        }


        private string GetMoveString()
        {
            string result = "position fen " + Fen + " moves ";

            for (int i = 0; i < playedMoves.Count; i++)
            {
                result += playedMoves[i];
                if (i < playedMoves.Count - 1) result += ' ';
            }

            return result;
        }

        private int GetOtherEngineIndex(int id)
        {
            return id == 1 ? 2 : 1;
        }
    }
}