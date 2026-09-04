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
    private static DiagnosticInfo Info(string? filePath) =>
        new DiagnosticInfo(
            Rules.NotPartial,
            filePath,
            default(TextSpan),
            default(LinePositionSpan),
            new EquatableArray<string>(["GreetingSchema", "Outer"]));

    /// <summary>
    /// Every diagnostic the generator reports comes from source and so has a file.
    /// A record with a nullable field still has to say what it does without one, or
    /// the answer is an exception in a consumer's compiler.
    /// </summary>
    [Fact]
    public void ADiagnosticWithNoFileIsReportedWithoutALocation() =>
        Info(null).ToDiagnostic().Location.ShouldBe(Location.None);

    [Fact]
    public void TwoInfosOverTheSameDiagnosticAreEqual() =>
        Info("Schema.cs").ShouldBe(Info("Schema.cs"));

    [Fact]
    public void TwoInfosOverDifferentFilesDiffer() =>
        Info("Schema.cs").ShouldNotBe(Info("Other.cs"));

    /// <summary>
    /// The message arguments are the reason the pipeline needs a by-value array.
    /// Two diagnostics differing only in what they name are different diagnostics,
    /// and a bare array would report them as the same.
    /// </summary>
    [Fact]
    public void TwoInfosNamingDifferentTypesDiffer()
    {
        var one = new DiagnosticInfo(
            Rules.NotPartial,
            "Schema.cs",
            default(TextSpan),
            default(LinePositionSpan),
            new EquatableArray<string>(["GreetingSchema", "Outer"]));

        var other = new DiagnosticInfo(
            Rules.NotPartial,
            "Schema.cs",
            default(TextSpan),
            default(LinePositionSpan),
            new EquatableArray<string>(["GreetingSchema", "Elsewhere"]));

        other.ShouldNotBe(one);
    }

    [Fact]
    public void TheMessageArgumentsReachTheDiagnostic() =>
        Info("Schema.cs")
           .ToDiagnostic()
           .GetMessage()
           .ShouldContain("'Outer' is not declared partial");
}
