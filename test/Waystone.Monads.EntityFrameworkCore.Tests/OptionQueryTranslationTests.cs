namespace Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using Shouldly;
using Waystone.Monads.Options;
using Xunit;

public sealed class OptionQueryTranslationTests : IDisposable
{
    private readonly SqliteDatabase database = new();

    public OptionQueryTranslationTests()
    {
        using PeopleContext context = database.Create();
        context.People.AddRange(
            new Person { Id = 1, Name = "Alison", Age = Option.Some(31) },
            new Person { Id = 2, Name = "Bo", Age = Option.Some(0) },
            new Person { Id = 3, Name = "Cass" });
        context.SaveChanges();
    }

    public void Dispose() => database.Dispose();

    [Fact]
    public void GivenACapturedSome_ThenItThrowsRatherThanRiskingASilentNone()
    {
        using PeopleContext context = database.Create();
        Option<int> wanted = Option.Some(31);

        Should.Throw<InvalidOperationException>(
                   () => context.People
                                .Where(person => person.Age == wanted)
                                .ToList())
              .Message.ShouldContain("captured option");
    }

    [Fact]
    public void GivenACapturedNone_ThenItThrowsRatherThanMatchingNothing()
    {
        using PeopleContext context = database.Create();
        Option<int> wanted = Option.None<int>();

        Should.Throw<InvalidOperationException>(
                   () => context.People
                                .Where(person => person.Age == wanted)
                                .ToList())
              .Message.ShouldContain("captured option");
    }

    [Fact]
    public void GivenAnEfPropertyIsNullCheck_ThenItFindsTheNoneRows()
    {
        using PeopleContext context = database.Create();

        context.People
               .Where(person => EF.Property<int?>(person, nameof(Person.Age))
                             == null)
               .Select(person => person.Id)
               .ShouldBe([3]);
    }

    [Fact]
    public void GivenAnInlineSome_ThenItTranslates()
    {
        using PeopleContext context = database.Create();

        context.People.Where(person => person.Age == Option.Some(31))
               .Select(person => person.Id)
               .ShouldBe([1]);
    }

    [Fact]
    public void GivenAnInlineNone_ThenItFindsTheNoneRows()
    {
        using PeopleContext context = database.Create();

        context.People.Where(person => person.Age == Option.None<int>())
               .Select(person => person.Id)
               .ShouldBe([3]);
    }

    [Fact]
    public void GivenAnInlineSomeHoldingTheDefaultValue_ThenItDoesNotMatchANone()
    {
        using PeopleContext context = database.Create();

        context.People.Where(person => person.Age == Option.Some(0))
               .Select(person => person.Id)
               .ShouldBe([2]);
    }

    [Fact]
    public void GivenAnInlineOptionAndNotEqual_ThenItTranslates()
    {
        using PeopleContext context = database.Create();

        context.People.Where(person => person.Age != Option.None<int>())
               .Select(person => person.Id)
               .ShouldBe([1, 2]);
    }

    [Fact]
    public void GivenIsSome_ThenItTranslates()
    {
        using PeopleContext context = database.Create();

        context.People.Where(person => person.Age.IsSome)
               .Select(person => person.Id)
               .ShouldBe([1, 2]);
    }

    [Fact]
    public void GivenIsNone_ThenItTranslates()
    {
        using PeopleContext context = database.Create();

        context.People.Where(person => person.Age.IsNone)
               .Select(person => person.Id)
               .ShouldBe([3]);
    }

    [Fact]
    public void GivenAReferenceTypeOption_ThenIsSomeTranslates()
    {
        using PeopleContext context = database.Create();

        context.People.Where(person => person.Nickname.IsNone)
               .Select(person => person.Id)
               .ShouldBe([1, 2, 3]);
    }

    [Fact]
    public void GivenAnEfPropertyNullCheck_ThenItTranslates()
    {
        using PeopleContext context = database.Create();

        context.People
               .Where(person => EF.Property<int?>(person, nameof(Person.Age))
                             != null)
               .Select(person => person.Id)
               .ShouldBe([1, 2]);
    }
}
