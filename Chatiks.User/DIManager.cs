using Chatiks.Tools.DI;
using Chatiks.User.Data.EF;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chatiks.User;

public class DIManager: IDiManager
{
    public void Register(IServiceCollection services)
    {
        services.AddScoped<UserContext>(p =>
        {
            var configuration = p.GetService<IConfiguration>();
            var connStr = Environment.GetEnvironmentVariable("EF_CORE_CONN") ?? configuration.GetValue<string>("PgDbConnectionString");
            var contextOpt = new DbContextOptionsBuilder<UserContext>().UseNpgsql(connStr).Options;
            var context = new UserContext(contextOpt);
            return context;
        });
        services.AddIdentity<Data.EF.Domain.User.User, IdentityRole<long>>()
            .AddEntityFrameworkStores<UserContext>()
            .AddDefaultTokenProviders();
    }
}