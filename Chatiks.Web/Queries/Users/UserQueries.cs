using Chatiks.Handlers.Users;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Chatiks.Queries.Users;

[ExtendObjectType("Query")]
public class UserQueries
{
    [Authorize]
    public class UserQueriesData
    {
        public async Task<GetUsersResponse> GetUsers(
            [Service] IMediator mediator,
            GetUsersQueryIn request) => await mediator.Send(request.Adapt<GetUsersRequest>());
    }

    public UserQueriesData Users => new UserQueriesData();
}

