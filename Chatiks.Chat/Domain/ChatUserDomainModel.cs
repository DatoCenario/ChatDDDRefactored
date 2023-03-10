using Chatiks.Tools.Domain;

namespace Chatiks.Chat.Domain;

public class ChatUserDomainModel: UniqueDeletableDomainModelBase
{
    public long UserId { get; }
    public long? InviterId { get; }
    
    public ChatUserDomainModel(long userId, long? inviterId)
    {
        UserId = userId;
        InviterId = inviterId;
    }
    
    public ChatUserDomainModel(long? id, long userId, long? inviterId): base(id)
    {
        UserId = userId;
        InviterId = inviterId;
    }
}