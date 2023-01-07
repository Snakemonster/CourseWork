using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using XOR_Cipher;

namespace TCPClient;

public class Client
{
    private readonly int _port;
    private readonly string _name;
    private readonly string _server;
    private readonly string _password;

    private TcpClient _client;
    private NetworkStream _stream;
    private XORCipher _cipher;
    
    public Client(string name, string host = "127.0.0.1", int port = 8888)
    {
        _server = host;
        _name = name;
        _port = port;
        _stream = null;
        _password = XORCipher.GetRandomKey(4643, 20);
        _cipher = new XORCipher();
        _client = new TcpClient();
    }

    public void StartClient()
    {
        try
        {
            _client.Connect(_server, _port);
            _stream = _client.GetStream();

            var receiveThread = new Thread(ReceiveMessage);
            receiveThread.Start();
            SendMessage();
        }
        catch (Exception ex) { Console.WriteLine(ex); }
        finally { Disconnect(); }
    }

    private void SendMessage()
    {
        var name = _cipher.Encrypt(_name, _password);
        var data = Encoding.Unicode.GetBytes(name);
        _stream.Write(data, 0, data.Length);
        Console.Write("Write which word you want to found (only lowercase and only one word) from server than hit Enter or if you want exit write \"Exit\"\n: ");
        while (true)
        {
            var message = Console.ReadLine();
            if (message == "Exit") Disconnect();
            Console.WriteLine($"You have choose word \"{message}\", please wait for server...");
            var encryptedMessage = _cipher.Encrypt(message, _password);
            data = Encoding.Unicode.GetBytes(encryptedMessage);
            _stream.Write(data, 0, data.Length);
        }
    }


    private void ReceiveMessage()
    {
        while (true)
        {
            try
            {
                var data = new byte[64];
                var builder = new StringBuilder();
                var bytes = 0;
                do
                {
                    bytes = _stream.Read(data, 0, data.Length);
                    builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                } while (_stream.DataAvailable);

                var decryptedMessage = _cipher.Decrypt(builder.ToString(), _password);
                if (decryptedMessage == "empty")
                {
                    Console.WriteLine("This word doesn't exist in files on server or you write wrong word");
                    continue;
                }
                Print(decryptedMessage);
            }
            catch
            {
                Console.WriteLine("Connection is lose!");
                Disconnect();
            }
        }
    }
    
    private void Disconnect()
    {
        _stream.Close();
        _client.Close();
        Environment.Exit(0);
    }

    void Print(string message)
    {
        var res = JsonSerializer.Deserialize<Dictionary<string, int>>(message);
        foreach (var result in res) { Console.WriteLine($"{result.Key.Replace(@"/", @"\")} :{result.Value}"); }
    }
}