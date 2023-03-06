using System.Data.Entity;
using Chatiks.User.Specifications;
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
    
    public static async Task<ICollection<Data.EF.Domain.User.User>> LoadUsersBySpecification(
        this UserManager<Data.EF.Domain.User.User> userManager,
        UserSpecification specification)
    {
        return await specification.Apply(userManager.Users)
            .AsNoTracking()
            .ToListAsync();
    }
    
    public static async Task<Data.EF.Domain.User.User> LoadUserBySpecification(
        this UserManager<Data.EF.Domain.User.User> userManager,
        UserSpecification specification)
    {
        return await specification.Apply(userManager.Users)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }
}