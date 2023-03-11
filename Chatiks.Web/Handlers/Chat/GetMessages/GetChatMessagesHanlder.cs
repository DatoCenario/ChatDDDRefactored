using Chatiks.Adapters;
using Chatiks.Chat.Managers;
using Chatiks.Chat.Specifications;
using Chatiks.Core.Managers;
using Chatiks.Core.Specifications;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Chatiks.Handlers.Chat.GetMessages;

public class GetChatMessagesHadnlder: CheckUserInChatQueryHandlerBase<GetChatMessagesRequest, GetChatMessagesResponse>
{
    private readonly TypeAdapterConfig _typeAdapterConfig;
    private readonly HttpContextAccessor _contextAccessor;
    private readonly ImagesManager _imagesManager;

    public GetChatMessagesHadnlder(ChatManager chatManager, UserManager<User.Data.EF.Domain.User.User> userManager, HttpContextAccessor contextAccessor, TypeAdapterConfig typeAdapterConfig, ImagesManager imagesManager) : base(userManager, contextAccessor, chatManager)
    {
        _contextAccessor = contextAccessor;
        _typeAdapterConfig = typeAdapterConfig;
        _imagesManager = imagesManager;
    }

    protected override async Task<GetChatMessagesResponse> InnerHandle(GetChatMessagesRequest request, CancellationToken cancellationToken)
    {
        request.Count = request.Count <= 0 ? int.MaxValue : request.Count;
        
        var me = await UserManager.FindByNameAsync(_contextAccessor.HttpContext.User.Identity.Name);

        var filter = new MessageFilter()
        {
            ChatId = request.ChatId
        };
        var allCount = await ChatManager.GetMessagesCountAsync(filter);
        var spec = new MessageSpecification(filter);
        spec.SetLimit(request.Count);
        spec.SetOffset(request.Offset);
        var messages = await ChatManager.LoadMessagesBySpecificationAsync(spec);
        
        // Create common methods for composing like this
        var imagesIds = messages
            .SelectMany(m => m.Images.Select(l => l.ImageExternalId.Value))
            .ToArray();
        var images = await _imagesManager.LoadBySpecificationAsync(new ImagesSpecification(new ImagesFilter()
        {
            Ids = imagesIds
        }));
        var sendersIds = messages.Select(m => m.UserId).ToArray();
        var senders = await UserManager.Users.Where(u => sendersIds.Contains(u.Id)).ToDictionaryAsync(k => k.Id);

        var messagesData = messages
            .Select(m => new ChatMessageAdapter(m, images, senders[m.UserId]).Adapt<ChatMessageReponse>())
            .ToArray();

        foreach (var mess in messagesData)
        {
            if (mess.OwnerId == me.Id)
            {
                mess.IsMe = true;
            }
        }
        
        return new GetChatMessagesResponse
        {
            ChatMessages = messagesData,

            EntitiesLeft = Math.Max(0, allCount - request.Offset - request.Count)
        };
    }
}