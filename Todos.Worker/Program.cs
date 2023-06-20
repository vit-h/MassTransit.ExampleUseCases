using System.Reflection;
using Microsoft.Extensions.Hosting;
using Todos.WebApi;

namespace Todos.Worker;

public class Program
{
    public static async Task Main(string[] args)
    {
        var currentAssembly = Assembly.GetExecutingAssembly();

        var builder = Host.CreateDefaultBuilder(args);
        builder.ConfigureServices((hostContext, services) =>
        {
            services.SetupMassTransit(hostContext.Configuration, currentAssembly);
        });

        await builder.Build().RunAsync();
    }
}