using System.Linq.Expressions;
using Chatiks.Chat.Data.EF.Domain.Chat;
using LinqKit;
using LinqSpecs;
using Microsoft.EntityFrameworkCore;

namespace Chatiks.Chat.Specifications;

public class MessageFilter: Specification<ChatMessage>
{
    public int? Id { get; set; }
    public int? OwnerId { get; set; }
    public string Text { get; set; }
    public DateTime? SendTimeFrom { get; set; }
    public DateTime? SendTimeTo { get; set; }
    
    
    public override Expression<Func<ChatMessage, bool>> ToExpression()
    {
        var expression = PredicateBuilder.True<ChatMessage>();

        if (Id.HasValue)
        {
            expression = expression.And(cm => cm.Id == Id.Value);
        }
        if (OwnerId.HasValue)
        {
            expression = expression.And(cm => cm.ExternalOwnerId == OwnerId.Value);
        }
        if (!string.IsNullOrEmpty(Text))
        {
            expression = expression.And(cm => EF.Functions.ILike(cm.Text, $"%{Text}%"));
        }
        if (SendTimeFrom.HasValue)
        {
            expression = expression.And(cm => cm.SendTime >= SendTimeFrom.Value);
        }
        if (SendTimeTo.HasValue)
        {
            expression = expression.And(cm => cm.SendTime <= SendTimeTo.Value);
        }

        return expression;
    }
}