using Chatiks.Chat.Data.EF.Domain.Chat;
using Chatiks.Chat.Managers;
using Chatiks.Chat.Specifications;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Chatiks.Handlers.Chat.GetChats;

public class GetChatsQueryHandler: IRequestHandler<GetChatsRequest, ICollection<GetChatsResponse>>
{
    private readonly UserManager<User.Data.EF.Domain.User.User> _userManager;
    private readonly HttpContextAccessor _contextAccessor;
    private readonly TypeAdapterConfig _typeAdapterConfig;
    private readonly ChatManager _chatManager;

    public GetChatsQueryHandler(UserManager<User.Data.EF.Domain.User.User> userManager, HttpContextAccessor contextAccessor, TypeAdapterConfig typeAdapterConfig, ChatManager chatManager)
    {
        _userManager = userManager;
        _contextAccessor = contextAccessor;
        _typeAdapterConfig = typeAdapterConfig;
        _chatManager = chatManager;
    }

    public async Task<ICollection<GetChatsResponse>> Handle(GetChatsRequest request, CancellationToken cancellationToken)
    {
        var chatsData = new List<GetChatsResponse>();
        var me = await _userManager.FindByNameAsync(_contextAccessor.HttpContext.User.Identity.Name);

        var spec = new ChatSpecification(new ChatFilter()
        {
            Name = request.NameFilter,
            HasUserIds = request.OnlyUserChats == true ? new []{ me.Id } : null
        });
        spec.IncludeLastMessages();
        spec.IncludeChatUsers();
        var chats = await _chatManager.GetChats(spec);

        foreach (var chat in chats)
        {
            var chatData = chat.Adapt<GetChatsResponse>();
            chatsData.Add(chatData);
            
            if (string.IsNullOrEmpty(chat.Name))
            {
                var other = chat.ChatUsers.FirstOrDefault(c => c.ExternalUserId != me.Id);
                if (other != null)
                {
                    var otherUser = await _userManager.FindByIdAsync(other.ExternalUserId.ToString());
                    chatData.Name = otherUser.FullName;
                }
                else
                {
                    chatData.Name = me.FullName;
                }
            }
            
            var lastMess = chat.Messages.LastOrDefault();
            if (lastMess != null)
            {
                var lastMessageOwner = await _userManager.FindByIdAsync(lastMess.ExternalOwnerId.ToString());
                chatData.LastMessage = lastMess.Text;
                chatData.LastMessageSender = lastMessageOwner.FullName;
                chatData.LastMessageSendTime = lastMess.SendTime.ToShortDateString();
            }
        }

        return chatsData;
    }
}