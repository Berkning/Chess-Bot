using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static class TimeManagement
{
    private static int whiteTime = 60000;
    private static int blackTime = 60000;

    private static CancellationTokenSource source = new CancellationTokenSource();

    public static int GetSearchTime()
    {
        return 100; //Spend max 20th of our total time on one move
    }

    public static void ScheduleSearchCancel()
    {
        source.Cancel();

        source = new CancellationTokenSource();
        Task.Run(() => CancelTimer(source.Token));
    }

    private static async Task CancelTimer(CancellationToken token)
    {
        Debug.Log("Cancel Timer Started");
        //await Awaitable.BackgroundThreadAsync();


        await Task.Delay(GetSearchTime(), token);

        if (!token.IsCancellationRequested)
        {
            Debug.Log("Cancelling");
            Search.cancelSearch = true;
        }
        else
        {
            Debug.Log("Timer Cancelled");
        }
    }
}