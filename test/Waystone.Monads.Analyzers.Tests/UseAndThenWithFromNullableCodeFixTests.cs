namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis.Testing;
using System;
using System.Threading.Tasks;
using Xunit;

public class UseAndThenWithFromNullableCodeFixTests
{
    [Fact]
    public Task FixesAProjectionOntoANullableReference() =>
        Verify.CompilerCodeFixAsync<UseAndThenWithFromNullableCodeFix>(
            """
            internal sealed class Customer;

            internal sealed class Order
            {
                public Customer? Customer { get; set; }
            }

            internal class Subject
            {
                internal Option<Customer> Project(Option<Order> order) =>
                    {|#0:order.Map|}(o => o.Customer);
            }
            """,
            """
            internal sealed class Customer;

            internal sealed class Order
            {
                public Customer? Customer { get; set; }
            }

            internal class Subject
            {
                internal Option<Customer> Project(Option<Order> order) =>
                    order.AndThen(o => Option.FromNullable(o.Customer));
            }
            """,
            new[] { Constraint(0) },
            Array.Empty<DiagnosticResult>());

    /// <remarks>
    /// The value type case is a build error and not only a warning, because
    /// Option&lt;int?&gt; does not convert to Option&lt;int&gt;. FromNullable's
    /// struct overload is what resolves it, so the fix is offered here too.
    /// </remarks>
    [Fact]
    public Task FixesAProjectionOntoANullableValueType() =>
        Verify.CompilerCodeFixAsync<UseAndThenWithFromNullableCodeFix>(
            """
            internal Option<int> Project(Option<int> option) =>
                {|#1:{|#0:option.Map|}(value => Count(value))|};

            private static int? Count(int value) => value > 0 ? value : null;
            """,
            """
            internal Option<int> Project(Option<int> option) =>
                option.AndThen(value => Option.FromNullable(Count(value)));

            private static int? Count(int value) => value > 0 ? value : null;
            """,
            new[]
            {
                Constraint(0),
                DiagnosticResult.CompilerError("CS0029").WithLocation(1),
            },
            Array.Empty<DiagnosticResult>());

    [Fact]
    public Task FixesTheStateOverloadWithoutDisturbingItsState() =>
        Verify.CompilerCodeFixAsync<UseAndThenWithFromNullableCodeFix>(
            """
            internal Option<string> Project(Option<int> option) =>
                {|#0:option.Map|}(2, static (value, factor) => Text(value * factor));

            private static string? Text(int value) => value > 0 ? "x" : null;
            """,
            """
            internal Option<string> Project(Option<int> option) =>
                option.AndThen(2, static (value, factor) => Option.FromNullable(Text(value * factor)));

            private static string? Text(int value) => value > 0 ? "x" : null;
            """,
            new[] { Constraint(0) },
            Array.Empty<DiagnosticResult>());

    /// <remarks>
    /// Result has no FromNullable, so the diagnostic stands unfixed rather than
    /// being offered a rewrite that does not compile.
    /// </remarks>
    [Fact]
    public Task LeavesAResultProjectionAlone() =>
        Unfixed(
            """
            internal Result<string, int> Project(Result<int, int> result) =>
                {|#0:result.Map|}(value => Text(value));

            private static string? Text(int value) => value > 0 ? "x" : null;
            """);

    /// <remarks>
    /// A method group has no body to wrap, and synthesising a lambda parameter
    /// would name something the source never chose.
    /// </remarks>
    [Fact]
    public Task LeavesAMethodGroupProjectionAlone() =>
        Unfixed(
            """
            internal Option<string> Project(Option<int> option) =>
                {|#0:option.Map|}(Text);

            private static string? Text(int value) => value > 0 ? "x" : null;
            """);

    /// <remarks>
    /// MapOrDefault reports the same constraint violation and is the nearest
    /// neighbour by name, so it pins that the fix keys on Map exactly rather
    /// than on a prefix.
    /// </remarks>
    [Fact]
    public Task LeavesANeighbouringProjectionAlone() =>
        Unfixed(
            """
            internal string Project(Option<int> option) =>
                {|#0:option.MapOrDefault|}(value => Text(value));

            private static string? Text(int value) => value > 0 ? "x" : null;
            """);

    /// <remarks>
    /// A block body has no single expression to wrap, and rewriting every
    /// return inside one is a different transformation from this one.
    /// </remarks>
    [Fact]
    public Task LeavesABlockBodiedLambdaAlone() =>
        Unfixed(
            """
            internal Option<string> Project(Option<int> option) =>
                {|#0:option.Map|}(value =>
                {
                    return Text(value);
                });

            private static string? Text(int value) => value > 0 ? "x" : null;
            """);

    /// <remarks>
    /// CS8714 is not this library's diagnostic, so the fix has to stay quiet on
    /// every other generic method carrying a notnull constraint.
    /// </remarks>
    [Fact]
    public Task LeavesAnUnrelatedNotNullConstraintAlone() =>
        Unfixed(
            """
            internal string Project() =>
                {|#0:Keep|}(Text());

            private static TOut Keep<TOut>(TOut value) where TOut : notnull =>
                value;

            private static string? Text() => null;
            """);

    private static DiagnosticResult Constraint(int location) =>
        DiagnosticResult.CompilerWarning("CS8714").WithLocation(location);

    /// <remarks>
    /// Asserts that the fix declines, by running it over a source whose fixed
    /// state is the source itself with the diagnostic still standing.
    /// </remarks>
    private static Task Unfixed(string source) =>
        Verify.CompilerCodeFixAsync<UseAndThenWithFromNullableCodeFix>(
            source,
            source,
            new[] { Constraint(0) },
            new[] { Constraint(0) });
}
