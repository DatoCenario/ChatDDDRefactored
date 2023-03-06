using System.Text.RegularExpressions;
using Chatiks.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Chatiks.Controllers;

[Route("api/{controller}")]
public class LoginController: Controller
{
    public static Regex PhoneDetectRegex = new Regex(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$");
    
    private readonly UserManager<User.Data.EF.Domain.User.User> _userManager;
    private readonly SignInManager<User.Data.EF.Domain.User.User> _signInManager;

    public LoginController(UserManager<User.Data.EF.Domain.User.User> userManager, SignInManager<User.Data.EF.Domain.User.User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet("Test")]
    public string Test()
    {
        return "Success";
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginModel request)
    {
        var user = await _userManager.FindByNameAsync(request.Login);

        if (user == null)
        {
            throw new Exception("");
        }

        var res = await _signInManager.PasswordSignInAsync(user, request.Password, true, true);

        if (!res.Succeeded)
        {
            return new JsonResult(new LoginResponse
            {
                Success = false
            });
        }
        
        return new JsonResult(new LoginResponse
        {
            Success = true
        });
    }
    
    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel request)
    {
        var isPhone = PhoneDetectRegex.Match(request.MobileOrEmail).Success;

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
            return new JsonResult(new LoginResponse
            {
                Success = false,
                FieldErrors = res.Errors.Select(x => x.Description).ToArray()
            });
        }

        await _signInManager.PasswordSignInAsync(user, request.Password, true, true);

        return new JsonResult(new LoginResponse
        {
            Success = true
        });
    }
}

public class LoginResponse
{
    public bool Success { get; set; }
    public ICollection<string> FieldErrors { get; set; }
}
