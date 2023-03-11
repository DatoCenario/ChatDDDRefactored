using System.Text.RegularExpressions;
using Chatiks.Core.Data.EF;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using Image = Chatiks.Core.Data.EF.Domain.Image;

namespace Chatiks.Core.Domain;

public class ImageDomainModelFactory
{
    
    
    private readonly CoreContext _coreContext;

    public ImageDomainModelFactory(CoreContext coreContext)
    {
        _coreContext = coreContext;
    }

    public ImageDomainModel CreateFromImage(Image image)
    {
        return new ImageDomainModel(image.Id, image.Base64Text, image.LoadDate, _coreContext);
    }
    
    public ImageDomainModel CreateNew(string base64Text)
    {
        return new ImageDomainModel(null, base64Text, DateTime.Now, _coreContext);
    }
}