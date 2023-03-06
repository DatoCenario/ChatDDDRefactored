using Chatiks.Chat.Managers;
using Chatiks.Handlers.Chat.GetChats;
using Chatiks.Handlers.Chat.GetMessages;
using Chatiks.Handlers.Users;
using HotChocolate.AspNetCore.Authorization;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Chatiks.Controllers;

// Add graphQL to front and remove it !!

[Route("api/{controller}")]
[Authorize]
public class ChatController: Controller
{
    private readonly IMediator _mediator;
    private readonly HttpContextAccessor _contextAccessor;
    private readonly UserManager<User.Data.EF.Domain.User.User> _userManager;
    private readonly ChatManager _chatManager;

    public ChatController(IMediator mediator, UserManager<User.Data.EF.Domain.User.User> userManager, HttpContextAccessor contextAccessor, ChatManager chatManager)
    {
        _mediator = mediator;
        _userManager = userManager;
        _contextAccessor = contextAccessor;
        _chatManager = chatManager;
    }

    [HttpPost("GetChats")]
    public Task<ICollection<GetChatsResponse>> GetChats([FromBody] GetChatsRequest request)
    {
        return _mediator.Send(request);
    }
    
    [HttpPost("GetMessages")]
    public Task<GetChatMessagesResponse> GetMessages([FromBody] GetChatMessagesRequest request)
    {
        return _mediator.Send(request);
    }
    
    [HttpPost("GetUsers")]
    public Task<GetUsersResponse> GetUsers([FromBody] GetUsersRequest request)
    {
        return _mediator.Send(request);
    }
}