namespace Api.Models;

/// <summary>
/// Represents a single fruit order stored by the service.
/// </summary>
/// <param name="Id">Unique order identifier.</param>
/// <param name="CustomerName">Name of the customer who placed the order.</param>
/// <param name="Items">Collection of ordered items.</param>
/// <param name="Status">Current order status.</param>
/// <param name="CreatedAt">UTC timestamp when the order was created.</param>
internal record FruitOrder(string Id, string CustomerName, List<FruitItem> Items, string Status, DateTime CreatedAt);
