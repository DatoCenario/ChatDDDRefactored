using Chatiks.Chat.Data.EF.Domain.Chat;
using Chatiks.Chat.Domain;
using Chatiks.Core.Data.EF.Domain;
using Chatiks.Core.Domain;
using Chatiks.Tools;

namespace Chatiks.Adapters;

public class ChatMessageAdapter
{
    private readonly ChatMessageDomainModel _message;
    private readonly User.Data.EF.Domain.User.User _sender;
    private readonly ICollection<ImageDomainModel> _images;

    public long Id => _message.Id.Value;
    public long ChatId => _message.ChatId;
    public long OwnerId => _message.UserId;
    public string Text => _message.Text;
    public string SendTime => _message.SendTime.ToString();
    public string SenderName => _sender?.FullName; 
    public bool IsMe => false;

    public ICollection<ChatImageAdapter> MessageImages => _message.Images.EmptyIfNull()
        .Join((_images ?? Array.Empty<ImageDomainModel>()), cml => cml.ImageExternalId, i => i.Id, (c, i) => i)
        .Select(i => new ChatImageAdapter(i))
        .ToArray();

    public ChatMessageAdapter(ChatMessageDomainModel message, ICollection<ImageDomainModel> images, User.Data.EF.Domain.User.User sender)
    {
        _message = message;
        _images = images;
        _sender = sender;
    }
}
