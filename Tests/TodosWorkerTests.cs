using System;
using System.Threading.Tasks;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Messaging.Todos;
using Microsoft.Extensions.DependencyInjection;
using Todos.Worker;
using Xunit;

namespace Tests;

public class TodosWorkerTests : IAsyncLifetime
{
    private ServiceProvider _provider = null!;
    private ITestHarness _harness = null!;

    public async Task InitializeAsync()
    {
        _provider = new ServiceCollection()
            //.AddYourBusinessServices() // register all of your normal business services
            .AddMassTransitTestHarness(x => { x.AddConsumer<CreateTodoItemConsumer>(); })
            .BuildServiceProvider(true);
        _harness = _provider.GetRequiredService<ITestHarness>();
        await _harness.Start();
    }

    [Fact]
    public async Task CreateTodo_Success()
    {
        // arrange
        var requestClient = _harness.Bus.CreateRequestClient<CreateTodoItemCmd>();
        var cmd = new CreateTodoItemCmd("Todo item text", false);

        // act
        var response = await requestClient.GetResponse<CreateTodoItemResult>(cmd);

        // assert
        var result = response.Message;
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        // validate data in the DB is in the correct state

        _harness.GetConsumerHarness<CreateTodoItemConsumer>()
            .Consumed.Select<CreateTodoItemCmd>().Should().HaveCount(1);

        _harness.Consumed.Select<CreateTodoItemCmd>().Should().HaveCount(1);
        _harness.Sent.Select<CreateTodoItemResult>().Should().HaveCount(1);
        _harness.Sent.Select<CreateTodoItemError>().Should().HaveCount(0);
        _harness.Published.Select<TodoItemCreatedNotification>().Should().HaveCount(1);

        response.Headers.Get<int>("myCustomHeader").Should().Be(123);
    }

    [Fact]
    public async Task CreateTodo_Error()
    {
        // arrange
        var requestClient = _harness.Bus.CreateRequestClient<CreateTodoItemCmd>();
        var cmd = new CreateTodoItemCmd("Exist", false);

        // act
        var response = await requestClient.GetResponse<CreateTodoItemError>(cmd);

        // assert
        var result = response.Message;
        result.Should().NotBeNull();
        result.Message.Should().Be("Item already exist");

        // validate data in the DB is in the correct state

        _harness.Consumed.Select<CreateTodoItemCmd>().Should().HaveCount(1);
        _harness.Sent.Select<CreateTodoItemResult>().Should().HaveCount(0);
        _harness.Sent.Select<CreateTodoItemError>().Should().HaveCount(1);
        _harness.Published.Select<TodoItemCreatedNotification>().Should().HaveCount(0);

        response.Headers.Get<int>("myCustomHeader").Should().Be(123);
    }

    public async Task DisposeAsync()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }
}