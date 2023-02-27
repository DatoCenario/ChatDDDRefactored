using System.Linq.Expressions;
using LinqKit;
using LinqSpecs;

namespace Chatiks.Chat.Specifications;

public class ChatFilter : Specification<Data.EF.Domain.Chat.Chat>
{
    private readonly long? _id;
    private readonly string _name;

    public ChatFilter(
        long? id = null,
        string name = null)
    {
        _id = id;
        _name = name;
    }

    public override Expression<Func<Data.EF.Domain.Chat.Chat, bool>> ToExpression()
    {
        var expression = PredicateBuilder.New<Data.EF.Domain.Chat.Chat>(true);
        
        if (_id.HasValue)
        {
            expression = expression.And(c => c.Id == _id.Value);
        }

        if (!string.IsNullOrEmpty(_name))
        {
            expression = expression.And(c => c.Name.ToLower().Contains(_name.ToLower()));
        }

        return expression;
    }
}
