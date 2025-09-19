using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace Analysis
{
    public class EngineController : MonoBehaviour
    {
        [SerializeField] private string enginePath;
        [Space, SerializeField] private bool logAll;

        Process engineProcess;
        StreamWriter engineInput;
        StreamReader engineOutput;

        StreamReader engineErrorOutput;

        ConcurrentQueue<string> engineResponseQueue = new ConcurrentQueue<string>();

        public UnityEvent<string> onEngineResponded = new UnityEvent<string>();

        void Start()
        {
            StartEngine();

            _ = EngineOutputReader(engineOutput, false);
            _ = EngineOutputReader(engineErrorOutput, true);

            StartCoroutine(UpdateUCI());
        }


        void StartEngine()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = enginePath,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true
            };

            engineProcess = new Process { StartInfo = startInfo };
            engineProcess.Start();

            engineInput = engineProcess.StandardInput;
            engineOutput = engineProcess.StandardOutput;
            engineErrorOutput = engineProcess.StandardError;

            // Example: send a UCI command
            engineInput.WriteLine("uci");
            engineInput.Flush();
        }

        private IEnumerator UpdateUCI()
        {
            while (true)
            {
                string response;

                if (engineResponseQueue.TryDequeue(out response))
                {
                    onEngineResponded.Invoke(response);
                    //EngineAnalysis.InterpretResponse(response);
                }

                yield return null;
            }
        }



        private async Awaitable EngineOutputReader(StreamReader output, bool isErrorOutput)
        {
            await Awaitable.BackgroundThreadAsync();

            while (true)
            {
                try
                {
                    string line = await output.ReadLineAsync();
                    if (line == null)
                    {
                        if (!isErrorOutput) UnityEngine.Debug.Log("Quitting Output Reader for engine");
                        else UnityEngine.Debug.Log("Quitting Error Output Reader for engine");
                        break;
                    }

                    if (!isErrorOutput)
                    {
                        if (logAll) UnityEngine.Debug.Log("Engine says " + line);

                        engineResponseQueue.Enqueue(line);
                    }
                    else
                    {
                        UnityEngine.Debug.LogError("Engine says " + line);
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError("Encountered Output reader error: " + e);

                    if (!isErrorOutput) UnityEngine.Debug.Log("Quitting Output Reader for engine");
                    else UnityEngine.Debug.Log("Quitting Error Output Reader for engine");
                    break;
                }
            }
        }





        public void TellEngine(string message)
        {
            if (logAll) UnityEngine.Debug.Log("Telling engine " + message);


            engineInput.WriteLine(message);
            engineInput.Flush();
        }


        void OnApplicationQuit()
        {
            if (!engineProcess.HasExited) engineProcess.Kill();

            StopCoroutine(UpdateUCI());
        }
    }
}