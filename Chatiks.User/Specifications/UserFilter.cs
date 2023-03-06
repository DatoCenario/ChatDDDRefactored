using System.Linq.Expressions;
using LinqKit;
using LinqSpecs;

namespace Chatiks.User.Specifications;

public class UserFilter: Specification<Data.EF.Domain.User.User>
{
    public long? Id { get; set; }
    
    
    public override Expression<Func<Data.EF.Domain.User.User, bool>> ToExpression()
    {
        var expression = PredicateBuilder.True<Data.EF.Domain.User.User>();

        if (Id.HasValue)
        {
            expression = expression.And(cm => cm.Id == Id.Value);
        }

        return expression;
    }
}