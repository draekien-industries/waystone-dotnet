namespace Waystone.Monads.Schemas.SourceGenerators.Fixtures;

using Shouldly;
using Waystone.Monads.Results;
using Waystone.Monads.Schemas;
using Xunit;

/// <summary>
/// Exercises the generated <c>Instance</c> as a consumer sees it: this project loads
/// the generator as an analyzer, so the member asserted below is emitted rather than
/// written. A snapshot proves the text; only this proves it compiles and runs.
/// </summary>
public sealed class GeneratedInstanceTests
{
    [Fact]
    public void TheSchemaHasAGeneratedInstance() =>
        GreetingSchema.Instance.ShouldNotBeNull();

    [Fact]
    public void TheInstanceIsSharedRatherThanRebuilt() =>
        GreetingSchema.Instance.ShouldBeSameAs(GreetingSchema.Instance);

    [Fact]
    public void TheInstanceParsesThroughTheHandWrittenConfigure() =>
        GreetingSchema.Instance.Parse("hello")
                      .Match(greeting => greeting, _ => string.Empty)
                      .ShouldBe("hello");

    [Fact]
    public void TheInstanceReportsTheFailuresConfigureDeclares() =>
        GreetingSchema.Instance.Parse(string.Empty)
                      .Match(
                           _ => string.Empty,
                           violation => violation.Violations[0].Code.Value)
                      .ShouldBe("schema_violation.out-of-range");

    /// <summary>
    /// A nested schema, so the fixture also proves the generator reopens the type
    /// around it rather than only top-level classes.
    /// </summary>
    [Fact]
    public void ANestedSchemaAlsoGetsOne() =>
        Outer.InnerSchema.Instance.Parse("hi")
             .Match(greeting => greeting, _ => string.Empty)
             .ShouldBe("hi");
}

public partial class GreetingSchema : SchemaConfig<string, string>
{
    protected override Result<string, SchemaViolation> Configure(
        string subject) =>
        Schema.Text.NotEmpty().Parse(subject);
}

public partial class Outer
{
    public partial class InnerSchema : SchemaConfig<string, string>
    {
        protected override Result<string, SchemaViolation> Configure(
            string subject) =>
            Schema.Text.Parse(subject);
    }
}
