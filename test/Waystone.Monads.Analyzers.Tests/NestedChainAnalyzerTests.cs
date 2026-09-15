namespace Waystone.Monads.Analyzers;

using System.Threading.Tasks;
using Xunit;

public class NestedChainAnalyzerTests
{
    private const string DepthOfThree = """
                                        [*.cs]
                                        dotnet_code_quality.WM2025.max_chain_depth = 3
                                        """;

    [Fact]
    public Task FlagsAChainInsideAChainDelegate() =>
        Verify.AnalyzerAsync<NestedChainAnalyzer>(
            """
            internal Option<int> Chain(Option<int> option, Option<int> next) =>
                option.AndThen(value => next.{|#0:Map|}(inner => inner + value));
            """,
            Verify.Diagnostic(Rules.NestedMonadChain)
               .WithLocation(0)
               .WithArguments("Map"));

    [Fact]
    public Task FlagsANestedResultChain() =>
        Verify.AnalyzerAsync<NestedChainAnalyzer>(
            """
            internal Result<int, string> Chain(
                Result<int, string> result,
                Result<int, string> next) =>
                result.AndThen(value => next.{|#0:Map|}(inner => inner + value));
            """,
            Verify.Diagnostic(Rules.NestedMonadChain)
               .WithLocation(0)
               .WithArguments("Map"));

    [Fact]
    public Task FlagsAChainInsideAMatchDelegate() =>
        Verify.AnalyzerAsync<NestedChainAnalyzer>(
            """
            internal int Chain(Option<int> option, Option<int> next) =>
                option.Match(
                    value => next.{|#0:Map|}(inner => inner + value).UnwrapOr(0),
                    () => 0);
            """,
            Verify.Diagnostic(Rules.NestedMonadChain)
               .WithLocation(0)
               .WithArguments("Map"));

    [Fact]
    public Task FlagsAChainInsideAnAsynchronousDelegate() =>
        Verify.AnalyzerAsync<NestedChainAnalyzer>(
            """
            internal ValueTask<Option<int>> Chain(
                Option<int> option,
                Option<int> next) =>
                option.AndThenAsync(
                    value => new ValueTask<Option<int>>(
                        next.{|#0:Map|}(inner => inner + value)));
            """,
            Verify.Diagnostic(Rules.NestedMonadChain)
               .WithLocation(0)
               .WithArguments("Map"));

    /// <summary>
    /// The binder's delegate is a chain delegate too, so binding the state rather
    /// than capturing it does not flatten anything.
    /// </summary>
    [Fact]
    public Task FlagsAChainInsideABinderDelegate() =>
        Verify.AnalyzerAsync<NestedChainAnalyzer>(
            """
            internal Option<int> Chain(
                Option<int> option,
                Option<int> next,
                int offset) =>
                option.With(offset)
                   .AndThen(
                        (value, state) =>
                            next.{|#0:Map|}(inner => inner + value + state));
            """,
            Verify.Diagnostic(Rules.NestedMonadChain)
               .WithLocation(0)
               .WithArguments("Map"));

    /// <summary>
    /// One pyramid is one diagnostic, on the call that opens it.
    /// </summary>
    [Fact]
    public Task FlagsOnlyTheShallowestCallOfAPyramid() =>
        Verify.AnalyzerAsync<NestedChainAnalyzer>(
            """
            internal Option<int> Chain(
                Option<int> first,
                Option<int> second,
                Option<int> third) =>
                first.AndThen(
                    one => second.{|#0:AndThen|}(
                        two => third.Map(three => one + two + three)));
            """,
            Verify.Diagnostic(Rules.NestedMonadChain)
               .WithLocation(0)
               .WithArguments("AndThen"));

    /// <summary>
    /// A run of calls inside one delegate is one nested region, so the call that
    /// opens the run reports and the rest of the run does not.
    /// </summary>
    [Fact]
    public Task FlagsOnlyTheFirstCallOfANestedRun() =>
        Verify.AnalyzerAsync<NestedChainAnalyzer>(
            """
            internal Option<int> Chain(Option<int> option, Option<int> next) =>
                option.AndThen(
                    value => next.{|#0:Map|}(inner => inner + value)
                       .Filter(inner => inner > 0));
            """,
            Verify.Diagnostic(Rules.NestedMonadChain)
               .WithLocation(0)
               .WithArguments("Map"));

    /// <summary>
    /// Two nested regions under one call are two problems, not one.
    /// </summary>
    [Fact]
    public Task FlagsEachDelegateOfAMatchSeparately() =>
        Verify.AnalyzerAsync<NestedChainAnalyzer>(
            """
            internal int Chain(Option<int> option, Option<int> next) =>
                option.Match(
                    value => next.{|#0:Map|}(inner => inner + value).UnwrapOr(0),
                    () => next.{|#1:Map|}(inner => inner + 1).UnwrapOr(0));
            """,
            Verify.Diagnostic(Rules.NestedMonadChain)
               .WithLocation(0)
               .WithArguments("Map"),
            Verify.Diagnostic(Rules.NestedMonadChain)
               .WithLocation(1)
               .WithArguments("Map"));

    [Fact]
    public Task DoesNotFlagAFlatChain() =>
        Verify.NoDiagnosticAsync<NestedChainAnalyzer>(
            """
            internal int Chain(Option<int> option) =>
                option.Filter(value => value > 0)
                   .Map(value => value + 1)
                   .Or(Option.Some(2))
                   .UnwrapOr(0);
            """);

    /// <summary>
    /// Only a monad's own delegate adds depth. A chain inside anything else is as
    /// shallow as one at statement level.
    /// </summary>
    [Fact]
    public Task DoesNotFlagAChainInsideAnOrdinaryDelegate() =>
        Verify.NoDiagnosticAsync<NestedChainAnalyzer>(
            """
            internal void Register(Option<int> option) =>
                Accept(() => option.Map(value => value + 1));

            private static void Accept(Func<Option<int>> factory)
            {
            }
            """);

    [Fact]
    public Task DoesNotFlagANestedCallOnSomethingOtherThanAMonad() =>
        Verify.NoDiagnosticAsync<NestedChainAnalyzer>(
            """
            internal Option<string> Chain(Option<string> option) =>
                option.Map(value => value.Trim().ToUpperInvariant());
            """);

    /// <summary>
    /// The call that produces the nested monad is not itself a chain call, so the
    /// depth it sits at is the delegate's rather than its own.
    /// </summary>
    [Fact]
    public Task DoesNotFlagAFactoryCallInsideADelegate() =>
        Verify.NoDiagnosticAsync<NestedChainAnalyzer>(
            """
            internal Option<int> Chain(Option<int> option) =>
                option.AndThen(value => Option.Some(value + 1));
            """);

    /// <summary>
    /// A nested query expression desugars into a SelectMany call inside another
    /// SelectMany's delegate, which is a nested chain the author did not write.
    /// </summary>
    [Fact]
    public Task DoesNotFlagANestedQueryExpression() =>
        Verify.NoDiagnosticAsync<NestedChainAnalyzer>(
            """
            internal static class Queries
            {
                public static Option<TResult> SelectMany<T, TCollection, TResult>(
                    this Option<T> source,
                    Func<T, Option<TCollection>> selector,
                    Func<T, TCollection, TResult> project)
                    where T : notnull
                    where TCollection : notnull
                    where TResult : notnull
                {
                    if (source is not Some<T>(var value))
                    {
                        return Option.None<TResult>();
                    }

                    return selector(value) is Some<TCollection>(var inner)
                        ? Option.Some(project(value, inner))
                        : Option.None<TResult>();
                }
            }

            internal class Subject
            {
                internal Option<int> Query(
                    Option<int> first,
                    Option<int> second,
                    Option<int> third) =>
                    from x in first
                    from y in (
                        from a in second
                        from b in third
                        select a + b)
                    select x + y;
            }
            """);

    [Fact]
    public Task DoesNotFlagOnceTheThresholdIsRaised() =>
        Verify.ConfiguredAnalyzerAsync<NestedChainAnalyzer>(
            """
            internal Option<int> Chain(Option<int> option, Option<int> next) =>
                option.AndThen(value => next.Map(inner => inner + value));
            """,
            DepthOfThree);

    [Fact]
    public Task FlagsAtTheConfiguredThreshold() =>
        Verify.ConfiguredAnalyzerAsync<NestedChainAnalyzer>(
            """
            internal Option<int> Chain(
                Option<int> first,
                Option<int> second,
                Option<int> third) =>
                first.AndThen(
                    one => second.AndThen(
                        two => third.{|#0:Map|}(three => one + two + three)));
            """,
            DepthOfThree,
            Verify.Diagnostic(Rules.NestedMonadChain)
               .WithLocation(0)
               .WithArguments("Map"));

    /// <summary>
    /// A consumer's typo falls back to the default rather than throwing out of the
    /// analyzer, which would take their build down.
    /// </summary>
    [Fact]
    public Task FallsBackToTheDefaultGivenAnUnparseableThreshold() =>
        Verify.ConfiguredAnalyzerAsync<NestedChainAnalyzer>(
            """
            internal Option<int> Chain(Option<int> option, Option<int> next) =>
                option.AndThen(value => next.{|#0:Map|}(inner => inner + value));
            """,
            """
            [*.cs]
            dotnet_code_quality.WM2025.max_chain_depth = deep
            """,
            Verify.Diagnostic(Rules.NestedMonadChain)
               .WithLocation(0)
               .WithArguments("Map"));

    /// <summary>
    /// A threshold of zero would report every call in the compilation, so it is read
    /// as unset rather than obeyed.
    /// </summary>
    [Fact]
    public Task FallsBackToTheDefaultGivenANonPositiveThreshold() =>
        Verify.ConfiguredAnalyzerAsync<NestedChainAnalyzer>(
            """
            internal Option<int> Chain(Option<int> option, Option<int> next) =>
                option.AndThen(value => next.{|#0:Map|}(inner => inner + value));
            """,
            """
            [*.cs]
            dotnet_code_quality.WM2025.max_chain_depth = 0
            """,
            Verify.Diagnostic(Rules.NestedMonadChain)
               .WithLocation(0)
               .WithArguments("Map"));

    /// <summary>
    /// The option is read per syntax tree, so a section that does not match the
    /// source leaves the default in place.
    /// </summary>
    [Fact]
    public Task IgnoresAThresholdSetForAnotherFileType() =>
        Verify.ConfiguredAnalyzerAsync<NestedChainAnalyzer>(
            """
            internal Option<int> Chain(Option<int> option, Option<int> next) =>
                option.AndThen(value => next.{|#0:Map|}(inner => inner + value));
            """,
            """
            [*.vb]
            dotnet_code_quality.WM2025.max_chain_depth = 3
            """,
            Verify.Diagnostic(Rules.NestedMonadChain)
               .WithLocation(0)
               .WithArguments("Map"));
}
