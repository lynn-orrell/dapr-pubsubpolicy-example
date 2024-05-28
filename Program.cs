using System.Text.Json.Serialization;
using Dapr;
using Dapr.Client;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Dapr will send serialized event object vs. being raw CloudEvent
app.UseCloudEvents();

// needed for Dapr pub/sub routing
app.MapSubscribeHandler();

if (app.Environment.IsDevelopment()) 
{
    app.UseDeveloperExceptionPage();
}

// Dapr subscription in [Topic] routes orders topic to this route
app.MapPost("/orders", [Topic("orderpubsub", "orders")] (Order order) => 
{
    Console.WriteLine("Subscriber received : " + order);
    return Results.Problem("Unable to process order.", statusCode: 500);
});

app.MapPost("/send", async () => 
{
    using var client = new DaprClientBuilder().Build();
    await client.PublishEventAsync("orderpubsub", "orders", new Order(1));
    return Results.Ok();
});

await app.RunAsync();

public record Order([property: JsonPropertyName("orderId")] int OrderId);