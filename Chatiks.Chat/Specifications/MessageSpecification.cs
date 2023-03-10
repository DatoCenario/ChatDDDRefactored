using Chatiks.Chat.Data.EF.Domain.Chat;
using Microsoft.EntityFrameworkCore;

namespace Chatiks.Chat.Specifications;

public class MessageSpecification: SpecificationBase<ChatMessage>
{
    public MessageSpecification(MessageFilter filter = null) : base(filter)
    {
    }

    protected override IQueryable<ChatMessage> Include(IQueryable<ChatMessage> query)
    {
        return query.Include(x => x.MessageImageLinks);
    }
}