using System.Reflection;

namespace Todos.WebApi;

public class Program
{
    public static async Task Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);
        var services = builder.Services;

        services.SetupMassTransit(builder.Configuration, Assembly.GetCallingAssembly());
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        var app = builder.Build();
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            //endpoints.MapHealthChecksEndpoints();
        });

        await app.RunAsync();
    }
}