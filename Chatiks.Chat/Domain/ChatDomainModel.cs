using Chatiks.Tools.Domain;

namespace Chatiks.Chat.Domain;

public class ChatDomainModel: UniqueDomainModelBase
{
    public ChatDomainModel(long? id, ICollection<ChatMessageDomainModel> messages) : base(id)
    {
        Messages = messages;
    }

    public ICollection<ChatMessageDomainModel> Messages { get; private set; }
    
    // method for sending message
    public void SendMessage(string text, ICollection<string> imagesBase64)
    {
        // validate that message not empty
        if (string.IsNullOrEmpty(text) && imagesBase64.Count == 0)
        {
            throw new Exception("Message is empty");
        }
        
        var images = new List<ChatMessageImageDomainModel>();
        foreach (var imageBase64 in imagesBase64)
        {
            images.Add(new ChatMessageImageDomainModel(null, imageBase64));
        }
        
        Messages.Add(new ChatMessageDomainModel(null, text, DateTime.Now, images));
    }
}