using Microsoft.AspNetCore.Identity;

namespace Chatiks.User.Extensions;

public static class UserManagerExtensions
{
    public static async Task ThrowIfNoExist(this UserManager<Data.EF.Domain.User.User> userManager, long id)
    {
        (await userManager.FindByIdAsync(id.ToString())).ThrowIfNull();
    }
    
    public static void ThrowIfNull(this Data.EF.Domain.User.User user)
    {
        if (user == null)
        {
            throw new Exception();
        }
    }
}