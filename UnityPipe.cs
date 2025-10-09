using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class UnityPipe
{
    public bool shouldShutDown;
    public string nextCommand = "";

    public async Task InitializePipe()
    {
        Console.WriteLine("Initializing Unity Pipe...");

        using (NamedPipeServerStream pipeServer =
            new NamedPipeServerStream("LichessToUnity", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
        {
            // Wait for a client to connect
            //Console.Write("Waiting for client connection...");
            pipeServer.WaitForConnection();

            //Console.WriteLine("Client connected.");
            await RunServer(pipeServer);
        }
    }

    private async Task RunServer(NamedPipeServerStream pipeServer)
    {
        try
        {
            using (BinaryWriter writer = new BinaryWriter(pipeServer, Encoding.UTF8, leaveOpen: true))
            using (BinaryReader reader = new BinaryReader(pipeServer, Encoding.UTF8, leaveOpen: true))
            {
                while (true)
                {
                    if (nextCommand != "")
                    {
                        //Console.WriteLine("Sending Command");
                        WriteMessage(writer, nextCommand);

                        // Wait for reply
                        string reply = ReadMessage(reader);
                        Console.WriteLine(reply);

                        nextCommand = "";
                    }

                    await Task.Yield();

                    if (shouldShutDown) break;
                }

                WriteMessage(writer, "quit");

                writer.Close();
                reader.Close();
            }
            Console.WriteLine("Connection Lost");
        }
        // Catch the IOException that is raised if the pipe is broken
        // or disconnected.
        catch (IOException e)
        {
            Console.WriteLine("ERROR: {0}", e.Message);
        }
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
