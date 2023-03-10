using Chatiks.Tools.Domain;

namespace Chatiks.Chat.Domain;

public class ChatDomainModel: UniqueDeletableDomainModelBase
{
    public string Name { get; private set; }
    public bool IsPrivate { get; private set; }
    public ICollection<ChatMessageDomainModel> Messages { get; private set; }
    public ICollection<ChatUserDomainModel> Users { get; private set; }
    public ImageLinkDomainModel ChatAvatar { get; private set; }
    
    
    public ChatDomainModel(
        long? id, 
        ICollection<ChatMessageDomainModel> messages = null,
        ICollection<ChatUserDomainModel> users = null) : base(id)
    {
        Messages = messages;
        Users = users;
    }


    public void SendMessage(string text, long userId, ICollection<string> imagesBase64)
    {
        ThrowOperationExceptionIfDeleted();
        
        Messages.Add(new ChatMessageDomainModel(null, text, DateTime.Now, userId, imagesBase64));
    }
    
    public void AddUser(long userId, long? inviterId)
    {
        ThrowOperationExceptionIfDeleted();
        
        Users.Add(new ChatUserDomainModel(userId, inviterId));
    }
    
    public void ChangeName(string name)
    {
        ThrowOperationExceptionIfDeleted();

        if (IsPrivate)
        {
            throw new Exception("Can't change name of private chat");
        }
        
        Name = name;
    }
    
    public void ChangeAvatar(string base64Text)
    {
        ThrowOperationExceptionIfDeleted();
        
        ChatAvatar = new ImageLinkDomainModel(null, base64Text);
    }
    
    public void ThrowOperationExceptionIfDeleted()
    {
        if (IsDeleted)
        {
            throw new Exception("Can't operate with deleted chat");
        }
    }
}