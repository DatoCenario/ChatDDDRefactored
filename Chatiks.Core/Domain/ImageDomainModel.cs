using Chatiks.Core.Data.EF;
using Chatiks.Tools.Domain;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace Chatiks.Core.Domain;

public class ImageDomainModel: UniqueDomainModelBase
{
    private readonly Image _image;
    private readonly CoreContext _coreContext;
    
    public bool Deleted { get; private set; }
    public int ImageWidth => _image.Width;
    public int ImageHeight => _image.Height;
    public string Base64ImageText => _image.ToBase64String(PngFormat.Instance);

    public ImageDomainModel(long? id, string base64imageText, CoreContext coreContext): base(id)
    {
        _coreContext = coreContext;
        var imageBytes = Convert.FromBase64String(base64imageText);
        _image = SixLabors.ImageSharp.Image.Load(imageBytes, new PngDecoder());
    }

    public void Resize(int width, int height)
    {
        //throw exception if ratio equals zero
        if (width == 0 || height == 0)
        {
            throw new Exception("Image ratio is zero");
        }
        
        // check previous ration equals new and not zero. if not throw exception
        if (ImageWidth != 0 && ImageHeight != 0 && ImageWidth / ImageHeight != width / height)
        {
            throw new Exception("Image ratio is not equal to previous");
        }
        
        _image.Mutate(x => x.Resize(width, height));
    }
    
    // method for deleting image
    public void Delete()
    {
        Deleted = true;
    }
    
    // method for executing image update
    public async Task UpdateAsync()
    {
        if (IsNew())
        {
            // check that image with same base64 text is not exists
            var imageExist = await _coreContext.Images
                .AnyAsync(i => i.Base64Text == Base64ImageText);

            if (!imageExist)
            {
                await _coreContext.Images.AddAsync(new Data.EF.Domain.Image
                {
                    Base64Text = Base64ImageText
                });
            }
        }
        else
        {
            await _coreContext.Images
                .Where(i => i.Id == Id)
                .ExecuteUpdateAsync(i => i.SetProperty(p => p.Base64Text, v => Base64ImageText));
        }
    }
}