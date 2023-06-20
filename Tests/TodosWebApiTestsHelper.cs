using System;
using System.Collections.Generic;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Todos.WebApi;

namespace Tests;

// When web app under test has a Startup class this helper can be refactored to
// CustomWebApplicationFactory and override CreateHostBuilder and ConfigureWebHost
public class TodosWebApiTestsHelper
{
    public static WebApplicationFactory<Program> GetAppFactory(Action<IBusRegistrationConfigurator>? configure = null)
    {
        var factory = new WebApplicationFactory<Todos.WebApi.Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services => services.AddMassTransitTestHarness(x =>
                {
                    configure?.Invoke(x);
                    // configure common responses if needed
                    // x.AddHandler<CreateTodoItemCmd>(context => context.RespondAsync(createTodoItemResult));
                }));

                OverrideConfigs(builder);

                ConfigureCommonTestDependencies(builder);
            });
        return factory;
    }

    // can be moved in common tests setup
    private static void ConfigureCommonTestDependencies(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(
            services =>
            {
                //// Create your mock service
                //var myMockService = new Mock<IMyService>();
                //myMockService.Setup(m => m.MyMethod()).Returns("Mock Result");

                //// Replace the real service with your mock
                //services.AddSingleton<IMyService>(myMockService.Object);
            }
        );
    }

    // can be moved in common tests setup
    private static void OverrideConfigs(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, conf) =>
        {
            var testConfig = new Dictionary<string, string>
            {
                { "Key1", "Value1" },
                { "Key2", "Value2" },
                // Add more test config values here
            };

            conf.AddInMemoryCollection(testConfig);
        });
    }
}