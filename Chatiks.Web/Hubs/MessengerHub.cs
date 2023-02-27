using System.Transactions;
using Chatiks.Chat.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebApplication2.Hubs.Models.Chat;

namespace Chatiks.Hubs;

[Authorize]
[Route("/hub/messengerHub")]
public class MessengerHub: Hub
{
    private static Dictionary<long, string> _userConnections = new Dictionary<long, string>();

    private readonly UserManager<User.Data.EF.Domain.User.User> _userManager;
    private readonly ChatManager _chatManager;


    public MessengerHub(UserManager<User.Data.EF.Domain.User.User> userManager, ChatManager chatManager)
    {
        _userManager = userManager;
        _chatManager = chatManager;
    }

    [HubMethodName("sendMessageToChat")]
    public async Task SendMessageToChat(SendMessageToChatRequest request)
    {
        using (var transactionScope = new TransactionScope())
        {
            var user = await _userManager.FindByNameAsync(Context.User.Identity.Name);

            var result = await _chatManager.SendMessageToChatAsync(user, request.ChatId, request.Text, request.ImagesBase64);

            // Refactor this !!!
            result.IsMe = true;
        
            await Clients.Caller.SendCoreAsync("messageSendEvent", new []
            {
                result
            });
        
            var userIds = await _applicationContext.Users
                .Where(u => u.ChatUsers.Any(c => c.ChatId == request.ChatId))
                .Select(x => x.Id)
                .ToArrayAsync();
        
            var connections = userIds
                .Where(u => u != user.Id)
                .Select(u => _userConnections.GetValueOrDefault(u))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();

            result.IsMe = false;

            var clientsFromChat = Clients.Clients(connections);

            await clientsFromChat.SendCoreAsync("messageSendEvent", new []
            {
                result
            });
        }
    }
    
    [HubMethodName("сreateChat")]
    public async Task CreateChat([FromBody] CreateChatRequest request)
    {
        var user = await _userManager.FindByNameAsync(Context.User.Identity.Name);

        var others = await _applicationContext.Users
            .Where(x => request.UsersIds.Contains(x.Id) && x.Id != user.Id)
            .ToArrayAsync();

        var result = await _messagesService.CreateNewChat(user, new [] {user}.Concat(others).ToArray(), request.Name);

        var connections = result.ChatUsers.Select(x => x.Id)
            .Select(u => _userConnections.GetValueOrDefault(u))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();
        
        var clientsFromChat = Clients.Clients(connections);
        
        await clientsFromChat.SendCoreAsync("chatCreateEvent", new []
        {
            result
        });
    }
    
    [HubMethodName("сreatePrivateChat")]
    public async Task CreatePrivateChat(long userId)
    {
        var user = await _userManager.FindByNameAsync(Context.User.Identity.Name);
        var other = await _userManager.FindByIdAsync(userId.ToString());

        var result = await _messagesService.CreateNewPrivateChat(user, other);

        var connections = result.ChatUsers.Select(x => x.Id)
            .Where(x => user.Id != x)
            .Select(u => _userConnections.GetValueOrDefault(u))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();
        
        var clientsFromChat = Clients.Clients(connections);
        
        result.Name = $"{other.FirstName} {other.LastName}";
        await Clients.Caller.SendCoreAsync("chatCreateEvent", new[]
        {
            result
        });
        
        result.Name = $"{user.FirstName} {user.LastName}";
        await clientsFromChat.SendCoreAsync("chatCreateEvent", new []
        {
            result
        });
    }

    [HubMethodName("addUserToChat")]
    public async Task AddUserToChat(AddUserToChatRequest request)
    {
        var inviter = await _userManager.FindByNameAsync(Context.User.Identity.Name);
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());

        var result = await _messagesService.AddUserToChat(inviter, user, request.ChatId);
        
        var userIds = await _applicationContext.Users
            .Where(u => u.ChatUsers.Any(c => c.ChatId == request.ChatId))
            .Select(x => x.Id)
            .ToArrayAsync();
        
        var connections = userIds
            .Select(u => _userConnections.GetValueOrDefault(u))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();
        
        var clientsFromChat = Clients.Clients(connections);

        await clientsFromChat.SendCoreAsync("addUserEvent", new []
        {
            result
        });
    }

    [HubMethodName("leaveChat")]
    public async Task LeaveChat(long chatId)
    {
        var me = await _userManager.FindByNameAsync(Context.User.Identity.Name);
        
        var tr = await _applicationContext.Database.BeginTransactionAsync();

        var result = await _messagesService.LeaveChat(me, chatId);
        
        await tr.CommitAsync();

        if (result == null)
        {
            return;
        }

        var userIds = await _applicationContext.Chats
            .Where(c => c.Id == chatId)
            .SelectMany(c => c.ChatUsers)
            .Select(u => u.Id)
            .ToArrayAsync();
        
        var connections = userIds
            .Select(u => _userConnections.GetValueOrDefault(u))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();
        
        var clientsFromChat = Clients.Clients(connections);

        await clientsFromChat.SendCoreAsync("leaveChatEvent", new []
        {
            result
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