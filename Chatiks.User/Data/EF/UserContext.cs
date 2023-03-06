using Chatiks.Tools.EF;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Chatiks.User.Data.EF;

public class UserContext : IdentityDbContext<Domain.User.User, IdentityRole<long>, long>
{
    public UserContext()
    {
        
    }
    
    public UserContext(DbContextOptions<UserContext> options): base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ConfigureDateTimeTypes();

        base.OnModelCreating(builder);
    }
}