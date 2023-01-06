using System.Net;
using System.Net.Sockets;
 
var server = new ServerObject();
await server.ListenAsync();
 
class ServerObject
{
    TcpListener tcpListener = new(IPAddress.Any, 8888);
    List<ClientObject> clients = new();
    protected internal void RemoveConnection(string id)
    {
        var client = clients.FirstOrDefault(c => c.Id == id);
        if (client != null) clients.Remove(client);
        client?.Close();
    }
    
    protected internal async Task ListenAsync()
    {
        try
        {
            tcpListener.Start();
            Console.WriteLine("Server is started. Waiting for connections...");
 
            while (true)
            {
                var tcpClient = await tcpListener.AcceptTcpClientAsync();
 
                var clientObject = new ClientObject(tcpClient, this);
                clients.Add(clientObject);
                await Task.Run(clientObject.ProcessAsync);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            Disconnect();
        }
    }
    
    protected internal async Task BroadcastMessageAsync(string message, string id)
    {
        foreach (var client in clients.Where(client => client.Id != id))
        {
            await client.Writer.WriteLineAsync(message);
            await client.Writer.FlushAsync();
        }
    }
    
    protected internal void Disconnect()
    {
        foreach (var client in clients)
        {
            client.Close();
        }
        tcpListener.Stop();
    }
}
class ClientObject
{
    protected internal string Id { get;} = Guid.NewGuid().ToString();
    protected internal StreamWriter Writer { get;}
    protected internal StreamReader Reader { get;}
 
    TcpClient client;
    ServerObject server;
 
    public ClientObject(TcpClient tcpClient, ServerObject serverObject)
    {
        client = tcpClient;
        server = serverObject;
        var stream = client.GetStream();
        Reader = new StreamReader(stream);
        Writer = new StreamWriter(stream);
    }
 
    public async Task ProcessAsync()
    {
        try
        {
            var userName = await Reader.ReadLineAsync();
            var message = $"{userName} enter to chat";
            await server.BroadcastMessageAsync(message, Id);
            Console.WriteLine(message);
            while (true)
            {
                try
                {
                    message = await Reader.ReadLineAsync();
                    if (message == null) continue;
                    message = $"{userName}: {message}";
                    Console.WriteLine(message);
                    await server.BroadcastMessageAsync(message, Id);
                }
                catch
                {
                    message = $"{userName} leave's chat";
                    Console.WriteLine(message);
                    await server.BroadcastMessageAsync(message, Id);
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        finally
        {
            server.RemoveConnection(Id);
        }
    }

    protected internal void Close()
    {
        Writer.Close();
        Reader.Close();
        client.Close();
    }
}