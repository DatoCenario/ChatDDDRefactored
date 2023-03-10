using Chatiks.Tools;
using Chatiks.Tools.Domain;

namespace Chatiks.Chat.Domain;

public class ChatDomainModel: UniqueDeletableDomainModelBase
{
    public string Name { get; private set; }
    public bool IsPrivate { get; private set; }
    public long? OwnerId { get; private set; }
    public ICollection<ChatMessageDomainModel> Messages { get; private set; }
    public ICollection<ChatUserDomainModel> Users { get; private set; }
    public ImageLinkDomainModel ChatAvatar { get; private set; }
    
    
    public ChatDomainModel(
        long? ownerId = null, long? id = null, string name = null,
        ICollection<ChatMessageDomainModel> messages = null,
        ICollection<ChatUserDomainModel> users = null,
        ImageLinkDomainModel chatAvatar = null) : base(id)
    {
        var isPrivate = name == null;

        if (isPrivate)
        {
            if (ownerId.HasValue)
            {
                throw new Exception("Can't create private chat with owner");
            }

            if (users.EmptyIfNull().Count > 2)
            {
                throw new Exception("Can't create private chat with more than 2 users");
            }

            if (chatAvatar != null)
            {
                throw new Exception("Can't create private chat with avatar");
            }
        }
        else
        {
            if (name == null)
            {
                throw new Exception("Can't create group chat without name");
            }
            if (ownerId == null)
            {
                throw new Exception("Can't create group chat without owner");
            }
        }
        
        OwnerId = ownerId;
        IsPrivate = isPrivate;
        Name = name;
        ChatAvatar = chatAvatar;
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
        
        if (IsPrivate)
        {
            throw new Exception("can't add user to private chat");
        }

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

        if (IsPrivate)
        {
            throw new Exception("Can't change avatar of private chat");
        }
        
        ChatAvatar = new ImageLinkDomainModel(null, base64Text);
    }
}