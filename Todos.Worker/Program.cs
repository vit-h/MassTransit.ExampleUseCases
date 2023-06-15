using Microsoft.Extensions.Hosting;
using System.Reflection;
using Todos.WebApi;

var currentAssembly = Assembly.GetExecutingAssembly();

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureServices((hostContext, services) =>
{
    services.SetupMassTransit(hostContext.Configuration, currentAssembly);
});

await builder.Build().RunAsync();