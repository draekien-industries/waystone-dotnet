namespace Waystone.Monads.Analyzers;

using System.Threading.Tasks;
using Xunit;

public class InheritanceAnalyzerTests
{
    [Fact]
    public Task FlagsATypeDerivingFromOption() =>
        Verify.AnalyzerAsync<InheritanceAnalyzer>(
            """
            internal sealed record Cache<T> : {|#0:Option<T>|}
                where T : notnull
            {
                public override bool IsSome => false;
                public override bool IsNone => true;
                public override bool IsSomeAnd(Func<T, bool> predicate) => false;
                public override bool IsNoneOr(Func<T, bool> predicate) => true;
                public override TOut Match<TOut>(Func<T, TOut> onSome, Func<TOut> onNone) => onNone();
                public override void Match(Action<T> onSome, Action onNone) => onNone();
                public override T Expect(string message) => throw new InvalidOperationException();
                public override T Unwrap() => throw new InvalidOperationException();
                public override T UnwrapOr(T value) => value;
                public override T? UnwrapOrDefault() => default;
                public override T UnwrapOrElse(Func<T> @else) => @else();
                public override Option<T2> Map<T2>(Func<T, T2> map) => Option.None<T2>();
                public override T2 MapOr<T2>(T2 @default, Func<T, T2> map) => @default;
                public override T2 MapOrElse<T2>(Func<T2> createDefault, Func<T, T2> map) => createDefault();
                public override Option<T> Inspect(Action<T> action) => this;
                public override Option<T> Filter(Func<T, bool> predicate) => this;
                public override Option<T> Or(Option<T> other) => other;
                public override Option<T> OrElse(Func<Option<T>> createElse) => createElse();
                public override Option<T> Xor(Option<T> other) => other;
                public override Option<(T, T2)> Zip<T2>(Option<T2> other) => Option.None<(T, T2)>();
                public override Option<TOut> ZipWith<TOther, TOut>(Option<TOther> other, Func<T, TOther, TOut> zip) => Option.None<TOut>();
                public override Result<T, TErr> OkOr<TErr>(TErr error) => Result.Err<T, TErr>(error);
                public override Result<T, TErr> OkOrElse<TErr>(Func<TErr> errorFactory) => Result.Err<T, TErr>(errorFactory());
            }
            """,
            Verify.Diagnostic(Rules.DerivesFromMonad)
               .WithLocation(0)
               .WithArguments("Cache<T>", "Option<T>"));

    [Fact]
    public Task IgnoresATypeThatMerelyHoldsAnOption() =>
        Verify.NoDiagnosticAsync<InheritanceAnalyzer>(
            """
            internal sealed record Cache<T>(Option<T> Value)
                where T : notnull;
            """);

    [Fact]
    public Task IgnoresATypeDerivingFromSomethingElse() =>
        Verify.NoDiagnosticAsync<InheritanceAnalyzer>(
            """
            internal abstract record Base;

            internal sealed record Cache : Base;
            """);

    [Fact]
    public Task IgnoresATypeThatOnlyReturnsOptions() =>
        Verify.NoDiagnosticAsync<InheritanceAnalyzer>(
            """
            internal sealed class Subject
            {
                internal Option<int> Present() => Option.Some(1);

                internal Option<int> Absent() => Option.None<int>();
            }
            """);
}
