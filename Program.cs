
public static class Program
{
    public static UnityPipe pipe = new UnityPipe();

    public static void Main(string[] args)
    {
        EngineUCI engineUCI = new EngineUCI();
        //pipe.InitializePipe();

        string message = String.Empty;

        while (message != "quit")
        {
            message = Console.ReadLine();
            engineUCI.RecieveCommand(message);
        }

        //pipe.shouldShutDown = true;
    }
}