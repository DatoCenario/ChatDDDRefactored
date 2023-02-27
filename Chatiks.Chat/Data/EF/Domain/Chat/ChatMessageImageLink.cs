using Chatiks.Tools.EF;

namespace Chatiks.Chat.Data.EF.Domain.Chat;

public class ChatMessageImageLink: IEntity
{
    public long Id { get; set; }
    
    public long ChatMessageId { get; set; }
    public long ExternalImageId { get; set; }
    public virtual ChatMessage ChatMessage { get; set; }
}