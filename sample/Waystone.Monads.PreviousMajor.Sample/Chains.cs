namespace Waystone.Monads.PreviousMajor.Sample;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Waystone.Monads.Options;
using Waystone.Monads.Options.Extensions;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;
using Waystone.Monads.Results.Extensions;

/// <summary>
/// The fluent surface as the documentation teaches it: synchronous steps composed
/// with <c>AndThen</c>, an async chain built from <c>*Async</c> members, and the
/// collection gatherers at the call site.
///
/// The async members here are what <c>DRA-115</c> changes. <see cref="BillAsync" />
/// hands its chain a <c>Task</c>-returning step by name, which is the binding that
/// the delegate return-type change is expected to break.
/// </summary>
internal class Chains
{
    private static readonly Error Refused =
        new Error("order.refused", "the order was refused");

    private static Result<Order, Error> HasSku(Order order) =>
        string.IsNullOrWhiteSpace(order.Sku)
            ? Result.Err<Order>(Refused)
            : Result.Ok<Order>(order);

    private static Result<Order, Error> HasQuantity(Order order) =>
        order.Quantity > 0
            ? Result.Ok<Order>(order)
            : Result.Err<Order>(Refused);

    internal static Result<Order, Error> Validated(Order order) =>
        HasSku(order).AndThen(HasQuantity);

    private static Quote Price(Order order) =>
        new Quote(order, order.Quantity * 9.99m);

    private static Invoice Render(Quote quote) =>
        new Invoice(quote.Order.Id, quote.Amount);

    internal static Result<Invoice, Error> Bill(Order order) =>
        Validated(order).Map(Price).Map(Render);

    internal static Result<IReadOnlyList<Invoice>, Error> BillAll(
        IEnumerable<Order> orders) =>
        orders.Select(Bill).Collect();

    internal static (IReadOnlyList<Invoice> Oks, IReadOnlyList<Error> Errs)
        BillEach(IEnumerable<Order> orders) =>
        orders.Select(Bill).Partition();

    /// <summary>
    /// <c>Try</c> and <c>Match</c>, which the documentation teaches as the two ways
    /// out of a monad.
    /// </summary>
    internal static string Describe(int id)
    {
        Option<Order> found = Option.Try(() => Lookup(id));

        return found.Match(
            order => $"order {order.Id}",
            () => "no order");
    }

    private static Order Lookup(int id) =>
        new Order(id, "WIDGET-1", 2, "3000");

    private static Task<Result<Order, Error>> FetchAsync(int id) =>
        Task.FromResult(Result.Ok<Order>(Lookup(id)));

    private static Task<Result<Quote, Error>> ReserveAsync(Order order) =>
        Task.FromResult(Result.Ok<Quote>(Price(order)));

    private static Task<Invoice> RenderAsync(Quote quote) =>
        Task.FromResult(Render(quote));

    /// <summary>
    /// An async chain built from <c>Task</c>-returning steps named as method
    /// groups. This is the shape <c>DRA-115</c> converts, so this member is the one
    /// whose diagnostics answer whether the conversion breaks a caller who reused
    /// steps rather than chains.
    /// </summary>
    internal static ValueTask<Result<Invoice, Error>> BillAsync(int id) =>
        FetchAsync(id)
           .AndThenAsync(Validated)
           .AndThenAsync(ReserveAsync)
           .MapAsync(RenderAsync);

    /// <summary>
    /// The same chain written with lambdas rather than method groups. A lambda is
    /// converted against the parameter type rather than matched to it, so this
    /// member should keep compiling across the conversion and the contrast with
    /// <see cref="BillAsync" /> is the point.
    /// </summary>
    internal static ValueTask<Result<Invoice, Error>> BillAsyncLambda(int id) =>
        FetchAsync(id)
           .AndThenAsync(async order => await ReserveAsync(order))
           .MapAsync(async quote => await RenderAsync(quote));

    /// <summary>
    /// The async Option surface, so a break is not measured on
    /// <see cref="Result{TOk,TErr}" /> alone.
    /// </summary>
    internal static ValueTask<Option<Invoice>> QuoteAsync(int id) =>
        FindAsync(id).AndThenAsync(PriceAsync).MapAsync(RenderAsync);

    private static Task<Option<Order>> FindAsync(int id) =>
        Task.FromResult(Option.Some(Lookup(id)));

    private static Task<Option<Quote>> PriceAsync(Order order) =>
        Task.FromResult(Option.Some(Price(order)));
}
