using System.Text.RegularExpressions;
using Chatiks.Core.Data.EF;
using Chatiks.Core.Data.EF.Domain;
using Chatiks.Tools.Domain;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

namespace Chatiks.Core.Domain;

public class ImageDomainModel: UniqueDeletableDomainModelBase, IDisposable
{
    private static int MaxImageBytes = 12000;
    private static Regex ReplaceImageHeaderReg = new Regex(@"^data:image\/(png|jpg);base64,");
    
    private readonly Image _image;
    private readonly CoreContext _coreContext;


    public int ImageWidth => _image.Width;
    public int ImageHeight => _image.Height;
    public string Base64ImageText => _image.ToBase64String(PngFormat.Instance);

    public ImageDomainModel(
        long? id,
        string base64imageText,
        CoreContext coreContext): base(id)
    {
        _coreContext = coreContext;
        
        base64imageText = ReplaceImageHeaderReg.Replace(base64imageText, "");
        var imageBytes = Convert.FromBase64String(base64imageText);
        _image = SixLabors.ImageSharp.Image.Load(imageBytes, new PngDecoder());

        if (!id.HasValue && imageBytes.Length > MaxImageBytes)
        {
            var delta = Math.Sqrt(imageBytes.Length / MaxImageBytes);
            _image.Mutate(o => o.Resize(new Size
            {
                Width = (int)(_image.Width / delta),
                Height = (int)(_image.Height / delta)
            }));
        }
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

    public void Dispose()
    {
        _image.Dispose();
    }
}