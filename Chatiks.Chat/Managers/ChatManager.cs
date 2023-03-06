using Chatiks.Chat.Commands;
using Chatiks.Chat.Data.EF;
using Chatiks.Chat.Data.EF.Domain.Chat;
using Chatiks.Chat.Specifications;
using Chatiks.Core.Managers;
using Microsoft.EntityFrameworkCore;

namespace Chatiks.Chat.Managers;

public class ChatManager
{
    private readonly ChatsRepository _chatsRepository;
    private readonly ImagesManager _imagesManager;

    public ChatManager(ChatsRepository chatsRepository, ImagesManager imagesManager)
    {
        _chatsRepository = chatsRepository;
        _imagesManager = imagesManager;
    }

    public async Task AddUserToChatAsync(long inviterId, long userId, long chatId)
    {
        using (var isolatedOperation = _chatsRepository.BeginIsolatedOperation())
        {
            var chatSpecification = new ChatSpecification(new ChatFilter(new []{chatId}));
            chatSpecification.IncludeMessages();
            chatSpecification.IncludeChatUsers();

            var chat = await _chatsRepository.LoadChatBySpecificationAsync(chatSpecification);

            if (chat == null)
            {
                throw new Exception("Chat not found");
            }

            if (chat.ChatUsers.All(u => u.ExternalUserId != inviterId))
            {
                throw new Exception("Inviter not in chat");
            }

            var addCommand = new AddChatUserCommand
            {
                ChatId = chatId,
                InviterId = inviterId,
                UserId = userId
            };

            await _chatsRepository.AddNewChatUserAsync(addCommand);

            await isolatedOperation.SaveChangesAsync();
        }
    }

    public async Task<long> SendMessageToChatAsync(
        long userId,
        long chatId,
        string text = null,
        params string[]? imagesBase64)
    {
        imagesBase64 ??= Array.Empty<string>();

        var imagesIds = await _imagesManager.UploadNewImagesAsync(imagesBase64);

        using (var io = _chatsRepository.BeginIsolatedOperation())
        {
            var spec = new ChatSpecification(new ChatFilter(new [] {chatId}));
            spec.IncludeChatUsers(new ChatUserFilter()
            {
                UserId = userId
            });
            
            var chat = await _chatsRepository.LoadChatBySpecificationAsync(spec);

            if (chat == null)
            {
                throw new Exception("Chat not found");
            }

            if (chat.ChatUsers.All(u => u.ExternalUserId != userId))
            {
                throw new Exception("User not in chat");
            }

            var command = new SendMessageToChatCommand(userId, chatId, text, DateTime.Now, imagesIds);

            var entry = await _chatsRepository.SendMessageToChatAsync(command);
            await _chatsRepository.SaveChangesAsync();
            return entry.Entity.Id;
        }
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