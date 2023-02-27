using Chatiks.Core.Data.EF.Domain;
using Chatiks.Tools.EF;
using Microsoft.EntityFrameworkCore;

namespace Chatiks.Core.Data.EF;

public class CoreContext : DbContext
{
    public CoreContext(DbContextOptions<CoreContext> options): base(options)
    {
        
    }
    
    public DbSet<Image> Images { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ConfigureDateTimeTypes();

        builder.Entity<Image>()
            .HasIndex(e => e.Id);

        base.OnModelCreating(builder);
    }
}