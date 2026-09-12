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
/// Every member here is callable on an <see cref="Option{T}" />: most also
/// have an awaited-receiver overload that runs the same operation on a
/// <see cref="Task{TResult}" /> or <see cref="ValueTask{TResult}" /> of one,
/// so a call chain stays in one expression before the option has been
/// awaited.
/// <para>
/// Operations over a sequence of options are the one thing that is not here.
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
    extension<T>(Option<T> option) where T : notnull
    {
        /// <summary>
        /// Binds a value to the option so that the next call can pass it to a
        /// delegate rather than have the delegate capture it.
        /// </summary>
        /// <remarks>
        /// The members on the returned <see cref="Option{T}.Bound{TState}" />
        /// correspond to the ones here, minus the state argument, and each returns
        /// the plain <see cref="Option{T}" /> again. The state is consumed by the
        /// call that uses it, and a chain binds as many times as it needs to:
        /// <code>
        /// option.With(limit)
        ///       .Filter(static (v, s) => v &lt;= s)
        ///       .With(format)
        ///       .Map(static (v, s) => v.ToString(s));
        /// </code>
        /// <para>
        /// Pass a tuple to bind more than one value. Mark every lambda
        /// <see langword="static" />, which makes the compiler reject any outer
        /// variable the lambda still captures directly — binding the state
        /// achieves nothing if the delegate captures one anyway.
        /// </para>
        /// <para>
        /// There is deliberately no awaited-receiver form: a task of a binder
        /// is not something a caller can chain. On a
        /// <see cref="Task{TResult}" /> option, await it and then call this on
        /// the result.
        /// </para>
        /// </remarks>
        /// <param name="state">
        /// The value passed to the delegate of whichever member is called next.
        /// Nothing reads it in the meantime, so it may be anything, including
        /// <see langword="null" />.
        /// </param>
        /// <typeparam name="TState">The type of the bound value.</typeparam>
        /// <returns>
        /// An <see cref="Option{T}.Bound{TState}" /> holding the option and
        /// <paramref name="state" /> together, whose members forward to the
        /// state-accepting overload of the same name on <see cref="Option{T}" />.
        /// </returns>
        [ExcludeFromAwaitedReceivers]
        public Option<T>.Bound<TState> With<TState>(TState state) =>
            new(option, state);
    }

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
