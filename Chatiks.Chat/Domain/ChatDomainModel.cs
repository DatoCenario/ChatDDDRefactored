using Chatiks.Tools.Domain;

namespace Chatiks.Chat.Domain;

public class ChatDomainModel: UniqueDomainModelBase
{
    public ChatDomainModel(long? id, ICollection<ChatMessageDomainModel> messages) : base(id)
    {
        Messages = messages;
    }

    public ICollection<ChatMessageDomainModel> Messages { get; private set; }
    

    public void SendMessage(string text, long userId, ICollection<string> imagesBase64)
    {
        Messages.Add(new ChatMessageDomainModel(null, text, DateTime.Now, userId, imagesBase64));
    }
}