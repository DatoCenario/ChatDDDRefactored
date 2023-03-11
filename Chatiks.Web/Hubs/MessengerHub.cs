using Chatiks.Chat.Managers;
using Chatiks.Chat.Specifications;
using Chatiks.Core.Managers;
using Chatiks.Core.Specifications;
using Chatiks.Models;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Hubs.Models.Chat;

namespace Chatiks.Hubs;

[Authorize]
[Route("/hub/messengerHub")]
public class MessengerHub: Hub
{
    private static Dictionary<long, string> _userConnections = new Dictionary<long, string>();

    private readonly UserManager<User.Data.EF.Domain.User.User> _userManager;
    private readonly ChatManager _chatManager;
    private readonly ImagesManager _imagesManager;


    public MessengerHub(UserManager<User.Data.EF.Domain.User.User> userManager, ChatManager chatManager, ImagesManager imagesManager)
    {
        _userManager = userManager;
        _chatManager = chatManager;
        _imagesManager = imagesManager;
    }

    [HubMethodName("sendMessageToChat")]
    public async Task SendMessageToChat(SendMessageToChatRequest request)
    {
        var response = new SendMessageToChatResponse();
        
        var user = await _userManager.FindByNameAsync(Context.User.Identity.Name);

        var spec = new ChatSpecification(new ChatFilter(new []{request.ChatId}));
        spec.IncludeChatUsers();
        spec.IncludeMessages(new MessageFilter()
        {
            Id = response.MessageId
        });
        var chat = await _chatManager.LoadChatBySpecificationAsync(spec);
        chat.SendMessage(request.Text, user.Id, request.ImagesBase64);
        chat = await _chatManager.UpdateChatAsync(chat);
        var message = chat.Messages.Last();

        response.MessageId = message.Id.Value;

        var userIds = chat.Users.Select(c => c.UserId).ToString();
        var images = await _imagesManager.LoadBySpecificationAsync(new ImagesSpecification(new ImagesFilter()
        {
            Ids = chat.Messages.First().Images.Select(i => i.ImageExternalId.Value).ToArray()
        }));
            
        response.Text = request.Text;
        response.ChatId = request.ChatId;
        response.SendTime = DateTime.Now;
        response.SenderName = user.FullName;
        response.Images = images.Select(i => new SendMessageToChatImageResponse
        {
            Base64String = i.Base64ImageText
        }).ToArray();

        // Refactor this !!!
        response.IsMe = true;

        await Clients.Caller.SendCoreAsync("messageSendEvent", new []
        {
            response
        });

        var connections = userIds
            .Where(u => u != user.Id)
            .Select(u => _userConnections.GetValueOrDefault(u))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();

        response.IsMe = false;

        var clientsFromChat = Clients.Clients(connections);

        await clientsFromChat.SendCoreAsync("messageSendEvent", new []
        {
            response
        });
    }
    
    [HubMethodName("сreateChat")]
    public async Task CreateChat([FromBody] CreateChatRequest request)
    {
        var response = new CreateChatResponse();
        
        var user = await _userManager.FindByNameAsync(Context.User.Identity.Name);
        var otherIds = request.UsersIds.Except(new[] { user.Id }).ToArray();
        
        var chat = await _chatManager.CreateGroupChatAsync(user.Id, request.Name, otherIds);
        response.ChatId = chat.Id.Value;

        var allChatUsersIds = otherIds.Concat(new[] { user.Id }).ToArray();

        var allChatUsers = await _userManager.Users
            .Where(u => allChatUsersIds.Contains(u.Id))
            .ToArrayAsync();

        var connections = allChatUsersIds
            .Select(u => _userConnections.GetValueOrDefault(u))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();
        
        var clientsFromChat = Clients.Clients(connections);

        response.Name = request.Name;
        response.ChatUsers = allChatUsers.Select(u => u.Adapt<CreateChatChatUserResponse>()).ToArray();
        
        await clientsFromChat.SendCoreAsync("chatCreateEvent", new []
        {
            response
        });
    }
    
    [HubMethodName("сreatePrivateChat")]
    public async Task CreatePrivateChat(long userId)
    {
        var response = new CreateChatResponse();
        
        var user = await _userManager.FindByNameAsync(Context.User.Identity.Name);
        var other = await _userManager.FindByIdAsync(userId.ToString());

        var chat = await _chatManager.CreateNewPrivateChatAsync(user.Id, other.Id);
        response.ChatId = chat.Id.Value;

        response.IsPrivate = true;
        response.ChatUsers = new[]{user, other}.Select(u => u.Adapt<CreateChatChatUserResponse>()).ToArray();

        response.Name = $"{other.FirstName} {other.LastName}";
        await Clients.Caller.SendCoreAsync("chatCreateEvent", new[]
        {
            response
        });

        if (_userConnections.ContainsKey(other.Id))
        {
            response.Name = user.FullName;
            await Clients.Client(_userConnections[other.Id]).SendCoreAsync("chatCreateEvent", new []
            {
                response
            });
        }
    }

    [HubMethodName("addUserToChat")]
    public async Task AddUserToChat(AddUserToChatRequest request)
    {
        var inviter = await _userManager.FindByNameAsync(Context.User.Identity.Name);
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        
        var response = new AddUserToChatResponse();
        response.ChatId = request.ChatId;
        response.FirstName = user.FirstName;
        response.LastName = user.LastName;

        var spec = new ChatSpecification(new ChatFilter(new []{request.ChatId}));
        spec.IncludeChatUsers();
        var chat = await _chatManager.LoadChatBySpecificationAsync(spec);
        
        chat.AddUser(inviter.Id, user.Id);
        chat = await _chatManager.UpdateChatAsync(chat);
        
        var connections = chat.Users.Select(u => u.UserId)
            .Select(u => _userConnections.GetValueOrDefault(u))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();
        
        var clientsFromChat = Clients.Clients(connections);

        await clientsFromChat.SendCoreAsync("addUserEvent", new []
        {
            response
        });
    }

    [HubMethodName("leaveChat")]
    public async Task LeaveChat(long chatId)
    {
        var me = await _userManager.FindByNameAsync(Context.User.Identity.Name);
        var response = new LeaveChatResponse()
        {
            ChatId = chatId,
            LeavedUserName = me.FullName
        };

        var spec = new ChatSpecification(new ChatFilter(new []{chatId}));
        spec.IncludeChatUsers();
        var chat = await _chatManager.LoadChatBySpecificationAsync(spec);
        
        chat.LeaveChat(me.Id);
        chat = await _chatManager.UpdateChatAsync(chat);
        
        var connections = chat.Users.Select(u => u.UserId)
            .Select(u => _userConnections.GetValueOrDefault(u))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();
        
        var clientsFromChat = Clients.Clients(connections);

        await clientsFromChat.SendCoreAsync("leaveChatEvent", new []
        {
            response
        });
    }
    
    public override async Task OnConnectedAsync()
    {
        var user = await _userManager.FindByNameAsync(Context.User.Identity.Name);

        _userConnections[user.Id] = Context.ConnectionId;
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = await _userManager.FindByNameAsync(Context.User.Identity.Name);

        _userConnections.Remove(user.Id);
        
        await base.OnDisconnectedAsync(exception);
    }
}