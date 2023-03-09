using System.Text.RegularExpressions;
using Chatiks.Core.Data.EF;
using Chatiks.Core.Data.EF.Domain;
using Chatiks.Core.Domain;
using Chatiks.Core.Specifications;
using Microsoft.EntityFrameworkCore;
using Image = Chatiks.Core.Data.EF.Domain.Image;

namespace Chatiks.Core.Managers;

public class ImagesManager
{
    private readonly CoreContext _coreContext;
    private readonly ImageDomainModelFactory _imageDomainModelFactory;
    
    public ImagesManager(ImageDomainModelFactory imageDomainModelFactory, CoreContext coreContext)
    {
        _imageDomainModelFactory = imageDomainModelFactory;
        _coreContext = coreContext;
    }
    
    public async Task<List<ImageDomainModel>> UploadNewImagesAsync(ICollection<string> base64texts)
    {
        var images= base64texts.Select(x => _imageDomainModelFactory.CreateNew(x)).ToList();

        await UpdateImagesAsync(images);

        return images;
    }
    
    // method that takes collection of ImageDomainModel and updates database
    public async Task UpdateImagesAsync(ICollection<ImageDomainModel> images)
    {
        var toUpdate = images.Where(i => !i.IsNew() && !i.Deleted).ToArray();
        var toDelete = images.Where(i => !i.IsNew() && i.Deleted).ToArray();
        var toCreate = images.Where(i => i.IsNew()).ToArray();

        if (toUpdate.Any())
        {
            foreach (var image in toUpdate)
            {
                await _coreContext.Images
                    .Where(x => x.Id == image.Id)
                    .ExecuteUpdateAsync(i => i.SetProperty(p => p.Base64Text, v => image.Base64ImageText));
            }
        }
        
        if (toDelete.Any())
        {
            foreach (var image in toUpdate)
            {
                var ids = toDelete.Select(x => x.Id).ToArray();
                await _coreContext.Images
                    .Where(i => ids.Contains(i.Id)) 
                    .ExecuteDeleteAsync();
            }
        }
        
        if(toCreate.Any()) 
        {
            await _coreContext.Images.AddRangeAsync(toCreate.Select(x => new Image()
            {
                Base64Text = x.Base64ImageText
            }));
        }
    }
}