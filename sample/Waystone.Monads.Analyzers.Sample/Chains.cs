namespace Waystone.Monads.Analyzers.Sample;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;
using Waystone.Monads.Results.Extensions;

internal record Quote(Order Order, decimal Amount);

internal record Invoice(int OrderId, decimal Total);

/// <summary>
/// The reusable half of <see cref="Ordering" />. Where that file shows one
/// finished chain, this one shows how the steps are shaped so a chain is possible
/// at all, and what it takes for a chain to be reused rather than retyped.
///
/// The rule every member here obeys: **one parameter in, one monad out**. That is
/// what lets a step be named at the call site as a method group rather than
/// wrapped in a lambda, and it is why <see cref="Validated" /> — itself a chain —
/// can be handed to <c>AndThen</c> exactly as a single step is.
/// </summary>
internal class Chains
{
    private readonly Func<Order, Result<Quote, Error>> _price;

    /// <summary>
    /// The varying step is held rather than passed, which is what keeps
    /// <see cref="Bill" /> a one-parameter method and therefore still composable.
    /// Taking the pricing function as a second parameter of <c>Bill</c> would read
    /// more directly and would cost every caller the ability to chain onto it.
    /// </summary>
    internal Chains(Func<Order, Result<Quote, Error>> price) => _price = price;

    /// <summary>
    /// A predicate is a reusable unit too, one layer below a step: it names the
    /// condition so that two guards asking the same question ask it the same way.
    /// What it is not is a step — it keeps no reason for a refusal, so it fits
    /// <c>Filter</c> and nothing that has to explain itself.
    /// </summary>
    private static bool IsPresent(string value) =>
        !string.IsNullOrWhiteSpace(value);

    private static bool IsPositive(int value) => value > 0;

    /// <summary>
    /// A guard step: it answers with the value it was handed, so it fits anywhere
    /// an <see cref="Order" /> is flowing. This is where a predicate becomes a
    /// step — the guard is the layer that attaches the reason the predicate cannot
    /// carry, which is why all three below have the same shape and differ only in
    /// which question they ask and which code they fail with.
    /// </summary>
    private static Result<Order, Error> HasSku(Order order) =>
        IsPresent(order.Sku)
            ? Result.Ok<Order>(order)
            : Result.Err<Order>(
                OrderErrorCodeCatalog.Errors.NotFound(
                    $"order {order.Id} names no sku"));

    private static Result<Order, Error> HasQuantity(Order order) =>
        IsPositive(order.Quantity)
            ? Result.Ok<Order>(order)
            : Result.Err<Order>(
                OrderErrorCodeCatalog.Errors.OutOfStock(
                    $"order {order.Id} asks for nothing"));

    private static Result<Order, Error> HasPostcode(Order order) =>
        IsPresent(order.Postcode)
            ? Result.Ok<Order>(order)
            : Result.Err<Order>(
                OrderErrorCodeCatalog.Errors.AddressIncomplete(
                    $"order {order.Id} has no postcode"));

    /// <summary>
    /// Three guard steps composed into one. Its signature is
    /// <c>Order → Result&lt;Order, Error&gt;</c> — a step's signature — so
    /// everything below reuses it by name and nothing re-states the three checks.
    /// </summary>
    internal static Result<Order, Error> Validated(Order order) =>
        HasSku(order).AndThen(HasQuantity).AndThen(HasPostcode);

    private static Invoice Render(Quote quote) =>
        new Invoice(quote.Order.Id, quote.Amount);

    private static Invoice Rounded(Invoice invoice) =>
        new Invoice(invoice.OrderId, decimal.Round(invoice.Total, 2));

    private Result<Quote, Error> Price(Order order) => _price(order);

    /// <summary>
    /// A chain over a chain: <see cref="Validated" /> arrives as a method group,
    /// with no lambda and no wrapper, because its shape already matches what
    /// <c>AndThen</c> accepts.
    /// </summary>
    internal Result<Invoice, Error> Bill(Order order) =>
        Validated(order).AndThen(Price).Map(Render);

    /// <summary>
    /// And <see cref="Bill" /> is in turn a step, which is the property that makes
    /// chains compose without limit. Nothing here knows how many steps <c>Bill</c>
    /// is made of.
    /// </summary>
    internal Result<Invoice, Error> BillRounded(Order order) =>
        Bill(order).Map(Rounded);

    /// <summary>
    /// The chain needs nothing added to run per element. Gathering is the caller's
    /// choice and stays at the call site: <c>Collect</c> stops at the first
    /// failure.
    /// </summary>
    internal Result<IReadOnlyList<Invoice>, Error> BillAll(
        IEnumerable<Order> orders) =>
        orders.Select(Bill).Collect();

    /// <summary>
    /// The same chain, gathered the other way. <c>Partition</c> reports every
    /// failure instead of the first, which is why the choice cannot live inside
    /// <see cref="Bill" />.
    /// </summary>
    internal (IReadOnlyList<Invoice> Oks, IReadOnlyList<Error> Errs) BillEach(
        IEnumerable<Order> orders) =>
        orders.Select(Bill).Partition();

    private static Task<Result<Order, Error>> FetchAsync(int id) =>
        Task.FromResult(
            Result.Ok<Order>(new Order(id, "WIDGET-1", 2, "3000")));

    private static Task<Result<Quote, Error>> ReserveAsync(Order order) =>
        Task.FromResult(Result.Ok<Quote>(new Quote(order, 19.98m)));

    /// <summary>
    /// An async step is <c>T → Task&lt;Result&lt;U, Error&gt;&gt;</c>, which is
    /// what an I/O method returns anyway, so <c>AndThenAsync</c> takes
    /// <see cref="ReserveAsync" /> by name. The synchronous
    /// <see cref="Validated" /> drops into the same chain untouched, because each
    /// <c>*Async</c> member accepts a synchronous delegate too.
    ///
    /// This chain is where async reuse stops. It hands back a
    /// <see cref="ValueTask{TResult}" />, and no <c>*Async</c> member accepts a
    /// <c>ValueTask</c>-returning delegate, so <c>BillAsync</c> cannot itself
    /// become a step. Reuse the async steps, not the async chain — converting with
    /// <c>AsTask</c> would buy composability with an allocation on every call.
    ///
    /// This chain also trips <c>CA2012</c>, which is silent at its default severity
    /// and fires for a project that raises the CA rules. It is a false positive:
    /// the rule does not count a reduced extension receiver as an argument, but
    /// each member awaits its receiver exactly once, so the single consumption a
    /// <see cref="ValueTask{TResult}" /> permits is the one it gets. Suppressing it
    /// is correct here; breaking the chain into locals to satisfy it would store
    /// the very thing the rule exists to keep out of a local.
    /// </summary>
    internal ValueTask<Result<Invoice, Error>> BillAsync(int id) =>
        FetchAsync(id)
           .AndThenAsync(Validated)
           .AndThenAsync(ReserveAsync)
           .MapAsync(Render);
}
