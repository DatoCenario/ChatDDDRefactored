using Chatiks.Chat.Managers;
using Chatiks.Chat.Specifications;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Chatiks.Handlers.Chat;

public abstract class CheckUserInChatQueryHandlerBase<TRequest, TOut>: IRequestHandler<TRequest, TOut>
    where TRequest: CheckUserInChatQueryInBase<TOut>
    where TOut: class
{
    protected readonly UserManager<User.Data.EF.Domain.User.User> UserManager;
    protected readonly HttpContextAccessor ContextAccessor;
    protected readonly ChatManager ChatManager;

    public CheckUserInChatQueryHandlerBase(
        UserManager<User.Data.EF.Domain.User.User> userManager, 
        HttpContextAccessor contextAccessor, 
        ChatManager chatManager)
    {
        UserManager = userManager;
        ContextAccessor = contextAccessor;
        ChatManager = chatManager;
    }
    

    public async Task<TOut> Handle(TRequest request, CancellationToken cancellationToken)
    {
        var user = await UserManager.FindByNameAsync(ContextAccessor.HttpContext.User.Identity.Name);
        
        if (!await ChatManager.IsUserInChat(user.Id, request.ChatId))
        {
            throw new Exception("user not belogs to chat");
        }

        return await InnerHandle(request, cancellationToken);
    }
    
    protected abstract Task<TOut> InnerHandle(TRequest request, CancellationToken cancellationToken);
}