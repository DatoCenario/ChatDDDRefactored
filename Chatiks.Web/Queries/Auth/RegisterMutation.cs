using Chatiks.Handlers.Auth;
using HotChocolate.AspNetCore.Authorization;
using Mapster;
using MediatR;

namespace Chatiks.Queries.Auth;

[ExtendObjectType("Mutation")]
public class RegisterMutations
{
    [Authorize]
    public class RegisterMutationsData
    {
        public async Task<RegisterResponse> Register(
            [Service] IMediator mediator,
            RegisterQueryIn request) => await mediator.Send(request.Adapt<RegisterRequest>());
    }

    public RegisterMutationsData Register => new RegisterMutationsData();
}

