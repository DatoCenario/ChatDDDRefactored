using Chatiks.Tools.Commands.Interfaces;

namespace Chatiks.Chat.Commands;

public class AddChatUserCommand: ICommand
{
    public long ChatId { get; set; }
    public long UserId { get; set; }
    public long InviterId { get; set; }
    
    public bool IsValid()
    {
        throw new NotImplementedException();
    }
}