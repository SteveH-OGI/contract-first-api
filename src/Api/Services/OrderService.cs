namespace Api.Services;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using Api.Models;

/// <summary>
/// Provides an in-memory store and operations for managing fruit orders.
/// This service is intended for development and testing and is registered as a singleton.
/// </summary>
internal class OrderService
{
    private readonly ConcurrentDictionary<string, FruitOrder> orders = [];
    private int idCounter = 0; // will be incremented to 1 on first use

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderService"/> class and seeds a sample order.
    /// </summary>
    public OrderService()
    {
        // Seed example order to match openApi.yml examples
        var seed = new FruitOrder(
            Id: "ord-001",
            CustomerName: "Alice",
            Items: [new FruitItem("Apple", 3), new FruitItem("Banana", 6)],
            Status: "Pending",
            CreatedAt: DateTime.Parse("2025-01-10T09:30:00Z")
        );
        orders[seed.Id] = seed;
    }

    /// <summary>
    /// Gets a snapshot collection of all stored orders.
    /// </summary>
    public IReadOnlyCollection<FruitOrder> Orders => orders.Values.ToList();

    /// <summary>
    /// Gets the total number of orders stored.
    /// </summary>
    public int Total => orders.Count;

    /// <summary>
    /// Returns a paged, ordered sequence of orders.
    /// </summary>
    /// <param name="offset">Number of items to skip from the start of the ordered list.</param>
    /// <param name="limit">Maximum number of items to return.</param>
    /// <returns>An enumerable of <see cref="FruitOrder"/> representing the requested page.</returns>
    public IEnumerable<FruitOrder> GetPaged(int offset, int limit)
    {
        return orders.Values
            .OrderBy(o => o.CreatedAt)
            .Skip(offset)
            .Take(limit);
    }

    /// <summary>
    /// Adds a new order to the store.
    /// </summary>
    /// <param name="newOrder">The order data to create.</param>
    /// <returns>The created <see cref="FruitOrder"/> including its generated identifier and timestamps.</returns>
    public FruitOrder Add(NewFruitOrder newOrder)
    {
        var id = GetNextId();
        var order = new FruitOrder(
            Id: id,
            CustomerName: newOrder.CustomerName,
            Items: [..newOrder.Items.Select(i => new FruitItem(i.Fruit, i.Quantity))],
            Status: "Pending",
            CreatedAt: DateTime.UtcNow
        );

        orders[order.Id] = order;
        return order;
    }

    /// <summary>
    /// Tries to retrieve an order by id.
    /// </summary>
    /// <param name="orderId">The order identifier to look up.</param>
    /// <param name="order">When this method returns, contains the found order if successful; otherwise null.</param>
    /// <returns><c>true</c> if the order was found; otherwise <c>false</c>.</returns>
    public bool TryGet(string orderId, out FruitOrder? order)
    {
        return orders.TryGetValue(orderId, out order);
    }

    private string GetNextId()
    {
        var next = Interlocked.Increment(ref idCounter);
        return $"ord-{next:D3}";
    }
}
