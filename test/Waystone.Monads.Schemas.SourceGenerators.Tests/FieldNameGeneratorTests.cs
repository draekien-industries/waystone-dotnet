namespace Waystone.Monads.Schemas.SourceGenerators;

using System.Linq;
using Shouldly;
using Xunit;

/// <summary>
/// <c>WMSC0008</c>, which reads the same argument text the runtime derives a field's
/// path from and says so when that text is not a name.
/// </summary>
public sealed class FieldNameGeneratorTests
{
    private const string Head = """
            public sealed class OrderDto
            {
                public string? Email { get; set; }

                public string? Coupon { get; set; }

                public string[] Lines { get; set; } = new string[0];

                public string? Trimmed() => Email?.Trim();
            }

            public partial class OrderSchema : SchemaConfig<OrderDto, string>
            {
                protected override Result<string, SchemaViolation> Configure(
                    OrderDto subject) =>
        """;

    private static string Configuring(string expression) =>
        $$"""
          {{Head}}
                      {{expression}}
              }
          """;

    /// <summary>
    /// The shape the whole design is built around. Reporting it would make the rule
    /// fire on nearly every field ever written.
    /// </summary>
    [Fact]
    public void AFieldBoundToAMemberIsNotReported() =>
        Verify.Run(
                   Configuring(
                       "Schema.Fields(Schema.Required(subject.Email, Schema.Text)).Into(a => a);"))
              .DiagnosticIds.ShouldBeEmpty();

    /// <summary>
    /// The runtime keeps what follows the last dot, so a conditional access still
    /// reduces to the member's own name and needs nothing said about it.
    /// </summary>
    [Fact]
    public void AFieldBoundThroughAConditionalAccessIsNotReported() =>
        Verify.Run(
                   Configuring(
                       "Schema.Fields(Schema.Required(subject?.Email, Schema.Text)).Into(a => a);"))
              .DiagnosticIds.ShouldBeEmpty();

    public static TheoryData<string, string> UnreadablePaths() =>
        new()
        {
            { "subject.Trimmed()", "trimmed()" },
            { "subject.Lines[0]", "lines[0]" },
            { "subject.Email!", "email!" },
            { "\"bob\"", "\"bob\"" },
        };

    /// <summary>
    /// The expected path is asserted rather than only the id, because showing the
    /// author what their own expression reduces to is the whole value of the rule.
    /// </summary>
    [Theory]
    [MemberData(nameof(UnreadablePaths))]
    public void AFieldWhosePathIsNotANameIsReported(
        string expression,
        string path)
    {
        GeneratorRun run = Verify.Run(
            Configuring(
                $"Schema.Fields(Schema.Required({expression}, Schema.Text)).Into(a => a);"));

        run.DiagnosticIds.ShouldBe(["WMSC0008"]);

        run.GeneratorDiagnostics.Single()
           .GetMessage()
           .ShouldBe(
                $"'OrderSchema' takes this field's path from the expression itself, so a violation reports it as '{path}'; add '.Named(\"...\")' to report it under a name a caller can act on");
    }

    [Fact]
    public void AnOptionalFieldIsCheckedTheSameWay() =>
        Verify.Run(
                   Configuring(
                       "Schema.Fields(Schema.Optional(subject.Trimmed(), Schema.Text)).Into(a => \"\");"))
              .DiagnosticIds.ShouldBe(["WMSC0008"]);

    [Fact]
    public void AForbiddenFieldIsCheckedTheSameWay() =>
        Verify.Run(
                   Configuring(
                       """
                       Schema.Fields(Schema.Required(subject.Email, Schema.Text))
                                   .Refine(Schema.Forbidden(subject.Trimmed(), "Do not send {Path}."))
                                   .Into(a => a);
                       """))
              .DiagnosticIds.ShouldBe(["WMSC0008"]);

    /// <summary>
    /// <c>Named</c> is the fix the message asks for, so a field that already carries
    /// one has nothing left to report.
    /// </summary>
    [Fact]
    public void ANamedFieldIsNotReported() =>
        Verify.Run(
                   Configuring(
                       """
                       Schema.Fields(Schema.Required(subject.Trimmed(), Schema.Text).Named("email"))
                                   .Into(a => a);
                       """))
              .DiagnosticIds.ShouldBeEmpty();

    /// <summary>
    /// Passing the path parameter by hand is discouraged and still legal, and the
    /// argument text is then not what gets reported, so the rule has nothing to say.
    /// </summary>
    [Fact]
    public void AFieldGivenItsPathExplicitlyIsNotReported() =>
        Verify.Run(
                   Configuring(
                       """
                       Schema.Fields(Schema.Required(subject.Trimmed(), Schema.Text, null, "email"))
                                   .Into(a => a);
                       """))
              .DiagnosticIds.ShouldBeEmpty();

    [Fact]
    public void AFieldGivenItsPathByNameIsNotReported() =>
        Verify.Run(
                   Configuring(
                       """
                       Schema.Fields(Schema.Required(subject.Trimmed(), Schema.Text, valueExpression: "email"))
                                   .Into(a => a);
                       """))
              .DiagnosticIds.ShouldBeEmpty();

    /// <summary>
    /// <c>Extend</c> reports at the subject's own path, so it derives no segment and
    /// takes no path parameter to get wrong.
    /// </summary>
    [Fact]
    public void AnExtendFieldIsNotReported() =>
        Verify.Run(
                   Configuring(
                       """
                       Schema.Fields(Schema.Required(subject.Email, Schema.Text))
                                   .Refine(Schema.Extend(subject, Schema.For<OrderDto>()))
                                   .Into(a => a);
                       """))
              .DiagnosticIds.ShouldBeEmpty();

    /// <summary>
    /// A method of that name belonging to somebody else is not a field factory, and
    /// binding is what tells the two apart.
    /// </summary>
    [Fact]
    public void SomebodyElsesRequiredIsNotReported() =>
        Verify.Run(
                   Configuring(
                       "Other.Required(subject.Trimmed()) is null ? Schema.Text.Parse(\"\") : Schema.Text.Parse(\"\");")
                 + "\n\npublic static class Other { public static string? Required(string? value) => value; }")
              .DiagnosticIds.ShouldBeEmpty();
}
