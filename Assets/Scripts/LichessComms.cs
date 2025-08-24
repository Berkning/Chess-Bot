using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        StartCoroutine(CommandReciever(token));
    }

    void OnDisable()
    {
        cancelSource.Cancel();
    }

    public struct EngineCommand
    {
        public enum CommandType { LoadFen, PlayMove, Stop, Debug, RandomMove };

        public TaskCompletionSource<string> completionSource;
        public CommandType command;

        public object data;

        public EngineCommand(CommandType _command)
        {
            command = _command;
            data = null;
            completionSource = new TaskCompletionSource<string>();
        }

        public EngineCommand(CommandType _command, string s)
        {
            command = _command;
            data = s;
            completionSource = new TaskCompletionSource<string>();
        }

        public EngineCommand(ushort moveData)
        {
            command = CommandType.PlayMove;
            data = moveData;
            completionSource = new TaskCompletionSource<string>();
        }
    }

    //Main Thread
    private IEnumerator CommandReciever(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (commandQueue.TryDequeue(out EngineCommand command))
            {
                Debug.Log("Command recieved on main thread");
                string result = RunCommand(command);
                command.completionSource.SetResult(result);
            }

            yield return null;
        }

        Debug.Log("Command Reciever Cancelled");
    }

    private string RunCommand(EngineCommand command)
    {
        string commandResult = "";

        switch (command.command)
        {
            case EngineCommand.CommandType.LoadFen:
                FenUtility.LoadPositionFromFen((string)command.data);
                break;
            case EngineCommand.CommandType.PlayMove:
                Board.MakeMove(BoardHelper.GetMoveFromUCIName((string)command.data));
                break;
            case EngineCommand.CommandType.Stop:
                break;
            case EngineCommand.CommandType.Debug:
                Debug.Log(command.data);
                break;
            case EngineCommand.CommandType.RandomMove:
                Move move = RandomBot.GetBestMove();
                commandResult = BoardHelper.GetMoveNameUCI(move);
                break;
        }

        return commandResult;
    }


    //Background Thread
    private bool isNewGame = true;
    private List<string> playedMoves = new List<string>();

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
            case "ucinewgame":
                isNewGame = true;
                playedMoves.Clear();
                return "Ready for new game";
            case "position":
                InterpretPositionCommand(args); //TODO: wait for position to load
                isNewGame = false;
                return "Position Loading";
            case "go":
                EngineCommand goCommand = CreateGoCommand(args);
                commandQueue.Enqueue(goCommand);
                string moveResponse = await goCommand.completionSource.Task;
                return "bestmove " + moveResponse; //TODO: only return after done searching
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

    private void InterpretPositionCommand(string[] args) //TODO: Add support for giving both a new position and some moves to play in that position from the start
    {
        if (isNewGame)
        {
            string fen = "";

            if (args[1] == "fen")
            {
                //Construct fen from args
                for (int i = 2; i < args.Length; i++)
                {
                    fen += args[i] + ' ';
                }
            }
            else fen = args[1]; //Prob startpos

            commandQueue.Enqueue(new EngineCommand(EngineCommand.CommandType.LoadFen, fen));
        }
        else
        {
            //Move command
            int moveStartIndex = args[1] == "startpos" ? 2 : 3;

            for (int i = moveStartIndex; i < args.Length; i++)
            {
                int playedMovesIndex = i - moveStartIndex;

                //If move has already been played we skip to the next one
                if (playedMoves.Count > playedMovesIndex && playedMoves[i - moveStartIndex] == args[i]) continue;


                commandQueue.Enqueue(CreateMoveCommand(args[i]));
                playedMoves.Add(args[i]);
            }
        }
    }

    private EngineCommand CreateMoveCommand(string moveString)
    {
        return new EngineCommand(EngineCommand.CommandType.PlayMove, moveString);
    }

    private EngineCommand CreateGoCommand(string[] args)
    {
        return new EngineCommand(EngineCommand.CommandType.RandomMove);
        //return new EngineCommand(EngineCommand.CommandType.Debug, "Go Command Not Implemented");
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