namespace Waystone.Monads.Options;

using System;
using System.Collections.Generic;
using Exceptions;
using Results;
#if !DEBUG
using System.Diagnostics;
#endif

/// <summary>No value of type <typeparamref name="T" />.</summary>
/// <typeparam name="T">The option value's type.</typeparam>
#if !DEBUG
[DebuggerStepThrough]
#endif
public sealed record None<T> : Option<T>
    where T : notnull
{
    /// <remarks>
    /// A <see cref="None{T}" /> holds nothing, so every instance of a given
    /// closed type is interchangeable and the runtime builds this one once.
    /// Structural equality is unaffected — two <c>None</c> values were already
    /// equal — but <c>ReferenceEquals</c> now answers true where it answered
    /// false before 6.0.0.
    /// </remarks>
    internal static readonly None<T> Instance = new();

    /// <inheritdoc />
    public override bool IsSome => false;

    /// <inheritdoc />
    public override bool IsNone => true;

    /// <inheritdoc />
    public override bool IsSomeAnd(Func<T, bool> predicate) =>
        false;

    /// <inheritdoc />
    public override bool IsSomeAnd<TState>(
        TState state,
        Func<T, TState, bool> predicate) => false;

    /// <inheritdoc />
    public override bool IsNoneOr(Func<T, bool> predicate) =>
        true;

    /// <inheritdoc />
    public override bool IsNoneOr<TState>(
        TState state,
        Func<T, TState, bool> predicate) => true;

    /// <inheritdoc />
    public override TOut Match<TOut>(
        Func<T, TOut> onSome,
        Func<TOut> onNone) => onNone();

    /// <inheritdoc />
    public override TOut Match<TState, TOut>(
        TState state,
        Func<T, TState, TOut> onSome,
        Func<TState, TOut> onNone) => onNone(state);

    /// <inheritdoc />
    public override void Match(Action<T> onSome, Action onNone)
    {
        onNone();
    }

    /// <inheritdoc />
    public override void Match<TState>(
        TState state,
        Action<T, TState> onSome,
        Action<TState> onNone)
    {
        onNone(state);
    }

    /// <inheritdoc />
    public override T Expect(string message) =>
        throw new UnmetExpectationException(message);

    /// <inheritdoc />
    public override T Unwrap() =>
        throw new UnwrapException("Unwrap called for a `None` value.");

    /// <inheritdoc />
    public override T UnwrapOr(T value) =>
        value;

    /// <inheritdoc />
    public override T? UnwrapOrDefault() =>
        default;

    /// <inheritdoc />
    public override T UnwrapOrElse(Func<T> @else) =>
        @else();

    /// <inheritdoc />
    public override T UnwrapOrElse<TState>(TState state, Func<TState, T> @else) =>
        @else(state);

    /// <inheritdoc />
    public override Option<TOut> And<TOut>(Option<TOut> other) =>
        Option.None<TOut>();

    /// <inheritdoc />
    public override Option<TOut> Map<TOut>(Func<T, TOut> map) =>
        Option.None<TOut>();

    /// <inheritdoc />
    public override Option<TOut> Map<TState, TOut>(
        TState state,
        Func<T, TState, TOut> map) => Option.None<TOut>();

    /// <inheritdoc />
    public override TOut MapOr<TOut>(TOut @default, Func<T, TOut> map) =>
        @default;

    /// <inheritdoc />
    public override TOut MapOr<TState, TOut>(
        TState state,
        TOut @default,
        Func<T, TState, TOut> map) => @default;

    /// <inheritdoc />
    public override TOut MapOrDefault<TOut>(Func<T, TOut> map) =>
        default!;

    /// <inheritdoc />
    public override TOut MapOrDefault<TState, TOut>(
        TState state,
        Func<T, TState, TOut> map) => default!;

    /// <inheritdoc />
    public override TOut MapOrElse<TOut>(
        Func<TOut> createDefault,
        Func<T, TOut> map) => createDefault();

    /// <inheritdoc />
    public override TOut MapOrElse<TState, TOut>(
        TState state,
        Func<TState, TOut> createDefault,
        Func<T, TState, TOut> map) => createDefault(state);

    /// <inheritdoc />
    public override Option<T> Inspect(Action<T> action) =>
        this;

    /// <inheritdoc />
    public override Option<T> Inspect<TState>(
        TState state,
        Action<T, TState> action) => this;

    /// <inheritdoc />
    public override Option<T> Filter(Func<T, bool> predicate) =>
        this;

    /// <inheritdoc />
    public override Option<T> Filter<TState>(
        TState state,
        Func<T, TState, bool> predicate) => this;

    /// <inheritdoc />
    public override Option<T> Or(Option<T> other) =>
        other;

    /// <inheritdoc />
    public override Option<T> OrElse(Func<Option<T>> createElse) =>
        createElse();

    /// <inheritdoc />
    public override Option<T> OrElse<TState>(
        TState state,
        Func<TState, Option<T>> createElse) => createElse(state);

    /// <inheritdoc />
    public override Option<T> Xor(Option<T> other) =>
        other.IsSome ? other : this;

    /// <inheritdoc />
    public override Option<(T, TOther)> Zip<TOther>(Option<TOther> other) =>
        Option.None<(T, TOther)>();

    /// <inheritdoc />
    public override Option<TOut> ZipWith<TOther, TOut>(
        Option<TOther> other,
        Func<T, TOther, TOut> zip) =>
        Option.None<TOut>();

    /// <inheritdoc />
    public override Option<T> Reduce(Option<T> other, Func<T, T, T> reduce) =>
        other;

    /// <inheritdoc />
    public override IEnumerable<T> AsEnumerable() =>
        Array.Empty<T>();

    /// <inheritdoc />
    public override Result<T, TErr> OkOr<TErr>(TErr error) =>
        Result.Err<T, TErr>(error);

    /// <inheritdoc />
    public override Result<T, TErr> OkOrElse<TErr>(Func<TErr> errorFactory) =>
        Result.Err<T, TErr>(errorFactory());

    /// <inheritdoc />
    public override Result<T, TErr> OkOrElse<TState, TErr>(
        TState state,
        Func<TState, TErr> errorFactory) =>
        Result.Err<T, TErr>(errorFactory(state));

    internal override void OnlyThisAssemblyMayDerive()
    { }
}
