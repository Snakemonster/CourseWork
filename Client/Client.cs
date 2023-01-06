using System.Net.Sockets;

namespace TCPClient;

public class Client
{
    public string Host { get; }
    public string Name { get; }
    public int Port { get; }
    private StreamReader? _reader;
    private StreamWriter? _writer;

    public Client(string name, string host = "127.0.0.1", int port = 8888)
    {
        Host = host;
        Name = name;
        Port = port;
        _reader = null;
        _writer = null;
    }

    public async void StartClient()
    {
        using var client = new TcpClient();
        try
        {
            client.Connect(Host, Port);
            _reader = new StreamReader(client.GetStream());
            _writer = new StreamWriter(client.GetStream());
            if (_writer is null || _reader is null) return;
            Task.Run(() => ReceiveMessageAsync(_reader));
            await SendMessageAsync(_writer);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        _writer?.Close();
        _reader?.Close();
    }

    async Task SendMessageAsync(StreamWriter writer)
    {
        await writer.WriteLineAsync(Name);
        await writer.FlushAsync();
        Console.WriteLine("For sending texting write your message and then hit Enter");

        while (true)
        {
            string? message = Console.ReadLine();
            await writer.WriteLineAsync(message);
            await writer.FlushAsync();
        }
    }
    
    
    async Task ReceiveMessageAsync(StreamReader reader)
    {
        while (true)
        {
            try
            {
                var message = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(message)) continue;
                Print(message);
            }
            catch
            {
                break;
            }
        }
    }

    void Print(string message)
    {
        if (OperatingSystem.IsWindows())
        {
            var position = Console.GetCursorPosition();
            var left = position.Left;
            var top = position.Top;
            Console.MoveBufferArea(0, top, left, 1, 0, top + 1);
            Console.SetCursorPosition(0, top);
            Console.WriteLine(message);
            Console.SetCursorPosition(left, top + 1);
        }
        else Console.WriteLine(message);
    }
}