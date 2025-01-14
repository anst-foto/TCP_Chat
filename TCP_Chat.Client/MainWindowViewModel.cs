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
    private Socket _server;
    private NetworkStream _networkStream;
    private IPAddress _ip;
    private int _port;
    
    public ObservableCollection<Message> Messages { get; } = [];

    [Reactive] public string? IP { get; set;}
    [Reactive] public string? Port { get; set;}
    [Reactive] public string? Name { get; set;}
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
        _ip = IPAddress.Parse(IP);
        _port = int.Parse(Port);
        _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        await _server.ConnectAsync(_ip, _port);
        
        _networkStream = new NetworkStream(_server);
        
        var message = new Message(Name, MessageType.Connect, "null");
        var json = JsonSerializer.Serialize(message);
        var buffer = Encoding.UTF8.GetBytes(json);
        await _networkStream.WriteAsync(buffer);
    }
    
    private async Task SendAsync() //FIXME
    {
        var message = new Message(Name, MessageType.Text, Message);
        var json = JsonSerializer.Serialize(message);
        var buffer = Encoding.UTF8.GetBytes(json);
        await _networkStream.WriteAsync(buffer);
    }
}