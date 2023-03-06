using System.Data.Entity;
using Chatiks.Core.Data.EF.Domain;
using Chatiks.Core.Specifications;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Chatiks.Core.Data.EF;

public class CoreRepository: Chatiks.Tools.EF.RepositoryBase<CoreContext>
{
    public CoreRepository(CoreContext context) : base(context)
    {
    }
    
    public async Task<ICollection<Image>> LoadImagesBySpecificationAsync(ImagesSpecification specification = null)
    {
        var query = Context.Images.AsQueryable();
        query = specification?.Apply(query) ?? query;
        return await query.ToListAsync();
    }
    
    public async Task<Image> LoadImageBySpecificationAsync(ImagesSpecification specification = null)
    {
        var query = Context.Images.AsQueryable();
        query = specification?.Apply(query) ?? query;
        return await query.FirstOrDefaultAsync();
    }

    public ValueTask<EntityEntry<Image>> AddImageAsync(string base64Text)
    {
        var image = new Image()
        {
            Base64Text = base64Text
        };
        
        return Context.Images.AddAsync(image);
    }
}