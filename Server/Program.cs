using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Server;

var server = new ServerObject();
await server.ListenAsync();
 
class ServerObject
{
    private TcpListener _tcpListener;
    private List<ClientObject> _clients;

    string[] _separatingStrings;
    private InvertedIndex _invertedIndex;

    public ServerObject()
    {
        _tcpListener = new TcpListener(IPAddress.Any, 8888);
        _clients = new List<ClientObject>();

        _separatingStrings = new[] { ". ", ",", "<br", " ", ":", ";", "/>", "<br/>", "\"", "?" };
        _invertedIndex = new InvertedIndex(@"../../../../files", _separatingStrings);
        _invertedIndex.GenerateDictionary();
    }

    internal string FindInvertedIndex(string text)
    {
        var result = _invertedIndex[text];
        return result == null ? null! : JsonSerializer.Serialize(result).Replace(@"\\", @"\");
    }

    protected internal async Task ListenAsync()
    {
        try
        {
            _tcpListener.Start();
            Console.WriteLine("Server is started. Waiting for connections...");
            while (true)
            {
                var tcpClient = await _tcpListener.AcceptTcpClientAsync();
                var clientObject = new ClientObject(tcpClient, this);
                _clients.Add(clientObject);
                await Task.Run(clientObject.ProcessAsync);
            }
        }
        catch (Exception ex) { Console.WriteLine(ex.Message); }
        finally { Disconnect(); }
    }

    protected internal async Task BroadcastMessageAsync(string message, string id)
    {
        var client = _clients.FirstOrDefault(c => c.Id == id);
        if (client == null) return;
        // var encryptMessage = _cipher.Encrypt(message, _password);
        Console.WriteLine(message.Length);
        await client.Writer.WriteLineAsync(message);
        await client.Writer.FlushAsync();
    }
    
    protected internal void RemoveConnection(string id)
    {
        var client = _clients.FirstOrDefault(c => c.Id == id);
        if (client != null) _clients.Remove(client);
        client?.Close();
    }
    
    protected internal void Disconnect()
    {
        foreach (var client in _clients) { client.Close(); }
        _tcpListener.Stop();
    }
}
class ClientObject
{
    protected internal string Id { get; } = Guid.NewGuid().ToString();
    protected internal StreamWriter Writer { get;}
    protected internal StreamReader Reader { get;}
 
    private TcpClient _client;
    private ServerObject _server;
 
    public ClientObject(TcpClient tcpClient, ServerObject serverObject)
    {
        _client = tcpClient;
        _server = serverObject;
        var stream = _client.GetStream();
        Reader = new StreamReader(stream);
        Writer = new StreamWriter(stream);
    }
 
    public async Task ProcessAsync()
    {
        try
        {
            var userName = await Reader.ReadLineAsync();
            var message = $"{userName} enter to chat";
            Console.WriteLine(message);
            while (true)
            {
                try
                {
                    message = await Reader.ReadLineAsync();
                    if (message == null) continue;
                    // var decryptMessage = _server.DecryptMessage(message);
                    Console.WriteLine($"Client receive file with text {message}");
                    var result = _server.FindInvertedIndex(message);
                    await _server.BroadcastMessageAsync(result, Id);
                }
                catch
                {
                    message = $"{userName} leave's chat";
                    Console.WriteLine(message);
                    break;
                }
            }
        }
        catch (Exception e) { Console.WriteLine(e.Message); }
        finally { _server.RemoveConnection(Id); }
    }

    protected internal void Close()
    {
        Writer.Close();
        Reader.Close();
        _client.Close();
    }
}