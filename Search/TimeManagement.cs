using System.Threading;
using System.Threading.Tasks;

public static class TimeManagement
{
    public static int whiteTime = 0;
    public static int blackTime = 0;
    public static int whiteIncrement = 0;
    public static int blackIncrement = 0;

    private static CancellationTokenSource source = new CancellationTokenSource();

    public static int GetSearchTime(int colorToMove)
    {
        if (colorToMove == Piece.White) return whiteTime / 20 + whiteIncrement / 2; //Spend max 20th of our total time on one move 

        return blackTime / 20 + blackIncrement / 2;
    }

    public static void UpdateTimes(int white, int black, int winc, int binc)
    {
        whiteTime = white;
        blackTime = black;

        whiteIncrement = winc;
        blackIncrement = binc;
    }
}