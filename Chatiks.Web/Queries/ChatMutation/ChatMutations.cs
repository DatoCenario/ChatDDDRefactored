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

            var chat = await chatManager.CreateGroupChatAsync(user.Id, name, new[] { user.Id });

            return new CreateChatResponse()
            {
                ChatId = chat.Id.Value,
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

            var chat = await chatManager.CreateNewPrivateChatAsync(user.Id, other.Id);

            return new CreateChatResponse()
            {
                ChatId = chat.Id.Value,
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
            
            var spec = new ChatSpecification(new ChatFilter(new []{chatId}));
            spec.IncludeChatUsers();
            var chat = await chatManager.LoadChatBySpecificationAsync(spec);
            chat.SendMessage(text, user.Id, null);
            chat = await chatManager.UpdateChatAsync(chat);
            var mess = chat.Messages.Last();

            var images = await imagesManager.LoadBySpecificationAsync(new ImagesSpecification(new ImagesFilter()
            {
                Ids = chat.Messages.First().Images.Select(i => i.ImageExternalId.Value).ToArray()
            }));

            return new SendMessageToChatResponse()
            {
                ChatId = chatId,
                Text = text,
                IsMe = true,
                MessageId = mess.Id.Value,
                SenderName = user.FullName,
                SendTime = DateTime.Now,
                Images = images.Select(i => new SendMessageToChatImageResponse
                {
                    Base64String = i.Base64ImageText
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

