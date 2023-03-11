using Chatiks.Chat.Managers;
using Chatiks.Chat.Specifications;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Chatiks.Handlers.Users;

public class GetUsersHandler: IRequestHandler<GetUsersRequest, GetUsersResponse>
{
    private readonly TypeAdapterConfig _typeAdapterConfig;
    private readonly UserManager<User.Data.EF.Domain.User.User> _userManager;
    private readonly HttpContextAccessor _contextAccessor;
    private readonly ChatManager _chatManager;

    public GetUsersHandler(HttpContextAccessor contextAccessor, UserManager<User.Data.EF.Domain.User.User> userManager, TypeAdapterConfig typeAdapterConfig, ChatManager chatManager)
    {
        _contextAccessor = contextAccessor;
        _userManager = userManager;
        _typeAdapterConfig = typeAdapterConfig;
        _chatManager = chatManager;
    }

    public async Task<GetUsersResponse> Handle(GetUsersRequest request, CancellationToken cancellationToken)
    {
        var me =  await _userManager.FindByNameAsync(_contextAccessor.HttpContext.User.Identity.Name);
        var usersQuery = _userManager.Users.AsQueryable();

        if (request.IdFilter != null && request.IdFilter.Any())
        {
            usersQuery = usersQuery
                .Where(u => request.IdFilter.Contains(u.Id));
        }
        
        if (request.ChatIdFilter != null && request.ChatIdFilter.Any())
        {
            var spec = new ChatSpecification(new ChatFilter(request.ChatIdFilter));
            spec.IncludeChatUsers();
            var chats = await _chatManager.LoadChatsBySpecificationAsync(spec);
            var userIdsFromChats = chats.SelectMany(c => c.Users.Select(y => y.UserId)).ToArray();
            
            usersQuery = usersQuery
                .Where(u => userIdsFromChats.Contains(u.Id));
        }
        
        if (!string.IsNullOrEmpty(request.FullNameFilter))
        {
            usersQuery = usersQuery
                .Where(u => (u.FirstName + " " + u.LastName).Contains(request.FullNameFilter) ||
                            (u.LastName + " " + u.FirstName).Contains(request.FullNameFilter));
        }
        
        if (!string.IsNullOrEmpty(request.PhoneOrMailFilter))
        {
            usersQuery = usersQuery
                .Where(u => EF.Functions.ILike(u.Email, $"%{request.PhoneOrMailFilter.Trim()}%") ||
                                                                          EF.Functions.ILike(u.PhoneNumber, $"%{request.PhoneOrMailFilter.Trim()}%"));
        }

        if (request.ExcludeMe == true)
        {
            usersQuery = usersQuery.Where(u => u.UserName != _contextAccessor.HttpContext.User.Identity.Name);
        }
        
        if (request.ExcludeHasChatsWithMe == true)
        {
            var spec = new ChatSpecification(new ChatFilter()
            {
                IsPrivate = true,
                HasUserIds = new []{me.Id}
            });
            spec.IncludeChatUsers();
            var myChats = await _chatManager.LoadChatsBySpecificationAsync(spec);
            var excludeUsers = myChats
                .SelectMany(c => c.Users.Select(u => u.UserId))
                .ToArray();

            usersQuery = usersQuery
                .Where(u => !excludeUsers.Contains(u.Id));
        }

        if (request.AdditionalIds != null && request.AdditionalIds.Any())
        {
            usersQuery = usersQuery
                .Union(_userManager.Users.Where(u => request.AdditionalIds.Contains(u.Id)));
        }

        var allCount = await usersQuery.CountAsync();
        
        var users = await usersQuery
            .Skip(request.Offset)
            .Take(request.Count)
            .ProjectToType<UserResponse>(_typeAdapterConfig)
            .ToArrayAsync();

        return new GetUsersResponse
        {
            EntitiesLeft = Math.Max(allCount - request.Offset - users.Length, 0),
            
            Users = users
        };
    }
}