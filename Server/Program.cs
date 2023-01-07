using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Server;
using XOR_Cipher;


var path = args.Length > 0 ? args[0] : string.Empty;
var server = new ServerObject(path);
var listenThread = new Thread(server.Listen);
listenThread.Start();
 
class ServerObject
{
    private TcpListener _tcpListener;
    private List<ClientObject> _clients;

    string[] _separatingStrings;
    private InvertedIndex _invertedIndex;

    private XORCipher _cipher;
    private string _password;
    
    public ServerObject(string pathToFolder)
    {
        _tcpListener = new TcpListener(IPAddress.Any, 8888);
        _clients = new List<ClientObject>();

        _separatingStrings = new[] { ". ", ",", "<br", " ", ":", ";", "/>", "<br/>", "\"", "?" };
        _invertedIndex = new InvertedIndex(pathToFolder == string.Empty ? @"../../../../files" : pathToFolder, _separatingStrings);
        _invertedIndex.GenerateDictionary();

        _cipher = new XORCipher();
        _password = XORCipher.GetRandomKey(4643, 20);
    }

    internal string DecryptMessage(string message) =>_cipher.Decrypt(message, _password);
    internal string FindInvertedIndex(string text)
    {
        var result = _invertedIndex[text];
        return result == null ? "empty" : JsonSerializer.Serialize(result).Replace(@"\\", @"/");
    }

    protected internal void Listen()
    {
        try
        {
            _tcpListener.Start();
            Console.WriteLine("Server is started. Waiting for connections...");
            while (true)
            {
                var tcpClient = _tcpListener.AcceptTcpClient();
                var clientObject = new ClientObject(tcpClient, this);
                _clients.Add(clientObject);
                var clientThread = new Thread(clientObject.Process);
                clientThread.Start();
            }
        }
        catch (Exception ex) { Console.WriteLine(ex.Message); }
        finally { Disconnect(); }
    }

    protected internal void BroadcastMessage(string message, string id)
    {
        var client = _clients.FirstOrDefault(c => c.Id == id);
        if (client == null) return;
        var encryptedMessage = _cipher.Encrypt(message, _password);
        var data = Encoding.Unicode.GetBytes(encryptedMessage);
        client.Stream.Write(data, 0, data.Length);
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
        Environment.Exit(0);
    }
}
class ClientObject
{
    protected internal string Id { get; }
    protected internal NetworkStream Stream { get; private set; }
 
    private TcpClient _client;
    private ServerObject _server;
 
    public ClientObject(TcpClient tcpClient, ServerObject serverObject)
    {
        Id = Guid.NewGuid().ToString();
        _client = tcpClient;
        _server = serverObject;
    }

    public void Process()
    {
        try
        {
            Stream = _client.GetStream();
            var userName = GetMessage();
            Console.WriteLine($"{userName} enter to chat");

            while (true)
            {
                try
                {
                    var message = GetMessage();
                    if (message == null) continue;
                    Console.WriteLine($"Client receive file with text {message}");
                    var result = _server.FindInvertedIndex(message);
                    _server.BroadcastMessage(result, Id);
                }
                catch
                {
                    Console.WriteLine($"{userName} leave's chat");
                    break;
                }
            }
        }
        catch (Exception e) { Console.WriteLine(e); }
        finally
        {
            _server.RemoveConnection(Id);
            Close();
        }
    }

    private string GetMessage()
    {
        var data = new byte[64];
        var builder = new StringBuilder();
        var bytes = 0;
        do
        {
            bytes = Stream.Read(data, 0, data.Length);
            builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
        } while (Stream.DataAvailable);
        var message = builder.ToString();
        var decryptedMessage = _server.DecryptMessage(message); 
        return decryptedMessage;
    }

    protected internal void Close()
    {
        Stream.Close();
        _client.Close();
    }
}