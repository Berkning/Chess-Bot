
public static class Program
{
    public static UnityPipe pipe = new UnityPipe();

    public static void Main(string[] args)
    {

        EngineUCI engineUCI = new EngineUCI();
        if (args.Length != 0)
        {
            if (args[0] == "bench")
            {
                //OpeningBook.Initialize();

                //Kinda janky but works
                engineUCI.RecieveCommand("bench");

                return;
            }
        }



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