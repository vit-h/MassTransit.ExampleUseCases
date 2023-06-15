using MassTransit;
using Messaging.Todos;
using Microsoft.AspNetCore.Mvc;

namespace Todos.WebApi;

public class TodosController : ControllerBase
{
    public static readonly Dictionary<Guid, DateTime?> Cache = new();
    private readonly IPublishEndpoint _publisher;
    private readonly IRequestClient<CreateTodoItemCmd> _requestClient;

    public TodosController(IPublishEndpoint publisher, IRequestClient<CreateTodoItemCmd> requestClient)
    {
        _publisher = publisher;
        _requestClient = requestClient;
    }

    [HttpPost("create-in-sync-mode")]
    public async Task<ActionResult> CreateInSyncMode(CreateTodoItemCmd cmd)
    {
        var response = await _requestClient.GetResponse<CreateTodoItemResult, CreateTodoItemError>(cmd);

        if(response.Message is CreateTodoItemError error) return Conflict(error.Message);

        return Ok(response.Message as CreateTodoItemResult);
    }

    [HttpPost("create-in-async-mode")]
    public async Task<ActionResult> CreateInAsyncMode(CreateTodoItemCmd cmd)
    {
        var processingId = Guid.NewGuid();
        Cache.Add(processingId, null);

        await _publisher.Publish(cmd);

        return Ok(new
        {
            Status = "Processing",
            ProcessingId = processingId
        });
    }

    [HttpGet("get-state/{processingId:Guid}")]
    public ActionResult GetState(Guid processingId)
    {
        if (!Cache.TryGetValue(processingId, out var completedAt)) return BadRequest(new { Status = "Invalid id" });
        if (completedAt == null) return Ok(new { Status = "Processing" });

        return Ok(new { Status = "Created", CompletedAt = completedAt });
    }
}