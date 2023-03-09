using Chatiks.Chat.Data.EF;

namespace Chatiks.Chat.Domain;

public class ChatDomainModelFactory
{
    private readonly ChatContext _chatContext;

    public ChatDomainModelFactory(ChatContext chatContext)
    {
        _chatContext = chatContext;
    }

    public ChatDomainModel CreateFromChat(Data.EF.Domain.Chat.Chat chat)
    {
        return new ChatDomainModel(
            chat.Id,
            chat.Messages.Select(m => CreateFromMessage(m)).ToList());
    }
    
    public ChatMessageDomainModel CreateFromMessage(Data.EF.Domain.Chat.ChatMessage message)
    {
        return new ChatMessageDomainModel(
            message.Id,
            message.Text,
            message.SendTime,
            message.ExternalOwnerId,
            message.MessageImageLinks.Select(i => CreateFromImageLink(i)).ToList());
    }   
    
    public ChatMessageImageDomainModel CreateFromImageLink(Data.EF.Domain.Chat.ChatMessageImageLink imageLink)
    {
        return new ChatMessageImageDomainModel(imageLink.ExternalImageId);
    }
    
    public ChatDomainModel CreateNewChat()
    {
        return new ChatDomainModel(null, new List<ChatMessageDomainModel>());
    }
    
    public ChatMessageDomainModel CreateNewMessage(long userId, string text = null, ICollection<string> imagesBase64 = null)
    {
        return new ChatMessageDomainModel(null, text, DateTime.Now, userId, imagesBase64);
    }

    public ChatMessageImageDomainModel CreateNewImage(string base64Text)
    {
        return new ChatMessageImageDomainModel(null, base64Text);
    }
}