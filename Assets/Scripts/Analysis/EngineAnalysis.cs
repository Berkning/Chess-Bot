using System.Collections.Generic;
using Analysis;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Analysis
{
    public class EngineAnalysis : MonoBehaviour
    {
        [SerializeField] private EngineController engine;
        [SerializeField] private Slider evalBar;
        [SerializeField] private TMP_Text evalText;
        [SerializeField] private int time;
        [SerializeField] private string position;
        private string posString;

        private string moveString = " moves";

        void Start()
        {
            if (position == "startpos") posString = "position startpos";
            else posString = "position fen " + position;

            engine.TellEngine(posString);

            engine.TellEngine("go movetime " + time);


            engine.onEngineResponded.AddListener(RecieveResponse);
        }

        public void PlayMove(string move)
        {
            moveString += " " + move;
            SendMoves();
        }

        private void SendMoves()
        {
            engine.TellEngine(posString + moveString);
            engine.TellEngine("go movetime " + time);
        }



        private void RecieveResponse(string response)
        {
            string[] args = response.Split(' ');

            switch (args[0])
            {
                case "info":
                    InterpretInfoCommand(args);
                    break;
            }
        }

        private void InterpretInfoCommand(string[] args)
        {
            int cp = int.MinValue;

            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "cp")
                {
                    cp = int.Parse(args[i + 1]);
                }
            }

            if (cp != int.MinValue) SetEval(cp);
        }


        private void SetEval(int cp)
        {
            float perspective = Board.colorToMove == Piece.Black ? -1f : 1f;

            float eval = cp / 100f * perspective;
            evalBar.value = 0.1125f * eval + 0.5f;
            evalText.text = eval.ToString("F1");
        }

        void OnApplicationQuit()
        {
            engine.onEngineResponded.RemoveAllListeners();
        }
    }
}