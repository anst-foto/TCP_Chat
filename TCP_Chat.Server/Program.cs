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

var client = await server.AcceptTcpClientAsync();
var clientStream = client.GetStream();

while (true)
{
    var buffer = new byte[250];
    var size = await clientStream.ReadAsync(buffer);
    var json = Encoding.UTF8.GetString(buffer, 0, size);
    var message = JsonSerializer.Deserialize<Message>(json);//FIXME
    switch (message.Type)
    {
        case MessageType.Connect:
            Console.WriteLine($"Клиент {message.SenderName} подключился");//TODO
            break;
        case MessageType.Disconnect:
            clientStream.Close();
            client.Close();
            server.Stop();
            return;//TODO
        case MessageType.Text:
            Console.WriteLine($"Сообщение от {message.SenderName}: {message.Content}");
            break;
        default:
            throw new ArgumentOutOfRangeException();
    }
}
