using Chatiks.Tools.Domain;

namespace Chatiks.Chat.Domain;

public class ChatMessageDomainModel: UniqueDeletableDomainModelBase
{
    private DateTime? _updateTime { get; set; }

    public string Text { get; private set; }
    public DateTime SendTime { get; }
    public ICollection<ChatMessageImageDomainModel> Images { get; private set; }

    public ChatMessageDomainModel(long? id, string text, DateTime sendTime, ICollection<ChatMessageImageDomainModel> images) : base(id)
    {
        Text = text;
        Images = images;
        SendTime = sendTime;
    }
    
    public ChatMessageDomainModel(string text, DateTime sendTime, ICollection<ChatMessageImageDomainModel> images)
    {
        Text = text;
        Images = images;
        SendTime = sendTime;
    }

    // method for editing text
    public void EditText(string text)
    {
        Text = text;
        _updateTime = DateTime.Now;
    }

    // method for adding image from base64 string
    public void AddImage(string[] base64imageTexts)
    {
        foreach (var base64imageText in base64imageTexts)
        {
            Images.Add(new ChatMessageImageDomainModel(null, base64imageText));
        }

        _updateTime = DateTime.Now;
    }

    public void DeleteAllImages()
    {
        foreach (var image in Images)
        {
            image.Delete();
        }

        _updateTime = DateTime.Now;
    }
}