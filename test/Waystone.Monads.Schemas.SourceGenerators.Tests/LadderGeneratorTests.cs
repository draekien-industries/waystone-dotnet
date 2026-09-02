namespace Waystone.Monads.Schemas.SourceGenerators;

using System;
using System.Linq;
using Shouldly;
using Xunit;

/// <summary>
/// The emitted text of the <c>Schema.Fields</c> ladder, and the two diagnostics that
/// come with it. What the emitted text compiles to is <c>GeneratedLadderTests</c>;
/// these cases pin what it says.
/// </summary>
public sealed class LadderGeneratorTests
{
    private const string Head = """
            public partial class GreetingSchema : SchemaConfig<string, string>
            {
                protected override Result<string, SchemaViolation> Configure(
                    string subject) =>
        """;

    /// <summary>
    /// The canonical one-field ladder. Most cases here vary something around it
    /// rather than the expression itself, so it is written once.
    /// </summary>
    private const string OneField =
        "Schema.Fields(Schema.Required(subject, Schema.Text)).Into(a => a);";

    /// <summary>
    /// A schema body with the given <c>Configure</c> expression. The shell is the
    /// same in every case here and the expression is the whole subject, so a change
    /// to the shell should be one edit rather than a dozen.
    /// </summary>
    private static string Configuring(string expression) =>
        $$"""
          {{Head}}
                      {{expression}}
              }
          """;

    [Fact]
    public void ASchemaThatCallsNoFieldsGetsNoLadder() =>
        Verify.Run(Configuring("Schema.Text.Parse(subject);"))
              .Generated[0]
              .ShouldNotContain("FieldSet");

    [Fact]
    public void ASchemaThatCallsFieldsGetsTheLadderAtThatArity()
    {
        string generated = Verify.Run(
                                      Configuring(
                                          "Schema.Fields(Schema.Required(subject, Schema.Text), Schema.Required(subject, Schema.Text)).Into((a, b) => a + b);"))
                                 .Generated[0];

        generated.ShouldContain("private readonly struct FieldSet<T1, T2>");
        generated.ShouldContain("public static FieldSet<T1, T2> Fields<T1, T2>(");
        generated.ShouldContain(
            "private sealed class Schema : global::Waystone.Monads.Schemas.Schema");
    }

    /// <summary>
    /// The ladder is reached by inheriting the schema entry point, because a
    /// generator cannot add an overload to a type in another assembly.
    /// </summary>
    [Fact]
    public void TheLadderIsAddedToASubclassOfTheSchemaEntryPoint() =>
        Verify.Run(
                   Configuring(
                       OneField))
              .Generated[0]
              .ShouldContain(": global::Waystone.Monads.Schemas.Schema");

    [Fact]
    public void TwoCallsAtOneArityEmitOneLadder()
    {
        string generated = Verify.Run(
                                      Configuring(
                                          """
                                          subject.Length > 0
                                                      ? Schema.Fields(Schema.Required(subject, Schema.Text)).Into(a => a)
                                                      : Schema.Fields(Schema.Required(subject, Schema.Text)).Into(a => a);
                                          """))
                                 .Generated[0];

        generated.Split(
                      ["private readonly struct FieldSet"],
                      StringSplitOptions.None)
                 .Length.ShouldBe(2);
    }

    [Fact]
    public void TwoCallsAtDifferentAritiesEmitBoth()
    {
        string generated = Verify.Run(
                                      Configuring(
                                          """
                                          subject.Length > 0
                                                      ? Schema.Fields(Schema.Required(subject, Schema.Text)).Into(a => a)
                                                      : Schema.Fields(Schema.Required(subject, Schema.Text), Schema.Required(subject, Schema.Text)).Into((a, b) => a);
                                          """))
                                 .Generated[0];

        generated.ShouldContain("private readonly struct FieldSet<T1>");
        generated.ShouldContain("private readonly struct FieldSet<T1, T2>");
    }

    [Fact]
    public void TheLadderConstrainsItsTypeParameters() =>
        Verify.Run(
                   Configuring(
                       OneField))
              .Generated[0]
              .ShouldContain("where T1 : notnull");

    /// <summary>
    /// C# 7.3 has no <c>notnull</c> constraint, so emitting one is a build failure in
    /// a consumer's project rather than in ours. Omitting it costs nothing there,
    /// because that compiler does not check nullability either.
    /// </summary>
    [Fact]
    public void TheLadderDropsItsConstraintsWhereTheyCannotBeSpelled() =>
        Verify.RunOnCSharp73(
                   Configuring(
                       OneField))
              .Generated[0]
              .ShouldNotContain("notnull");

    [Fact]
    public void AnIntoLambdaOfTheWrongArityIsReported()
    {
        GeneratorRun run = Verify.Run(
            Configuring(
                "Schema.Fields(Schema.Required(subject, Schema.Text), Schema.Required(subject, Schema.Text)).Into(a => a);"));

        run.DiagnosticIds.ShouldBe(["WMSC0004"]);

        run.GeneratorDiagnostics.Single()
           .GetMessage()
           .ShouldBe(
                "'GreetingSchema' passes 2 fields to 'Schema.Fields' but its 'Into' lambda takes 1; give the lambda one parameter per field, in the order the fields are listed");
    }

    /// <summary>
    /// The ladder is still emitted alongside the diagnostic. Withholding it would
    /// bury the one message that explains the problem under every name in the body
    /// failing to resolve.
    /// </summary>
    [Fact]
    public void AnIntoLambdaOfTheWrongArityStillGetsItsLadder() =>
        Verify.Run(
                   Configuring(
                       "Schema.Fields(Schema.Required(subject, Schema.Text), Schema.Required(subject, Schema.Text)).Into(a => a);"))
              .Generated[0]
              .ShouldContain("private readonly struct FieldSet<T1, T2>");

    [Fact]
    public void AnIntoLambdaOfTheRightArityIsNotReported() =>
        Verify.Run(
                   Configuring(
                       "Schema.Fields(Schema.Required(subject, Schema.Text), Schema.Required(subject, Schema.Text)).Into((a, b) => a + b);"))
              .DiagnosticIds.ShouldBeEmpty();

    /// <summary>
    /// A method group has no parameter list to count, and the compiler already
    /// reports a mismatch against it. Guessing here would report a second time on the
    /// cases it got right.
    /// </summary>
    [Fact]
    public void AnIntoArgumentThatIsNotALambdaIsLeftAlone() =>
        Verify.Run(
                   Configuring(
                       "Schema.Fields(Schema.Required(subject, Schema.Text)).Into(Echo);")
                 + "\n\n    static string Echo(string value) => value;")
              .DiagnosticIds.ShouldNotContain("WMSC0004");

    [Fact]
    public void ARefinementThatYieldsAValueIsReported()
    {
        GeneratorRun run = Verify.Run(
            Configuring(
                "Schema.Fields(Schema.Required(subject, Schema.Text)).Refine(Schema.Required(subject, Schema.Text)).Into(a => a);"));

        run.DiagnosticIds.ShouldBe(["WMSC0005"]);

        run.GeneratorDiagnostics.Single()
           .GetMessage()
           .ShouldStartWith(
                "'Schema.Required(subject, Schema.Text)' yields 'string' and 'Refine' discards it");
    }

    /// <summary>
    /// <c>Forbidden</c> yields <c>Checked</c>, which carries nothing, so it is what
    /// <c>Refine</c> is for and must never be reported.
    /// </summary>
    [Fact]
    public void ARefinementThatOnlyGatesIsNotReported() =>
        Verify.Run(
                   Configuring(
                       """
                       Schema.Fields(Schema.Required(subject, Schema.Text))
                                   .Refine(Schema.Forbidden(subject, "Do not send {Path}."))
                                   .Into(a => a);
                       """))
              .DiagnosticIds.ShouldBeEmpty();

    [Fact]
    public void ALadderWithNoRefinementIsNotReported() =>
        Verify.Run(
                   Configuring(
                       OneField))
              .DiagnosticIds.ShouldBeEmpty();

    /// <summary>
    /// The chain is walked rather than pattern-matched at one depth, so a
    /// <c>Refine</c> between the fields and the <c>Into</c> hides neither from the
    /// checks.
    /// </summary>
    [Fact]
    public void ARefinementBetweenTheFieldsAndTheIntoHidesNeither() =>
        Verify.Run(
                   Configuring(
                       """
                       Schema.Fields(Schema.Required(subject, Schema.Text), Schema.Required(subject, Schema.Text))
                                   .Refine(Schema.Required(subject, Schema.Text))
                                   .Into(a => a);
                       """))
              .DiagnosticIds.ShouldBe(["WMSC0004", "WMSC0005"], ignoreOrder: true);

    /// <summary>
    /// <c>Fields</c> with no arguments builds nothing, so there is no ladder to emit
    /// and the compiler's own message about the missing overload is the better one.
    /// </summary>
    [Fact]
    public void FieldsWithNoArgumentsEmitsNoLadder() =>
        Verify.Run(Configuring("Schema.Fields().Into(() => subject);"))
              .Generated[0]
              .ShouldNotContain("FieldSet");

    /// <summary>
    /// An anonymous method has a parameter list to count, so it is checked like a
    /// lambda rather than waved through.
    /// </summary>
    [Fact]
    public void AnAnonymousMethodOfTheWrongArityIsReported() =>
        Verify.Run(
                   Configuring(
                       "Schema.Fields(Schema.Required(subject, Schema.Text), Schema.Required(subject, Schema.Text)).Into(delegate(string a) { return a; });"))
              .DiagnosticIds.ShouldContain("WMSC0004");

    /// <summary>
    /// The generator is handed a semantic model for one file. A schema whose
    /// <c>Configure</c> sits in another part has to be read through a model asked for
    /// separately, and getting that wrong loses the ladder entirely.
    /// </summary>
    [Fact]
    public void AConfigureInAnotherFileStillDeclaresTheLadder()
    {
        GeneratorRun run = Verify.RunRaw(
            [
                """
                using Waystone.Monads.Schemas;

                namespace Sample
                {
                    public partial class SplitSchema : SchemaConfig<string, string>
                    {
                    }
                }
                """,
                """
                using Waystone.Monads.Results;
                using Waystone.Monads.Schemas;

                namespace Sample
                {
                    public partial class SplitSchema
                    {
                        protected override Result<string, SchemaViolation> Configure(
                            string subject) =>
                            Schema.Fields(Schema.Required(subject, Schema.Text))
                                  .Into(a => a);
                    }
                }
                """,
            ]);

        run.Generated[0].ShouldContain("private readonly struct FieldSet<T1>");
    }

    /// <summary>
    /// The non-generic field has already erased its value side, which is what
    /// <c>Refine</c> is built to take, so it is never the thing being warned about.
    /// </summary>
    [Fact]
    public void ARefinementTypedAsTheNonGenericFieldIsNotReported() =>
        Verify.Run(
                   Configuring(
                       "Schema.Fields(Schema.Required(subject, Schema.Text)).Refine(Gate).Into(a => a);")
                 + "\n\n    static Field Gate => Schema.Forbidden((string?)null, \"Do not send {Path}.\");")
              .DiagnosticIds.ShouldBeEmpty();

    /// <summary>
    /// An argument that does not bind has no type to read, and the compiler already
    /// has a better message about it than this rule would.
    /// </summary>
    [Fact]
    public void ARefinementThatDoesNotBindIsLeftAlone() =>
        Verify.Run(
                   Configuring(
                       "Schema.Fields(Schema.Required(subject, Schema.Text)).Refine(NoSuchThing).Into(a => a);"))
              .DiagnosticIds.ShouldNotContain("WMSC0005");

    /// <summary>
    /// <c>Refine</c> takes a <c>params</c> array, so a caller may hand it the array
    /// itself. That argument is the whole set rather than one field, and reading it
    /// as a field would report against a type that yields nothing at all.
    /// </summary>
    [Fact]
    public void ARefinementPassedAsAnArrayIsLeftAlone() =>
        Verify.Run(
                   Configuring(
                       """
                       Schema.Fields(Schema.Required(subject, Schema.Text))
                                   .Refine(new Field[] { Schema.Forbidden((string?)null, "Do not send {Path}.") })
                                   .Into(a => a);
                       """))
              .DiagnosticIds.ShouldBeEmpty();

    /// <summary>
    /// A <c>Fields</c> call on something that is not the schema entry point is
    /// somebody else's method with a name this generator has no claim on. It binds,
    /// which is what keeps <c>WMSC0007</c> off it.
    /// </summary>
    [Fact]
    public void AFieldsCallOnAnotherReceiverIsIgnored()
    {
        GeneratorRun run = Verify.Run(
            Configuring(
                "Other.Fields(subject) is null ? Schema.Text.Parse(subject) : Schema.Text.Parse(subject);")
          + "\n\npublic static class Other { public static string Fields(string value) => value; }");

        run.Generated[0].ShouldNotContain("FieldSet");
        run.DiagnosticIds.ShouldBeEmpty();
    }

    /// <summary>
    /// The receiver is matched on its last name, so qualifying the nested
    /// <c>Schema</c> by the class holding it reaches the same ladder. Anchoring on a
    /// bare identifier instead cost the author the ladder and told them nothing.
    /// </summary>
    [Fact]
    public void AFieldsCallQualifiedByItsSchemaStillGetsTheLadder()
    {
        GeneratorRun run = Verify.Run(
            Configuring(
                "GreetingSchema.Schema.Fields(Schema.Required(subject, Schema.Text)).Into(a => a);"));

        run.Generated[0].ShouldContain("private readonly struct FieldSet<T1>");
        run.DiagnosticIds.ShouldBeEmpty();
    }

    public static TheoryData<string> UnrecognisedSpellings() =>
        new()
        {
            "Fields(Schema.Required(subject, Schema.Text)).Into(a => a);",
            "this.Fields(Schema.Required(subject, Schema.Text)).Into(a => a);",
            "subject.Fields(Schema.Required(subject, Schema.Text)).Into(a => a);",
        };

    /// <summary>
    /// Every spelling here binds to nothing, which is the test that separates a
    /// field set the generator failed to see from a consumer's own method. Left
    /// unreported, the author sees only a missing member on a type they never wrote.
    /// </summary>
    [Theory]
    [MemberData(nameof(UnrecognisedSpellings))]
    public void AFieldsCallTheGeneratorDoesNotRecogniseIsReported(
        string expression)
    {
        GeneratorRun run = Verify.Run(Configuring(expression));

        run.DiagnosticIds.ShouldBe(["WMSC0007"]);
        run.Generated[0].ShouldNotContain("FieldSet");
    }

    [Fact]
    public void AnUnrecognisedFieldsCallIsNamedAsTheAuthorSpeltIt() =>
        Verify.Run(
                   Configuring(
                       "Fields(Schema.Required(subject, Schema.Text)).Into(a => a);"))
              .GeneratorDiagnostics.Single()
              .GetMessage()
              .ShouldStartWith(
                   "'GreetingSchema' spells its field-set call 'Fields'");
}
