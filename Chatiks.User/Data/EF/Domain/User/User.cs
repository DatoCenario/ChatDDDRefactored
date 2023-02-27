using Microsoft.AspNetCore.Identity;

namespace Chatiks.User.Data.EF.Domain.User;

public class User: IdentityUser<long>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}