using System.Linq;
using Api;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// register OrderService as singleton
builder.Services.AddSingleton<OrderService>();

var app = builder.Build();

// Middleware to ensure malformed JSON / BadHttpRequestException is returned as JSON Error
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (BadHttpRequestException)
    {
        context.Response.Clear();
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";
        var err = new Error("invalid_request", "Request body is invalid or missing required fields");
        await context.Response.WriteAsJsonAsync(err);
    }
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c => c.SetCustomDocumentSerializer<CustomSwaggerDocumentSerializer>());
    app.UseSwaggerUI();
}

// Health and readiness probes, not part of OpenAPI spec
app.MapGet("/health/liveness", () => Results.Ok("live"));
app.MapGet("/health/readiness", () => Results.Ok("Ready"));

app.MapGet("/orders", (OrderService svc, int? limit, int? offset) =>
{
    // Validate query parameters per contract - return 400 when values violate schema
    if (limit.HasValue && (limit.Value < 1 || limit.Value > 100))
    {
        return Results.BadRequest(new Error("invalid_request", "Query parameter validation failed"));
    }

    if (offset.HasValue && offset.Value < 0)
    {
        return Results.BadRequest(new Error("invalid_request", "Query parameter validation failed"));
    }

    var actualOffset = Math.Max(0, offset ?? 0);
    var actualLimit = Math.Clamp(limit ?? 50, 1, 100);

    var items = svc.GetPaged(actualOffset, actualLimit).ToList();

    var result = new
    {
        total = svc.Total,
        items
    };

    return Results.Ok(result);
});

app.MapPost("/orders", (OrderService svc, NewFruitOrder? newOrder) =>
{
    if (newOrder is null)
    {
        return Results.BadRequest(new Error("invalid_request", "Request body is invalid or missing required fields"));
    }

    if (string.IsNullOrWhiteSpace(newOrder.CustomerName) || newOrder.Items == null || newOrder.Items.Count == 0)
    {
        return Results.BadRequest(new Error("invalid_request", "customerName and items are required"));
    }

    // Validate items
    foreach (var it in newOrder.Items)
    {
        if (string.IsNullOrWhiteSpace(it.Fruit) || it.Quantity < 1)
        {
            return Results.BadRequest(new Error("invalid_request", "Each item must have a fruit name and quantity >= 1"));
        }
    }

    var order = svc.Add(newOrder);

    var location = $"/orders/{order.Id}";

    return Results.Created(location, order);
});

app.MapGet("/orders/{orderId}", (OrderService svc, string orderId) =>
{
    if (string.IsNullOrWhiteSpace(orderId) || !svc.TryGet(orderId, out var order))
    {
        return Results.NotFound(new Error("not_found", "Order with the specified ID was not found"));
    }

    return Results.Ok(order);
});

app.Run();
