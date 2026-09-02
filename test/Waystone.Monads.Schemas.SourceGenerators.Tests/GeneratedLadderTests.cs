namespace Waystone.Monads.Schemas.SourceGenerators.Fixtures;

using Shouldly;
using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Waystone.Monads.Schemas;
using Xunit;

/// <summary>
/// Exercises the generated <c>Schema.Fields</c> ladder as a consumer writes it. The
/// schemas at the bottom of this file compile against emitted source, so this is the
/// only thing here that proves the ladder binds, infers and runs — a snapshot proves
/// the text alone.
/// </summary>
public sealed class GeneratedLadderTests
{
    private static readonly PersonDto Valid =
        new PersonDto("Ada", "ada@example.com", 36, null);

    [Fact]
    public void GivenAValidSubject_WhenParsing_ThenBuildTheDomainType()
    {
        Person person = PersonSchema.Instance.Parse(Valid).Unwrap();

        person.Name.ShouldBe("Ada");
        person.Email.ShouldBe("ada@example.com");
        person.Age.Unwrap().ShouldBe(36);
    }

    /// <summary>
    /// An absent optional reaches the constructed object as a <c>None</c>, so null
    /// never gets there and the ladder still builds.
    /// </summary>
    [Fact]
    public void GivenNoOptionalValue_WhenParsing_ThenBuildWithNone() =>
        PersonSchema.Instance.Parse(
                         new PersonDto("Ada", "ada@example.com", null, null))
                    .Unwrap()
                    .Age.IsNone.ShouldBeTrue();

    /// <summary>
    /// The promise the package rests on. Two fields are wrong and both are reported,
    /// rather than the parse stopping at the first.
    /// </summary>
    [Fact]
    public void GivenSeveralBadFields_WhenParsing_ThenReportEveryOne()
    {
        SchemaViolation violation =
            PersonSchema.Instance.Parse(
                             new PersonDto(string.Empty, string.Empty, 36, null))
                        .UnwrapErr();

        violation.Violations.Count.ShouldBe(2);
        violation.Violations[0].Path.ToString().ShouldBe("name");
        violation.Violations[1].Path.ToString().ShouldBe("email");
    }

    /// <summary>
    /// A refinement gates the parse without taking a slot in the <c>Into</c> lambda,
    /// which is what the non-generic <c>Field</c> parameter buys.
    /// </summary>
    [Fact]
    public void GivenAForbiddenFieldThatWasSent_WhenParsing_ThenRefuseIt()
    {
        SchemaViolation violation =
            PersonSchema.Instance.Parse(
                             new PersonDto(
                                 "Ada",
                                 "ada@example.com",
                                 36,
                                 "The Countess"))
                        .UnwrapErr();

        violation.Violations.Count.ShouldBe(1);
        violation.Violations[0].Path.ToString().ShouldBe("nickname");
    }

    [Fact]
    public void GivenOneField_WhenParsing_ThenTheLadderStillBuilds() =>
        LabelSchema.Instance.Parse(" hello ").Unwrap().ShouldBe("hello");

    /// <summary>
    /// A schema that only validates finishes with <c>Checked</c> rather than
    /// <c>Into</c>, so there is nothing to construct and nothing to name.
    /// </summary>
    [Fact]
    public void GivenAGatingSchema_WhenParsing_ThenCheckWithoutBuilding()
    {
        ConsentSchema.Instance.Parse(new ConsentDto("yes", "yes"))
                     .IsOk.ShouldBeTrue();

        ConsentSchema.Instance.Parse(new ConsentDto(string.Empty, "yes"))
                     .IsErr.ShouldBeTrue();
    }

    /// <summary>
    /// Two arities in one schema, so the generator emits two ladders into one class
    /// and neither collides with the other.
    /// </summary>
    [Fact]
    public void GivenTwoAritiesInOneSchema_WhenParsing_ThenBothLaddersExist()
    {
        BranchingSchema.Instance.Parse(new PairDto("a", "b", true))
                       .Unwrap()
                       .ShouldBe("a|b");

        BranchingSchema.Instance.Parse(new PairDto("a", "b", false))
                       .Unwrap()
                       .ShouldBe("a");
    }
}

public sealed record PersonDto(
    string? Name,
    string? Email,
    int? Age,
    string? Nickname);

public sealed class Person
{
    internal Person(string name, string email, Option<int> age)
    {
        Name = name;
        Email = email;
        Age = age;
    }

    public string Name { get; }

    public string Email { get; }

    public Option<int> Age { get; }
}

public partial class PersonSchema : SchemaConfig<PersonDto, Person>
{
    protected override Result<Person, SchemaViolation> Configure(
        PersonDto subject) =>
        Schema.Fields(
                   Schema.Required(subject.Name, Schema.Text.NotEmpty()),
                   Schema.Required(subject.Email, Schema.Text.NotEmpty()),
                   Schema.Optional(subject.Age, Schema.For<int>()))
              .Refine(
                   Schema.Forbidden(
                       subject.Nickname,
                       "Do not send {Path}."))
              .Into((name, email, age) => new Person(name, email, age));
}

public partial class LabelSchema : SchemaConfig<string, string>
{
    protected override Result<string, SchemaViolation> Configure(
        string subject) =>
        Schema.Fields(Schema.Required(subject, Schema.Text.Trim().NotEmpty()))
              .Into(text => text);
}

public sealed record ConsentDto(string? Terms, string? Privacy);

public partial class ConsentSchema : SchemaConfig<ConsentDto, Checked>
{
    protected override Result<Checked, SchemaViolation> Configure(
        ConsentDto subject) =>
        Schema.Fields(
                   Schema.Required(subject.Terms, Schema.Text.NotEmpty()),
                   Schema.Required(subject.Privacy, Schema.Text.NotEmpty()))
              .Checked();
}

public sealed record PairDto(string? Left, string? Right, bool Both);

public partial class BranchingSchema : SchemaConfig<PairDto, string>
{
    protected override Result<string, SchemaViolation> Configure(
        PairDto subject) =>
        subject.Both
            ? Schema.Fields(
                        Schema.Required(subject.Left, Schema.Text.NotEmpty()),
                        Schema.Required(subject.Right, Schema.Text.NotEmpty()))
                    .Into((left, right) => left + "|" + right)
            : Schema.Fields(
                        Schema.Required(subject.Left, Schema.Text.NotEmpty()))
                    .Into(left => left);
}
