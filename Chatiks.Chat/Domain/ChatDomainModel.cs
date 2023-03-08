using Chatiks.Tools.Domain;

namespace Chatiks.Chat.Domain;

public class ChatDomainModel: IUniqueDomainModel<long>
{
    public ChatDomainModel(ICollection<ChatMessageDomainModel> messages)
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
            images.Add(new ChatMessageImageDomainModel(imageBase64));
        }
        
        Messages.Add(new ChatMessageDomainModel(text, DateTime.Now, images));
    }

    public long Id { get; }
}