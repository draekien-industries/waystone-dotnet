namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis.Testing;
using System.Threading.Tasks;
using Xunit;

public class ClosedHierarchyTests
{
    [Fact]
    public Task AnOutsideAssemblyCannotDeriveFromOption() =>
        Verify.CompilerDiagnosticsAsync(
            """
            using System;
            using System.Collections.Generic;
            using Waystone.Monads.Options;
            using Waystone.Monads.Results;

            public sealed record {|#0:Maybe|}<T> : Option<T> where T : notnull
            {
                public Maybe(Option<T> original) : base(original) { }

                public override bool IsSome => false;
                public override bool IsNone => true;
                public override bool IsSomeAnd(Func<T, bool> predicate) => false;
                public override bool IsSomeAnd<TState>(TState state, Func<T, TState, bool> predicate) => false;
                public override bool IsNoneOr(Func<T, bool> predicate) => true;
                public override bool IsNoneOr<TState>(TState state, Func<T, TState, bool> predicate) => true;
                public override TOut Match<TOut>(Func<T, TOut> onSome, Func<TOut> onNone) => onNone();
                public override TOut Match<TState, TOut>(TState state, Func<T, TState, TOut> onSome, Func<TState, TOut> onNone) => onNone(state);
                public override void Match(Action<T> onSome, Action onNone) => onNone();
                public override void Match<TState>(TState state, Action<T, TState> onSome, Action<TState> onNone) => onNone(state);
                public override T Expect(string message) => throw new Exception();
                public override T Unwrap() => throw new Exception();
                public override T UnwrapOr(T value) => value;
                public override T? UnwrapOrDefault() => default;
                public override T UnwrapOrElse(Func<T> @else) => @else();
                public override T UnwrapOrElse<TState>(TState state, Func<TState, T> @else) => @else(state);
                public override Option<T2> And<T2>(Option<T2> other) => Option.None<T2>();
                public override Option<T2> Map<T2>(Func<T, T2> map) => Option.None<T2>();
                public override Option<T2> Map<TState, T2>(TState state, Func<T, TState, T2> map) => Option.None<T2>();
                public override T2 MapOr<T2>(T2 @default, Func<T, T2> map) => @default;
                public override T2 MapOr<TState, T2>(TState state, T2 @default, Func<T, TState, T2> map) => @default;
                public override T2 MapOrDefault<T2>(Func<T, T2> map) => default!;
                public override T2 MapOrDefault<TState, T2>(TState state, Func<T, TState, T2> map) => default!;
                public override T2 MapOrElse<T2>(Func<T2> createDefault, Func<T, T2> map) => createDefault();
                public override T2 MapOrElse<TState, T2>(TState state, Func<TState, T2> createDefault, Func<T, TState, T2> map) => createDefault(state);
                public override Option<T> Inspect(Action<T> action) => this;
                public override Option<T> Inspect<TState>(TState state, Action<T, TState> action) => this;
                public override Option<T> Filter(Func<T, bool> predicate) => this;
                public override Option<T> Filter<TState>(TState state, Func<T, TState, bool> predicate) => this;
                public override Option<T> Or(Option<T> other) => other;
                public override Option<T> OrElse(Func<Option<T>> createElse) => createElse();
                public override Option<T> OrElse<TState>(TState state, Func<TState, Option<T>> createElse) => createElse(state);
                public override Option<T> Xor(Option<T> other) => other;
                public override Option<(T, T2)> Zip<T2>(Option<T2> other) => Option.None<(T, T2)>();
                public override Option<TOut> ZipWith<TOther, TOut>(Option<TOther> other, Func<T, TOther, TOut> zip) => Option.None<TOut>();
                public override Option<T> Reduce(Option<T> other, Func<T, T, T> reduce) => other;
                public override IEnumerable<T> AsEnumerable() => Array.Empty<T>();
                public override Result<T, TErr> OkOr<TErr>(TErr error) => Result.Err<T, TErr>(error);
                public override Result<T, TErr> OkOrElse<TErr>(Func<TErr> errorFactory) => Result.Err<T, TErr>(errorFactory());
                public override Result<T, TErr> OkOrElse<TState, TErr>(TState state, Func<TState, TErr> errorFactory) => Result.Err<T, TErr>(errorFactory(state));
            }
            """,
            DiagnosticResult.CompilerError("CS0534").WithLocation(0));

    [Fact]
    public Task AnOutsideAssemblyCannotDeriveFromResult() =>
        Verify.CompilerDiagnosticsAsync(
            """
            using System;
            using System.Collections.Generic;
            using Waystone.Monads.Options;
            using Waystone.Monads.Results;

            public sealed record {|#0:Either|}<TOk, TErr> : Result<TOk, TErr>
                where TOk : notnull where TErr : notnull
            {
                public Either(Result<TOk, TErr> original) : base(original) { }

                public override bool IsOk => false;
                public override bool IsErr => true;
                public override bool IsOkAnd(Func<TOk, bool> predicate) => false;
                public override bool IsErrAnd(Func<TErr, bool> predicate) => true;
                public override TOut Match<TOut>(Func<TOk, TOut> onOk, Func<TErr, TOut> onErr) => throw new Exception();
                public override void Match(Action<TOk> onOk, Action<TErr> onErr) => throw new Exception();
                public override Result<TOut, TErr> And<TOut>(Result<TOut, TErr> other) => throw new Exception();
                public override Result<TOut, TErr> AndThen<TOut>(Func<TOk, Result<TOut, TErr>> map) => throw new Exception();
                public override Result<TOk, TOut> Or<TOut>(Result<TOk, TOut> other) => other;
                public override Result<TOk, TOut> OrElse<TOut>(Func<TErr, Result<TOk, TOut>> map) => throw new Exception();
                public override TOk Expect(string message) => throw new Exception();
                public override TErr ExpectErr(string message) => throw new Exception();
                public override TOk Unwrap() => throw new Exception();
                public override TOk UnwrapOr(TOk @default) => @default;
                public override TOk? UnwrapOrDefault() => default;
                public override TOk UnwrapOrElse(Func<TErr, TOk> onErr) => throw new Exception();
                public override TErr UnwrapErr() => throw new Exception();
                public override Result<TOk, TErr> Inspect(Action<TOk> action) => this;
                public override Result<TOk, TErr> InspectErr(Action<TErr> action) => this;
                public override Result<TOut, TErr> Map<TOut>(Func<TOk, TOut> map) => throw new Exception();
                public override Result<TOut, TErr> Map<TState, TOut>(TState state, Func<TOk, TState, TOut> map) => throw new Exception();
                public override TOut MapOr<TOut>(TOut @default, Func<TOk, TOut> map) => @default;
                public override TOut MapOr<TState, TOut>(TState state, TOut @default, Func<TOk, TState, TOut> map) => @default;
                public override TOut MapOrDefault<TOut>(Func<TOk, TOut> map) => default!;
                public override TOut MapOrElse<TOut>(Func<TErr, TOut> createDefault, Func<TOk, TOut> map) => throw new Exception();
                public override TOut MapOrElse<TState, TOut>(TState state, Func<TErr, TState, TOut> createDefault, Func<TOk, TState, TOut> map) => throw new Exception();
                public override Result<TOk, TOut> MapErr<TOut>(Func<TErr, TOut> map) => throw new Exception();
                public override Result<TOk, TOut> MapErr<TState, TOut>(TState state, Func<TErr, TState, TOut> map) => throw new Exception();
                public override IEnumerable<TOk> AsEnumerable() => Array.Empty<TOk>();
                public override Option<TOk> GetOk() => Option.None<TOk>();
                public override Option<TErr> GetErr() => Option.None<TErr>();
            }
            """,
            DiagnosticResult.CompilerError("CS0534").WithLocation(0));
}
