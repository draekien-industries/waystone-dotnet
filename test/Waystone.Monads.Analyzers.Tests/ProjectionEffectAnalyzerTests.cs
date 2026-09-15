namespace Waystone.Monads.Analyzers;

using System.Threading.Tasks;
using Xunit;

public class ProjectionEffectAnalyzerTests
{
    [Fact]
    public Task FlagsAnIncrementOfACapturedLocal() =>
        Verify.AnalyzerAsync<ProjectionEffectAnalyzer>(
            """
            internal Option<int> Project(Option<int> option)
            {
                int seen = 0;

                return option.{|#0:Map|}(
                    value =>
                    {
                        seen++;

                        return value + seen;
                    });
            }
            """,
            Verify.Diagnostic(Rules.EffectInsideProjection)
               .WithLocation(0)
               .WithArguments("Map", "seen"));

    [Fact]
    public Task FlagsAnAssignmentToAField() =>
        Verify.AnalyzerAsync<ProjectionEffectAnalyzer>(
            """
            private int _lastSeen;

            internal Option<int> Project(Option<int> option) =>
                option.{|#0:Map|}(
                    value =>
                    {
                        _lastSeen = value;

                        return value + 1;
                    });
            """,
            Verify.Diagnostic(Rules.EffectInsideProjection)
               .WithLocation(0)
               .WithArguments("Map", "_lastSeen"));

    /// <summary>
    /// The sharpest case: the predicate runs only for a value that is present, so
    /// the effect is conditional on what it is testing.
    /// </summary>
    [Fact]
    public Task FlagsACollectionMutationInsideAPredicate() =>
        Verify.AnalyzerAsync<ProjectionEffectAnalyzer>(
            """
            private readonly System.Collections.Generic.List<int> _audit = new();

            internal Option<int> Project(Option<int> option) =>
                option.{|#0:Filter|}(
                    value =>
                    {
                        _audit.Add(value);

                        return value > 0;
                    });
            """,
            Verify.Diagnostic(Rules.EffectInsideProjection)
               .WithLocation(0)
               .WithArguments("Filter", "_audit"));

    [Fact]
    public Task FlagsACompoundAssignmentInsideAndThen() =>
        Verify.AnalyzerAsync<ProjectionEffectAnalyzer>(
            """
            private int _count;

            internal Option<int> Project(Option<int> option) =>
                option.{|#0:AndThen|}(
                    value =>
                    {
                        _count += 1;

                        return Option.Some(value);
                    });
            """,
            Verify.Diagnostic(Rules.EffectInsideProjection)
               .WithLocation(0)
               .WithArguments("AndThen", "_count"));

    [Fact]
    public Task FlagsTheLazyFallbackOfAnEagerPair() =>
        Verify.AnalyzerAsync<ProjectionEffectAnalyzer>(
            """
            private int _misses;

            internal int Project(Option<int> option) =>
                option.{|#0:UnwrapOrElse|}(
                    () =>
                    {
                        _misses++;

                        return 0;
                    });
            """,
            Verify.Diagnostic(Rules.EffectInsideProjection)
               .WithLocation(0)
               .WithArguments("UnwrapOrElse", "_misses"));

    [Fact]
    public Task FlagsAResultProjection() =>
        Verify.AnalyzerAsync<ProjectionEffectAnalyzer>(
            """
            private bool _handled;

            internal Result<int, string> Project(Result<int, string> result) =>
                result.{|#0:Map|}(
                    value =>
                    {
                        _handled = true;

                        return value + 1;
                    });
            """,
            Verify.Diagnostic(Rules.EffectInsideProjection)
               .WithLocation(0)
               .WithArguments("Map", "_handled"));

    /// <summary>
    /// The value-returning delegate of Match is a projection like any other.
    /// </summary>
    [Fact]
    public Task FlagsTheValueReturningOverloadOfMatch() =>
        Verify.AnalyzerAsync<ProjectionEffectAnalyzer>(
            """
            private int _seen;

            internal int Project(Option<int> option) =>
                option.{|#0:Match|}(
                    value =>
                    {
                        _seen++;

                        return value;
                    },
                    () => 0);
            """,
            Verify.Diagnostic(Rules.EffectInsideProjection)
               .WithLocation(0)
               .WithArguments("Match", "_seen"));

    /// <summary>
    /// One delegate carrying three effects is one decision to undo, so the names
    /// share a diagnostic rather than each taking one.
    /// </summary>
    [Fact]
    public Task NamesEveryMutatedSymbolInOneDiagnostic() =>
        Verify.AnalyzerAsync<ProjectionEffectAnalyzer>(
            """
            private int _count;

            private int _total;

            internal Option<int> Project(Option<int> option) =>
                option.{|#0:Map|}(
                    value =>
                    {
                        _count++;
                        _total += value;

                        return value;
                    });
            """,
            Verify.Diagnostic(Rules.EffectInsideProjection)
               .WithLocation(0)
               .WithArguments("Map", "_count', '_total"));

    [Fact]
    public Task FlagsAMutationReachedThroughACapturedObject() =>
        Verify.AnalyzerAsync<ProjectionEffectAnalyzer>(
            """
            internal class Tally
            {
                internal int Count { get; set; }
            }

            internal class Subject
            {
                private readonly Tally _tally = new();

                internal Option<int> Project(Option<int> option) =>
                    option.{|#0:Map|}(
                        value =>
                        {
                            _tally.Count = value;

                            return value;
                        });
            }
            """,
            Verify.Diagnostic(Rules.EffectInsideProjection)
               .WithLocation(0)
               .WithArguments("Map", "Count"));

    [Fact]
    public Task DoesNotFlagInspect() =>
        Verify.NoDiagnosticAsync<ProjectionEffectAnalyzer>(
            """
            private int _seen;

            internal Option<int> Project(Option<int> option) =>
                option.Inspect(value => _seen = value);
            """);

    [Fact]
    public Task DoesNotFlagInspectErr() =>
        Verify.NoDiagnosticAsync<ProjectionEffectAnalyzer>(
            """
            private string _failure = string.Empty;

            internal Result<int, string> Project(Result<int, string> result) =>
                result.InspectErr(error => _failure = error);
            """);

    [Fact]
    public Task DoesNotFlagTheActionOverloadOfMatch() =>
        Verify.NoDiagnosticAsync<ProjectionEffectAnalyzer>(
            """
            private int _seen;

            private int _misses;

            internal void Project(Option<int> option) =>
                option.Match(
                    value => { _seen = value; },
                    () => { _misses++; });
            """);

    /// <summary>
    /// An assignment is an expression, so a delegate written as one binds to the
    /// value-returning overload however it reads. The rule follows what the
    /// compiler bound rather than what the line looks like, and the braces that
    /// silence it are the same braces that pick the Action overload.
    /// </summary>
    [Fact]
    public Task FlagsAnExpressionBodiedMatchDelegateThatBindsToTheValueOverload() =>
        Verify.AnalyzerAsync<ProjectionEffectAnalyzer>(
            """
            private int _seen;

            private int _misses;

            internal void Project(Option<int> option) =>
                option.{|#0:Match|}(value => _seen = value, () => _misses++);
            """,
            Verify.Diagnostic(Rules.EffectInsideProjection)
               .WithLocation(0)
               .WithArguments("Match", "_seen', '_misses"));

    [Fact]
    public Task DoesNotFlagALocalDeclaredInsideTheDelegate() =>
        Verify.NoDiagnosticAsync<ProjectionEffectAnalyzer>(
            """
            internal Option<int> Project(Option<int> option) =>
                option.Map(
                    value =>
                    {
                        int total = value;

                        total += 1;

                        return total;
                    });
            """);

    /// <summary>
    /// Whether a method mutates is not decidable, so a delegate that hands its work
    /// to one is never judged.
    /// </summary>
    [Fact]
    public Task DoesNotFlagAnOpaqueCall() =>
        Verify.NoDiagnosticAsync<ProjectionEffectAnalyzer>(
            """
            internal Option<int> Project(Option<int> option) =>
                option.Map(value => Recalculate(value));

            private int Recalculate(int value) => value + 1;
            """);

    [Fact]
    public Task DoesNotFlagAReadOfOutsideState() =>
        Verify.NoDiagnosticAsync<ProjectionEffectAnalyzer>(
            """
            private int _offset;

            internal Option<int> Project(Option<int> option) =>
                option.Map(value => value + _offset);
            """);

    /// <summary>
    /// Whether the projected value can be mutated is settled by its type, so the
    /// delegate's own parameter is out of scope.
    /// </summary>
    [Fact]
    public Task DoesNotFlagAMutationOfTheDelegatesOwnParameter() =>
        Verify.NoDiagnosticAsync<ProjectionEffectAnalyzer>(
            """
            internal class Tally
            {
                internal int Count { get; set; }
            }

            internal class Subject
            {
                internal Option<Tally> Project(Option<Tally> option) =>
                    option.Map(
                        tally =>
                        {
                            tally.Count = 1;

                            return tally;
                        });
            }
            """);

    /// <summary>
    /// A name alone decides nothing: the declaring type has to be a collection.
    /// </summary>
    [Fact]
    public Task DoesNotFlagAnAddThatIsNotACollectionsOwn() =>
        Verify.NoDiagnosticAsync<ProjectionEffectAnalyzer>(
            """
            internal class Ledger
            {
                internal int Add(int value) => value + 1;
            }

            internal class Subject
            {
                private readonly Ledger _ledger = new();

                internal Option<int> Project(Option<int> option) =>
                    option.Map(value => _ledger.Add(value));
            }
            """);

    [Fact]
    public Task DoesNotFlagACollectionDeclaredInsideTheDelegate() =>
        Verify.NoDiagnosticAsync<ProjectionEffectAnalyzer>(
            """
            internal Option<int> Project(Option<int> option) =>
                option.Map(
                    value =>
                    {
                        var seen = new System.Collections.Generic.List<int>();

                        seen.Add(value);

                        return seen.Count;
                    });
            """);

    [Fact]
    public Task DoesNotFlagACallOnSomethingOtherThanAMonad() =>
        Verify.NoDiagnosticAsync<ProjectionEffectAnalyzer>(
            """
            private int _seen;

            internal int Project(System.Collections.Generic.List<int> values) =>
                values.FindIndex(
                    value =>
                    {
                        _seen++;

                        return value > 0;
                    });
            """);
}
