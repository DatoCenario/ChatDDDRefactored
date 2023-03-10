using Chatiks.Tools.Domain;

namespace Chatiks.Chat.Domain;

public class ChatMessageImageDomainModel: ImageLinkDomainModel
{
    public ChatMessageImageDomainModel(long? id, string base64Text = null) : base(id, base64Text)
    {
    }
    
    public ChatMessageImageDomainModel(string base64Text = null) : base(base64Text)
    {
    }
}