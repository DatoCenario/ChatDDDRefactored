using Chatiks.Tools.EF;
namespace Chatiks.Chat.Data.EF.Domain.Chat;

public class ChatMessage: IEntity
{
    public long Id { get; set; }
    public long ChatId { get; set; }
    public long ExternalOwnerId { get; set; }
    public string Text { get; set; }
    public DateTime SendTime { get; set; }
    public DateTime EditTime { get; set; }
    public virtual ICollection<ChatMessageImageLink> MessageImageLinks { get; set; }
    public virtual Chat Chat { get; set; }
}