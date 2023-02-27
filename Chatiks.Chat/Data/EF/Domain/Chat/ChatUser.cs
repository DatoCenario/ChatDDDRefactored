using Chatiks.Tools.EF;

namespace Chatiks.Chat.Data.EF.Domain.Chat;

public class ChatUser: IEntity
{
    public long Id { get; set; }
    public long ChatId { get; set; }
    public long ExternalUserId { get; set; }
    public long? ExternalInviterId { get; set; }
    public virtual Chat Chat { get; set; }
}