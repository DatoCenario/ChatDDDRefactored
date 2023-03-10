using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chatiks.Tools.DI;

public interface IDiManager
{
    void Register(WebApplicationBuilder builder);
}