namespace Waystone.Monads.Analyzers.Sample;

using System.Collections.Generic;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

[ErrorCodeCatalog(Format = "order.{member:kebab}")]
internal enum OrderErrorCode
{
    NotFound,
    AlreadyShipped,
    AddressIncomplete,
    OutOfStock,
    PaymentDeclined,
}

internal record Order(int Id, string Sku, int Quantity, string Postcode);

internal record Reservation(Order Order, string WarehouseId);

internal record Shipment(int OrderId, string TrackingNumber);

/// <summary>
/// One pass of an ordering pipeline. Every step returns a
/// <c>Result&lt;T, Error&gt;</c> and every failure carries a code the generator
/// produced from <see cref="OrderErrorCode" />, so the codes the API returns are
/// the same strings this file names.
///
/// The enum declares a format, so the codes are <c>order.not-found</c> rather than
/// <c>OrderErrorCode.NotFound</c> — the shape you would want on a wire, without a
/// runtime factory.
/// </summary>
internal class Ordering
{
    private readonly Dictionary<int, Order> _orders = new Dictionary<int, Order>
    {
        [1] = new Order(1, "WIDGET-1", 2, "3000"),
    };

    private readonly HashSet<int> _shipped = new HashSet<int> { 7 };

    private readonly Dictionary<string, int> _stock =
        new Dictionary<string, int> { ["WIDGET-1"] = 5 };

    internal Result<Shipment, Error> Place(int orderId) =>
        Find(orderId)
           .AndThen(NotYetShipped)
           .AndThen(Deliverable)
           .AndThen(Reserve)
           .AndThen(Charge)
           .AndThen(Dispatch);

    /// <summary>
    /// The pipeline's failures reach the caller as codes, so the boundary decides
    /// what to do with a code rather than with an exception type. The
    /// <c>case</c> labels are what make this method possible at all — a label
    /// needs a compile-time constant, which is what <c>Names</c> gives you and no
    /// code worked out at run time can.
    /// </summary>
    internal int StatusCodeFor(Error error)
    {
        switch (error.Code.Value)
        {
            case OrderErrorCodeCatalog.Names.NotFound:
                return 404;
            case OrderErrorCodeCatalog.Names.AlreadyShipped:
                return 409;
            case OrderErrorCodeCatalog.Names.AddressIncomplete:
                return 422;
            case OrderErrorCodeCatalog.Names.OutOfStock:
                return 409;
            case OrderErrorCodeCatalog.Names.PaymentDeclined:
                return 402;
            default:
                return 500;
        }
    }

    private Result<Order, Error> Find(int orderId) =>
        _orders.TryGetValue(orderId, out Order order)
            ? Result.Ok<Order>(order)
            : Result.Err<Order>(
                OrderErrorCodeCatalog.Errors.NotFound(
                    $"no order with id {orderId}"));

    private Result<Order, Error> NotYetShipped(Order order) =>
        _shipped.Contains(order.Id)
            ? Result.Err<Order>(
                OrderErrorCodeCatalog.Errors.AlreadyShipped(
                    $"order {order.Id} shipped already and cannot be placed again"))
            : Result.Ok<Order>(order);

    private Result<Order, Error> Deliverable(Order order) =>
        string.IsNullOrWhiteSpace(order.Postcode)
            ? Result.Err<Order>(
                OrderErrorCodeCatalog.Errors.AddressIncomplete(
                    $"order {order.Id} has no postcode"))
            : Result.Ok<Order>(order);

    /// <summary>
    /// The warehouse hands back a bare enum rather than an <c>Error</c>, which is
    /// what <c>ToError</c> is for — it attaches the message at the boundary where
    /// one is worth writing.
    /// </summary>
    private Result<Reservation, Error> Reserve(Order order)
    {
        OrderErrorCode? refusal = AskWarehouse(order);

        return refusal.HasValue
            ? Result.Err<Reservation>(
                refusal.Value.ToError(
                    $"cannot reserve {order.Quantity} of {order.Sku}"))
            : Result.Ok<Reservation>(new Reservation(order, "MEL-1"));
    }

    private OrderErrorCode? AskWarehouse(Order order) =>
        _stock.TryGetValue(order.Sku, out int available)
     && available >= order.Quantity
            ? (OrderErrorCode?)null
            : OrderErrorCode.OutOfStock;

    private Result<Reservation, Error> Charge(Reservation reservation) =>
        reservation.Order.Quantity > 3
            ? Result.Err<Reservation>(
                OrderErrorCodeCatalog.Errors.PaymentDeclined(
                    $"the issuer declined the charge for order {reservation.Order.Id}"))
            : Result.Ok<Reservation>(reservation);

    private Result<Shipment, Error> Dispatch(Reservation reservation) =>
        Result.Ok<Shipment>(
            new Shipment(
                reservation.Order.Id,
                $"{reservation.WarehouseId}-{reservation.Order.Id:D8}"));
}
