using System.Text.RegularExpressions;
using Chatiks.Core.Data.EF;
using Chatiks.Core.Data.EF.Domain;
using Chatiks.Core.Domain;
using Chatiks.Core.Specifications;
using Chatiks.Tools.EF;
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
    
    public async Task<ICollection<ImageDomainModel>> UploadNewImagesAsync(ICollection<string> base64texts)
    {
        var images= base64texts.Select(x => _imageDomainModelFactory.CreateNew(x)).ToList();

        return await UpdateImagesAsync(images);
    }
    
    // method that takes collection of ImageDomainModel and updates database
    public async Task<ICollection<ImageDomainModel>> UpdateImagesAsync(ICollection<ImageDomainModel> images)
    {
        var resultDtoList = new List<Image>();
        
        var toUpdate = images.Where(i => !i.IsNew() && !i.Deleted).ToArray();
        var toDelete = images.Where(i => !i.IsNew() && i.Deleted).ToArray();
        var toCreate = images.Where(i => i.IsNew()).ToArray();

        using (var io = _coreContext.BeginIsolatedOperation())
        {
            if (toUpdate.Any())
            {
                var imagesDtos = await  _coreContext.Images.LoadBySpecificationAsync(
                    new ImagesSpecification(new ImagesFilter()
                    {
                        Ids = toUpdate.Select(x => x.Id.Value).ToArray()
                    }));
                
                foreach (var image in toUpdate)
                {
                    var imageDto = imagesDtos.FirstOrDefault(i => i.Id == image.Id);
                    if (imageDto != null)
                    {
                        imageDto.Base64Text = image.Base64ImageText;
                        resultDtoList.Add(imageDto);
                    }
                }
            }
        
            if (toDelete.Any())
            {
                foreach (var image in toUpdate)
                {
                    var ids = toDelete.Select(x => x.Id).ToArray();
                    await _coreContext.Images
                        .Where(i => ids.Contains(i.Id)) 
                        .ExecuteUpdateAsync(i => i.SetProperty(p => p.Base64Text, v => "deleted from server"));
                }
            }
        
            if(toCreate.Any())
            {
                var newImages = toCreate.Select(x => new Image()
                {
                    Base64Text = x.Base64ImageText
                });
            
                await _coreContext.Images.AddRangeAsync(newImages);
            
                resultDtoList.AddRange(newImages);
            }

            await _coreContext.SaveChangesAsync();
        }

        return resultDtoList.Select(i => _imageDomainModelFactory.CreateFromImage(i)).ToList();
    }
}