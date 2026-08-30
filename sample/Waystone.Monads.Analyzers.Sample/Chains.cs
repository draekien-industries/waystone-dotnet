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
/// Every member here obeys one rule: one parameter in, one monad out. That is
/// what lets a step be named at a call site as a method group rather than wrapped
/// in a lambda, and it is why <see cref="Validated" /> — itself a chain — can be
/// handed to <c>AndThen</c> exactly as a single step is.
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

    private static bool IsPresent(string value) =>
        !string.IsNullOrWhiteSpace(value);

    private static bool IsPositive(int value) => value > 0;

    /// <summary>
    /// A guard step: it answers with the value it was handed, so it fits anywhere
    /// an <see cref="Order" /> is flowing. The predicate is the reusable unit one
    /// layer below, naming the condition so two guards asking the same question
    /// ask it the same way; the guard is the layer that attaches the reason a
    /// <c>bool</c> cannot carry. That split is why all three guards below are
    /// structurally identical and differ only in the question and the code.
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
    /// <see cref="Bill" /> takes it as a method group and <c>Bill</c> is in turn a
    /// step for <see cref="BillRounded" />. That is the whole of chain reuse, and
    /// nothing downstream knows how many steps it is consuming.
    /// </summary>
    internal static Result<Order, Error> Validated(Order order) =>
        HasSku(order).AndThen(HasQuantity).AndThen(HasPostcode);

    private static Invoice Render(Quote quote) =>
        new Invoice(quote.Order.Id, quote.Amount);

    private static Invoice Rounded(Invoice invoice) =>
        new Invoice(invoice.OrderId, decimal.Round(invoice.Total, 2));

    private Result<Quote, Error> Price(Order order) => _price(order);

    internal Result<Invoice, Error> Bill(Order order) =>
        Validated(order).AndThen(Price).Map(Render);

    internal Result<Invoice, Error> BillRounded(Order order) =>
        Bill(order).Map(Rounded);

    /// <summary>
    /// The chain needs nothing added to run per element, so gathering stays at the
    /// call site and stays the caller's choice: <c>Collect</c> stops at the first
    /// failure and the <c>Partition</c> in <see cref="BillEach" /> reports all of
    /// them. A chain that gathered its own results would have answered that for
    /// every caller, and would have stopped being a step besides.
    /// </summary>
    internal Result<IReadOnlyList<Invoice>, Error> BillAll(
        IEnumerable<Order> orders) =>
        orders.Select(Bill).Collect();

    internal (IReadOnlyList<Invoice> Oks, IReadOnlyList<Error> Errs) BillEach(
        IEnumerable<Order> orders) =>
        orders.Select(Bill).Partition();

    private static ValueTask<Result<Order, Error>> FetchAsync(int id) =>
        new ValueTask<Result<Order, Error>>(
            Result.Ok<Order>(new Order(id, "WIDGET-1", 2, "3000")));

    private static ValueTask<Result<Quote, Error>> ReserveAsync(Order order) =>
        new ValueTask<Result<Quote, Error>>(
            Result.Ok<Quote>(new Quote(order, 19.98m)));

    private static Result<Quote, Error> Confirmed(Quote quote) =>
        quote.Amount > 0
            ? Result.Ok<Quote>(quote)
            : Result.Err<Quote>(
                OrderErrorCodeCatalog.Errors.OutOfStock(
                    $"order {quote.Order.Id} priced at nothing"));

    /// <summary>
    /// An async chain that is also a step. Its signature is
    /// <c>Order → ValueTask&lt;Result&lt;Quote, Error&gt;&gt;</c>, which is what
    /// every <c>*Async</c> step parameter takes, so <see cref="BillAsync" /> hands
    /// it over as a method group — exactly as the synchronous
    /// <see cref="Validated" /> is handed to <c>AndThen</c>. Nothing downstream
    /// knows it is consuming two links rather than one.
    /// </summary>
    private static ValueTask<Result<Quote, Error>> QuotedAsync(Order order) =>
        ReserveAsync(order).AndThenAsync(Confirmed);

    /// <summary>
    /// An async step is <c>T → ValueTask&lt;Result&lt;U, Error&gt;&gt;</c>, so
    /// <c>AndThenAsync</c> takes <see cref="QuotedAsync" /> by name — and the
    /// synchronous <see cref="Validated" /> drops into the same chain untouched,
    /// because each <c>*Async</c> member accepts a synchronous delegate too.
    ///
    /// This file is where async reuse used to stop. Up to 6.x every step parameter
    /// took a <c>Task</c>-returning delegate while every member returned a
    /// <see cref="ValueTask{TResult}" />, so a chain could never be a step and the
    /// advice was to reuse async steps rather than async chains.
    /// <see cref="QuotedAsync" /> is the proof that it no longer holds.
    ///
    /// The chain still trips <c>CA2012</c>, silently at that rule's default
    /// severity and in build output for a project that raises it. It is a false
    /// positive: the rule does not count a reduced extension receiver as an
    /// argument, but each member awaits its receiver exactly once, so the single
    /// consumption a <see cref="ValueTask{TResult}" /> permits is the one it gets.
    /// Breaking the chain into locals to satisfy it would store the very thing the
    /// rule exists to keep out of a local.
    /// </summary>
    internal ValueTask<Result<Invoice, Error>> BillAsync(int id) =>
        FetchAsync(id)
           .AndThenAsync(Validated)
           .AndThenAsync(QuotedAsync)
           .MapAsync(Render);
}
