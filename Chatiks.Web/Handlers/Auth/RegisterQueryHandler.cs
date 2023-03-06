using Chatiks.Controllers;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Chatiks.Handlers.Auth;

public class RegisterQueryHandler: IRequestHandler<RegisterRequest, RegisterResponse>
{
    private readonly SignInManager<User.Data.EF.Domain.User.User> _signInManager;
    private readonly UserManager<User.Data.EF.Domain.User.User> _userManager;

    public RegisterQueryHandler(SignInManager<User.Data.EF.Domain.User.User> signInManager, UserManager<User.Data.EF.Domain.User.User> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public async Task<RegisterResponse> Handle(RegisterRequest request, CancellationToken cancellationToken)
    {
        var isPhone = LoginController.PhoneDetectRegex.Match(request.MobileOrEmail).Success;

        var user = new User.Data.EF.Domain.User.User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = isPhone ? request.MobileOrEmail : null,
            Email = isPhone ? null : request.MobileOrEmail,
            UserName = request.Login
        };
        var res = await _userManager.CreateAsync(user, request.Password);

        if (!res.Succeeded)
        {
            return new RegisterResponse()
            {
                Errors = res.Errors.Select(e => e.Description).ToList()
            };
        }

        return new RegisterResponse();
    }
}