using LinqSpecs;
using Microsoft.EntityFrameworkCore;

namespace Chatiks.Chat.Specifications;

public class ChatSpecification: SpecificationBase<Data.EF.Domain.Chat.Chat>
{
    private bool _includeMessages;
    private bool _includeLastMessages;
    private bool _includeChatUsers;
    
    private MessageFilter _messageFilter;
    private ChatUserFilter _chatUserFilter;

    public ChatSpecification(Specification<Data.EF.Domain.Chat.Chat> filter) : base(filter)
    {
    }

    public void IncludeMessages(MessageFilter messageFilter = null)
    {
        _includeMessages = true;
        _messageFilter = (messageFilter ?? new MessageFilter());
    }
    
    public void IncludeLastMessages()
    {
        _messageFilter = new MessageFilter();
        _includeLastMessages = true;
    }
    
    public void IncludeChatUsers(ChatUserFilter chatUserFilter = null)
    {
        _includeChatUsers = true;
        _chatUserFilter = (chatUserFilter ?? new ChatUserFilter());
    }


    protected override IQueryable<Data.EF.Domain.Chat.Chat> Include(IQueryable<Data.EF.Domain.Chat.Chat> query)
    {
        if (_includeLastMessages)
        {
            query = query.Include(CreateIncludeAndOrderExpressionFromFilter(c => c.Messages, _messageFilter.ToExpression(), m => m.SendTime, true, 1))
                .ThenInclude(m => m.MessageImageLinks);
        }
        else if (_includeMessages)
        {
            query = query.Include(CreateIncludeExpressionFromFilter(c => c.Messages, _messageFilter.ToExpression()))
                .ThenInclude(m => m.MessageImageLinks);
        }
        
        if (_includeChatUsers)
        {
            query = query.Include(CreateIncludeExpressionFromFilter(c => c.ChatUsers, _chatUserFilter.ToExpression()));
        }

        return query;
    }
}