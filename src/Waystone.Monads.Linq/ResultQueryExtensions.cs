namespace Waystone.Monads.Linq;

using System;
using Results;

/// <summary>
/// The LINQ spelling of <see cref="Result{TOk,TErr}" />'s projection
/// operations, so C# query syntax works over a result.
/// </summary>
/// <remarks>
/// Every member forwards to a core member and adds no behaviour:
/// <see cref="Select{TOk,TErr,TOut}" /> to <c>Map</c> and
/// <see cref="SelectMany{TOk,TErr,TOut}" /> to <c>AndThen</c>.
/// <para>
/// There is no <c>Where</c>, which is the one place a query over a result is
/// poorer than a query over an <see cref="Options.Option{T}" />: discarding an
/// ok value would have to invent the error that replaces it, and a signature
/// taking an error factory is not the one query syntax looks for. A query over
/// a result therefore has no <c>where</c> clause; filter before entering the
/// query, or use <c>OkOr</c> to convert the option that a filter produces.
/// </para>
/// <para>
/// See <see cref="OptionQueryExtensions" /> for why the LINQ names ship in
/// this package rather than in <c>Waystone.Monads</c>, and why there are no
/// <c>…Async</c> shapes.
/// </para>
/// </remarks>
public static class ResultQueryExtensions
{
    /// <summary>Projects the ok value, under LINQ's name for the operation.</summary>
    /// <remarks>
    /// Forwards to <see cref="Result{TOk,TErr}.Map{TOut}(Func{TOk,TOut})" /> and
    /// behaves identically; the two differ only in name. It is here so that a
    /// <c>select</c> clause binds, and so a reader who knows
    /// <see cref="System.Linq.Enumerable" /> finds the operation under the name
    /// they already use. The error is carried through untouched — to project that
    /// instead there is no LINQ name, and
    /// <see cref="Result{TOk,TErr}.MapErr{TOut}(Func{TErr,TOut})" /> is the only
    /// spelling.
    /// </remarks>
    /// <typeparam name="TOk">The result's ok value type.</typeparam>
    /// <typeparam name="TErr">The result's error type.</typeparam>
    /// <typeparam name="TOut">The projected ok value's type.</typeparam>
    /// <param name="result">The result to project.</param>
    /// <param name="selector">Projects the ok value. Called only for an
    /// <see cref="Ok{TOk,TErr}" />, and must not return null.</param>
    /// <returns>
    /// An <see cref="Ok{TOk,TErr}" /> of the projected value if the result was an
    /// <see cref="Ok{TOk,TErr}" />, otherwise the original
    /// <see cref="Err{TOk,TErr}" />.
    /// </returns>
    public static Result<TOut, TErr> Select<TOk, TErr, TOut>(
        this Result<TOk, TErr> result,
        Func<TOk, TOut> selector)
        where TOk : notnull where TErr : notnull where TOut : notnull =>
        result.Map(selector);

    /// <summary>Chains a result-producing step, under LINQ's name for bind.</summary>
    /// <remarks>
    /// Forwards to
    /// <see cref="Result{TOk,TErr}.AndThen{TOut}(Func{TOk,Result{TOut,TErr}})" />.
    /// This is the overload a method-syntax caller writes; query syntax uses the
    /// three-argument sibling instead, and a single <c>from</c> never reaches this
    /// one.
    /// <para>
    /// Note that the chained step must fail with the same
    /// <typeparamref name="TErr" />. A step carrying a different error type has to
    /// be mapped onto this one first, which is the constraint that makes a query
    /// over a result less pliable than one over an
    /// <see cref="Options.Option{T}" />.
    /// </para>
    /// </remarks>
    /// <typeparam name="TOk">The result's ok value type.</typeparam>
    /// <typeparam name="TErr">The error type both steps share.</typeparam>
    /// <typeparam name="TOut">The next step's ok value type.</typeparam>
    /// <param name="result">The result to chain from.</param>
    /// <param name="resultFactory">Produces the next result from the ok value.
    /// Called only for an <see cref="Ok{TOk,TErr}" />, and must not return
    /// null.</param>
    /// <returns>
    /// The result <paramref name="resultFactory" /> produced if
    /// <paramref name="result" /> was an <see cref="Ok{TOk,TErr}" />, otherwise
    /// the original <see cref="Err{TOk,TErr}" />.
    /// </returns>
    public static Result<TOut, TErr> SelectMany<TOk, TErr, TOut>(
        this Result<TOk, TErr> result,
        Func<TOk, Result<TOut, TErr>> resultFactory)
        where TOk : notnull where TErr : notnull where TOut : notnull =>
        result.AndThen(resultFactory);

    /// <summary>Joins two results and projects the pair, as a multi-clause query does.</summary>
    /// <remarks>
    /// The shape the compiler requires for a query with more than one
    /// <c>from</c> clause; nothing else needs it, and a hand-written call is
    /// almost always clearer as
    /// <see cref="Result{TOk,TErr}.AndThen{TOut}(Func{TOk,Result{TOut,TErr}})" />.
    /// Both delegates are threaded through the core type's state-passing
    /// overloads rather than captured, so a query clause allocates no closure.
    /// <para>
    /// Short-circuits at the first <see cref="Err{TOk,TErr}" />, and the error
    /// that surfaces is the first one encountered: if <paramref name="result" />
    /// is an <see cref="Err{TOk,TErr}" /> neither delegate runs, and if
    /// <paramref name="resultFactory" /> yields an <see cref="Err{TOk,TErr}" />
    /// then <paramref name="resultSelector" /> does not run. Later errors are
    /// never collected — this is not a validation combinator.
    /// </para>
    /// <para>
    /// The two delegates are named by different schemes on purpose.
    /// <paramref name="resultFactory" /> returns a result, so it is a chain step
    /// and takes this library's name for one;
    /// <paramref name="resultSelector" /> returns a plain value, so it keeps
    /// LINQ's.
    /// </para>
    /// </remarks>
    /// <typeparam name="TOk">The result's ok value type.</typeparam>
    /// <typeparam name="TErr">The error type every clause shares.</typeparam>
    /// <typeparam name="TCollection">The joined result's ok value type.</typeparam>
    /// <typeparam name="TResult">The projected result's ok value type.</typeparam>
    /// <param name="result">The result the query's first <c>from</c> clause reads.</param>
    /// <param name="resultFactory">Produces the result to join, from
    /// <paramref name="result" />'s ok value.</param>
    /// <param name="resultSelector">Combines both ok values. Must not return
    /// null.</param>
    /// <returns>
    /// An <see cref="Ok{TOk,TErr}" /> of the combined value if both results were
    /// an <see cref="Ok{TOk,TErr}" />, otherwise the first
    /// <see cref="Err{TOk,TErr}" /> of the two.
    /// </returns>
    public static Result<TResult, TErr> SelectMany<TOk, TErr, TCollection,
        TResult>(
        this Result<TOk, TErr> result,
        Func<TOk, Result<TCollection, TErr>> resultFactory,
        Func<TOk, TCollection, TResult> resultSelector)
        where TOk : notnull
        where TErr : notnull
        where TCollection : notnull
        where TResult : notnull =>
        result.AndThen(
            (resultFactory, resultSelector),
            static (value, selectors) => selectors.resultFactory(value)
               .Map(
                    (value, selectors.resultSelector),
                    static (collected, state) => state.resultSelector(
                        state.value,
                        collected)));
}
