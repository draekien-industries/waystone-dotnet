namespace Waystone.Monads.Results.Extensions;

using System;
using Options;
using Waystone.SourceGenerators;
#if !DEBUG
using System.Diagnostics;
#endif

/// <summary>
/// Everything callable on a <see cref="Result{TOk,TErr}" /> that is not
/// declared on the type itself.
/// </summary>
/// <remarks>
/// Two kinds of member live here. Most are generated: the awaited-receiver
/// overloads that let a call chain stay in one expression when the result is
/// still inside a <see cref="System.Threading.Tasks.Task{TResult}" /> or a
/// <see cref="System.Threading.Tasks.ValueTask{TResult}" />, listed in the
/// attributes below and forwarding into the member of the same name on
/// <see cref="Result{TOk,TErr}" />. The rest are hand-written, and each is here
/// because its receiver is a *particular* result rather than any result:
/// <c>Flatten</c> reads a nested result, <c>Transpose</c> a result of an
/// <see cref="Option{T}" />, and <c>UnwrapOrNull</c> one whose ok value is a
/// value type. All three are lifted onto the two awaited receivers as well, so
/// none needs an attribute.
/// <para>
/// Operations over a *sequence* of results are the one thing that is not here.
/// They take an <see cref="System.Collections.Generic.IEnumerable{T}" />
/// receiver rather than a <see cref="Result{TOk,TErr}" />, so they share
/// nothing with the members below and live in
/// <see cref="ResultsCollectionExtensions" />.
/// </para>
/// </remarks>
#if !DEBUG
[DebuggerStepThrough]
#endif
[GenerateAwaitedReceivers(typeof(Result<,>))]
[GenerateAwaitedMember(nameof(Result<,>.And))]
[GenerateAwaitedMember(nameof(Result<,>.AndThen))]
[GenerateAwaitedMember(nameof(Result<,>.AndThenAsync))]
[GenerateAwaitedMember(nameof(Result<,>.AsEnumerable))]
[GenerateAwaitedMember(nameof(Result<,>.Expect))]
[GenerateAwaitedMember(nameof(Result<,>.ExpectErr))]
[GenerateAwaitedMember(
    nameof(Result<,>.GetErr),
    Summary = "Awaits the result and returns an option holding its error.")]
[GenerateAwaitedMember(
    nameof(Result<,>.GetOk),
    Summary = "Awaits the result and returns an option holding its success value.")]
[GenerateAwaitedMember(nameof(Result<,>.Inspect))]
[GenerateAwaitedMember(nameof(Result<,>.InspectAsync))]
[GenerateAwaitedMember(nameof(Result<,>.InspectErr))]
[GenerateAwaitedMember(nameof(Result<,>.InspectErrAsync))]
[GenerateAwaitedMember(nameof(Result<,>.IsErrAnd))]
[GenerateAwaitedMember(nameof(Result<,>.IsErrAndAsync))]
[GenerateAwaitedMember(nameof(Result<,>.IsOkAnd))]
[GenerateAwaitedMember(nameof(Result<,>.IsOkAndAsync))]
[GenerateAwaitedMember(nameof(Result<,>.Map))]
[GenerateAwaitedMember(nameof(Result<,>.MapAsync))]
[GenerateAwaitedMember(nameof(Result<,>.MapErr))]
[GenerateAwaitedMember(nameof(Result<,>.MapErrAsync))]
[GenerateAwaitedMember(nameof(Result<,>.MapOr))]
[GenerateAwaitedMember(nameof(Result<,>.MapOrAsync))]
[GenerateAwaitedMember(nameof(Result<,>.MapOrDefault))]
[GenerateAwaitedMember(nameof(Result<,>.MapOrDefaultAsync))]
[GenerateAwaitedMember(nameof(Result<,>.MapOrElse))]
[GenerateAwaitedMember(nameof(Result<,>.MapOrElseAsync))]
[GenerateAwaitedMember(nameof(Result<,>.MapOrNull))]
[GenerateAwaitedMember(nameof(Result<,>.MapOrNullAsync))]
[GenerateAwaitedMember(nameof(Result<,>.Match))]
[GenerateAwaitedMember(nameof(Result<,>.MatchAsync))]
[GenerateAwaitedMember(nameof(Result<,>.Or))]
[GenerateAwaitedMember(nameof(Result<,>.OrElse))]
[GenerateAwaitedMember(nameof(Result<,>.OrElseAsync))]
[GenerateAwaitedMember(nameof(Result<,>.Unwrap))]
[GenerateAwaitedMember(nameof(Result<,>.UnwrapErr))]
[GenerateAwaitedMember(nameof(Result<,>.UnwrapOr))]
[GenerateAwaitedMember(nameof(Result<,>.UnwrapOrDefault))]
[GenerateAwaitedMember(nameof(Result<,>.UnwrapOrElse))]
[GenerateAwaitedMember(nameof(Result<,>.UnwrapOrElseAsync))]
public static partial class ResultExtensions
{
    extension<TOk, TErr>(Result<Result<TOk, TErr>, TErr> result)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Collapses one level of nesting, so a result of a result becomes a plain
        /// result.
        /// </summary>
        /// <returns>
        /// The inner result unchanged if the outer result is an
        /// <see cref="Ok{TOk,TErr}" /> — including when that inner result is itself
        /// an <see cref="Err{TOk,TErr}" /> — otherwise the outer error re-wrapped as
        /// an <see cref="Err{TOk,TErr}" /> of <typeparamref name="TOk" />.
        /// </returns>
        public Result<TOk, TErr> Flatten()
        {
            if (result.IsOk) return result.Expect("Expected Ok but found Err.");

            TErr err = result.ExpectErr("Expected Err but found Ok.");

            return Result.Err<TOk, TErr>(err);
        }
    }

    extension<TOk, TErr>(Result<Option<TOk>, TErr> result)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>
        /// Turns a result of an option into an option of a result.
        /// </summary>
        /// <returns>
        /// <see cref="None{T}" /> if the result is an <see cref="Ok{TOk,TErr}" /> of
        /// <see cref="None{T}" />; <see cref="Some{T}" /> of an
        /// <see cref="Ok{TOk,TErr}" /> if it is an <see cref="Ok{TOk,TErr}" /> of
        /// <see cref="Some{T}" />; and <see cref="Some{T}" /> of an
        /// <see cref="Err{TOk,TErr}" /> if it is an <see cref="Err{TOk,TErr}" />. An
        /// error is never discarded.
        /// </returns>
        public Option<Result<TOk, TErr>> Transpose()
        {
            if (result.IsErr)
            {
                TErr err = result.ExpectErr("Expected Err but found Ok.");
                Result<TOk, TErr> errResult = Result.Err<TOk, TErr>(err);

                return Option.Some(errResult);
            }

            Option<TOk> option = result.Expect("Expected Ok but found Err.");

            if (option.IsNone)
            {
                return Option.None<Result<TOk, TErr>>();
            }

            TOk value = option.Expect("Expected Some but found None.");

            return Option.Some(Result.Ok<TOk, TErr>(value));
        }
    }

    extension<TOk, TErr>(Result<TOk, TErr> result)
        where TOk : notnull where TErr : notnull
    {
        /// <summary>Projects the ok value, under LINQ's name for the operation.</summary>
        /// <remarks>
        /// Forwards to
        /// <see cref="Result{TOk,TErr}.Map{TOut}(Func{TOk,TOut})" /> and
        /// behaves identically; the two differ only in name. It is here so that a
        /// <c>select</c> clause binds, and so a reader who knows
        /// <see cref="System.Linq.Enumerable" /> finds the operation under the name
        /// they already use. The error is carried through untouched — to project
        /// that instead, there is no LINQ name and
        /// <see cref="Result{TOk,TErr}.MapErr{TOut}(Func{TErr,TOut})" /> is
        /// the only spelling.
        /// </remarks>
        /// <typeparam name="TOut">The projected ok value's type.</typeparam>
        /// <param name="selector">Projects the ok value. Called only for an
        /// <see cref="Ok{TOk,TErr}" />, and must not return null.</param>
        /// <returns>
        /// An <see cref="Ok{TOk,TErr}" /> of the projected value if the result was an
        /// <see cref="Ok{TOk,TErr}" />, otherwise the original
        /// <see cref="Err{TOk,TErr}" />.
        /// </returns>
        public Result<TOut, TErr> Select<TOut>(Func<TOk, TOut> selector)
            where TOut : notnull =>
            result.Map(selector);

        /// <summary>Chains a result-producing step, under LINQ's name for bind.</summary>
        /// <remarks>
        /// Forwards to
        /// <see cref="Result{TOk,TErr}.AndThen{TOut}(Func{TOk,Result{TOut,TErr}})" />.
        /// This is the overload a method-syntax caller writes; query syntax uses the
        /// three-argument sibling instead, and a single <c>from</c> never reaches
        /// this one.
        /// <para>
        /// Note that the chained step must fail with the same
        /// <typeparamref name="TErr" />. A step carrying a different error type has
        /// to be mapped onto this one first, which is the constraint that makes a
        /// query over <see cref="Result{TOk,TErr}" /> less pliable than one over
        /// <see cref="Option{T}" />.
        /// </para>
        /// </remarks>
        /// <typeparam name="TOut">The next step's ok value type.</typeparam>
        /// <param name="resultFactory">Produces the next result from the ok value.
        /// Called only for an <see cref="Ok{TOk,TErr}" />, and must not return
        /// null.</param>
        /// <returns>
        /// The result <paramref name="resultFactory" /> produced if this result was an
        /// <see cref="Ok{TOk,TErr}" />, otherwise the original
        /// <see cref="Err{TOk,TErr}" />.
        /// </returns>
        public Result<TOut, TErr> SelectMany<TOut>(
            Func<TOk, Result<TOut, TErr>> resultFactory)
            where TOut : notnull =>
            result.AndThen(resultFactory);

        /// <summary>Joins two results and projects the pair, as a multi-clause query does.</summary>
        /// <remarks>
        /// The shape the compiler requires for a query with more than one
        /// <c>from</c> clause; nothing else needs it, and a hand-written call is
        /// almost always clearer as
        /// <see cref="Result{TOk,TErr}.AndThen{TOut}(Func{TOk,Result{TOut,TErr}})" />.
        /// Both selectors are threaded through this library's state-passing
        /// overloads rather than captured, so a query allocates no closure per
        /// clause.
        /// <para>
        /// Short-circuits at the first <see cref="Err{TOk,TErr}" />, and the error
        /// that surfaces is the first one encountered: if this result is an
        /// <see cref="Err{TOk,TErr}" /> neither selector runs, and if
        /// <paramref name="resultFactory" /> yields an
        /// <see cref="Err{TOk,TErr}" /> then <paramref name="resultSelector" /> does
        /// not run. Later errors are never collected — this is not a validation
        /// combinator.
        /// </para>
        /// </remarks>
        /// <typeparam name="TCollection">The joined result's ok value type.</typeparam>
        /// <typeparam name="TResult">The projected result's ok value type.</typeparam>
        /// <param name="resultFactory">Produces the result to join, from this
        /// result's ok value.</param>
        /// <param name="resultSelector">Combines both ok values. Must not return
        /// null.</param>
        /// <returns>
        /// An <see cref="Ok{TOk,TErr}" /> of the combined value if both results were
        /// an <see cref="Ok{TOk,TErr}" />, otherwise the first
        /// <see cref="Err{TOk,TErr}" /> of the two.
        /// </returns>
        public Result<TResult, TErr> SelectMany<TCollection, TResult>(
            Func<TOk, Result<TCollection, TErr>> resultFactory,
            Func<TOk, TCollection, TResult> resultSelector)
            where TCollection : notnull where TResult : notnull =>
            result.AndThen(
                (resultFactory, resultSelector),
                static (value, selectors) => selectors.resultFactory(value)
                   .Map(
                        (value, selectors.resultSelector),
                        static (collected, state) => state.resultSelector(
                            state.value,
                            collected)));
    }

    extension<TOk, TErr>(Result<TOk, TErr> result)
        where TOk : struct where TErr : notnull
    {
        /// <summary>
        /// Returns the contained value if the result is an
        /// <see cref="Ok{TOk,TErr}" />, otherwise <see langword="null" />.
        /// </summary>
        /// <remarks>
        /// Prefer this to <see cref="Result{TOk,TErr}.UnwrapOrDefault" /> when
        /// <typeparamref name="TOk" /> is a value type. <c>UnwrapOrDefault</c> returns
        /// the default of <typeparamref name="TOk" /> for an
        /// <see cref="Err{TOk,TErr}" />, which is indistinguishable from a legitimate
        /// zero.
        /// </remarks>
        /// <returns>
        /// The contained value if the result was an <see cref="Ok{TOk,TErr}" />,
        /// otherwise <see langword="null" />.
        /// </returns>
        public TOk? UnwrapOrNull() =>
            result.Match<TOk?>(value => value, _ => null);
    }
}
