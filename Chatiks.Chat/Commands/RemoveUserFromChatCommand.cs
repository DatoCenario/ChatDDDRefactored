namespace Chatiks.Chat.Commands;

public class RemoveUserFromChatCommand
{
    public RemoveUserFromChatCommand(long chatId, long userId)
    {
        ChatId = chatId;
        UserId = userId;
    }

    public long ChatId { get; }
    public long UserId { get; }
}