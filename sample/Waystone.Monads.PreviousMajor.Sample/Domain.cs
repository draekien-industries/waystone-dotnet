namespace Waystone.Monads.PreviousMajor.Sample;

internal record Order(int Id, string Sku, int Quantity, string Postcode);

internal record Quote(Order Order, decimal Amount);

internal record Invoice(int OrderId, decimal Total);

internal enum OrderError
{
    NotFound,
    AlreadyShipped,
    OutOfStock,
}
