using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebHookDemo.Model;
using WebHookDemo.Repositories;
using WebHookDemo.Services;

namespace WebHookDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateSlimBuilder(args);

            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
            });

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddSingleton<InMemoryOrderRepository>();
            builder.Services.AddSingleton<InMemoryWebHookRepository>();
            builder.Services.AddHttpClient<WebHookDispatcher>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/openapi/v1.json", "OpenAPI V1");
                });

            }

            Todo[] sampleTodos =
            [
                new(1, "Walk the dog"),
                new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
                new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
                new(4, "Clean the bathroom"),
                new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
            ];

            var todosApi = app.MapGroup("/todos");
            todosApi.MapGet("/", () => sampleTodos)
                    .WithName("GetTodos");

            todosApi.MapGet("/{id}", Results<Ok<Todo>, NotFound> (int id) =>
                sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
                    ? TypedResults.Ok(todo)
                    : TypedResults.NotFound())
                .WithName("GetTodoById");


            app.MapPost("/orders", async (CreateOrderRequest request, InMemoryOrderRepository repo,WebHookDispatcher dispatcher) =>
            {
                var order = new Order(Guid.NewGuid(), request.CustomerName, request.Amount, DateTime.UtcNow);
                repo.AddOrder(order);

                await dispatcher.DispatchAsync("order.created", order);
                return Results.Ok(order);

            }).WithDescription("Order Create")
            .WithDisplayName("Order Create")
            .WithTags("Orders");


            app.MapGet("/orders", (InMemoryOrderRepository repo) =>
            {
                return Results.Ok(repo.GetOrders());
            }).WithTags("Orders");

            app.MapPost("webhooks/subscriptions", (CreateWebhookRequest request,
                                                   InMemoryWebHookRepository repo) =>
            {
                WebHookSubscription subscription = new WebHookSubscription(
                        Guid.NewGuid(),
                        request.EventType,
                        request.WebHookUrl,
                        DateTime.UtcNow

                );



                repo.AddWebHook(subscription);
                return Results.Ok(subscription);

            }).WithDescription("webhook subscription");

            app.Run();
        }
    }

    public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

    public sealed record WebHookEvent(Guid Id, string EventType, Guid SubscriptionId, DateTime TimeStamp, JsonElement Data);

    [JsonSerializable(typeof(Todo[]))]
    [JsonSerializable(typeof(CreateOrderRequest))]
    [JsonSerializable(typeof(CreateWebhookRequest))]
    [JsonSerializable(typeof(Order))]
    [JsonSerializable(typeof(IReadOnlyList<Order>))]
    [JsonSerializable(typeof(WebHookSubscription))]
    [JsonSerializable(typeof(WebHookEvent))]
    internal partial class AppJsonSerializerContext : JsonSerializerContext
    {

    }
}
