using System.Reflection;
using Todos.WebApi;

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

app.Run();
