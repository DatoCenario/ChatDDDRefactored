using Chatiks.Chat.Data.EF.Domain.Chat;
using Chatiks.Tools.Domain;

namespace Chatiks.Chat.Domain;

public class ChatMessageDomainModel: UniqueDeletableDomainModelBase
{
    private DateTime? _updateTime { get; set; }

    public string Text { get; private set; }
    public long UserId { get; }
    public DateTime SendTime { get; }
    public ICollection<ChatMessageImageDomainModel> Images { get; private set; }
    
    public ChatMessageDomainModel(long? id, string text, DateTime sendTime, long userId, ICollection<string> imagesBase64) : base(id)
    {
        // validate that message not empty
        if (string.IsNullOrEmpty(text) && (imagesBase64 == null || imagesBase64.Count == 0))
        {
            throw new Exception("Message is empty");
        }
        
        Text = text;
        Images = imagesBase64.Select(i => new ChatMessageImageDomainModel(null, i)).ToList();
        UserId = userId;
        SendTime = sendTime;
    }

    public ChatMessageDomainModel(
        long? id,
        string text,
        DateTime sendTime,
        long userId,
        ICollection<ChatMessageImageDomainModel> images) : base(id)
    {
        // validate that message not empty
        if (string.IsNullOrEmpty(text) && (images == null || images.Count == 0))
        {
            throw new Exception("Message is empty");
        }
        
        Text = text;
        Images = images;
        UserId = userId;
        SendTime = sendTime;
    }
    
    public ChatMessageDomainModel(
        string text,
        long userId,
        DateTime sendTime,
        ICollection<ChatMessageImageDomainModel> images)
    {
        // validate that message not empty
        if (string.IsNullOrEmpty(text) && (images == null || images.Count == 0))
        {
            throw new Exception("Message is empty");
        }

        Text = text;
        Images = images;
        UserId = userId;
        SendTime = sendTime;
    }

    // method for editing text
    public void EditText(string text)
    {
        ThrowOperationExceptionIfDeleted();
        
        Text = text;
        _updateTime = DateTime.Now;
    }

    // method for adding image from base64 string
    public void AddImage(string[] base64imageTexts)
    {
        ThrowOperationExceptionIfDeleted();
        
        foreach (var base64imageText in base64imageTexts)
        {
            Images.Add(new ChatMessageImageDomainModel(null, base64imageText));
        }

        _updateTime = DateTime.Now;
    }

    public void DeleteAllImages()
    {
        ThrowOperationExceptionIfDeleted();
        
        foreach (var image in Images)
        {
            image.Delete();
        }

        _updateTime = DateTime.Now;
    }
}