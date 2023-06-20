using MassTransit;
using Messaging.Todos;

namespace Todos.Worker;

public class CreateTodoItemConsumer : IConsumer<CreateTodoItemCmd>
{
    private readonly IPublishEndpoint _publisher;

    public CreateTodoItemConsumer(IPublishEndpoint publisher) => _publisher = publisher;

    private readonly Action<SendContext> _setCustomHeader = context => context.Headers.Set("myCustomHeader", 123);

    public async Task Consume(ConsumeContext<CreateTodoItemCmd> context)
    {
        var cmd = context.Message;
        Console.WriteLine($"Received: {cmd.Text}");

        if (cmd.Text == "Exist") //await _db.TodoItems.Any(x => x.Text == cmd.Text);
        {
            // Respond back with the Error
            await context.RespondAsync(new CreateTodoItemError("Item already exist") as object, _setCustomHeader);
            return;
        }

        // Save item to database
        // var newItem = new TodoItem() { Text = cmd.Text, IsCompleted = cmd.IsCompleted };
        // _db.Add(newItem);
        // await _db.SaveChangesAsync(ct);
        var newTodoItemId = Guid.NewGuid();

        // Notify other services
        await _publisher.Publish(new TodoItemCreatedNotification(newTodoItemId));

        // Respond back with the Result
        await context.RespondAsync(new CreateTodoItemResult(newTodoItemId, DateTime.UtcNow) as object, _setCustomHeader);
    }
}