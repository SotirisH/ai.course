using Microsoft.Extensions.Hosting;
using Wolverine;
using Wolverine.FluentValidation;

namespace Ai.Api.Application;

public static class DependencyInjection
{
    public static IHostBuilder AddApplication(this IHostBuilder host)
    {
        host.UseWolverine(opts =>
        {
            opts.UseFluentValidation();
            opts.Discovery.IncludeAssembly(typeof(DependencyInjection).Assembly);
        });

        return host;
    }
}
