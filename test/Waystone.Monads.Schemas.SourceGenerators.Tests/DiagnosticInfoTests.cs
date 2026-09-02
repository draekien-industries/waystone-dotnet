namespace Waystone.Monads.Schemas.SourceGenerators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Shouldly;
using Xunit;

/// <summary>
/// The type exists so a diagnostic can survive the incremental pipeline, which
/// compares its values for equality and cannot hold a <c>Location</c>. These cases
/// reach it directly, for the shapes the generator itself never produces.
/// </summary>
public sealed class DiagnosticInfoTests
{
    /// <summary>
    /// Every diagnostic the generator reports comes from source and so has a file.
    /// A record with a nullable field still has to say what it does without one, or
    /// the answer is an exception in a consumer's compiler.
    /// </summary>
    [Fact]
    public void ADiagnosticWithNoFileIsReportedWithoutALocation() =>
        new DiagnosticInfo(
                Rules.NotPartial,
                null,
                default(TextSpan),
                default(LinePositionSpan),
                "GreetingSchema",
                "Outer").ToDiagnostic()
                        .Location.ShouldBe(Location.None);

    [Fact]
    public void TwoInfosOverTheSameDiagnosticAreEqual()
    {
        var one = new DiagnosticInfo(
            Rules.NotPartial,
            "Schema.cs",
            default(TextSpan),
            default(LinePositionSpan),
            "GreetingSchema",
            "Outer");

        var other = new DiagnosticInfo(
            Rules.NotPartial,
            "Schema.cs",
            default(TextSpan),
            default(LinePositionSpan),
            "GreetingSchema",
            "Outer");

        other.ShouldBe(one);
    }
}
