namespace Waystone.Monads.Analyzers;

using System.Threading.Tasks;
using Xunit;

/// <remarks>
/// The bail-out cases matter more than the rewrites. A code fix that produces
/// source which does not compile is worse than no fix at all, because a
/// consumer accepts it from a lightbulb without reading it. Every test here
/// compiles its fixed state — <c>Verify.CodeFixAsync</c> fails on a compiler
/// error in the output — so a rewrite that loses a parameter or misnames a
/// tuple member fails rather than merely reading oddly.
/// </remarks>
public class UseStateBindingCodeFixTests
{
    [Fact]
    public Task BindsASingleCapturedParameter() =>
        Verify.CodeFixAsync<StateOverloadAnalyzer, UseStateBindingCodeFix>(
            """
            internal Option<int> Shift(Option<int> option, int offset) =>
                option.{|#0:Map|}(value => value + offset);
            """,
            """
            internal Option<int> Shift(Option<int> option, int offset) =>
                option.With(offset).Map(static (value, state) => value + state);
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("Map", "offset"));

    [Fact]
    public Task BindsOnTheResultSide() =>
        Verify.CodeFixAsync<StateOverloadAnalyzer, UseStateBindingCodeFix>(
            """
            internal Result<int, string> Tag(
                Result<int, string> result,
                string prefix) =>
                result.{|#0:MapErr|}(error => prefix + error);
            """,
            """
            internal Result<int, string> Tag(
                Result<int, string> result,
                string prefix) =>
                result.With(prefix).MapErr(static (error, state) => state + error);
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("MapErr", "prefix"));

    /// <remarks>
    /// The delegate on this branch receives the state alone, because there is no
    /// value to hand it. A rewrite that added the state as a second parameter
    /// here would not compile.
    /// </remarks>
    [Fact]
    public Task BindsABranchThatTakesNoValue() =>
        Verify.CodeFixAsync<StateOverloadAnalyzer, UseStateBindingCodeFix>(
            """
            internal int Read(Option<int> option, int fallback) =>
                option.{|#0:UnwrapOrElse|}(() => fallback);
            """,
            """
            internal int Read(Option<int> option, int fallback) =>
                option.With(fallback).UnwrapOrElse(static (state) => state);
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("UnwrapOrElse", "fallback"));

    /// <remarks>
    /// Both delegates take the state even though only one reads it, because the
    /// binder hands the same value to each. The unused one has to declare the
    /// parameter and ignore it.
    /// </remarks>
    [Fact]
    public Task BindsBothDelegatesWhenOnlyOneCaptures() =>
        Verify.CodeFixAsync<StateOverloadAnalyzer, UseStateBindingCodeFix>(
            """
            internal int Read(Option<int> option, int fallback) =>
                option.{|#0:MapOrElse|}(() => fallback, value => value);
            """,
            """
            internal int Read(Option<int> option, int fallback) =>
                option.With(fallback).MapOrElse(static (state) => state, static (value, state) => value);
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("MapOrElse", "fallback"));

    /// <remarks>
    /// Two captures become a tuple, and C# infers the member names from the
    /// variable names, so nothing has to be invented — the rewrite reads
    /// <c>state.offset</c> because the argument was written <c>offset</c>.
    /// </remarks>
    [Fact]
    public Task BindsTwoCapturesAsATuple() =>
        Verify.CodeFixAsync<StateOverloadAnalyzer, UseStateBindingCodeFix>(
            """
            internal int Fold(Option<int> option, int offset, int fallback) =>
                option.{|#0:Match|}(value => value + offset, () => fallback);
            """,
            """
            internal int Fold(Option<int> option, int offset, int fallback) =>
                option.With((offset, fallback)).Match(static (value, state) => value + state.offset, static (state) => state.fallback);
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("Match", "offset', 'fallback"));

    /// <remarks>
    /// The asynchronous reach the binder gate opened. The rewrite has to keep
    /// the <c>async</c> modifier, and <c>static</c> goes in front of it.
    /// </remarks>
    [Fact]
    public Task BindsAnAsynchronousDelegate() =>
        Verify.CodeFixAsync<StateOverloadAnalyzer, UseStateBindingCodeFix>(
            """
            internal ValueTask<Option<int>> Shift(
                Option<int> option,
                int offset) =>
                option.{|#0:MapAsync|}(
                    async value => await Task.FromResult(value + offset));
            """,
            """
            internal ValueTask<Option<int>> Shift(
                Option<int> option,
                int offset) =>
                option.With(offset).MapAsync(
                    static async (value, state) => await Task.FromResult(value + state));
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("MapAsync", "offset"));

    /// <remarks>
    /// The static factories bind through a factory of their own rather than
    /// through the extension, so this rewrite needs no import — <c>Option</c> is
    /// already named at the call site.
    /// </remarks>
    [Fact]
    public Task BindsAFactoryCall() =>
        Verify.CodeFixAsync<StateOverloadAnalyzer, UseStateBindingCodeFix>(
            """
            internal Option<int> Parse(string text) =>
                Option.{|#0:Try|}(() => int.Parse(text));
            """,
            """
            internal Option<int> Parse(string text) =>
                Option.With(text).Try(static (state) => int.Parse(state));
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("Try", "text"));

    /// <remarks>
    /// <c>With</c> is an extension, so unlike every other fix in this assembly
    /// this one cannot fall back to a qualified name — the rewrite does not
    /// compile without the import. The wrapped tests above never catch a missing
    /// one, because the harness prepends that using to every source it wraps.
    /// </remarks>
    [Fact]
    public Task ImportsTheExtensionsNamespaceWhenItIsMissing() =>
        Verify.RawCodeFixAsync<StateOverloadAnalyzer, UseStateBindingCodeFix>(
            """
            internal class Subject
            {
                internal Waystone.Monads.Options.Option<int> Shift(
                    Waystone.Monads.Options.Option<int> option,
                    int offset) =>
                    option.{|#0:Map|}(value => value + offset);
            }
            """,
            """
            using Waystone.Monads.Options.Extensions;

            internal class Subject
            {
                internal Waystone.Monads.Options.Option<int> Shift(
                    Waystone.Monads.Options.Option<int> option,
                    int offset) =>
                    option.With(offset).Map(static (value, state) => value + state);
            }
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("Map", "offset"));

    /// <remarks>
    /// A local already called <c>state</c> is itself a capture, so it joins the
    /// tuple and the new parameter takes the next free name rather than
    /// shadowing it.
    /// </remarks>
    [Fact]
    public Task NamesTheParameterAroundAnExistingState() =>
        Verify.CodeFixAsync<StateOverloadAnalyzer, UseStateBindingCodeFix>(
            """
            internal Option<int> Shift(Option<int> option, int offset)
            {
                var state = 1;

                return option.{|#0:Map|}(value => value + offset + state);
            }
            """,
            """
            internal Option<int> Shift(Option<int> option, int offset)
            {
                var state = 1;

                return option.With((offset, state)).Map(static (value, state1) => value + state1.offset + state1.state);
            }
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("Map", "offset', 'state"));

    /// <remarks>
    /// A method group cannot grow a parameter, and the binder's delegate needs
    /// one, so there is nothing to offer. Reported and left alone.
    /// </remarks>
    [Fact]
    public Task DeclinesAMethodGroupArgument() =>
        Verify.CodeFixAsync<StateOverloadAnalyzer, UseStateBindingCodeFix>(
            """
            internal int Read(Option<int> option, int fallback) =>
                option.{|#0:MapOrElse|}(() => fallback, Identity);

            internal static int Identity(int value) => value;
            """,
            """
            internal int Read(Option<int> option, int fallback) =>
                option.{|#0:MapOrElse|}(() => fallback, Identity);

            internal static int Identity(int value) => value;
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("MapOrElse", "fallback"));

    /// <remarks>
    /// One delegate declares a parameter with a captured variable's name, which
    /// C# 8 made legal. Rewriting around it would work here and not in the next
    /// case, so the fix declines both rather than deciding which shadow is safe.
    /// </remarks>
    [Fact]
    public Task DeclinesACaptureShadowedByALambdaParameter() =>
        Verify.CodeFixAsync<StateOverloadAnalyzer, UseStateBindingCodeFix>(
            """
            internal int Fold(Option<int> option, int offset) =>
                option.{|#0:Match|}(offset => offset, () => offset);
            """,
            """
            internal int Fold(Option<int> option, int offset) =>
                option.{|#0:Match|}(offset => offset, () => offset);
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("Match", "offset"));

    /// <remarks>
    /// <c>Rest</c> cannot be a tuple member name, so the inferred tuple would
    /// not carry it and <c>state.Rest</c> would not compile. A single capture of
    /// the same name is fine, because it is passed as itself rather than as a
    /// tuple member.
    /// </remarks>
    [Fact]
    public Task DeclinesACaptureThatCannotNameATupleMember() =>
        Verify.CodeFixAsync<StateOverloadAnalyzer, UseStateBindingCodeFix>(
            """
            internal int Fold(Option<int> option, int Rest, int fallback) =>
                option.{|#0:Match|}(value => value + Rest, () => fallback);
            """,
            """
            internal int Fold(Option<int> option, int Rest, int fallback) =>
                option.{|#0:Match|}(value => value + Rest, () => fallback);
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("Match", "Rest', 'fallback"));
}
