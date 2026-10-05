namespace Waystone.Monads.Schemas.SourceGenerators;

using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Shouldly;
using Xunit;

/// <summary>
/// <c>WMSC0010</c> and its code fix, which move a schema off the <c>Schema.Fields</c>
/// spelling. Every subject runs through the generator first, because the call this
/// rule reports binds only to a member the generator emitted.
/// </summary>
public sealed class QualifiedFieldsAnalyzerTests
{
    private const string Head = """
            public partial class GreetingSchema : SchemaConfig<string, string>
            {
                protected override Result<string, SchemaViolation> Configure(
                    string subject) =>
        """;

    private static string Configuring(string expression, string members = "") =>
        $$"""
          {{Head}}
                      {{expression}}
          {{members}}
              }
          """;

    private static ImmutableArray<Diagnostic> Analyze(string source) =>
        Verify.AnalyzeGenerated(new QualifiedFieldsAnalyzer(), source);

    private static Task<string> FixAsync(string source) =>
        Verify.FixAsync(
            new QualifiedFieldsAnalyzer(),
            new UseUnqualifiedFieldsCodeFix(),
            source);

    [Fact]
    public void AQualifiedFieldsCallIsReportedOnItsReceiver()
    {
        Diagnostic diagnostic =
            Analyze(
                    Configuring(
                        "Schema.Fields(Schema.Required(subject, Schema.Text)).Into(a => a);"))
               .ShouldHaveSingleItem();

        diagnostic.Id.ShouldBe("WMSC0010");
        diagnostic.Severity.ShouldBe(DiagnosticSeverity.Info);
        diagnostic.Descriptor.CustomTags.ShouldContain(WellKnownDiagnosticTags.Unnecessary);

        diagnostic.Location.SourceTree!
                  .GetText(TestContext.Current.CancellationToken)
                  .ToString(diagnostic.Location.SourceSpan)
                  .ShouldBe("Schema.");

        diagnostic.GetMessage()
                  .ShouldBe(
                       "'GreetingSchema' calls 'Schema.Fields', which is removed in 8.0.0; call 'Fields(...)' with no receiver so that 'Schema' binds to the library's own type");
    }

    /// <summary>
    /// The nested class qualified by the schema holding it is the same member, and the
    /// span covers the whole receiver the fix deletes.
    /// </summary>
    [Fact]
    public void AFieldsCallQualifiedByItsSchemaIsReportedOnTheWholeReceiver()
    {
        Diagnostic diagnostic =
            Analyze(
                    Configuring(
                        "GreetingSchema.Schema.Fields(Schema.Required(subject, Schema.Text)).Into(a => a);"))
               .ShouldHaveSingleItem();

        diagnostic.Location.SourceTree!
                  .GetText(TestContext.Current.CancellationToken)
                  .ToString(diagnostic.Location.SourceSpan)
                  .ShouldBe("GreetingSchema.Schema.");
    }

    [Fact]
    public void AnUnqualifiedFieldsCallIsNotReported() =>
        Analyze(
                Configuring(
                    "Fields(Schema.Required(subject, Schema.Text)).Into(a => a);"))
           .ShouldBeEmpty();

    /// <summary>
    /// Where another call in the body is already unqualified, the generated method
    /// is in scope, and it is the one the rewritten call would reach.
    /// </summary>
    [Fact]
    public void AQualifiedCallBesideAnUnqualifiedOneIsReported() =>
        Analyze(
                Configuring(
                    """
                    subject.Length > 0
                                ? Fields(Schema.Required(subject, Schema.Text)).Into(a => a)
                                : Schema.Fields(Schema.Required(subject, Schema.Text), Schema.Required(subject, Schema.Text)).Into((a, b) => a + b);
                    """))
           .ShouldHaveSingleItem()
           .Id.ShouldBe("WMSC0010");

    /// <summary>
    /// Rewritten, the call would bind to the schema's own member, so there is no
    /// spelling to suggest.
    /// </summary>
    [Fact]
    public void AQualifiedCallBesideTheSchemasOwnFieldsIsLeftAlone() =>
        Analyze(
                Configuring(
                    "Schema.Fields(Schema.Required(subject, Schema.Text)).Into(a => a);",
                    "    static int Fields => 0;"))
           .ShouldBeEmpty();

    public static TheoryData<string, string> OtherSchemas() =>
        new()
        {
            {
                "public static class Schema { public static string Fields(string value) => value; }",
                "Schema.Fields(\"a\")"
            },
            {
                "public static class Outer { public static class Schema { public static string Fields(string value) => value; } }",
                "Outer.Schema.Fields(\"a\")"
            },
            {
                "namespace Other { public class Schema { } } public static class Outer { public sealed class Schema : Other.Schema { public static string Fields(string value) => value; } }",
                "Outer.Schema.Fields(\"a\")"
            },
        };

    /// <summary>
    /// A <c>Fields</c> on a class named <c>Schema</c> that the generator did not
    /// write is somebody else's method, nested or not.
    /// </summary>
    [Theory]
    [MemberData(nameof(OtherSchemas))]
    public void AFieldsOnAnotherSchemaClassIsLeftAlone(
        string declaration,
        string call) =>
        Analyze(
                declaration
              + "\n\npublic static class Caller { public static string Call() => "
              + call
              + "; }")
           .ShouldBeEmpty();

    /// <summary>
    /// A call that binds to nothing has no generated member behind it, and the
    /// compiler already reports it.
    /// </summary>
    [Fact]
    public void AnUnboundFieldsCallIsLeftAlone() =>
        Analyze(
                Configuring(
                    "subject.Fields(Schema.Required(subject, Schema.Text)).Into(a => a);"))
           .ShouldBeEmpty();

    [Fact]
    public void EveryRuleTheAnalyzerReportsIsTheOneItDeclares() =>
        new QualifiedFieldsAnalyzer().SupportedDiagnostics
                                     .ShouldHaveSingleItem()
                                     .ShouldBe(Rules.PreferUnqualifiedFields);

    [Theory]
    [InlineData("Schema.")]
    [InlineData("GreetingSchema.Schema.")]
    public async Task TheFixDeletesTheWholeReceiver(string receiver) =>
        (await FixAsync(
             Configuring(
                 receiver
               + "Fields(Schema.Required(subject, Schema.Text)).Into(a => a);")))
       .ShouldBe(
            Configuring(
                "Fields(Schema.Required(subject, Schema.Text)).Into(a => a);"));

    /// <summary>
    /// The fix replaces the receiver and the name together, so a comment ahead of the
    /// call and the layout of the chain after it both survive.
    /// </summary>
    [Fact]
    public async Task TheFixKeepsTheTriviaAroundTheCall() =>
        (await FixAsync(
             Configuring(
                 """
                 /* fields */ Schema.Fields(Schema.Required(subject, Schema.Text))
                                   .Into(a => a);
                 """)))
       .ShouldBe(
            Configuring(
                """
                /* fields */ Fields(Schema.Required(subject, Schema.Text))
                                  .Into(a => a);
                """));

    [Fact]
    public async Task EveryQualifiedCallInABodyIsFixed() =>
        (await FixAsync(
             Configuring(
                 """
                 subject.Length > 0
                             ? Schema.Fields(Schema.Required(subject, Schema.Text)).Into(a => a)
                             : Schema.Fields(Schema.Required(subject, Schema.Text), Schema.Required(subject, Schema.Text)).Into((a, b) => a + b);
                 """)))
       .ShouldBe(
            Configuring(
                """
                subject.Length > 0
                            ? Fields(Schema.Required(subject, Schema.Text)).Into(a => a)
                            : Fields(Schema.Required(subject, Schema.Text), Schema.Required(subject, Schema.Text)).Into((a, b) => a + b);
                """));

    [Fact]
    public void TheFixAnswersToTheRuleItFixes() =>
        new UseUnqualifiedFieldsCodeFix().FixableDiagnosticIds
                                         .ShouldBe(["WMSC0010"]);
}
