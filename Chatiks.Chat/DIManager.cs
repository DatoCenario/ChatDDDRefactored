using Chatiks.Chat.Data.EF;
using Chatiks.Chat.Managers;
using Chatiks.Core.Managers;
using Chatiks.Tools.DI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chatiks.Chat;

public class DIManager: IDiManager
{
    public void Register(IServiceCollection services)
    {
        services.AddScoped<ChatManager>(p =>
        {
            var configuration = p.GetService<IConfiguration>();
            var connStr = Environment.GetEnvironmentVariable("EF_CORE_CONN") ?? configuration.GetValue<string>("PgDbConnectionString");
            var contextOpt = new DbContextOptionsBuilder<ChatContext>().UseNpgsql(connStr).Options;
            var context = new ChatContext(contextOpt);
            var imgManager = p.GetService<ImagesManager>();
            return new ChatManager(new ChatsRepository(context), imgManager);
        });
    }
}