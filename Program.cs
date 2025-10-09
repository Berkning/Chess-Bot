
public static class Program
{
    public static UnityPipe pipe = new UnityPipe();

    public static void Main(string[] args)
    {
        EngineUCI engine = new EngineUCI();
        //pipe.InitializePipe();

        string message = String.Empty;

        while (message != "quit")
        {
            message = Console.ReadLine();
            engine.RecieveCommand(message);
        }

        //pipe.shouldShutDown = true;
    }
}