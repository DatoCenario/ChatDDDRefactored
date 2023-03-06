using System.Linq.Expressions;
using System.Net.Mime;
using Chatiks.Core.Data.EF.Domain;
using LinqKit;
using LinqSpecs;
using Microsoft.EntityFrameworkCore;

namespace Chatiks.Core.Specifications;

public class ImagesFilter: Specification<Image>
{
    public long[] Ids { get; set; }
    public string Base64Text { get; set; }
    
    
    public override Expression<Func<Image, bool>> ToExpression()
    {
        var expression = PredicateBuilder.True<Image>();

        if (Ids != null)
        {
            expression = expression.And(cm => Ids.Contains(cm.Id));
        }
        
        if (Base64Text != null)
        {
            expression = expression.And(cm => cm.Base64Text == Base64Text);
        }

        return expression;
    }
}