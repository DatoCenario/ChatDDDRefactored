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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // For migrator because ef tools v7 not support --provider option
        base.OnConfiguring(optionsBuilder.UseNpgsql());
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ConfigureDateTimeTypes();

        base.OnModelCreating(builder);
    }
}