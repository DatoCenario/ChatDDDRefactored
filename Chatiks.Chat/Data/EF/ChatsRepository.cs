using Chatiks.Chat.Commands;
using Chatiks.Chat.Data.EF.Domain.Chat;
using Chatiks.Chat.Specifications;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Chatiks.Chat.Data.EF;

public class ChatsRepository: Chatiks.Tools.EF.RepositoryBase<ChatContext>
{
    public ChatsRepository(ChatContext context) : base(context)
    {
    }

    public async Task<ICollection<Domain.Chat.Chat>> LoadBySpecificationAsync(ChatSpecification specification = null)
    {
        var query = Context.Chats.AsQueryable();
        query = specification?.Apply(query) ?? query;
        return await query.ToListAsync();
    }

    public async Task<Domain.Chat.Chat> LoadFirstOrDefaultBySpecificationAsync(ChatSpecification specification = null)
    {
        var query = Context.Chats.AsQueryable();
        query = specification?.Apply(query) ?? query;
        return await query.FirstOrDefaultAsync();
    }

    public ValueTask<EntityEntry<ChatUser>> AddNewChatUserAsync(AddChatUserCommand command)
    {
        var entity = command.Adapt<ChatUser>();
        return Context.AddAsync(entity);
    }
}