using System.Reflection;
using Chatiks;
using Chatiks.Extensions;
using Chatiks.GraphQL;
using Chatiks.Hubs;
using Chatiks.Tools.DI;using Chatiks.Tools.Extensions.Models;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<HttpContextAccessor>();

builder.Services.AddMediatR(Assembly.GetExecutingAssembly());

builder.Services.AddControllersWithViews();

builder.Services.AddSignalR();

builder.Services.ConfigureGraphQl<ErrorFilter>();

builder.Services.Configure<HubOptions>(options =>
{
    options.MaximumReceiveMessageSize = null;
});

builder.Services.Configure<IdentityOptions>(o =>
{
    o.User.AllowedUserNameCharacters ="abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+/";
    o.Password.RequireDigit = false;
    o.Password.RequireLowercase = false;
    o.Password.RequireUppercase = false;
    o.Password.RequireNonAlphanumeric = false;
    o.Lockout = new LockoutOptions
    {
        MaxFailedAccessAttempts = 100000
    };
    o.SignIn.RequireConfirmedEmail = false;
});

builder.Services.AddAuthorization();

var config = new TypeAdapterConfig();
MapsterConfigure.Configure(config);
builder.Services.AddSingleton(config);

var diManagerTypes =  AppDomain.CurrentDomain.GetAssemblies()
    .SelectMany(a => a.GetTypes())
    .Where(t => typeof(IDiManager).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

foreach (var diManagerType in diManagerTypes)
{
    var diManagerInstance = (IDiManager)Activator.CreateInstance(diManagerType);
    diManagerInstance?.Register(builder.Services);
}

var app = builder.Build();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
app.MapGraphQL();
app.MapHub<MessengerHub>("/hub/messengerHub");

app.Run();