using Chatiks.Tools;
using Chatiks.Tools.Domain;

namespace Chatiks.Chat.Domain;

public class ChatDomainModel: UniqueDeletableDomainModelBase
{
    public string Name { get; private set; }
    public bool IsPrivate { get; private set; }
    public long? CreatorId { get; private set; }
    public ICollection<ChatMessageDomainModel> Messages { get; private set; }
    public ICollection<ChatUserDomainModel> Users { get; private set; }
    public ImageLinkDomainModel ChatAvatar { get; private set; }
    
    
    public ChatDomainModel(long? id = null, long? creatorId = null, string name = null,
        long? privateChatUser1 = null, long? privateChatUser2 = null,
        ICollection<ChatMessageDomainModel> messages = null,
        ICollection<ChatUserDomainModel> users = null,
        ImageLinkDomainModel chatAvatar = null) : base(id)
    {
        users ??= new List<ChatUserDomainModel>();

        var isPrivate = name == null;

        if (isPrivate)
        {
            if (users.Count > 2)
            {
                throw new Exception("Can't create private chat with more than 2 users");
            }
            if (chatAvatar != null)
            {
                throw new Exception("Can't create private chat with avatar");
            }
            if(creatorId != null)
            {
                throw new Exception("Can't create private chat with creator");
            }
            if (privateChatUser1 == null || privateChatUser2 == null)
            {
                throw new Exception("Can't create private chat without users");
            }
            
            users = users.Append(new ChatUserDomainModel(privateChatUser1.Value, null))
                .Append(new ChatUserDomainModel(privateChatUser2.Value, null))
                .ToList();
        }
        else
        {
            if (name == null)
            {
                throw new Exception("Can't create group chat without name");
            }
            if (creatorId == null)
            {
                throw new Exception("Can't create group chat without creator");
            }
            
            users = users.Append(new ChatUserDomainModel(creatorId.Value, null)).ToList();
        }
        
        CreatorId = creatorId;
        IsPrivate = isPrivate;
        Name = name;
        ChatAvatar = chatAvatar;
        Messages = messages;
        Users = users;
    }


    public void SendMessage(string text, long userId, ICollection<string> imagesBase64 = null)
    {
        ThrowOperationExceptionIfDeleted();
        
        Messages.Add(new ChatMessageDomainModel(null, text, DateTime.Now, userId, imagesBase64.EmptyIfNull()));
    }
    
    public void AddUsers(ICollection<long> userIds, long? inviterId)
    {
        ThrowOperationExceptionIfDeleted();
        
        if (IsPrivate)
        {
            throw new Exception("can't add user to private chat");
        }

        if (Users.Any(u => userIds.Contains(u.UserId)))
        {
            throw new Exception("user already in chat");
        }

        foreach (var id in userIds)
        {
            Users.Add(new ChatUserDomainModel(id, inviterId));
        }
    }
    
    public void AddUser(long userId, long? inviterId)
    {
        ThrowOperationExceptionIfDeleted();
        
        if (IsPrivate)
        {
            throw new Exception("can't add user to private chat");
        }

        if (Users.Any(u => u.UserId == userId))
        {
            throw new Exception("user already in chat");
        }

        Users.Add(new ChatUserDomainModel(userId, inviterId));
    }
    
    public void LeaveChat(long userId)
    {
        ThrowOperationExceptionIfDeleted();
        
        if (IsPrivate)
        {
            throw new Exception("can't remove user from private chat");
        }

        var user = Users.FirstOrDefault(u => u.UserId == userId);
        
        if (user == null)
        {
            throw new Exception("user not found");
        }

        user.Delete();
    }
    
    public void DeleteUserFromChat(long userId, long inviterId)
    {
        ThrowOperationExceptionIfDeleted();
        
        if (IsPrivate)
        {
            throw new Exception("can't remove user from private chat");
        }

        var user = Users.FirstOrDefault(u => u.UserId == userId);
        
        if (user == null)
        {
            throw new Exception("user not found");
        }

        if (user.IsDeleted)
        {
            throw new Exception("user already removed from chat");
        }

        if (user.InviterId != inviterId)
        {
            throw new Exception("user can't be removed from chat by this user");
        }

        user.Delete();
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
    
    public bool IsUserInChat(long userId)
    {
        return Users.NotDeleted().Any(u => u.UserId == userId);
    }
}