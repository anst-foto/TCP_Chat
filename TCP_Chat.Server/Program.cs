using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TCP_Chat.Server;

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

var port = int.Parse(config.GetConnectionString("ServerPort"));//FIXME

var ip = new IPEndPoint(IPAddress.Any, port);
var server = new TcpListener(ip);
server.Start();
Console.WriteLine("Сервер запущен");

var clients = new Dictionary<string, NetworkStream>();

while (true)
{
    var client = await server.AcceptTcpClientAsync();
    var stream = client.GetStream();

    Task.Run(async () =>
    {
        while (true)
        {
            var buffer = new byte[256];
            var size = await stream.ReadAsync(buffer);
            var json = Encoding.UTF8.GetString(buffer, 0, size);
            var message = JsonSerializer.Deserialize<Message>(json);
            switch (message.Type)
            {
                case MessageType.Connect:
                    Console.WriteLine($"Клиент {message.SenderName} подключился");
                    foreach (var (_, clientStream) in clients)
                    {
                        var messageSend = new Message("server", MessageType.Text, $"Клиент {message.SenderName} подключился");
                        var jsonSend = JsonSerializer.Serialize(messageSend);
                        var bufferSend = Encoding.UTF8.GetBytes(jsonSend);
                        await clientStream.WriteAsync(bufferSend);
                        await clientStream.FlushAsync();
                    }
                    clients.Add(message.SenderName, stream);
                    break;
                case MessageType.Disconnect:
                    Console.WriteLine($"Клиент {message.SenderName} отключился");
                    clients.Remove(message.SenderName);
                    foreach (var (_, clientStream) in clients)
                    {
                        var messageSend = new Message("server", MessageType.Text, $"Клиент {message.SenderName} отключился");
                        var jsonSend = JsonSerializer.Serialize(messageSend);
                        var bufferSend = Encoding.UTF8.GetBytes(jsonSend);
                        await clientStream.WriteAsync(bufferSend);
                        await clientStream.FlushAsync();
                    }
                    break;
                case MessageType.Text:
                    Console.WriteLine($"Сообщение от {message.SenderName}: {message.Content}");

                    foreach (var (clientName, clientStream) in clients)
                    {
                        if(clientName == message.SenderName) continue;

                        var messageSend = new Message("server", MessageType.Text, $"Сообщение от {message.SenderName}: {message.Content}");
                        var jsonSend = JsonSerializer.Serialize(messageSend);
                        var bufferSend = Encoding.UTF8.GetBytes(jsonSend);
                        await clientStream.WriteAsync(bufferSend);
                        await clientStream.FlushAsync();
                    }
                    break;
            }
        }
    });
}
