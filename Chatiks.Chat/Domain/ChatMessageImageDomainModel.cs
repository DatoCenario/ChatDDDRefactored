using Chatiks.Tools.Domain;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace Chatiks.Chat.Domain;

public class ChatMessageImageDomainModel: IUniqueDomainModel<long>
{
    private readonly Image _image;
    
    public bool Deleted { get; private set; }
    public int ImageWidth => _image.Width;
    public int ImageHeight => _image.Height;
    public string Base64ImageText => _image.ToBase64String(PngFormat.Instance);

    public ChatMessageImageDomainModel(string base64imageText)
    {
        var imageBytes = Convert.FromBase64String(base64imageText);
        _image = SixLabors.ImageSharp.Image.Load(imageBytes, new PngDecoder());
    }

    public void SetSize(int width, int height)
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

    public long Id { get; }
}