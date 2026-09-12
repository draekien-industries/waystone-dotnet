namespace Waystone.Monads.Analyzers;

using System.Threading.Tasks;
using Xunit;

public class StateOverloadAnalyzerTests
{
    [Fact]
    public Task FlagsMapCapturingAParameter() =>
        Verify.AnalyzerAsync<StateOverloadAnalyzer>(
            """
            internal Option<int> Shift(Option<int> option, int offset) =>
                option.{|#0:Map|}(value => value + offset);
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("Map", "offset"));

    [Fact]
    public Task FlagsMapCapturingALocal() =>
        Verify.AnalyzerAsync<StateOverloadAnalyzer>(
            """
            internal Option<int> Shift(Option<int> option)
            {
                var offset = 5;

                return option.{|#0:Map|}(value => value + offset);
            }
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("Map", "offset"));

    [Fact]
    public Task FlagsFilterCapturingAParameter() =>
        Verify.AnalyzerAsync<StateOverloadAnalyzer>(
            """
            internal Option<int> Big(Option<int> option, int limit) =>
                option.{|#0:Filter|}(value => value > limit);
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("Filter", "limit"));

    [Fact]
    public Task FlagsAndThenCapturingAParameter() =>
        Verify.AnalyzerAsync<StateOverloadAnalyzer>(
            """
            internal Option<int> Chain(Option<int> option, int offset) =>
                option.{|#0:AndThen|}(value => Option.Some(value + offset));
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("AndThen", "offset"));

    [Fact]
    public Task FlagsMapErrCapturingAParameter() =>
        Verify.AnalyzerAsync<StateOverloadAnalyzer>(
            """
            internal Result<int, string> Tag(
                Result<int, string> result,
                string prefix) =>
                result.{|#0:MapErr|}(error => prefix + error);
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("MapErr", "prefix"));

    /// <remarks>
    /// MapOrElse takes two delegates and the state overload hands one state to
    /// both, so a capture in either is enough to report.
    /// </remarks>
    [Fact]
    public Task FlagsMapOrElseWhenOnlyTheDefaultCaptures() =>
        Verify.AnalyzerAsync<StateOverloadAnalyzer>(
            """
            internal int Read(Option<int> option, int fallback) =>
                option.{|#0:MapOrElse|}(() => fallback, value => value);
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("MapOrElse", "fallback"));

    [Fact]
    public Task FlagsOptionTryCapturingAParameter() =>
        Verify.AnalyzerAsync<StateOverloadAnalyzer>(
            """
            internal Option<int> Parse(string text) =>
                Option.{|#0:Try|}(() => int.Parse(text));
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("Try", "text"));

    [Fact]
    public Task FlagsResultTryCapturingAParameter() =>
        Verify.AnalyzerAsync<StateOverloadAnalyzer>(
            """
            internal Result<int, Error> Parse(string text) =>
                Result.{|#0:Try|}(() => int.Parse(text));
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("Try", "text"));

    [Fact]
    public Task ListsEveryCapturedName() =>
        Verify.AnalyzerAsync<StateOverloadAnalyzer>(
            """
            internal Option<int> Shift(
                Option<int> option,
                int first,
                int second) =>
                option.{|#0:Map|}(value => value + first + second);
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("Map", "first', 'second"));

    [Fact]
    public Task ReportsACapturedNameOnce() =>
        Verify.AnalyzerAsync<StateOverloadAnalyzer>(
            """
            internal Option<int> Shift(Option<int> option, int offset) =>
                option.{|#0:Map|}(value => value + offset + offset);
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("Map", "offset"));

    [Fact]
    public Task FlagsAnAnonymousMethodThatCaptures() =>
        Verify.AnalyzerAsync<StateOverloadAnalyzer>(
            """
            internal Option<int> Shift(Option<int> option, int offset) =>
                option.{|#0:Map|}(
                    delegate(int value) { return value + offset; });
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("Map", "offset"));

    [Fact]
    public Task IgnoresALambdaThatCapturesNothing() =>
        Verify.NoDiagnosticAsync<StateOverloadAnalyzer>(
            """
            internal Option<int> Doubled(Option<int> option) =>
                option.Map(value => value * 2);
            """);

    [Fact]
    public Task IgnoresAStaticLambda() =>
        Verify.NoDiagnosticAsync<StateOverloadAnalyzer>(
            """
            internal Option<int> Doubled(Option<int> option) =>
                option.Map(static value => value * 2);
            """);

    [Fact]
    public Task IgnoresALocalDeclaredInsideTheLambda() =>
        Verify.NoDiagnosticAsync<StateOverloadAnalyzer>(
            """
            internal Option<int> Doubled(Option<int> option) =>
                option.Map(value =>
                {
                    var twice = value * 2;

                    return twice;
                });
            """);

    [Fact]
    public Task IgnoresACallAlreadyOnTheStateOverload() =>
        Verify.NoDiagnosticAsync<StateOverloadAnalyzer>(
            """
            internal Option<int> Shift(Option<int> option, int offset) =>
                option.Map(offset, (value, state) => value + state);
            """);

    /// <remarks>
    /// Capturing <c>this</c> allocates a delegate rather than a display class,
    /// and firing on it would hit most ordinary instance-method code. Both the
    /// field read and the instance call reach <c>this</c> the same way.
    /// </remarks>
    [Fact]
    public Task IgnoresALambdaThatOnlyReadsAField() =>
        Verify.NoDiagnosticAsync<StateOverloadAnalyzer>(
            """
            internal class Subject
            {
                private readonly int _offset = 5;

                internal Option<int> Shift(Option<int> option) =>
                    option.Map(value => value + _offset);
            }
            """);

    [Fact]
    public Task IgnoresALambdaThatOnlyCallsAnInstanceMethod() =>
        Verify.NoDiagnosticAsync<StateOverloadAnalyzer>(
            """
            internal class Subject
            {
                private int Compute(int value) => value * 2;

                internal Option<int> Doubled(Option<int> option) =>
                    option.Map(value => Compute(value));
            }
            """);

    [Fact]
    public Task FlagsResultAndThenCapturingAParameter() =>
        Verify.AnalyzerAsync<StateOverloadAnalyzer>(
            """
            internal Result<int, string> Chain(
                Result<int, string> result,
                int offset) =>
                result.{|#0:AndThen|}(
                    value => Result.Ok<int, string>(value + offset));
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("AndThen", "offset"));

    /// <remarks>
    /// Match is the reach DRA-108 added, and the case worth the most: both
    /// branches capture, so one display class and two delegates are allocated
    /// where the other members allocate one of each. The captured names are
    /// reported once each and in source order, not once per branch.
    /// </remarks>
    [Fact]
    public Task FlagsMatchWhenBothBranchesCapture() =>
        Verify.AnalyzerAsync<StateOverloadAnalyzer>(
            """
            internal int Fold(Option<int> option, int offset, int fallback) =>
                option.{|#0:Match|}(
                    value => value + offset,
                    () => fallback);
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("Match", "offset', 'fallback"));

    /// <remarks>
    /// The Result side of the reach DRA-108 added. Result.Match is worth its
    /// own case because both of its branches receive a value, so neither
    /// delegate is the argument-free shape the Option pair has, and a rule that
    /// keyed on arity rather than on the state type parameter would miss it.
    /// </remarks>
    [Fact]
    public Task FlagsResultMatchWhenBothBranchesCapture() =>
        Verify.AnalyzerAsync<StateOverloadAnalyzer>(
            """
            internal int Fold(
                Result<int, string> result,
                int offset,
                int fallback) =>
                result.{|#0:Match|}(
                    value => value + offset,
                    error => fallback);
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("Match", "offset', 'fallback"));

    /// <remarks>
    /// ZipWith takes a delegate on a type that carries state overloads on other
    /// methods, and has none of its own. This pins the containing-type lookup:
    /// a rule that fired because the receiver has state overloads somewhere
    /// would name an overload that does not exist.
    /// ZipWith and Reduce are the pin because DRA-108 declined them
    /// permanently — both delegates get every operand from the call, so there
    /// is nothing to capture and a state parameter would be one callers pass
    /// null to. Inspect held this test until DRA-108 gave it a state overload.
    /// </remarks>
    [Fact]
    public Task IgnoresZipWithWhichHasNoStateOverload() =>
        Verify.NoDiagnosticAsync<StateOverloadAnalyzer>(
            """
            internal Option<int> Combine(
                Option<int> option,
                Option<int> other,
                int offset) =>
                option.ZipWith(
                    other,
                    (value, otherValue) => value + otherValue + offset);
            """);

    [Fact]
    public Task IgnoresAStateOverloadOnATypeOutsideTheLibrary() =>
        Verify.NoDiagnosticAsync<StateOverloadAnalyzer>(
            """
            internal class Mine
            {
                internal int Map(Func<int, int> map) => map(1);

                internal int Map<TState>(
                    TState state,
                    Func<int, TState, int> map) =>
                    map(1, state);
            }

            internal class Subject
            {
                internal int Use(Mine mine, int offset) =>
                    mine.Map(value => value + offset);
            }
            """);

    /// <remarks>
    /// The reach the binder gate adds. No *Async member of Option or Result has
    /// ever had a state overload, so the previous gate — does an overload of
    /// this name take a TState — was false for every one of them and a
    /// capturing asynchronous lambda was never reported. This case and the
    /// Result one below are the whole point of the regate; the sync cases above
    /// pass either way.
    /// </remarks>
    [Fact]
    public Task FlagsMapAsyncCapturingAParameter() =>
        Verify.AnalyzerAsync<StateOverloadAnalyzer>(
            """
            internal ValueTask<Option<int>> Shift(
                Option<int> option,
                int offset) =>
                option.{|#0:MapAsync|}(
                    async value =>
                    {
                        await Task.Yield();
                        return value + offset;
                    });
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("MapAsync", "offset"));

    [Fact]
    public Task FlagsResultMapAsyncCapturingAParameter() =>
        Verify.AnalyzerAsync<StateOverloadAnalyzer>(
            """
            internal ValueTask<Result<int, string>> Shift(
                Result<int, string> result,
                int offset) =>
                result.{|#0:MapAsync|}(
                    async value =>
                    {
                        await Task.Yield();
                        return value + offset;
                    });
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("MapAsync", "offset"));

    /// <remarks>
    /// Reduce sits beside ZipWith as the second member the binder declines
    /// permanently, and the pair is worth pinning separately: ZipWith would
    /// keep passing if the gate matched on arity, while Reduce takes two
    /// operands of the same type and would not.
    /// </remarks>
    [Fact]
    public Task IgnoresReduceWhichTheBinderDoesNotDeclare() =>
        Verify.NoDiagnosticAsync<StateOverloadAnalyzer>(
            """
            internal Option<int> Fold(
                Option<int> option,
                Option<int> other,
                int offset) =>
                option.Reduce(
                    other,
                    (value, otherValue) => value + otherValue + offset);
            """);

    /// <remarks>
    /// An awaited receiver reaches the library through an extension member, so
    /// the containing type is the extension grouping type and no binder
    /// resolves. Silence is the correct answer rather than a gap: With is
    /// declared on Option&lt;T&gt; and not on Task&lt;Option&lt;T&gt;&gt;, so
    /// there is no rewrite to name. Pinned because the state overloads on these
    /// members take a synchronous delegate, which an asynchronous lambda cannot
    /// be passed to — a gate that reached them would advise the impossible.
    /// </remarks>
    [Fact]
    public Task IgnoresAnAwaitedReceiverWhichHasNoWith() =>
        Verify.NoDiagnosticAsync<StateOverloadAnalyzer>(
            """
            internal ValueTask<Option<int>> Shift(
                Task<Option<int>> source,
                int offset) =>
                source.MapAsync(
                    async value =>
                    {
                        await Task.Yield();
                        return value + offset;
                    });
            """);

    /// <remarks>
    /// MapOrNull was the one delegate-taking member of Option with neither a
    /// state overload nor a binder member, and was pinned as silent for that
    /// reason. DRA-211 gave it both, so the silence would now be the gate
    /// misfiring rather than a known gap. The rule reads its member set from the
    /// binder, so this began reporting without the analyzer being touched —
    /// which is the property worth pinning on the way back.
    /// </remarks>
    [Fact]
    public Task FlagsMapOrNullCapturingAParameter() =>
        Verify.AnalyzerAsync<StateOverloadAnalyzer>(
            """
            internal int? Read(Option<int> option, int offset) =>
                option.{|#0:MapOrNull|}(value => value + offset);
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("MapOrNull", "offset"));

    [Fact]
    public Task FlagsMapOrNullOnAResultCapturingAParameter() =>
        Verify.AnalyzerAsync<StateOverloadAnalyzer>(
            """
            internal int? Read(Result<int, string> result, int offset) =>
                result.{|#0:MapOrNull|}(value => value + offset);
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("MapOrNull", "offset"));
}
