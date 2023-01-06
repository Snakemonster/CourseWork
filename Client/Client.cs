using System.Net.Sockets;
using System.Text.Json;

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
        catch (Exception ex) { Console.WriteLine(ex.Message); }

        _writer?.Close();
        _reader?.Close();
    }

    async Task SendMessageAsync(StreamWriter writer)
    {
        await writer.WriteLineAsync(Name);
        await writer.FlushAsync();
        Console.Write("Write which word you want to found (only lowercase and only one word) from server than hit " +
                      "Enter or if you want exit write \"Exit\"\n: ");
        while (true)
        {
            var message = Console.ReadLine();
            if(message == "Exit") break;
            Console.WriteLine($"You have choose word \"{message}\", please wait for server...");
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
                if (string.IsNullOrEmpty(message))
                {
                    Console.WriteLine("This word doesn't exist in files on server or you write wrong word");
                    continue;
                }
                Print(message);
            }
            catch { break; }
        }
    }

    void Print(string message)
    {
        var res = JsonSerializer.Deserialize<Dictionary<string, int>>(message);
        foreach (var result in res) { Console.WriteLine($"{result.Key.Replace(@"/", @"\")} :{result.Value}"); }
    }
}