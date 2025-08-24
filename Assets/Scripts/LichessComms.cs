using System.Collections.Concurrent;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class LichessComms : MonoBehaviour
{
    private ConcurrentQueue<EngineCommand> commandQueue = new ConcurrentQueue<EngineCommand>();
    private CancellationTokenSource cancelSource = new CancellationTokenSource();

    void Start()
    {
        CancellationToken token = cancelSource.Token;

        RunClient(token);

        CommandReciever(token);
    }

    void OnDisable()
    {
        cancelSource.Cancel();
    }

    public struct EngineCommand
    {
        public enum CommandType { LoadFen, PlayMove, Stop, Debug };

        public CommandType command;

        public object data;

        public EngineCommand(CommandType _command)
        {
            command = _command;
            data = null;
        }

        public EngineCommand(CommandType _command, string s)
        {
            command = _command;
            data = s;
        }

        public EngineCommand(ushort moveData)
        {
            command = CommandType.PlayMove;
            data = moveData;
        }

    }

    //Main Thread
    private async Task CommandReciever(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (commandQueue.TryDequeue(out EngineCommand command))
            {
                RunCommand(command);
            }

            await Task.Yield();
        }

        Debug.Log("Command Reciever Cancelled");
    }

    private void RunCommand(EngineCommand command)
    {
        switch (command.command)
        {
            case EngineCommand.CommandType.LoadFen:
                break;
            case EngineCommand.CommandType.PlayMove:
                break;
            case EngineCommand.CommandType.Stop:
                break;
            case EngineCommand.CommandType.Debug:
                Debug.Log(command.data);
                break;
        }
    }





    //Background Thread
    private async Awaitable RunClient(CancellationToken token)
    {
        await Awaitable.BackgroundThreadAsync();
        Debug.Log("Pipe Client Moved to Background Thread");

        using (NamedPipeClientStream pipeClient =
            new NamedPipeClientStream(".", "LichessToUnity", PipeDirection.InOut, PipeOptions.Asynchronous))
        {

            // Connect to the pipe or wait until the pipe is available.
            Debug.Log("Attempting to connect to server...");
            pipeClient.Connect();

            Debug.Log("Connected to Server.");
            //Debug.Log("There are currently {0} pipe server instances open.", pipeClient.NumberOfServerInstances);

            using (BinaryReader reader = new BinaryReader(pipeClient, Encoding.UTF8, leaveOpen: true))
            {
                using (BinaryWriter writer = new BinaryWriter(pipeClient, Encoding.UTF8, leaveOpen: true))
                {
                    while (!token.IsCancellationRequested)
                    {
                        // Wait for command
                        string command = ReadMessage(reader);
                        //Debug.Log("Got command: " + command);


                        string reply = await InterpretUCI(command);



                        WriteMessage(writer, reply);
                        Debug.Log("Sent reply: " + reply);

                        if (command == "quit") break;
                    }

                    reader.Close();
                    writer.Close();
                }
            }
        }
        Debug.Log("Connection Quit");
    }

    //Returns the response string
    private async Task<string> InterpretUCI(string command)
    {
        string[] args = command.Split(' ');

        switch (args[0])
        {
            case "position":
                commandQueue.Enqueue(CreatePositionCommand(args));
                return "Position Loaded"; //TODO:
            case "go":
                commandQueue.Enqueue(CreateGoCommand(args));
                await Task.Delay(1000);
                return "bestmove a1a1"; //TODO: only return after done searching
            case "stop":
                commandQueue.Enqueue(new EngineCommand(EngineCommand.CommandType.Stop));
                return "bestmove a1a1"; //TODO: only return after search is cancelled and has returned
            case "debug":
                commandQueue.Enqueue(new EngineCommand(EngineCommand.CommandType.Debug, command.Remove(0, 6)));
                return "Logged Debug Message";
            default:
                Debug.LogError("Command '" + command + "' not recognized by uci interpreter");
                return "Command Not Recognised";
        }
    }

    private EngineCommand CreatePositionCommand(string[] args)
    {
        return new EngineCommand(EngineCommand.CommandType.LoadFen);
    }

    private EngineCommand CreateGoCommand(string[] args)
    {
        return new EngineCommand(EngineCommand.CommandType.PlayMove);
    }





    private void WriteMessage(BinaryWriter writer, string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        writer.Write(data.Length);
        writer.Write(data);
        writer.Flush();
    }

    private string ReadMessage(BinaryReader reader)
    {
        int length = reader.ReadInt32();
        byte[] data = reader.ReadBytes(length);
        return Encoding.UTF8.GetString(data);
    }
}