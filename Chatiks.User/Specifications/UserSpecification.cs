using Chatiks.Chat.Specifications;

namespace Chatiks.User.Specifications;

public class UserSpecification: SpecificationBase<Data.EF.Domain.User.User>
{
    protected override IQueryable<Data.EF.Domain.User.User> Include(IQueryable<Data.EF.Domain.User.User> query)
    {
        return query;
    }
}