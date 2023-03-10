using Chatiks.Chat.Commands;
using Chatiks.Chat.Data.EF;
using Chatiks.Chat.Data.EF.Domain.Chat;
using Chatiks.Chat.Domain;
using Chatiks.Chat.Specifications;
using Chatiks.Core.Managers;
using Chatiks.Tools.EF;
using Microsoft.EntityFrameworkCore;

namespace Chatiks.Chat.Managers;

public class ChatManager
{
    private readonly ChatContext _chatContext;
    private readonly ImagesManager _imagesManager;
    private readonly ChatDomainModelFactory _chatDomainModelFactory;

    public ChatManager(ImagesManager imagesManager, ChatDomainModelFactory chatDomainModelFactory, ChatContext chatContext)
    {
        _imagesManager = imagesManager;
        _chatDomainModelFactory = chatDomainModelFactory;
        _chatContext = chatContext;
    }

    public async Task AddUserToChatAsync(long inviterId, long userId, long chatId)
    {
        using (var isolatedOperation = _chatContext.BeginIsolatedOperation())
        {
            var chatSpecification = new ChatSpecification(new ChatFilter(new []{chatId}));
            chatSpecification.IncludeMessages();
            chatSpecification.IncludeChatUsers();

            var chat = await LoadChatBySpecificationAsync(chatSpecification);

            if (chat.Users.All(u => u.InviterId != inviterId))
            {
                throw new Exception("Inviter not in chat");
            }
            
            if (chat.Users.Any(u => u.UserId == userId))
            {
                throw new Exception("User already in chat");
            }
            
            chat.AddUser(userId, inviterId);

            await isolatedOperation.SaveChangesAsync();
        }
    }

    public async Task<ICollection<ChatDomainModel>> UpdateChatsAsync(ICollection<ChatDomainModel> chats)
    {
        var resultDtoList = new List<Data.EF.Domain.Chat.Chat>();
        
        var toUpdate = chats.Where(i => !i.IsNew() && !i.IsDeleted).ToArray();
        var toDelete = chats.Where(i => !i.IsNew() && i.IsDeleted).ToArray();
        var toCreate = chats.Where(i => i.IsNew()).ToArray();

        var allNewImages = toCreate.Concat(toUpdate)
            .SelectMany(c => c.Messages)
            .SelectMany(m => m.Images)
            .Where(i => i.IsNew())
            .Select(x => x.Base64Text)
            .ToArray();
        
        var newImages = (await _imagesManager.UploadNewImagesAsync(allNewImages))
            .ToDictionary(k => k.Base64ImageText);

        using (var io = _chatContext.BeginIsolatedOperation())
        {
            if (toUpdate.Any())
            {
                foreach (var chat in toUpdate)
                {
                    resultDtoList.Add(UpdateOrCreateNewChat(chat));
                }
            }
        
            if (toDelete.Any())
            {
                var deleteIds = toDelete.Select(x => x.Id.Value).ToArray();
                await _chatContext.Chats.Where(x => deleteIds.Contains(x.Id))
                    .ExecuteDeleteAsync();
            }
        
            if (toCreate.Any())
            {
                foreach (var chat in toCreate)
                {
                   resultDtoList.Add(UpdateOrCreateNewChat(chat));
                }
            }
        
            await io.SaveChangesAsync();
        }

        Data.EF.Domain.Chat.Chat UpdateOrCreateNewChat(ChatDomainModel chatDomainModel, Data.EF.Domain.Chat.Chat chat = null)
        {
            if (chat == null)
            {
                chat = new Data.EF.Domain.Chat.Chat();
            }

            chat.Name = chatDomainModel.IsPrivate ? null : chatDomainModel.Name;
            chat.IsPrivate = chatDomainModel.IsPrivate;
            chat.ExternalOwnerId = chatDomainModel.OwnerId;
            chat.ExternalAvatarid = chatDomainModel.ChatAvatar.IsNew() ? newImages[chatDomainModel.ChatAvatar.Base64Text].Id.Value : chatDomainModel.ChatAvatar.ImageExternalId.Value;
            chat.Messages = chatDomainModel.Messages.Select(m => UpdateOrCreateNewMessage(m)).ToList();
            chat.ChatUsers = chatDomainModel.Users.Select(u => new ChatUser()
            {
                ExternalUserId = u.UserId,
                ExternalInviterId = u.InviterId
            }).ToList();

            return chat;
        }

        ChatMessage UpdateOrCreateNewMessage(ChatMessageDomainModel messageDomainModel, ChatMessage message = null)
        {
            if (message == null)
            {
                message = new ChatMessage();
            }

            message.ExternalOwnerId = messageDomainModel.UserId;
            message.Text = messageDomainModel.Text;
            message.MessageImageLinks = messageDomainModel.Images
                .Select(x => new ChatMessageImageLink()
                {
                    ExternalImageId = x.IsNew() ? newImages[x.Base64Text].Id.Value : x.ImageExternalId.Value
                }).ToList();

            return message;
        }

        return resultDtoList.Select(c => _chatDomainModelFactory.CreateFromChat(c)).ToArray();
    }

    public async Task<ICollection<ChatDomainModel>> LoadChatsBySpecificationAsync(ChatSpecification specification)
    {
        var chats = await _chatContext.Chats.LoadBySpecificationAsync(specification);

        return chats.Select(c => _chatDomainModelFactory.CreateFromChat(c)).ToList();
    }
    
    public async Task<ChatDomainModel> LoadChatBySpecificationAsync(ChatSpecification specification)
    {
        var chat = await _chatContext.Chats.FirstOrDefaultBySpecificationAsync(specification);
        
        if (chat == null)
        {
            throw new Exception("Chat not found");
        }

        return _chatDomainModelFactory.CreateFromChat(chat);
    }

    public async Task<long> SendMessageToChatAsync(
        long userId,
        long chatId,
        string text = null,
        params string[]? imagesBase64)
    {
        imagesBase64 ??= Array.Empty<string>();
        
        var chat = await LoadChatBySpecificationAsync(new ChatSpecification(new ChatFilter(new []{chatId})));
        
        chat.SendMessage(text, userId, imagesBase64);

        await UpdateChatsAsync(new List<ChatDomainModel> { chat });
    }

    public Task<long> CreateNewPrivateChatAsync(long userId, long otherId)
    {
        return CreateNewChatAsync(userId, new List<long> { otherId }, null, true);
    }

    public async Task<long> CreateNewChatAsync(
        long creatorId,
        ICollection<long> userIds,
        string name,
        bool privateChat = false)
    {
        userIds ??= new List<long>();
        userIds = userIds.Union(new[] { creatorId }).ToArray();
        
        using (var io = _chatsRepository.BeginIsolatedOperation())
        {
            var entry = await _chatsRepository.CreateChatAsync(new CreateNewChatCommand(creatorId,
                userIds, name, privateChat));

            await _chatsRepository.SaveChangesAsync();
            return entry.Entity.Id;
        }
    }
    
    public async Task LeaveChat(long userId, long chatId)
    {
        using (var io = _chatsRepository.BeginIsolatedOperation())
        {
            await _chatsRepository.RemoveUserFromChat(new RemoveUserFromChatCommand(chatId, userId));
        }
    }

    public Task<Data.EF.Domain.Chat.Chat> GetChat(ChatSpecification specification)
    {
        using (var io = _chatsRepository.BeginIsolatedOperation())
        {
            return _chatsRepository.LoadChatBySpecificationAsync(specification);
        }
    }

    public Task<ICollection<Data.EF.Domain.Chat.Chat>> GetChats(ChatSpecification specification)
    {
        using (var io = _chatsRepository.BeginIsolatedOperation())
        {
            return _chatsRepository.LoadChatsBySpecificationAsync(specification);
        }
    }
    
    public async Task<ICollection<ChatMessage>> GetMessagesAsync(MessageSpecification specification = null)
    {
        using (var io = _chatsRepository.BeginIsolatedOperation())
        {
            var query = _chatsRepository.ChatMessages.AsNoTracking();
            query = specification == null ? query : specification.Apply(query);
            return await query.ToArrayAsync();
        }
    }

    public Task<int> GetMessagesCountAsync(MessageFilter messageFilter)
    {
        using (var io = _chatsRepository.BeginIsolatedOperation())
        { 
            return _chatsRepository.ChatMessages
                .Where(messageFilter)
                .CountAsync();
        }
    }

    public async Task<bool> IsUserInChat(long userId, long chatId)
    {
        var spec = new ChatSpecification(new ChatFilter(new []{chatId}));
        spec.IncludeChatUsers(new ChatUserFilter()
        {
            UserId = userId
        });

        return (await GetChat(spec)).ChatUsers.Any();
    }
}