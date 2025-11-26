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
}