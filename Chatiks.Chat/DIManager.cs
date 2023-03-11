using System.Reflection;
using Chatiks.Chat.Data.EF;
using Chatiks.Chat.Domain;
using Chatiks.Chat.Managers;
using Chatiks.Core.Managers;
using Chatiks.Tools.DI;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chatiks.Chat;

public class DIManager: IDiManager
{
    private static bool _migated = false;
    public void Register(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<ChatContext>(p =>
        {
            var configuration = p.GetService<IConfiguration>();
            var connStr = Environment.GetEnvironmentVariable("EF_CORE_CONN") ?? configuration.GetValue<string>("PgDbConnectionString");
            var contextOpt = new DbContextOptionsBuilder<ChatContext>().UseNpgsql(connStr).Options;
            var context = new ChatContext(contextOpt);

            // Refactor!!!
            if (!_migated)
            {
                context.Database.Migrate();
                _migated = true;
            }

            return context;
        });

        builder.Services.AddScoped<ChatDomainModelFactory>();
        builder.Services.AddScoped<ChatManager>();
    }
}