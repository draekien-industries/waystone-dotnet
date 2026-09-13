namespace Waystone.Internal.SourceGenerators;

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Waystone.Internal.SourceGenerators.AwaitedReceivers;
using Xunit;

/// <summary>
/// Covers the half of <see cref="CallerInfo" /> that reads a
/// <c>CallerArgumentExpression</c> whose constructor never bound.
/// </summary>
/// <remarks>
/// These build their own compilation instead of going through <see cref="Verify" />,
/// because the shape under test is an attribute the compilation cannot resolve and
/// <see cref="Verify" /> hands its subject the whole test host — where
/// <c>CallerArgumentExpression</c> resolves to the real type and binds. A compilation
/// with no references at all reproduces what PolySharp leaves behind on
/// <c>netstandard2.0</c>: the right name, <see cref="TypeKind.Error" />, and no
/// constructor arguments.
/// </remarks>
public sealed class CallerInfoTests
{
    private const string Attribute =
        "global::System.Runtime.CompilerServices.CallerArgumentExpressionAttribute";

    [Fact]
    public void RepointsAnUnboundNameofAtTheGeneratedReceiver() =>
        Render("(nameof(box))").ShouldBe($"[{Attribute}(\"boxTask\")] ");

    [Fact]
    public void RepointsAnUnboundStringLiteralAtTheGeneratedReceiver() =>
        Render("(\"box\")").ShouldBe($"[{Attribute}(\"boxTask\")] ");

    /// <remarks>
    /// The receiver is the one parameter the generated member renames, so a target
    /// naming anything else has to survive the fallback unchanged. Asserting only the
    /// re-pointing above would pass against a reader that rewrote every target.
    /// </remarks>
    [Fact]
    public void LeavesAnUnboundTargetThatIsNotTheReceiverAlone() =>
        Render("(nameof(expression))").ShouldBe($"[{Attribute}(\"expression\")] ");

    /// <remarks>
    /// Each of these is a spelling the fallback declines to guess at. They are not
    /// spellings anyone writes — an attribute argument has to be a constant, so
    /// <c>nameof</c> and a literal are the only two that reach a build — but the
    /// reader sees syntax rather than a bound argument, and syntax parses all of
    /// them. Writing an attribute from one would put a name the caller never chose
    /// into a consumer's assertion message.
    /// </remarks>
    [Theory]
    [InlineData("")]
    [InlineData("()")]
    [InlineData("(nameof(box), nameof(expression))")]
    [InlineData("(Box.Name(box))")]
    [InlineData("(Name(box))")]
    [InlineData("(nameof(box, expression))")]
    [InlineData("(nameof(box.Length))")]
    [InlineData("(42)")]
    public void WritesNoAttributeWhenTheUnboundTargetIsUnreadable(string arguments) =>
        Render(arguments).ShouldBeEmpty();

    private static string Render(string arguments)
    {
        CSharpCompilation compilation = CSharpCompilation.Create(
            "Waystone.Internal.SourceGenerators.Tests.Unbound",
            [
                CSharpSyntaxTree.ParseText(
                    $$"""
                      public class Box
                      {
                          public void Read(
                              string box,
                              [System.Runtime.CompilerServices.CallerArgumentExpressionAttribute{{arguments}}]
                              string expression) { }
                      }
                      """,
                    new CSharpParseOptions(LanguageVersion.Preview),
                    cancellationToken: TestContext.Current.CancellationToken),
            ],
            []);

        AttributeData attribute = compilation.GetTypeByMetadataName("Box")
                                             .ShouldNotBeNull()
                                             .GetMembers("Read")
                                             .OfType<IMethodSymbol>()
                                             .Single()
                                             .Parameters[1]
                                             .GetAttributes()
                                             .Single();

        attribute.AttributeClass.ShouldNotBeNull().TypeKind.ShouldBe(TypeKind.Error);
        attribute.ConstructorArguments.ShouldBeEmpty();

        return CallerInfo.Render(attribute, "box", "boxTask");
    }
}
