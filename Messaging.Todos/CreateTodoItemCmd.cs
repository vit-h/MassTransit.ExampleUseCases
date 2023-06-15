namespace Messaging.Todos;

public record CreateTodoItemCmd(string Text, bool IsCompleted);