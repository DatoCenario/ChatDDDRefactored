using System.Linq.Expressions;
using System.Net.Mime;
using Chatiks.Chat.Data.EF.Domain.Chat;
using LinqKit;
using LinqSpecs;
using Microsoft.EntityFrameworkCore;

namespace Chatiks.Chat.Specifications;

public class ChatUserFilter: Specification<ChatUser>
{
    public long? UserId { get; set; }
    
    public override Expression<Func<ChatUser, bool>> ToExpression()
    {
        var expression = PredicateBuilder.True<ChatUser>();

        if (UserId.HasValue)
        {
            expression = expression.And(cm => cm.ExternalUserId == UserId.Value);
        }

        return expression;
    }
}