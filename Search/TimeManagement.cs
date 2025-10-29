using System.Threading;
using System.Threading.Tasks;

public static class TimeManagement
{
    public static int whiteTime = 60000;
    public static int blackTime = 60000;

    private static CancellationTokenSource source = new CancellationTokenSource();

    public static int GetSearchTime(int colorToMove)
    {
        if (colorToMove == Piece.White) return whiteTime / 20; //Spend max 20th of our total time on one move

        return blackTime / 20;
    }

    public static void UpdateTimes(int white, int black)
    {
        whiteTime = white;
        blackTime = black;
    }

    public static void ScheduleSearchCancel(int time)
    {
        source.Cancel();

        source = new CancellationTokenSource();
        Task.Run(() => CancelTimer(source.Token, time));
    }

    private static async Task CancelTimer(CancellationToken token, int time)
    {
        //Debug.Log("Cancel Timer Started");
        //await Awaitable.BackgroundThreadAsync();


        await Task.Delay(time, token);

        if (!token.IsCancellationRequested)
        {
            //Debug.Log("Cancelling");
            Search.cancelSearch = true;
        }
        else
        {
            //Debug.Log("Timer Cancelled");
        }
    }
}