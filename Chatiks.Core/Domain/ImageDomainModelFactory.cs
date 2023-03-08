using System.Text.RegularExpressions;
using Chatiks.Core.Data.EF;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using Image = Chatiks.Core.Data.EF.Domain.Image;

namespace Chatiks.Core.Domain;

public class ImageDomainModelFactory
{
    private readonly int _maxImageBytes = 12000;
    private readonly Regex _replaceImageHeaderReg = new Regex(@"^data:image\/(png|jpg);base64,");
    
    private readonly CoreContext _coreContext;

    public ImageDomainModelFactory(CoreContext coreContext)
    {
        _coreContext = coreContext;
    }

    public ImageDomainModel CreateFromImage(Image image)
    {
        return new ImageDomainModel(image.Id, image.Base64Text, _coreContext);
    }
    
    public ImageDomainModel CreateNew(string base64Text)
    {
        base64Text = _replaceImageHeaderReg.Replace(base64Text, "");
        var imageBytes = Convert.FromBase64String(base64Text);
        if (imageBytes.Length > _maxImageBytes)
        {
            using (var image = SixLabors.ImageSharp.Image.Load(imageBytes, new PngDecoder()))
            {
                var delta = Math.Sqrt(imageBytes.Length / _maxImageBytes);
                image.Mutate(o => o.Resize(new Size
                {
                    Width = (int)(image.Width / delta),
                    Height = (int)(image.Height / delta)
                }));
                base64Text = image.ToBase64String(PngFormat.Instance);
                base64Text = _replaceImageHeaderReg.Replace(base64Text, "");
            }
        }
        
        return new ImageDomainModel(null, base64Text, _coreContext);
    }
}