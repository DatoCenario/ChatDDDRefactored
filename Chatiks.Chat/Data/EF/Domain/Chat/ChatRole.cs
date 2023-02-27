using Chatiks.Tools.EF;

namespace Chatiks.Chat.Data.EF.Domain.Chat;

public class ChatRole: IEntity
{
    public bool IsAdmin { get; set; }
    public string Name { get; set; }
    public long Id { get; set; }
}