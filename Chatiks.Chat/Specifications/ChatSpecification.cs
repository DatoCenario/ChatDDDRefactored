using LinqSpecs;
using Microsoft.EntityFrameworkCore;

namespace Chatiks.Chat.Specifications;

public class ChatSpecification: SpecificationBase<Data.EF.Domain.Chat.Chat>
{
    private MessageFilter _messageFilter;
    private ChatUserFilter _chatUserFilter;

    public ChatSpecification(Specification<Data.EF.Domain.Chat.Chat> filter) : base(filter)
    {
    }

    public void IncludeMessages(MessageFilter messageFilter = null)
    {
        _messageFilter = (messageFilter ?? new MessageFilter());
    }
    
    public void IncludeChatUsers(ChatUserFilter chatUserFilter = null)
    {
        _chatUserFilter = (chatUserFilter ?? new ChatUserFilter());
    }


    protected override IQueryable<Data.EF.Domain.Chat.Chat> Include(IQueryable<Data.EF.Domain.Chat.Chat> query)
    {
        if (_messageFilter != null)
        {
            query = query.Include(CreateIncludeExpressionFromFilter(c => c.Messages, _messageFilter));
        }
        
        if (_chatUserFilter != null)
        {
            query = query.Include(CreateIncludeExpressionFromFilter(c => c.ChatUsers, _chatUserFilter));
        }

        return query;
    }
}