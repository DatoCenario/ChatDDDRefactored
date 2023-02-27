using System.Net.Mime;
using System.Text.RegularExpressions;
using Chatiks.Chat.Commands;
using Chatiks.Chat.Data.EF;
using Chatiks.Chat.Data.EF.Domain.Chat;
using Chatiks.Chat.Specifications;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace Chatiks.Chat.Managers;

public class ChatManager
{
    private readonly ChatsRepository _chatsRepository;
    
    private readonly int _maxImageBytes = 12000;
    private readonly Regex _replaceImageHeaderReg = new Regex(@"^data:image\/(png|jpg);base64,");

    public ChatManager(ChatsRepository chatsRepository)
    {
        _chatsRepository = chatsRepository;
    }
    
    public async Task AddUserToChat(long inviterId, long userId, long chatId)
    {
        using (var isolatedOperation = _chatsRepository.BeginIsolatedOperation())
        {
            var chatSpecification = new ChatSpecification(new ChatFilter(chatId));
            chatSpecification.IncludeMessages();
            chatSpecification.IncludeChatUsers();

            var chat = await _chatsRepository.LoadFirstOrDefaultBySpecificationAsync(chatSpecification);
            
            if (chat == null)
            {
                throw new Exception("Chat not found");
            }

            if (chat.ChatUsers.All(u => u.ExternalUserId != inviterId))
            {
                throw new Exception("Inviter not in chat");
            }

            var addCommand = new AddChatUserCommand
            {
                ChatId = chatId,
                InviterId = inviterId,
                UserId = userId
            };

            await _chatsRepository.AddNewChatUserAsync(addCommand);

            await isolatedOperation.SaveChangesAsync();
        }
    }
    
    public async Task<SendMessageToChatEventArgs> SendMessageToChatAsync(
        long userId,
        long chatId,
        string text = null,
        params string[]? imagesBase64)
    {
        imagesBase64 ??= Array.Empty<string>();

        imagesBase64 = imagesBase64.Select(i =>
            {
                i = _replaceImageHeaderReg.Replace(i, "");
                var imageBytes = Convert.FromBase64String(i);
                if (imageBytes.Length > _maxImageBytes)
                {
                    // no need to load my server (not quite correct algorithm - optimizes size by delta but not bytes lenght)
                    using (var image = SixLabors.ImageSharp.Image.Load(imageBytes, new PngDecoder()))
                    {
                        var delta = Math.Sqrt(imageBytes.Length / _maxImageBytes);
                        image.Mutate(o => o.Resize(new Size
                        {
                            Width = (int)(image.Width / delta),
                            Height = (int)(image.Height / delta)
                        }));
                        i = image.ToBase64String(PngFormat.Instance);
                        i = _replaceImageHeaderReg.Replace(i, "");
                    }
                }

                return i;
            })
            .ToArray();

        var spec = new ChatSpecification(new ChatFilter(chatId));
        spec.IncludeChatUsers(new ChatUserFilter()
        {
            Id = userId
        });
        var chat = await _chatsRepository.LoadFirstOrDefaultBySpecificationAsync();

        if (chat == null)
        {
            throw new Exception("Chat not found");
        }

        if (chat.ChatUsers.All(u => u.ExternalUserId != userId))
        {
            throw new Exception("User not in chat");
        }

        var mess = new ChatMessage
        {
            ExternalOwnerId = userId,
            Text = text,
            ChatId = chat.Id,
            SendTime = DateTime.Now,
            MessageImageLinks = imagesBase64.Select(i => new ChatMessageImageLink
                {
                    Image = new MediaTypeNames.Image()
                    {
                        Base64Text = i
                    }
                })
                .ToArray()
        };

        return new SendMessageToChatEventArgs
        {
            MessageId = mess.Id,
            SenderName = $"{user.FirstName} {user.LastName}",
            Text = mess.Text,
            SendTime = mess.SendTime.ToShortTimeString(),
            ChatId = chatId,
            Images = imagesBase64.Select(x => new SendMessageToChatEventImage
            {
                Base64String = x
            }).ToArray()
        };
    }
}