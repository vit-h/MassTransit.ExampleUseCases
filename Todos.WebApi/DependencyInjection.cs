using MassTransit;
using System.Reflection;

namespace Todos.WebApi;

public static class DependencyInjection
{
    /// <summary>
    /// Setup MassTransit services
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Must have "MassTransit:RabbitMq" section</param>
    /// <param name="assemblies">Assemblies to get consumers from</param>
    public static IServiceCollection SetupMassTransit(this IServiceCollection services, IConfiguration configuration, params Assembly[] assemblies) =>
        services.AddMassTransit(o =>
        {
            o.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter(true));
            o.AddConsumers(assemblies);
            o.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMqConfig = configuration.GetSection("MassTransit:RabbitMq");
                cfg.Host(new Uri($"rabbitmq://{rabbitMqConfig["Host"]}"), rabbit =>
                {
                    rabbit.PublisherConfirmation = true;
                    rabbit.Username(rabbitMqConfig["Username"]);
                    rabbit.Password(rabbitMqConfig["Password"]);
                });

                cfg.ConfigureEndpoints(context);
            });
        });
}