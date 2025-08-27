using System.Collections.Generic;
using UnityEngine;

namespace EngVEng
{
    public class UCIManager : MonoBehaviour
    {
        public static string Fen;
        [SerializeField] private EngineManager engineManager;

        void Start()
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
            engineManager.TellEngine(engineToStart, "go");
        }

        private List<string> playedMoves = new List<string>();

        public void InterpretResponse(int sender, string message)
        {
            string[] args = message.Split(' ');

            switch (args[0])
            {
                case "bestmove":
                    RecieveBestMove(sender, args[1]);
                    break;
                default:
                    break; //Ignore
            }
        }

        private void RecieveBestMove(int sender, string move)
        {
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
            else if (playedMoves.Count > 350)
            {
                Debug.Log("Draw bc exceeded max moves");
                engineManager.Draw();
                return;
            }

            int otherEngine = GetOtherEngineIndex(sender);
            engineManager.TellEngine(otherEngine, GetMoveString());
            engineManager.TellEngine(otherEngine, "go");
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