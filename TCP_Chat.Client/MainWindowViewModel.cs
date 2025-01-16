using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Reactive;
using System.Text;
using System.Text.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace TCP_Chat.Client;

public class MainWindowViewModel : ViewModelBase
{
    private TcpClient _server;
    private NetworkStream _networkStream;
    
    public ObservableCollection<Message> Messages { get; } = [];

    [Reactive] public string? IP { get; set;} = "127.0.0.1";
    [Reactive] public string? Port { get; set; } = "65000";
    [Reactive] public string? Name { get; set; } = "anst";
    [Reactive] public string? Message { get; set;}//FIXME
    
    public ReactiveCommand<Unit, Unit> CommandConnect { get;}
    public ReactiveCommand<Unit, Unit> CommandSend { get;}
    
    public MainWindowViewModel()
    {
        CommandConnect = ReactiveCommand.CreateFromTask(ConnectAsync);
        CommandSend = ReactiveCommand.CreateFromTask(SendAsync);
    }

    private async Task ConnectAsync() //FIXME
    {
        var ip = IPAddress.Parse(IP);
        var port = int.Parse(Port);
        _server = new TcpClient();
        await _server.ConnectAsync(ip, port);
        
        _networkStream = _server.GetStream();
        
        var message = new Message(Name, MessageType.Connect, "null");
        var json = JsonSerializer.Serialize(message);
        var buffer = Encoding.UTF8.GetBytes(json);
        await _networkStream.WriteAsync(buffer);
        await _networkStream.FlushAsync();
        
            while (true)
            {
                var buffer2 = new byte[256];
                var size = await _networkStream.ReadAsync(buffer2);
                var json2 = Encoding.UTF8.GetString(buffer2, 0, size);
                var message2 = JsonSerializer.Deserialize<Message>(json2);
                
                Messages.Add(message2);
            }
    }
    
    private async Task SendAsync() //FIXME
    {
        var message = new Message(Name, MessageType.Text, Message);
        var json = JsonSerializer.Serialize(message);
        var buffer = Encoding.UTF8.GetBytes(json);
        await _networkStream.WriteAsync(buffer);
    }
}