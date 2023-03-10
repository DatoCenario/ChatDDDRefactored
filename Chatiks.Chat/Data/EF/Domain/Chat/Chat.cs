using Chatiks.Tools.EF;

namespace Chatiks.Chat.Data.EF.Domain.Chat;

public class Chat: IEntity
{
    public long OwnerId { get; set; }
    public long Id { get; set; }
    public string? Name { get; set; }
    public bool IsPrivate { get; set; }
    
    public virtual ICollection<ChatUser> ChatUsers { get; set; }
    public virtual ICollection<ChatMessage> Messages { get; set; }
}