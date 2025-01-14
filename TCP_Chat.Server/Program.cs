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
var server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
server.Bind(ip);
server.Listen(1);

var client = await server.AcceptAsync();
var clientStream = new NetworkStream(client);

while (true)
{
    var buffer = new byte[512];
    await clientStream.ReadAsync(buffer);
    var json = Encoding.UTF8.GetString(buffer);
    var message = JsonSerializer.Deserialize<Message>(json);//FIXME
    switch (message.Type)
    {
        case MessageType.Connect:
            Console.WriteLine($"Клиент {message.SenderName} подключился");//TODO
            break;
        case MessageType.Disconnect:
            clientStream.Close();
            client.Close();
            server.Close();
            return;//TODO
        case MessageType.Text:
            Console.WriteLine($"Сообщение от {message.SenderName}: {message.Content}");
            break;
        default:
            throw new ArgumentOutOfRangeException();
    }
}
