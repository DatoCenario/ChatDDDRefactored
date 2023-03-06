namespace Chatiks.Chat.Commands;

public class SendMessageToChatCommand
{
    public SendMessageToChatCommand(
        long messageOwnerId, 
        long chatId, 
        string text,
        DateTime sendTime,
        ICollection<long> images)
    {
        ExternalOwnerId = messageOwnerId;
        Text = text;
        ChatId = chatId;
        SendTime = sendTime;
        Images = images;
    }

    public long ExternalOwnerId {get;}
    public string Text{get;}
    public long ChatId{get;}
    public DateTime SendTime {get;}
    public ICollection<long> Images {get;}
}