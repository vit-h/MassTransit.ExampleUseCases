using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Messaging.Todos;
using Xunit;

namespace Tests;

public class TodosWebApiTests
{
    [Fact]
    public async Task CreateTodo_Success()
    {
        var createTodoItemResult = new CreateTodoItemResult(Guid.NewGuid(), DateTime.UtcNow);
        var factory = TodosWebApiTestsHelper.GetAppFactory(x =>
            x.AddHandler<CreateTodoItemCmd>(context => context.RespondAsync(createTodoItemResult)));
        var client = factory.CreateClient();
        //var harness = factory.Services.GetTestHarness();
        //var scope = factory.Services.CreateScope();
        //var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // seed some data to the DB

        // arrange
        var cmd = new CreateTodoItemCmd("Todo item text", false);

        // act
        var response = await client.PostAsync("todos/create-in-sync-mode", JsonContent.Create(cmd));

        // assert
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CreateTodoItemResult>();
        result.Should().BeEquivalentTo(createTodoItemResult);

        // validate data in the DB in the correct state
    }

    [Fact]
    public async Task CreateTodo_Error()
    {
        var createTodoItemResult = new CreateTodoItemError("It is an error");
        var factory = TodosWebApiTestsHelper.GetAppFactory(x =>
            x.AddHandler<CreateTodoItemCmd>(context => context.RespondAsync(createTodoItemResult)));
        var client = factory.CreateClient();
        //var harness = factory.Services.GetTestHarness();
        //var scope = factory.Services.CreateScope();
        //var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // seed some data to the DB

        // arrange
        var cmd = new CreateTodoItemCmd("Todo item text", false);

        // act
        var response = await client.PostAsync("todos/create-in-sync-mode", JsonContent.Create(cmd));

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var result = await response.Content.ReadFromJsonAsync<CreateTodoItemError>();
        result.Should().BeEquivalentTo(createTodoItemResult);

        // validate data in the DB in the correct state
    }

    [Fact]
    public async Task CreateTodoAsync_Processing()
    {
        var factory = TodosWebApiTestsHelper.GetAppFactory();
        var client = factory.CreateClient();
        var harness = factory.Services.GetTestHarness();
        //var scope = factory.Services.CreateScope();
        //var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // seed some data to the DB

        // arrange
        var cmd = new CreateTodoItemCmd("Todo item text", false);

        // act
        var response = await client.PostAsync("todos/create-in-async-mode", JsonContent.Create(cmd));

        // assert
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CreateTodoItemAsyncResult>();
        result!.Status.Should().Be("Processing");
        result.ProcessingId.Should().NotBeEmpty();

        harness.Published.Select<CreateTodoItemCmd>().Should().HaveCount(1);

        // validate data in the DB in the correct state
    }

    public record CreateTodoItemAsyncResult(string Status, Guid ProcessingId);
}

