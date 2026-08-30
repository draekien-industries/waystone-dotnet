namespace Waystone.Monads.Linq;

using System;
using Options;

/// <summary>
/// The LINQ spelling of <see cref="Option{T}" />'s projection operations, so
/// C# query syntax works over an option.
/// </summary>
/// <remarks>
/// Every member forwards to a core member and adds no behaviour:
/// <see cref="Select{T,TOut}" /> to <c>Map</c>,
/// <see cref="SelectMany{T,TOut}" /> to <c>AndThen</c>, and
/// <see cref="Where{T}" /> to <c>Filter</c>. The package exists so that a
/// consumer who wants query syntax can have it and one who does not never
/// sees a second name for an operation they already know.
/// <para>
/// The names are deliberately absent from <c>Waystone.Monads</c>. Two
/// spellings of one operation on the core type would mean every reader
/// learning both, and the awaited-receiver generator would lift each of these
/// onto <see cref="System.Threading.Tasks.Task{TResult}" /> and
/// <see cref="System.Threading.Tasks.ValueTask{TResult}" /> receivers
/// automatically — tripling the core package's public surface for names that
/// query syntax cannot await anyway.
/// </para>
/// <para>
/// There are no <c>…Async</c> shapes here for that same reason: a query
/// expression cannot await, so an async LINQ name would buy only method-syntax
/// parity with <c>MapAsync</c>, which the core package already provides under
/// the name this library actually teaches.
/// </para>
/// </remarks>
public static class OptionQueryExtensions
{
    /// <summary>Projects the contained value, under LINQ's name for the operation.</summary>
    /// <remarks>
    /// Forwards to <see cref="Option{T}.Map{TOut}(Func{T,TOut})" /> and behaves
    /// identically; the two differ only in name. It is here so that a
    /// <c>select</c> clause binds, and so a reader who knows
    /// <see cref="System.Linq.Enumerable" /> finds the operation under the name
    /// they already use. Reach for <c>Map</c> in a method-syntax chain, where it
    /// reads alongside the rest of this library's vocabulary, and expect no
    /// difference in behaviour or cost either way.
    /// </remarks>
    /// <typeparam name="T">The option's value type.</typeparam>
    /// <typeparam name="TOut">The projected value's type.</typeparam>
    /// <param name="option">The option to project.</param>
    /// <param name="selector">Projects the contained value. Called only for a
    /// <see cref="Some{T}" />, and must not return null.</param>
    /// <returns>
    /// A <see cref="Some{T}" /> of the projected value if the option was a
    /// <see cref="Some{T}" />, otherwise <see cref="None{T}" />.
    /// </returns>
    public static Option<TOut> Select<T, TOut>(
        this Option<T> option,
        Func<T, TOut> selector) where T : notnull where TOut : notnull =>
        option.Map(selector);

    /// <summary>Chains an option-producing step, under LINQ's name for bind.</summary>
    /// <remarks>
    /// Forwards to <see cref="Option{T}.AndThen{TOut}(Func{T,Option{TOut}})" />.
    /// This is the overload a method-syntax caller writes; query syntax uses the
    /// three-argument sibling instead, and a single <c>from</c> never reaches
    /// this one.
    /// </remarks>
    /// <typeparam name="T">The option's value type.</typeparam>
    /// <typeparam name="TOut">The next step's value type.</typeparam>
    /// <param name="option">The option to chain from.</param>
    /// <param name="optionFactory">Produces the next option from the contained
    /// value. Called only for a <see cref="Some{T}" />, and must not return
    /// null.</param>
    /// <returns>
    /// The option <paramref name="optionFactory" /> produced if
    /// <paramref name="option" /> was a <see cref="Some{T}" />, otherwise
    /// <see cref="None{T}" />.
    /// </returns>
    public static Option<TOut> SelectMany<T, TOut>(
        this Option<T> option,
        Func<T, Option<TOut>> optionFactory)
        where T : notnull where TOut : notnull =>
        option.AndThen(optionFactory);

    /// <summary>Joins two options and projects the pair, as a multi-clause query does.</summary>
    /// <remarks>
    /// The shape the compiler requires for a query with more than one
    /// <c>from</c> clause; nothing else needs it, and a hand-written call is
    /// almost always clearer as
    /// <see cref="Option{T}.AndThen{TOut}(Func{T,Option{TOut}})" />. Both
    /// delegates are threaded through the core type's state-passing overloads
    /// rather than captured, so a query clause allocates no closure.
    /// <para>
    /// Short-circuits at the first <see cref="None{T}" />: if
    /// <paramref name="option" /> is a <see cref="None{T}" /> neither delegate
    /// runs, and if <paramref name="optionFactory" /> yields a
    /// <see cref="None{T}" /> then <paramref name="resultSelector" /> does not
    /// run.
    /// </para>
    /// <para>
    /// The two delegates are named by different schemes on purpose.
    /// <paramref name="optionFactory" /> returns an option, so it is a chain
    /// step and takes this library's name for one;
    /// <paramref name="resultSelector" /> returns a plain value, so it keeps
    /// LINQ's.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">The option's value type.</typeparam>
    /// <typeparam name="TCollection">The joined option's value type.</typeparam>
    /// <typeparam name="TResult">The projected result's type.</typeparam>
    /// <param name="option">The option the query's first <c>from</c> clause reads.</param>
    /// <param name="optionFactory">Produces the option to join, from
    /// <paramref name="option" />'s contained value.</param>
    /// <param name="resultSelector">Combines both contained values. Must not
    /// return null.</param>
    /// <returns>
    /// A <see cref="Some{T}" /> of the combined value if both options were a
    /// <see cref="Some{T}" />, otherwise <see cref="None{T}" />.
    /// </returns>
    public static Option<TResult> SelectMany<T, TCollection, TResult>(
        this Option<T> option,
        Func<T, Option<TCollection>> optionFactory,
        Func<T, TCollection, TResult> resultSelector)
        where T : notnull
        where TCollection : notnull
        where TResult : notnull =>
        option.AndThen(
            (optionFactory, resultSelector),
            static (value, selectors) => selectors.optionFactory(value)
               .Map(
                    (value, selectors.resultSelector),
                    static (collected, state) => state.resultSelector(
                        state.value,
                        collected)));

    /// <summary>Discards the contained value unless it satisfies a predicate.</summary>
    /// <remarks>
    /// Forwards to <see cref="Option{T}.Filter(Func{T,bool})" />, and is what
    /// makes a <c>where</c> clause bind in a query over an option. There is no
    /// counterpart for <see cref="Results.Result{TOk,TErr}" />: discarding an
    /// ok value would have to invent the error that replaces it, and a signature
    /// taking an error factory is not the one query syntax looks for. So a query
    /// over a result has no <c>where</c> clause available.
    /// </remarks>
    /// <typeparam name="T">The option's value type.</typeparam>
    /// <param name="option">The option to filter.</param>
    /// <param name="predicate">Tests the contained value. Called only for a
    /// <see cref="Some{T}" />.</param>
    /// <returns>
    /// <paramref name="option" /> if it was a <see cref="Some{T}" /> whose value
    /// satisfied <paramref name="predicate" />, otherwise
    /// <see cref="None{T}" />.
    /// </returns>
    public static Option<T> Where<T>(
        this Option<T> option,
        Func<T, bool> predicate) where T : notnull =>
        option.Filter(predicate);
}
