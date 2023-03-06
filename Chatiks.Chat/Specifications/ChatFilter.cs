using System.Linq.Expressions;
using LinqKit;
using LinqSpecs;

namespace Chatiks.Chat.Specifications;

public class ChatFilter : Specification<Data.EF.Domain.Chat.Chat>
{
    public bool? IsPrivate { get; set; }
    public long[] Ids { get; set; }
    public string Name { get; set; }
    public long? CreatorUserId { get; set; }
    public long[] HasUserIds { get; set; }


    public ChatFilter(
        long[] ids = null,
        string name = null)
    {
        Ids = ids;
        Name = name;
    }

    public override Expression<Func<Data.EF.Domain.Chat.Chat, bool>> ToExpression()
    {
        var expression = PredicateBuilder.New<Data.EF.Domain.Chat.Chat>(true);
        
        if (Ids != null)
        {
            expression = expression.And(c => Ids.Contains(c.Id));
        }

        if (!string.IsNullOrEmpty(Name))
        {
            expression = expression.And(c => c.Name.ToLower().Contains(Name.ToLower()));
        }

        if (CreatorUserId.HasValue)
        {
            expression = expression.And(c => c.ChatUsers.Any(u => u.ExternalInviterId == null && u.ExternalUserId == CreatorUserId));
        }
        
        if (HasUserIds != null)
        {
            expression = expression.And(c => c.ChatUsers.Any(u => HasUserIds.Contains(u.ExternalUserId)));
        }
        
        if (IsPrivate.HasValue)
        {
            expression = expression.And(c => c.IsPrivate);
        }

        return expression;
    }
}
