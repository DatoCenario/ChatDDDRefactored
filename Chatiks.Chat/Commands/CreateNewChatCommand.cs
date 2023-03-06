namespace Chatiks.Chat.Commands;

public class CreateNewChatCommand
{
    public CreateNewChatCommand(long creatorId, ICollection<long> userIds, string name, bool privateChat)
    {
        CreatorId = creatorId;
        UserIds = userIds;
        Name = name;
        PrivateChat = privateChat;
    }

    public long CreatorId { get; }
    public ICollection<long> UserIds { get; }
    public string Name { get; }
    public bool PrivateChat { get; }
}