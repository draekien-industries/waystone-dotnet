namespace Waystone.Internal.Analyzers;

using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Shouldly;
using Xunit;

/// <remarks>
/// The rule stands in for a check nothing else performs. A dropped parameter attribute
/// leaves a generated member that compiles and ships, and the public API baseline does
/// not record parameter attributes, so RS0016 and RS0017 — the oracle for everything
/// else about that surface — report nothing. Most of these cases are therefore the
/// silent ones: a rule that over-reports here fails a build nobody can fix from the
/// message, and one that under-reports is indistinguishable from the bug.
/// </remarks>
public sealed class UncarriedParameterAttributeAnalyzerTests
{
    private const string Box = """
        public sealed class Box<T> where T : notnull
        {
            public T Get() => default!;
        }


        """;

    [Fact]
    public async Task
        GivenAnAttributeTheGeneratorDrops_WhenAnalysed_ThenReportWa0004()
    {
        ImmutableArray<Diagnostic> diagnostics = await VerifyAwaitedReceivers.Run(
            Box
          + """
            [GenerateAwaitedReceivers(typeof(Box<>))]
            public static partial class BoxExtensions
            {
                extension<T>(Box<T> box) where T : notnull
                {
                    public T Read(
                        [System.Runtime.InteropServices.Optional] T fallback) =>
                        box.Get();
                }
            }
            """);

        diagnostics.Select(diagnostic => diagnostic.Id).ShouldBe(["WA0004"]);
    }

    [Fact]
    public async Task
        GivenAnAttributeTheGeneratorDrops_WhenAnalysed_ThenNameItAndBothFixes()
    {
        ImmutableArray<Diagnostic> diagnostics = await VerifyAwaitedReceivers.Run(
            Box
          + """
            [GenerateAwaitedReceivers(typeof(Box<>))]
            public static partial class BoxExtensions
            {
                extension<T>(Box<T> box) where T : notnull
                {
                    public T Read(
                        [System.Runtime.InteropServices.Optional] T fallback) =>
                        box.Get();
                }
            }
            """);

        string message = diagnostics.Single().GetMessage();

        message.ShouldContain("'OptionalAttribute'");
        message.ShouldContain("'fallback'");
        message.ShouldContain("'Read'");
        message.ShouldContain("[ExcludeFromAwaitedReceivers]");
    }

    /// <remarks>
    /// The receiver is dropped from the generated member's parameter list outright and
    /// its attributes with it, so this is the one case where the message's second fix
    /// does not apply. It is still reported, because the attribute still goes missing.
    /// </remarks>
    [Fact]
    public async Task
        GivenAnAttributeOnTheReceiver_WhenAnalysed_ThenReportItToo()
    {
        ImmutableArray<Diagnostic> diagnostics = await VerifyAwaitedReceivers.Run(
            Box
          + """
            [GenerateAwaitedReceivers(typeof(Box<>))]
            public static partial class BoxExtensions
            {
                public static T Read<T>(
                    [System.Runtime.InteropServices.Optional] this Box<T> box)
                    where T : notnull => box.Get();
            }
            """);

        diagnostics.Single().GetMessage().ShouldContain("'box'");
    }

    /// <remarks>
    /// A classic <c>static (this T)</c> method is lifted exactly as an
    /// <c>extension</c> block member is, so rewriting one in that form does not dodge
    /// the rule any more than it dodges the lift.
    /// </remarks>
    [Fact]
    public async Task
        GivenAClassicExtensionMethod_WhenAnalysed_ThenReportItToo()
    {
        ImmutableArray<Diagnostic> diagnostics = await VerifyAwaitedReceivers.Run(
            Box
          + """
            [GenerateAwaitedReceivers(typeof(Box<>))]
            public static partial class BoxExtensions
            {
                public static T Read<T>(
                    this Box<T> box,
                    [System.Runtime.InteropServices.Optional] T fallback)
                    where T : notnull => box.Get();
            }
            """);

        diagnostics.Select(diagnostic => diagnostic.Id).ShouldBe(["WA0004"]);
    }

    [Fact]
    public async Task
        GivenTheCallerInfoAttributes_WhenAnalysed_ThenStaySilent()
    {
        ImmutableArray<Diagnostic> diagnostics = await VerifyAwaitedReceivers.Run(
            Box
          + """
            [GenerateAwaitedReceivers(typeof(Box<>))]
            public static partial class BoxExtensions
            {
                extension<T>(Box<T> box) where T : notnull
                {
                    public T Read(
                        [CallerArgumentExpression(nameof(box))] string? expression = null,
                        [CallerMemberName] string? member = null,
                        [CallerFilePath] string? file = null,
                        [CallerLineNumber] int line = 0) => box.Get();
                }
            }
            """);

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task
        GivenAMemberKeptOffTheAwaitedShapes_WhenAnalysed_ThenStaySilent()
    {
        ImmutableArray<Diagnostic> diagnostics = await VerifyAwaitedReceivers.Run(
            Box
          + """
            [GenerateAwaitedReceivers(typeof(Box<>))]
            public static partial class BoxExtensions
            {
                extension<T>(Box<T> box) where T : notnull
                {
                    [ExcludeFromAwaitedReceivers]
                    public T Read(
                        [System.Runtime.InteropServices.Optional] T fallback) =>
                        box.Get();
                }
            }
            """);

        diagnostics.ShouldBeEmpty();
    }

    /// <remarks>
    /// The generated shapes themselves are members of this shape, and they are
    /// analysed like any other source because they are deliberately not marked as
    /// generated code. Nothing is lifted from an awaited receiver, so nothing here is
    /// dropped.
    /// </remarks>
    [Fact]
    public async Task
        GivenAMemberAlreadyOnAnAwaitedReceiver_WhenAnalysed_ThenStaySilent()
    {
        ImmutableArray<Diagnostic> diagnostics = await VerifyAwaitedReceivers.Run(
            Box
          + """
            [GenerateAwaitedReceivers(typeof(Box<>))]
            public static partial class BoxExtensions
            {
                extension<T>(Task<Box<T>> boxTask) where T : notnull
                {
                    public async ValueTask<T> ReadAsync(
                        [System.Runtime.InteropServices.Optional] T fallback) =>
                        (await boxTask.ConfigureAwait(false)).Get();
                }
            }
            """);

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task
        GivenANonPublicMember_WhenAnalysed_ThenStaySilent()
    {
        ImmutableArray<Diagnostic> diagnostics = await VerifyAwaitedReceivers.Run(
            Box
          + """
            [GenerateAwaitedReceivers(typeof(Box<>))]
            public static partial class BoxExtensions
            {
                extension<T>(Box<T> box) where T : notnull
                {
                    internal T Read(
                        [System.Runtime.InteropServices.Optional] T fallback) =>
                        box.Get();
                }
            }
            """);

        diagnostics.ShouldBeEmpty();
    }

    [Fact]
    public async Task
        GivenAnUnmarkedClass_WhenAnalysed_ThenStaySilent()
    {
        ImmutableArray<Diagnostic> diagnostics = await VerifyAwaitedReceivers.Run(
            Box
          + """
            public static partial class BoxExtensions
            {
                extension<T>(Box<T> box) where T : notnull
                {
                    public T Read(
                        [System.Runtime.InteropServices.Optional] T fallback) =>
                        box.Get();
                }
            }
            """);

        diagnostics.ShouldBeEmpty();
    }

    /// <remarks>
    /// The marker attribute is injected by the generator, so a project that does not
    /// import its props cannot name it and has nothing for this rule to act on. That is
    /// what makes importing the analyzers cheap enough to be the default.
    /// </remarks>
    [Fact]
    public async Task
        GivenACompilationWithoutTheGenerator_WhenAnalysed_ThenStaySilent()
    {
        ImmutableArray<Diagnostic> diagnostics =
            await VerifyAwaitedReceivers.RunWithoutTheGenerator(
                Box
              + """
                public static partial class BoxExtensions
                {
                    extension<T>(Box<T> box) where T : notnull
                    {
                        public T Read(
                            [System.Runtime.InteropServices.Optional] T fallback) =>
                            box.Get();
                    }
                }
                """);

        diagnostics.ShouldBeEmpty();
    }

    /// <remarks>
    /// The compiler has already reported the name on this very line, and the thing it
    /// could not bind was never going to reach a generated member. Reporting again adds
    /// a second diagnostic to a build that cannot get past the first.
    /// </remarks>
    [Fact]
    public async Task
        GivenAnAttributeThatDoesNotBind_WhenAnalysed_ThenStaySilent()
    {
        ImmutableArray<Diagnostic> diagnostics = await VerifyAwaitedReceivers.Run(
            Box
          + """
            [GenerateAwaitedReceivers(typeof(Box<>))]
            public static partial class BoxExtensions
            {
                extension<T>(Box<T> box) where T : notnull
                {
                    public T Read([NoSuchThing] T fallback) => box.Get();
                }
            }
            """);

        diagnostics.ShouldBeEmpty();
    }
}
