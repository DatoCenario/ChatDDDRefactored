using Chatiks.Chat.Specifications;
using Chatiks.Core.Data.EF.Domain;
using LinqSpecs;

namespace Chatiks.Core.Specifications;

public class ImagesSpecification: SpecificationBase<Image>
{
    public ImagesSpecification(ImagesFilter filter = null) : base(filter)
    {
    }

    protected override IQueryable<Image> Include(IQueryable<Image> query)
    {
        return query;
    }
}