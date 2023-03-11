using Chatiks.Core.Data.EF.Domain;
using Chatiks.Core.Domain;

namespace Chatiks.Adapters;

public class ChatImageAdapter
{
    private readonly ImageDomainModel _image;

    public ChatImageAdapter(ImageDomainModel image)
    {
        _image = image;
    }

    public string Base64Text => _image.Base64ImageText;
}
