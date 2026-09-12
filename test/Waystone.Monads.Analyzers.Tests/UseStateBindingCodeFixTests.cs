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

    /// <remarks>
    /// MapOrNull reached the fix by gaining a binder member in DRA-211 rather
    /// than by anything changing here, so the rewrite it produces has never been
    /// exercised. Its nullable return is what makes it worth a case of its own:
    /// the fix rewrites the delegate and must leave the <c>?</c> alone.
    /// </remarks>
    [Fact]
    public Task BindsAMemberWithANullableReturn() =>
        Verify.CodeFixAsync<StateOverloadAnalyzer, UseStateBindingCodeFix>(
            """
            internal int? Read(Option<int> option, int offset) =>
                option.{|#0:MapOrNull|}(value => value + offset);
            """,
            """
            internal int? Read(Option<int> option, int offset) =>
                option.With(offset).MapOrNull(static (value, state) => value + state);
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("MapOrNull", "offset"));

    /// <remarks>
    /// Every other member this fix rewrites hands its delegate one value. ZipWith
    /// and Reduce hand it two, and reached the fix in DRA-211 by gaining binder
    /// members rather than by anything changing here — so this is the first
    /// exercise of the two-value shape, and the case where a fix that appended the
    /// state parameter positionally rather than last would emit source that does
    /// not compile.
    /// </remarks>
    [Fact]
    public Task BindsADelegateTakingTwoValues() =>
        Verify.CodeFixAsync<StateOverloadAnalyzer, UseStateBindingCodeFix>(
            """
            internal Option<int> Combine(
                Option<int> option,
                Option<int> other,
                int offset) =>
                option.{|#0:ZipWith|}(
                    other,
                    (value, otherValue) => value + otherValue + offset);
            """,
            """
            internal Option<int> Combine(
                Option<int> option,
                Option<int> other,
                int offset) =>
                option.With(offset).ZipWith(
                    other,
                    static (value, otherValue, state) => value + otherValue + state);
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("ZipWith", "offset"));

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
    /// The first argument is a value rather than a delegate, and it stays where
    /// it is — only the delegate grows a parameter, and only the receiver moves.
    /// </remarks>
    [Fact]
    public Task BindsAMemberWithAValueArgumentBesideTheDelegate() =>
        Verify.CodeFixAsync<StateOverloadAnalyzer, UseStateBindingCodeFix>(
            """
            internal int Read(Option<int> option, int offset, int fallback) =>
                option.{|#0:MapOr|}(fallback, value => value + offset);
            """,
            """
            internal int Read(Option<int> option, int offset, int fallback) =>
                option.With(offset).MapOr(fallback, static (value, state) => value + state);
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("MapOr", "offset"));

    /// <remarks>
    /// Both <c>state</c> and <c>state1</c> are taken, so the search for a free
    /// name has to run past its first candidate. It cannot run out: each name it
    /// rejects is itself one of the names in scope.
    /// </remarks>
    [Fact]
    public Task NamesTheParameterAroundEveryTakenState() =>
        Verify.CodeFixAsync<StateOverloadAnalyzer, UseStateBindingCodeFix>(
            """
            internal Option<int> Shift(Option<int> option, int offset)
            {
                var state = 1;
                var state1 = 2;

                return option.{|#0:Map|}(value => value + offset + state + state1);
            }
            """,
            """
            internal Option<int> Shift(Option<int> option, int offset)
            {
                var state = 1;
                var state1 = 2;

                return option.With((offset, state, state1)).Map(static (value, state2) => value + state2.offset + state2.state + state2.state1);
            }
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("Map", "offset', 'state', 'state1"));

    /// <remarks>
    /// The shadow is a local inside one delegate rather than a parameter of it,
    /// and it belongs to the delegate that does not capture. Rewriting the other
    /// one would compile and would silently change which <c>offset</c> the body
    /// of this one reads, so the fix declines here too.
    /// </remarks>
    [Fact]
    public Task DeclinesACaptureShadowedByALocalInAnotherDelegate() =>
        Verify.CodeFixAsync<StateOverloadAnalyzer, UseStateBindingCodeFix>(
            """
            internal int Fold(Option<int> option, int offset) =>
                option.{|#0:Match|}(
                    value =>
                    {
                        int offset = value;

                        return offset;
                    },
                    () => offset);
            """,
            """
            internal int Fold(Option<int> option, int offset) =>
                option.{|#0:Match|}(
                    value =>
                    {
                        int offset = value;

                        return offset;
                    },
                    () => offset);
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("Match", "offset"));

    /// <remarks>
    /// <c>ItemN</c> names a tuple member only in position <c>N</c>, and the
    /// capture order is the reader's rather than something the fix may reorder.
    /// The companion to the <c>Rest</c> case below, which is barred everywhere.
    /// </remarks>
    [Fact]
    public Task DeclinesACaptureThatCannotNameATupleMemberInItsPosition() =>
        Verify.CodeFixAsync<StateOverloadAnalyzer, UseStateBindingCodeFix>(
            """
            internal int Fold(Option<int> option, int Item2, int fallback) =>
                option.{|#0:Match|}(value => value + Item2, () => fallback);
            """,
            """
            internal int Fold(Option<int> option, int Item2, int fallback) =>
                option.{|#0:Match|}(value => value + Item2, () => fallback);
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("Match", "Item2', 'fallback"));

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

    /// <remarks>
    /// The import lands beside the usings already there, keeping their
    /// indentation and the blank line that followed the last of them. Nothing
    /// formats this node, so the trivia in the output is the trivia the fix
    /// wrote.
    /// </remarks>
    [Fact]
    public Task AppendsTheImportAfterAnExistingUsing() =>
        Verify.RawCodeFixAsync<StateOverloadAnalyzer, UseStateBindingCodeFix>(
            """
            using System;

            internal class Subject
            {
                internal Waystone.Monads.Options.Option<int> Shift(
                    Waystone.Monads.Options.Option<int> option,
                    int offset) =>
                    option.{|#0:Map|}(value => value + offset);
            }
            """,
            """
            using System;
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
    /// A file that puts its usings inside the namespace gets the import there
    /// too, indented to match. Adding it at the top of such a file would compile
    /// and would still be wrong, because the next author would move it.
    /// </remarks>
    [Fact]
    public Task AppendsTheImportInsideANamespaceThatHoldsTheUsings() =>
        Verify.RawCodeFixAsync<StateOverloadAnalyzer, UseStateBindingCodeFix>(
            """
            namespace Consumer
            {
                using System;

                internal class Subject
                {
                    internal Waystone.Monads.Options.Option<int> Shift(
                        Waystone.Monads.Options.Option<int> option,
                        int offset) =>
                        option.{|#0:Map|}(value => value + offset);
                }
            }
            """,
            """
            namespace Consumer
            {
                using System;
                using Waystone.Monads.Options.Extensions;

                internal class Subject
                {
                    internal Waystone.Monads.Options.Option<int> Shift(
                        Waystone.Monads.Options.Option<int> option,
                        int offset) =>
                        option.With(offset).Map(static (value, state) => value + state);
                }
            }
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("Map", "offset"));

    /// <remarks>
    /// A namespace with no usings of its own says nothing about where they go, so
    /// the import takes the file's own list rather than opening one inside the
    /// namespace.
    /// </remarks>
    [Fact]
    public Task ImportsAboveANamespaceThatHoldsNoUsings() =>
        Verify.RawCodeFixAsync<StateOverloadAnalyzer, UseStateBindingCodeFix>(
            """
            namespace Consumer
            {
                internal class Subject
                {
                    internal Waystone.Monads.Options.Option<int> Shift(
                        Waystone.Monads.Options.Option<int> option,
                        int offset) =>
                        option.{|#0:Map|}(value => value + offset);
                }
            }
            """,
            """
            using Waystone.Monads.Options.Extensions;

            namespace Consumer
            {
                internal class Subject
                {
                    internal Waystone.Monads.Options.Option<int> Shift(
                        Waystone.Monads.Options.Option<int> option,
                        int offset) =>
                        option.With(offset).Map(static (value, state) => value + state);
                }
            }
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("Map", "offset"));

    /// <remarks>
    /// The namespace already imports it, so the fix adds nothing. A second
    /// directive would compile, and a fix that leaves one behind on every call it
    /// rewrites is one a consumer stops trusting.
    /// </remarks>
    [Fact]
    public Task AddsNoImportWhenTheNamespaceAlreadyHasIt() =>
        Verify.RawCodeFixAsync<StateOverloadAnalyzer, UseStateBindingCodeFix>(
            """
            namespace Consumer
            {
                using Waystone.Monads.Options.Extensions;

                internal class Subject
                {
                    internal Waystone.Monads.Options.Option<int> Shift(
                        Waystone.Monads.Options.Option<int> option,
                        int offset) =>
                        option.{|#0:Map|}(value => value + offset);
                }
            }
            """,
            """
            namespace Consumer
            {
                using Waystone.Monads.Options.Extensions;

                internal class Subject
                {
                    internal Waystone.Monads.Options.Option<int> Shift(
                        Waystone.Monads.Options.Option<int> option,
                        int offset) =>
                        option.With(offset).Map(static (value, state) => value + state);
                }
            }
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("Map", "offset"));

    /// <remarks>
    /// A file with no line ending anywhere in it still has to get one with the
    /// import, and LF is the ending to pick — the fix has nothing to copy, and a
    /// repository that normalises on commit would rewrite CRLF anyway. Keep the
    /// source on a single line: an end-of-line trivia anywhere in it is one the
    /// fix would copy instead, and this is the only case that has none.
    /// </remarks>
    [Fact]
    public Task ImportsWithLineFeedIntoASourceThatHasNoLineEnding() =>
        Verify.RawCodeFixAsync<StateOverloadAnalyzer, UseStateBindingCodeFix>(
            """
            internal class S { internal Waystone.Monads.Options.Option<int> M(Waystone.Monads.Options.Option<int> o, int x) => o.{|#0:Map|}(v => v + x); }
            """,
            """
            using Waystone.Monads.Options.Extensions;

            internal class S { internal Waystone.Monads.Options.Option<int> M(Waystone.Monads.Options.Option<int> o, int x) => o.With(x).Map(static (v, state) => v + state); }
            """,
            Verify.Diagnostic(Rules.DelegateCapturesInsteadOfState)
               .WithLocation(0)
               .WithArguments("Map", "x"));

    /// <remarks>
    /// Neither directive brings <c>With</c> into scope: an alias names the
    /// namespace without importing it, and a static import names a type. Reading
    /// either as the import already being there would leave a rewrite that does
    /// not compile.
    /// </remarks>
    [Fact]
    public Task ImportsPastAnAliasAndAStaticDirective() =>
        Verify.RawCodeFixAsync<StateOverloadAnalyzer, UseStateBindingCodeFix>(
            """
            using static System.Math;
            using S = Waystone.Monads.Options.Extensions;

            internal class Subject
            {
                internal Waystone.Monads.Options.Option<int> Shift(
                    Waystone.Monads.Options.Option<int> option,
                    int offset) =>
                    option.{|#0:Map|}(value => value + offset);
            }
            """,
            """
            using static System.Math;
            using S = Waystone.Monads.Options.Extensions;
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
}
