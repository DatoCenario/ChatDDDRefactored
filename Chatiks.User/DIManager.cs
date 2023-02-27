using Chatiks.Tools.DI;
using Chatiks.User.Data.EF;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Chatiks.User;

public class DIManager: IDiManager
{
    public void Register(IServiceCollection services)
    {
        services.AddIdentity<Data.EF.Domain.User.User, IdentityRole<long>>()
            .AddEntityFrameworkStores<UserContext>()
            .AddDefaultTokenProviders();
    }
}