using Chatiks.Tools.Domain;

namespace Chatiks.Chat.Domain;

public class ChatMessageImageDomainModel: ImageLinkDomainModel
{
    public ChatMessageImageDomainModel(long? id, string base64Text) : base(id, base64Text)
    {
    }
}