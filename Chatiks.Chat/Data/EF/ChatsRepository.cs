using Chatiks.Chat.Commands;
using Chatiks.Chat.Data.EF.Domain.Chat;
using Chatiks.Chat.Specifications;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Chatiks.Chat.Data.EF;

public class ChatsRepository: Chatiks.Tools.EF.RepositoryBase<ChatContext>
{
    public IQueryable<Domain.Chat.Chat> Chats => Context.Chats.AsQueryable();
    public IQueryable<Domain.Chat.ChatMessage> ChatMessages => Context.ChatMessages.AsQueryable();

    public ChatsRepository(ChatContext context) : base(context)
    {
    }

    public async Task<ICollection<Domain.Chat.Chat>> LoadChatsBySpecificationAsync(ChatSpecification specification = null)
    {
        var query = Context.Chats.AsQueryable().AsNoTracking();
        query = specification?.Apply(query) ?? query;
        return await query.ToListAsync();
    }

    public async Task<Domain.Chat.Chat> LoadChatBySpecificationAsync(ChatSpecification specification = null)
    {
        var query = Context.Chats.AsQueryable().AsNoTracking();
        query = specification?.Apply(query) ?? query;
        return await query.FirstOrDefaultAsync();
    }

    public ValueTask<EntityEntry<ChatUser>> AddNewChatUserAsync(AddChatUserCommand command)
    {
        var entity = command.Adapt<ChatUser>();
        return Context.AddAsync(entity);
    }

    public async Task RemoveUserFromChat(RemoveUserFromChatCommand command)
    {
        var cu = await Context.ChatUsers.FirstAsync(u => u.ExternalUserId == command.UserId && u.ChatId == command.ChatId);
        Context.ChatUsers.Remove(cu);
    }

    public ValueTask<EntityEntry<Domain.Chat.ChatMessage>> SendMessageToChatAsync(SendMessageToChatCommand command)
    {
        var entity = command.Adapt<ChatMessage>();
        entity.MessageImageLinks = command.Images.Select(i => new ChatMessageImageLink()
        {
            ExternalImageId = i
        }).ToArray();
        
        return Context.AddAsync(entity);
    }

    public ValueTask<EntityEntry<Domain.Chat.Chat>> CreateChatAsync(CreateNewChatCommand command)
    {
        var userIds = command.UserIds.Union(new[] { command.CreatorId }).ToArray();

        var chat = new Data.EF.Domain.Chat.Chat
        {
            ChatUsers = userIds.Select(u => new ChatUser
            {
                ExternalUserId = u,
                ExternalInviterId = command.CreatorId
            }).ToArray(),
            Name = command.Name,
            IsPrivate = command.PrivateChat
        };

        return Context.Chats.AddAsync(chat);
    }
}