using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;

public class EngineTimer : MonoBehaviour
{
    [SerializeField] private TMP_Text engine1Text;
    [SerializeField] private TMP_Text engine2Text;
    private List<long> engine1Times = new List<long>();
    private List<long> engine2Times = new List<long>();
    private Stopwatch timer = new Stopwatch();



    public void RestartEngineTimer()
    {
        timer.Restart();
    }

    public void StopEngineTimer(int id)
    {
        timer.Stop();
        if (id == 1)
        {
            engine1Times.Add(timer.ElapsedMilliseconds);
        }
        else
        {
            engine2Times.Add(timer.ElapsedMilliseconds);
        }

        UpdateAverage(id);
    }

    private void UpdateAverage(int id)
    {
        if (id == 1)
        {
            long sum = 0;
            for (int i = 0; i < engine1Times.Count; i++)
            {
                sum += engine1Times[i];
            }
            engine1Text.text = "Engine 1: " + (sum / engine1Times.Count).ToString() + "ms";
        }
        else
        {
            long sum = 0;
            for (int i = 0; i < engine2Times.Count; i++)
            {
                sum += engine2Times[i];
            }
            engine2Text.text = "Engine 2: " + (sum / engine2Times.Count).ToString() + "ms";
        }
    }
}