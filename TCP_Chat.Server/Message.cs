namespace TCP_Chat.Server;

public enum MessageType
{
    Connect,
    Disconnect,
    Text
}

public record Message(string SenderName, MessageType Type, string Content);