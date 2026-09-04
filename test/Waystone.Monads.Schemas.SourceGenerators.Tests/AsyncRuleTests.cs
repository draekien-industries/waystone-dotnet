namespace Waystone.Monads.Schemas.SourceGenerators;

using System.Linq;
using Shouldly;
using Xunit;

/// <summary>
/// <c>WMSC0006</c>: an asynchronous rule written where only the synchronous path
/// can reach it.
/// </summary>
/// <remarks>
/// The rule sees only what the schema spells out. A schema held in a static field
/// or handed in from another assembly looks like any other schema to the compiler,
/// and the runtime throw covers those — <c>SchemaCheckAsyncTests</c> is that half.
/// </remarks>
public sealed class AsyncRuleTests
{
    private const string Head = """
            public partial class GreetingSchema : SchemaConfig<string, string>
            {
                protected override Result<string, SchemaViolation> Configure(
                    string subject) =>
        """;

    private const string AsyncRule = """
        Schema.Text.CheckAsync(
                        (value, token) => new global::System.Threading.Tasks.ValueTask<bool>(true),
                        ViolationCode.Mismatched,
                        "{Path} is taken.")
        """;

    /// <summary>
    /// The rule on its own, outside any field set. Written once because two cases
    /// below assert different things about the same run.
    /// </summary>
    private static readonly string BareRule =
        Configuring(AsyncRule + ".Parse(subject);");

    [Fact]
    public void AnAsynchronousRuleReachedFromConfigureIsReported()
    {
        GeneratorRun run = Verify.Run(
            Configuring(
                "Schema.Fields(Schema.Required(subject, " + AsyncRule + ")).Into(a => a);"));

        run.DiagnosticIds.ShouldBe(["WMSC0006"]);

        run.GeneratorDiagnostics.Single()
           .GetMessage()
           .ShouldBe(
                "'GreetingSchema' reaches an asynchronous rule from 'Configure', which only ever runs the synchronous path, so the rule throws rather than deciding anything; use 'Check' if the rule can answer from the value alone, or compose this schema outside a field set and parse it with 'ParseAsync'");
    }

    /// <summary>
    /// A field set is not the only shape it can hide in, and the rule does not care
    /// which one it is — a schema evaluated through <c>Configure</c> runs
    /// synchronously either way.
    /// </summary>
    [Fact]
    public void AnAsynchronousRuleOutsideAFieldSetIsStillReported() =>
        Verify.Run(BareRule)
              .DiagnosticIds.ShouldBe(["WMSC0006"]);

    /// <summary>
    /// Reported at the <c>CheckAsync</c> name rather than at the schema, because
    /// that is the token a reader has to change.
    /// </summary>
    [Fact]
    public void AnAsynchronousRuleIsReportedAtTheCallThatAddedIt() =>
        Verify.Run(BareRule)
              .GeneratorDiagnostics.Single()
              .Location.SourceSpan.Length.ShouldBe("CheckAsync".Length);

    /// <summary>
    /// The ladder is emitted regardless. Withholding it would bury the one message
    /// that explains the problem under every name in the body failing to resolve.
    /// </summary>
    [Fact]
    public void AnAsynchronousRuleStillGetsItsLadder() =>
        Verify.Run(
                   Configuring(
                       "Schema.Fields(Schema.Required(subject, " + AsyncRule + ")).Into(a => a);"))
              .Generated[0]
              .ShouldContain("private readonly struct FieldSet<T1>");

    [Fact]
    public void ASynchronousRuleIsNotReported() =>
        Verify.Run(
                   Configuring(
                       """
                       Schema.Fields(
                                       Schema.Required(
                                           subject,
                                           Schema.Text.Check(
                                               value => value.Length > 0,
                                               ViolationCode.OutOfRange,
                                               "Too short.")))
                                   .Into(a => a);
                       """))
              .DiagnosticIds.ShouldBeEmpty();

    /// <summary>
    /// The name alone is not enough. Somebody else's <c>CheckAsync</c> has nothing
    /// to do with a schema, and reporting it would be a false positive nothing in
    /// the message could explain.
    /// </summary>
    [Fact]
    public void AForeignMemberOfTheSameNameIsNotReported() =>
        Verify.Run(
                   Configuring("Store.CheckAsync(subject).Parse(subject);")
                 + """


                   public static class Store
                   {
                       public static Schema<string, string> CheckAsync(string value) =>
                           Schema.Text;
                   }
                   """)
              .DiagnosticIds.ShouldBeEmpty();

    /// <summary>
    /// Nor is the shape enough. Somebody else's two-parameter <c>Schema</c> matches
    /// on metadata name alone, so the namespace is what tells it from ours — the
    /// generator does not reference the runtime and has no symbol to compare
    /// against.
    /// </summary>
    [Fact]
    public void AForeignSchemaOfTheSameShapeIsNotReported() =>
        Verify.Run(
                   Configuring("Local.CheckAsync().Parse(subject);")
                 + """


                   public class Local : Mine.Schema<string, string>;

                   }

                   namespace Mine
                   {
                       public class Schema<TIn, TOut>
                       {
                           public Waystone.Monads.Schemas.Schema<string, string> CheckAsync() =>
                               Waystone.Monads.Schemas.Schema.Text;
                       }
                   """)
              .DiagnosticIds.ShouldBeEmpty();

    /// <summary>
    /// An unbound call has no symbol to inspect, and the compiler already has a
    /// better message for it than this rule would.
    /// </summary>
    [Fact]
    public void AnUnboundMemberOfTheSameNameIsNotReported() =>
        Verify.Run(Configuring("Missing.CheckAsync(subject);"))
              .DiagnosticIds.ShouldBeEmpty();

    private static string Configuring(string expression) =>
        $$"""
          {{Head}}
                      {{expression}}
              }
          """;
}
