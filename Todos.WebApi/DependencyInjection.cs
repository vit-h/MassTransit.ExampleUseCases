using MassTransit;
using RabbitMQ.Client;
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


                // If need to receive messages to multiple instances:
                // Create a unique queue name for each instance and map to the same consumer
                // For example to get duplicated messages to all replicated instances:
                //var uniquePrefix = Environment.MachineName.ToLower();
                //var queueName = $"some-name-for-lookup-{uniquePrefix}";
                //cfg.ReceiveEndpoint(queueName, e => e.Consumer<UpdatesConsumer>(context));

                // If need to Delete the queue when the application closes
                //AppDomain.CurrentDomain.ProcessExit += (s, e) =>
                //{
                //    var connectionFactory = new ConnectionFactory()
                //    {
                //        HostName = rabbitMqConfig["Host"],
                //        UserName = rabbitMqConfig["Username"],
                //        Password = rabbitMqConfig["Password"]
                //    };
                //    using var connection = connectionFactory.CreateConnection();
                //    using var channel = connection.CreateModel();
                //    channel.QueueDelete(queueName);
                //    channel.ExchangeDelete(queueName);
                //};
            });
        });
}