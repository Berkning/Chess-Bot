using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace EngVEng
{
    public class BenchmarkManager : MonoBehaviour, UCIManager
    {
        public static BenchmarkManager instance;

        public static string Fen;
        [SerializeField] private EngineManager engineManager;
        [SerializeField] private int depth = 7;
        [SerializeField] private long minMarginMS = 15;
        private Stopwatch engine1Timer = new Stopwatch();
        private Stopwatch engine2Timer = new Stopwatch();
        private bool winnerFound = false; //Whether one of the engines has finished
        private long winnerTime = long.MaxValue;

        [Space, Header("Results")]
        [SerializeField] private long totalMargin1; //Total ms saved by this engine
        [SerializeField] private long totalMargin2; //Total ms saved by this engine
        [SerializeField] private List<float> lossPercent1; //List of the percentage difference in time between the engines everytime engine 1 loses
        [SerializeField] private List<float> lossPercent2; //List of the percentage difference in time between the engines everytime engine 2 loses
        [SerializeField] private float avgLoss1;
        [SerializeField] private float avgLoss2;

        void Awake()
        {
            instance = this;
        }

        public void Initialize()
        {
            ResetGame();
        }

        public void InterpretResponse(int sender, string message)
        {
            if (!message.Contains("bestmove")) return;

            if (sender == 1)
            {
                engine1Timer.Stop();
                UnityEngine.Debug.Log("Engine 1 Finished after: " + engine1Timer.ElapsedMilliseconds + "ms");

                if (winnerFound) //Means this engine finished last
                {
                    long margin = engine1Timer.ElapsedMilliseconds - winnerTime;
                    float percentage = (float)margin / winnerTime * 100f; //Literally do not understand why i need to calculate it here and cant add this to list direcly, doesn't make any fucking sense

                    //UnityEngine.Debug.Log("Margin: " + (float)margin / winnerTime * 100f);

                    if (margin >= minMarginMS)
                    {
                        UnityEngine.Debug.Log("Engine 2 Won!");
                        engineManager.Win(2);

                        totalMargin2 += margin;
                        lossPercent1.Add(percentage);

                        avgLoss1 = GetAverage(lossPercent1);
                    }
                    else
                    {
                        UnityEngine.Debug.Log("Draw by insufficient margin");
                        engineManager.Draw();
                    }
                }
                else
                {
                    winnerFound = true;
                    winnerTime = engine1Timer.ElapsedMilliseconds;
                }
            }
            else
            {
                engine2Timer.Stop();
                UnityEngine.Debug.Log("Engine 2 Finished after: " + engine2Timer.ElapsedMilliseconds + "ms");

                if (winnerFound) //Means this engine finished last
                {
                    long margin = engine2Timer.ElapsedMilliseconds - winnerTime;
                    float percentage = (float)margin / winnerTime * 100f; //Literally do not understand why i need to calculate it here and cant add this to list direcly, doesn't make any fucking sense

                    //UnityEngine.Debug.Log("Margin: " + percentage);

                    if (margin >= minMarginMS)
                    {
                        UnityEngine.Debug.Log("Engine 1 Won!");
                        engineManager.Win(1);

                        totalMargin1 += margin;
                        lossPercent2.Add(percentage);

                        avgLoss2 = GetAverage(lossPercent2);
                    }
                    else
                    {
                        UnityEngine.Debug.Log("Draw by insufficient margin");
                        engineManager.Draw();
                    }
                }
                else
                {
                    winnerFound = true;
                    winnerTime = engine2Timer.ElapsedMilliseconds;
                }
            }
        }

        public void ResetGame()
        {
            winnerFound = false;
            winnerTime = long.MaxValue;

            Fen = PositionGenerator.instance.GetRandomFen();

            UnityEngine.Debug.Log("Picked fen: " + Fen);

            engineManager.TellEngine(1, "ucinewgame");
            engineManager.TellEngine(2, "ucinewgame");

            FenUtility.LoadPositionFromFen(Fen);

            engineManager.TellEngine(1, "position fen " + Fen);
            engineManager.TellEngine(2, "position fen " + Fen);


            engine1Timer.Restart();
            engineManager.TellEngine(1, "go depth " + depth);
            engine2Timer.Restart();
            engineManager.TellEngine(2, "go depth " + depth);
        }


        private float GetAverage(List<float> list)
        {
            float result = 0;

            for (int i = 0; i < list.Count; i++) result += list[i];

            return result / list.Count;
        }
    }
}