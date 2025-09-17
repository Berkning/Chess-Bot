using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace EngVEng
{
    public interface UCIManager
    {
        public void Initialize();

        public void ResetGame();

        public void InterpretResponse(int sender, string message);
    }

    public class EngineManager : MonoBehaviour
    {
        [SerializeField] private int gamesToPlay;
        private int playedGames = 0;

        [Space, SerializeField] private UCIManager uciManager;
        [SerializeField] private ResultManager resultManager;
        [SerializeField] private string engine1Path;
        [SerializeField] private string engine2Path;
        [Space, SerializeField] private string debugMessage;
        [SerializeField] private bool send;
        [SerializeField, Range(0, 2)] private int target;
        [SerializeField] private float moveDelay = 1f;
        [Space, SerializeField] private bool logAll;

        Process engine1Process;
        StreamWriter engine1Input;
        StreamReader engine1Output;
        Process engine2Process;
        StreamWriter engine2Input;
        StreamReader engine2Output;

        StreamReader engine1ErrorOutput;
        StreamReader engine2ErrorOutput;

        ConcurrentQueue<string> engine1ResponseQueue = new ConcurrentQueue<string>();
        ConcurrentQueue<string> engine2ResponseQueue = new ConcurrentQueue<string>();

        private bool attemptingRestart = false;

        public void Win(int winner)
        {
            if (winner == 1) resultManager.Engine1Wins++;
            else resultManager.Engine2Wins++;

            playedGames++;

            if (playedGames < gamesToPlay) uciManager.ResetGame(); //Play again
            else UnityEngine.Debug.Log("Finished");
        }

        public void Draw()
        {
            resultManager.DrawCount++;

            playedGames++;

            if (playedGames < gamesToPlay) uciManager.ResetGame(); //Play again
            else
            {
                UnityEngine.Debug.Log("Finished");
                StopCoroutine(UpdateUCI());
            }
        }


        void Start()
        {
            uciManager = TournamentManager.instance;

            StartEngine(1);
            StartEngine(2);

            _ = EngineOutputReader(engine1Output, 1, false);
            _ = EngineOutputReader(engine2Output, 2, false);
            _ = EngineOutputReader(engine1ErrorOutput, 1, true);
            _ = EngineOutputReader(engine2ErrorOutput, 2, true);

            StartCoroutine(UpdateUCI());

            uciManager.Initialize();
        }

        void StartEngine(int id)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = id == 1 ? engine1Path : engine2Path,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true
            };

            Process engineProcess = new Process { StartInfo = startInfo };
            engineProcess.Start();

            StreamWriter engineInput = engineProcess.StandardInput;
            StreamReader engineOutput = engineProcess.StandardOutput;
            StreamReader engineErrorOutput = engineProcess.StandardError;

            // Example: send a UCI command
            engineInput.WriteLine("uci");
            engineInput.Flush();

            if (id == 1)
            {
                engine1Process = engineProcess;
                engine1Input = engineInput;
                engine1Output = engineOutput;
                engine1ErrorOutput = engineErrorOutput;
            }
            else
            {
                engine2Process = engineProcess;
                engine2Input = engineInput;
                engine2Output = engineOutput;
                engine2ErrorOutput = engineErrorOutput;
            }
        }

        private IEnumerator UpdateUCI()
        {
            while (true)
            {
                string response;

                if (engine1ResponseQueue.TryDequeue(out response))
                {
                    uciManager.InterpretResponse(1, response);
                }

                if (engine2ResponseQueue.TryDequeue(out response))
                {
                    uciManager.InterpretResponse(2, response);
                }

                if (moveDelay > 0f) yield return new WaitForSeconds(moveDelay);
                else yield return null;
            }
        }



        private async Awaitable EngineOutputReader(StreamReader output, int id, bool isErrorOutput)
        {
            await Awaitable.BackgroundThreadAsync();

            while (true)
            {
                try
                {
                    string line = await output.ReadLineAsync();
                    if (line == null)
                    {
                        if (!isErrorOutput) UnityEngine.Debug.Log("Quitting Output Reader for engine" + id);
                        else UnityEngine.Debug.Log("Quitting Error Output Reader for engine" + id);
                        break;
                    }

                    if (!isErrorOutput)
                    {
                        if (logAll) UnityEngine.Debug.Log("Engine" + id + " says " + line);

                        if (id == 1) engine1ResponseQueue.Enqueue(line);
                        else engine2ResponseQueue.Enqueue(line);
                    }
                    else
                    {
                        UnityEngine.Debug.LogError("Engine" + id + " says " + line);

                        await Awaitable.MainThreadAsync();
                        if (!attemptingRestart)
                        {
                            UnityEngine.Debug.LogError("EngineError. Attempting to restart engine");
                            UnityEngine.Debug.LogError("Restarting...");
                            StartEngine(id);

                            uciManager.ResetGame();

                            attemptingRestart = true;
                        }
                        else UnityEngine.Debug.Log("Awaiting restart...");

                        await Awaitable.BackgroundThreadAsync();

                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError("Encountered Output reader error: " + e);

                    if (!isErrorOutput) UnityEngine.Debug.Log("Quitting Output Reader for engine" + id);
                    else UnityEngine.Debug.Log("Quitting Error Output Reader for engine" + id);
                    break;
                }
            }
        }





        public void TellEngine(int id, string message)
        {
            if (logAll) UnityEngine.Debug.Log("Telling engine" + id + " " + message);

            if (id == 1)
            {
                engine1Input.WriteLine(message);
                engine1Input.Flush();
            }
            else
            {
                engine2Input.WriteLine(message);
                engine2Input.Flush();
            }
        }


        void OnValidate()
        {
            if (send)
            {
                send = false;

                if (target == 0 || target == 1) TellEngine(1, debugMessage);
                if (target == 0 || target == 2) TellEngine(2, debugMessage);
            }
        }


        void OnApplicationQuit()
        {
            if (!engine1Process.HasExited)
                engine1Process.Kill();

            if (!engine2Process.HasExited)
                engine2Process.Kill();

            StopCoroutine(UpdateUCI());
        }
    }
}