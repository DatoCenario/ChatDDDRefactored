using Chatiks.Core.Data.EF;
using Chatiks.Core.Managers;
using Chatiks.Tools.DI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chatiks.Core;

public class DiManager: IDiManager
{
    public void Register(IServiceCollection services)
    {
        services.AddScoped<ImagesManager>(p =>
        {
            var configuration = p.GetService<IConfiguration>();
            var connStr = Environment.GetEnvironmentVariable("EF_CORE_CONN") ?? configuration.GetValue<string>("PgDbConnectionString");
            var contextOpt = new DbContextOptionsBuilder<CoreContext>().UseNpgsql(connStr).Options;
            var context = new CoreContext(contextOpt);
            return new ImagesManager(new CoreRepository(context));
        });
    }
}