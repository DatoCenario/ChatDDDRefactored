using Chatiks.Chat.Managers;
using Chatiks.Chat.Specifications;
using Chatiks.Core.Managers;
using Chatiks.Core.Specifications;
using Chatiks.Models;
using HotChocolate.AspNetCore.Authorization;
using Mapster;
using Microsoft.AspNetCore.Identity;

namespace Chatiks.Queries.ChatMutation;

[ExtendObjectType("Mutation")]
public class ChatMutations
{
    [Authorize]
    public class ChatMutationsData
    {
        public async Task<CreateChatResponse> CreateNewChatWithOneUser(
            [Service] HttpContextAccessor contextAccessor,
            [Service] UserManager<User.Data.EF.Domain.User.User> userManager,
            [Service] ChatManager chatManager,
            string name)
        {
            var user = await userManager.FindByNameAsync(contextAccessor.HttpContext.User.Identity.Name);

            var id = await chatManager.CreateNewChatAsync(user.Id, null, name);

            return new CreateChatResponse()
            {
                ChatId = id,
                ChatUsers = new[] { user }.Select(u => u.Adapt<CreateChatChatUserResponse>()).ToArray()
            };
        }
        
        public async Task<CreateChatResponse> CreatePrivateChat(
            [Service] HttpContextAccessor contextAccessor,
            [Service] UserManager<User.Data.EF.Domain.User.User> userManager,
            [Service] ChatManager chatManager,
            long otherUserId)
        {
            var user = await userManager.FindByNameAsync(contextAccessor.HttpContext.User.Identity.Name);
            var other = await userManager.FindByIdAsync(otherUserId.ToString());

            var id = await chatManager.CreateNewPrivateChatAsync(user.Id, other.Id);

            return new CreateChatResponse()
            {
                ChatId = id,
                ChatUsers = new[] { user, other }.Select(u => u.Adapt<CreateChatChatUserResponse>()).ToArray()
            };
        }
        
        public async Task<SendMessageToChatResponse> SendMessageToChat(
            [Service] HttpContextAccessor contextAccessor,
            [Service] UserManager<User.Data.EF.Domain.User.User> userManager,
            [Service] ChatManager chatManager,
            [Service] ImagesManager imagesManager,
            long chatId,
            string text)
        {
            var user = await userManager.FindByNameAsync(contextAccessor.HttpContext.User.Identity.Name);

            var messId = await chatManager.SendMessageToChatAsync(user.Id, chatId, text);
            
            var spec = new ChatSpecification(new ChatFilter(new []{chatId}));
            spec.IncludeChatUsers();
            spec.IncludeMessages(new MessageFilter()
            {
                Id = messId
            });
            var chat = await chatManager.GetChat(spec);
            var images = await imagesManager.GetImages(new ImagesSpecification(new ImagesFilter()
            {
                Ids = chat.Messages.First().MessageImageLinks.Select(i => i.ExternalImageId).ToArray()
            }));

            return new SendMessageToChatResponse()
            {
                ChatId = chatId,
                Text = text,
                IsMe = true,
                MessageId = messId,
                SenderName = user.FullName,
                SendTime = DateTime.Now,
                Images = images.Select(i => new SendMessageToChatImageResponse
                {
                    Base64String = i.Base64Text
                }).ToArray()
            };
        }
        
        public async Task AddUserToChat(
            [Service] HttpContextAccessor contextAccessor,
            [Service] UserManager<User.Data.EF.Domain.User.User> userManager,
            [Service] ChatManager chatManager,
            long chatId,
            long userId)
        {
            var inviter = await userManager.FindByNameAsync(contextAccessor.HttpContext.User.Identity.Name);
            var user = await userManager.FindByIdAsync(userId.ToString());

            await chatManager.AddUserToChatAsync(inviter.Id, user.Id, chatId);
        }
    }

    public ChatMutationsData Chat => new ChatMutationsData();
}

