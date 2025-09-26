using System;
using System.Collections.Generic;
using Analysis;
using TMPro;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace Analysis
{
    public class EngineAnalysis : MonoBehaviour
    {
        [SerializeField] private EngineController engine;
        [SerializeField] private Slider evalBar;
        private float currentEval = 0f;
        //private float timer = 0f;
        [SerializeField] private float evalReactionSpeed = 0.5f;

        [SerializeField] private TMP_Text evalText;
        [SerializeField] private int time;
        [SerializeField] private string position;
        private string posString;

        private string moveString = " moves";

        void Start()
        {
            if (position == "startpos") posString = "position startpos";
            else posString = "position fen " + position;

            engine.TellEngine("table size 512");
            engine.TellEngine(posString);

            engine.TellEngine("go movetime " + time);



            engine.onEngineResponded.AddListener(RecieveResponse);
        }

        void Update()
        {
            //if (timer >= 1f)
            //{
            //evalBar.value = 0.1125f * currentEval + 0.5f;
            //return;
            //}

            //timer += Time.deltaTime * evalReactionSpeed;

            evalBar.value = Mathf.MoveTowards(evalBar.value, 0.1125f * currentEval + 0.5f, Time.deltaTime * evalReactionSpeed);
        }



        public void PlayMove(string move)
        {
            moveString += " " + move;
            SendMoves();
        }

        private void SendMoves()
        {
            engine.TellEngine("stop");
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
                case "bestmove":
                    BoardGraphics.instance.ShowMove(BoardHelper.GetMoveFromUCIName(args[1]));
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
                else if (args[i] == "mate")
                {
                    int mateScore = int.Parse(args[i + 1]);
                    cp = Math.Sign(mateScore) * 10000;
                }
                else if (args[i] == "pv")
                {
                    BoardGraphics.instance.ShowMove(BoardHelper.GetMoveFromUCIName(args[i + 1]));
                }
            }

            if (cp != int.MinValue) SetEval(cp);
        }


        private void SetEval(int cp)
        {
            //timer = 0f;
            //prevEval = currentEval;

            float perspective = Board.colorToMove == Piece.Black ? -1f : 1f;

            currentEval = cp / 100f * perspective;
            evalText.text = currentEval.ToString("F1");
        }

        void OnApplicationQuit()
        {
            engine.onEngineResponded.RemoveAllListeners();
        }
    }
}