namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;
using Results;
using SourceGenerators;
using static Option;
#if !DEBUG
using System.Diagnostics;
#endif

/// <summary>
/// Everything callable on an <see cref="Option{T}" /> that is not declared on
/// the type itself.
/// </summary>
/// <remarks>
/// Two kinds of member live here. Most are generated: the awaited-receiver
/// overloads that let a call chain stay in one expression when the option is
/// still inside a <see cref="Task{TResult}" /> or a
/// <see cref="ValueTask{TResult}" />, listed in the attributes below and
/// forwarding into the member of the same name on <see cref="Option{T}" />.
/// The rest are hand-written, and each is here because its receiver is a
/// *particular* option rather than any option — <c>Unzip</c> reads one holding
/// a tuple, <c>Flatten</c> a nested option, <c>Transpose</c> a
/// <see cref="Result{TOk,TErr}" />, <c>UnwrapOrNull</c> a value type — or
/// because the shape awaits an argument as well as the receiver, which the
/// generator has no way to reach.
/// <para>
/// Operations over a *sequence* of options are the one thing that is not here.
/// They take an <see cref="System.Collections.Generic.IEnumerable{T}" />
/// receiver rather than an <see cref="Option{T}" />, so they share nothing with
/// the members below and live in <see cref="OptionsCollectionExtensions" />.
/// </para>
/// </remarks>
#if !DEBUG
[DebuggerStepThrough]
#endif
[GenerateAwaitedReceivers(typeof(Option<>))]
[GenerateAwaitedMember(nameof(Option<>.And))]
[GenerateAwaitedMember(nameof(Option<>.AndThen))]
[GenerateAwaitedMember(nameof(Option<>.AndThenAsync))]
[GenerateAwaitedMember(nameof(Option<>.AsEnumerable))]
[GenerateAwaitedMember(nameof(Option<>.Expect))]
[GenerateAwaitedMember(nameof(Option<>.Filter))]
[GenerateAwaitedMember(nameof(Option<>.FilterAsync))]
[GenerateAwaitedMember(nameof(Option<>.Inspect))]
[GenerateAwaitedMember(nameof(Option<>.InspectAsync))]
[GenerateAwaitedMember(nameof(Option<>.IsNoneOr))]
[GenerateAwaitedMember(nameof(Option<>.IsNoneOrAsync))]
[GenerateAwaitedMember(nameof(Option<>.IsSomeAnd))]
[GenerateAwaitedMember(nameof(Option<>.IsSomeAndAsync))]
[GenerateAwaitedMember(nameof(Option<>.Map))]
[GenerateAwaitedMember(nameof(Option<>.MapAsync))]
[GenerateAwaitedMember(nameof(Option<>.MapOr))]
[GenerateAwaitedMember(nameof(Option<>.MapOrAsync))]
[GenerateAwaitedMember(nameof(Option<>.MapOrDefault))]
[GenerateAwaitedMember(nameof(Option<>.MapOrDefaultAsync))]
[GenerateAwaitedMember(nameof(Option<>.MapOrElse))]
[GenerateAwaitedMember(nameof(Option<>.MapOrElseAsync))]
[GenerateAwaitedMember(nameof(Option<>.MapOrNull))]
[GenerateAwaitedMember(nameof(Option<>.MapOrNullAsync))]
[GenerateAwaitedMember(nameof(Option<>.Match))]
[GenerateAwaitedMember(nameof(Option<>.MatchAsync))]
[GenerateAwaitedMember(nameof(Option<>.OkOr))]
[GenerateAwaitedMember(nameof(Option<>.OkOrElse))]
[GenerateAwaitedMember(nameof(Option<>.OkOrElseAsync))]
[GenerateAwaitedMember(nameof(Option<>.Or))]
[GenerateAwaitedMember(nameof(Option<>.OrElse))]
[GenerateAwaitedMember(nameof(Option<>.OrElseAsync))]
[GenerateAwaitedMember(nameof(Option<>.Reduce))]
[GenerateAwaitedMember(nameof(Option<>.ReduceAsync))]
[GenerateAwaitedMember(nameof(Option<>.Unwrap))]
[GenerateAwaitedMember(nameof(Option<>.UnwrapOr))]
[GenerateAwaitedMember(nameof(Option<>.UnwrapOrDefault))]
[GenerateAwaitedMember(nameof(Option<>.UnwrapOrElse))]
[GenerateAwaitedMember(nameof(Option<>.UnwrapOrElseAsync))]
[GenerateAwaitedMember(nameof(Option<>.Xor))]
[GenerateAwaitedMember(nameof(Option<>.Zip))]
[GenerateAwaitedMember(nameof(Option<>.ZipWith))]
[GenerateAwaitedMember(nameof(Option<>.ZipWithAsync))]
public static partial class OptionExtensions
{
    extension<T1, T2>(Option<(T1, T2)> option)
        where T1 : notnull where T2 : notnull
    {
        /// <summary>Unzips an option containing a tuple value into two options.</summary>
        /// <returns>
        /// A pair of <see cref="Some{T}" /> options carrying the two halves of the
        /// tuple if the option is a <see cref="Some{T}" />, otherwise a pair of
        /// <see cref="None{T}" />.
        /// </returns>
        public (Option<T1>, Option<T2>) Unzip() =>
            option.Match(
                tuple => (Some(tuple.Item1), Some(tuple.Item2)),
                () => (None<T1>(), None<T2>()));
    }

    extension<T>(Option<Option<T>> option) where T : notnull
    {
        /// <summary>
        /// Converts from <c>Option&lt;Option&lt;T&gt;&gt;</c> to
        /// <c>Option&lt;T&gt;</c>.
        /// </summary>
        /// <remarks>Flattening only removes one level of nesting at a time.</remarks>
        /// <returns>
        /// The inner option if the outer option is a <see cref="Some{T}" />,
        /// otherwise <see cref="None{T}" />.
        /// </returns>
        public Option<T> Flatten() =>
            option.Match(innerOption => innerOption, None<T>);
    }

    extension<TOk, TErr>(Option<Result<TOk, TErr>> option)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Transposes an <see cref="Option{T}" /> of a
        /// <see cref="Result{TOk,TErr}" /> into a <see cref="Result{TOk,TErr}" /> of
        /// an <see cref="Option{T}" />.
        /// </summary>
        /// <returns>
        /// <list type="bullet">
        /// <item>
        /// <see cref="None{T}" /> maps to <see cref="Ok{TOk,TErr}" /> of
        /// <see cref="None{T}" />
        /// </item>
        /// <item>
        /// <see cref="Some{T}" /> of <see cref="Ok{TOk,TErr}" /> maps to
        /// <see cref="Ok{TOk,TErr}" /> of <see cref="Some{T}" />
        /// </item>
        /// <item>
        /// <see cref="Some{T}" /> of <see cref="Err{TOk,TErr}" /> maps to
        /// <see cref="Err{TOk,TErr}" />, discarding the option
        /// </item>
        /// </list>
        /// </returns>
        public Result<Option<TOk>, TErr> Transpose() =>
            option.Match(
                some => some.Match(
                    ok => Result.Ok<Option<TOk>, TErr>(Some(ok)),
                    Result.Err<Option<TOk>, TErr>),
                () => Result.Ok<Option<TOk>, TErr>(None<TOk>()));
    }

    extension<T>(Option<T> option) where T : notnull
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
        /// <typeparam name="TOut">The projected value's type.</typeparam>
        /// <param name="selector">Projects the contained value. Called only for a
        /// <see cref="Some{T}" />, and must not return null.</param>
        /// <returns>
        /// A <see cref="Some{T}" /> of the projected value if the option was a
        /// <see cref="Some{T}" />, otherwise <see cref="None{T}" />.
        /// </returns>
        public Option<TOut> Select<TOut>(Func<T, TOut> selector)
            where TOut : notnull =>
            option.Map(selector);

        /// <summary>Chains an option-producing step, under LINQ's name for bind.</summary>
        /// <remarks>
        /// Forwards to <see cref="Option{T}.AndThen{TOut}(Func{T,Option{TOut}})" />.
        /// This is the overload a method-syntax caller writes; query syntax uses the
        /// three-argument sibling instead, and a single <c>from</c> never reaches
        /// this one.
        /// </remarks>
        /// <typeparam name="TOut">The next step's value type.</typeparam>
        /// <param name="optionFactory">Produces the next option from the contained
        /// value. Called only for a <see cref="Some{T}" />, and must not return
        /// null.</param>
        /// <returns>
        /// The option <paramref name="optionFactory" /> produced if this option was a
        /// <see cref="Some{T}" />, otherwise <see cref="None{T}" />.
        /// </returns>
        public Option<TOut> SelectMany<TOut>(Func<T, Option<TOut>> optionFactory)
            where TOut : notnull =>
            option.AndThen(optionFactory);

        /// <summary>Joins two options and projects the pair, as a multi-clause query does.</summary>
        /// <remarks>
        /// The shape the compiler requires for a query with more than one
        /// <c>from</c> clause; nothing else needs it, and a hand-written call is
        /// almost always clearer as
        /// <see cref="Option{T}.AndThen{TOut}(Func{T,Option{TOut}})" />. Both
        /// selectors are threaded through this library's state-passing overloads
        /// rather than captured, so a query allocates no closure per clause.
        /// <para>
        /// Short-circuits at the first <see cref="None{T}" />: if this option is a
        /// <see cref="None{T}" /> neither selector runs, and if
        /// <paramref name="optionFactory" /> yields a <see cref="None{T}" />
        /// then <paramref name="resultSelector" /> does not run.
        /// </para>
        /// </remarks>
        /// <typeparam name="TCollection">The joined option's value type.</typeparam>
        /// <typeparam name="TResult">The projected result's type.</typeparam>
        /// <param name="optionFactory">Produces the option to join, from this
        /// option's contained value.</param>
        /// <param name="resultSelector">Combines both contained values. Must not
        /// return null.</param>
        /// <returns>
        /// A <see cref="Some{T}" /> of the combined value if both options were a
        /// <see cref="Some{T}" />, otherwise <see cref="None{T}" />.
        /// </returns>
        public Option<TResult> SelectMany<TCollection, TResult>(
            Func<T, Option<TCollection>> optionFactory,
            Func<T, TCollection, TResult> resultSelector)
            where TCollection : notnull where TResult : notnull =>
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
        /// counterpart on <see cref="Result{TOk,TErr}" />: discarding an
        /// <see cref="Ok{TOk,TErr}" /> would have to invent the error that replaces
        /// it, and a signature taking an error factory is not the one query syntax
        /// looks for.
        /// </remarks>
        /// <param name="predicate">Tests the contained value. Called only for a
        /// <see cref="Some{T}" />.</param>
        /// <returns>
        /// This option if it was a <see cref="Some{T}" /> whose value satisfied
        /// <paramref name="predicate" />, otherwise <see cref="None{T}" />.
        /// </returns>
        public Option<T> Where(Func<T, bool> predicate) =>
            option.Filter(predicate);
    }

    extension<T>(Option<T> option) where T : struct
    {
        /// <summary>
        /// Returns the contained value if the option is a <see cref="Some{T}" />,
        /// otherwise <see langword="null" />.
        /// </summary>
        /// <remarks>
        /// Prefer this to <see cref="Option{T}.UnwrapOrDefault" />, which returns
        /// the default of <typeparamref name="T" /> for a <see cref="None{T}" /> —
        /// for a value type that is indistinguishable from a legitimate zero.
        /// </remarks>
        /// <returns>
        /// The contained value if the option was a <see cref="Some{T}" />,
        /// otherwise <see langword="null" />.
        /// </returns>
        public T? UnwrapOrNull() => option.Match<T?>(value => value, () => null);
    }

    extension<TSelf>(Task<Option<TSelf>> optionTask) where TSelf : notnull
    {
        /// <summary>
        /// Awaits two <see cref="Task{TResult}" /> options and combines their
        /// values, when both hold one.
        /// </summary>
        /// <param name="otherTask">The awaited option to combine with.</param>
        /// <param name="zip">
        /// Combines the two contained values. It is invoked only when both options
        /// are a <see cref="Some{T}" />.
        /// </param>
        /// <typeparam name="TOther">The value type of the other option.</typeparam>
        /// <typeparam name="TOut">The type the delegate produces.</typeparam>
        /// <returns>
        /// <see cref="Some{T}" /> of what <paramref name="zip" /> produced when both
        /// options hold a value, otherwise <see cref="None{T}" />.
        /// </returns>
        public async ValueTask<Option<TOut>> ZipWithAsync<TOther, TOut>(
            Task<Option<TOther>> otherTask,
            Func<TSelf, TOther, Task<TOut>> zip)
            where TOther : notnull
            where TOut : notnull
        {
            Option<TSelf> option = await optionTask.ConfigureAwait(false);
            Option<TOther> other = await otherTask.ConfigureAwait(false);

            return await option.ZipWithAsync(other, zip).ConfigureAwait(false);
        }
    }

    extension<TSelf>(ValueTask<Option<TSelf>> optionTask) where TSelf : notnull
    {
        /// <summary>
        /// Awaits two <see cref="ValueTask{TResult}" /> options and combines
        /// their values, when both hold one.
        /// </summary>
        /// <param name="otherTask">The awaited option to combine with.</param>
        /// <param name="zip">
        /// Combines the two contained values. It is invoked only when both options
        /// are a <see cref="Some{T}" />.
        /// </param>
        /// <typeparam name="TOther">The value type of the other option.</typeparam>
        /// <typeparam name="TOut">The type the delegate produces.</typeparam>
        /// <returns>
        /// <see cref="Some{T}" /> of what <paramref name="zip" /> produced when both
        /// options hold a value, otherwise <see cref="None{T}" />.
        /// </returns>
        public async ValueTask<Option<TOut>> ZipWithAsync<TOther, TOut>(
            ValueTask<Option<TOther>> otherTask,
            Func<TSelf, TOther, Task<TOut>> zip)
            where TOther : notnull
            where TOut : notnull
        {
            Option<TSelf> option = await optionTask.ConfigureAwait(false);
            Option<TOther> other = await otherTask.ConfigureAwait(false);

            return await option.ZipWithAsync(other, zip).ConfigureAwait(false);
        }
    }
}
