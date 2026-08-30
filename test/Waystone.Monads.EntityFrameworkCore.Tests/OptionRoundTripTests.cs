namespace Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using Shouldly;
using Waystone.Monads.Options;
using Xunit;

public sealed class OptionRoundTripTests : IDisposable
{
    private readonly SqliteDatabase database = new();

    public void Dispose() => database.Dispose();

    [Fact]
    public void GivenSomeValues_ThenTheyReloadAsTheSameSomes()
    {
        Save(new Person
        {
            Id = 1,
            Name = "Alison",
            Nickname = Option.Some("Ally"),
            Age = Option.Some(31),
        });

        Person reloaded = Reload(1);

        reloaded.Nickname.ShouldBeSomeValue("Ally");
        reloaded.Age.ShouldBeSomeValue(31);
    }

    [Fact]
    public void GivenNoneValues_ThenTheyReloadAsNones()
    {
        Save(new Person { Id = 2, Name = "Bo" });

        Person reloaded = Reload(2);

        reloaded.Nickname.ShouldBeNone();
        reloaded.Age.ShouldBeNone();
    }

    [Fact]
    public void GivenSomeHoldingTheDefaultValue_ThenItReloadsAsASomeNotANone()
    {
        Save(new Person
        {
            Id = 3,
            Name = "Cass",
            Nickname = Option.Some(string.Empty),
            Age = Option.Some(0),
        });

        Person reloaded = Reload(3);

        reloaded.Nickname.ShouldBeSomeValue(string.Empty);
        reloaded.Age.ShouldBeSomeValue(0);
    }

    [Fact]
    public void GivenANone_ThenTheColumnHoldsNull()
    {
        Save(new Person { Id = 4, Name = "Dev" });

        database.ReadColumnType("Age", 4).ShouldBe("null");
        database.ReadColumnType("Nickname", 4).ShouldBe("null");
    }

    [Fact]
    public void GivenASome_ThenTheColumnHoldsTheHeldTypesValue()
    {
        Save(new Person
        {
            Id = 5,
            Name = "Eze",
            Nickname = Option.Some("Ez"),
            Age = Option.Some(7),
        });

        database.ReadColumnType("Age", 5).ShouldBe("integer");
        database.ReadColumnType("Nickname", 5).ShouldBe("text");
    }

    [Fact]
    public void WhenASomeIsReassigned_ThenChangeTrackingNoticesAndSaves()
    {
        Save(new Person { Id = 6, Name = "Fay", Age = Option.Some(1) });

        using (PeopleContext context = database.Create())
        {
            Person person = context.People.Single(candidate => candidate.Id == 6);
            person.Age = Option.Some(2);
            context.ChangeTracker.DetectChanges();
            context.ChangeTracker.HasChanges().ShouldBeTrue();
            context.SaveChanges();
        }

        Reload(6).Age.ShouldBeSomeValue(2);
    }

    [Fact]
    public void WhenASomeBecomesANone_ThenChangeTrackingNoticesAndSaves()
    {
        Save(new Person { Id = 7, Name = "Gil", Age = Option.Some(1) });

        using (PeopleContext context = database.Create())
        {
            Person person = context.People.Single(candidate => candidate.Id == 7);
            person.Age = Option.None<int>();
            context.SaveChanges();
        }

        Reload(7).Age.ShouldBeNone();
    }

    [Fact]
    public void GivenAnUninitialisedProperty_ThenItSavesAsNullAndReloadsAsANone()
    {
        Save(new Person
        {
            Id = 8,
            Name = "Hal",
            Nickname = null!,
            Age = null!,
        });

        Person reloaded = Reload(8);

        reloaded.Nickname.ShouldBeNone();
        reloaded.Age.ShouldBeNone();
    }

    private void Save(Person person)
    {
        using PeopleContext context = database.Create();
        context.People.Add(person);
        context.SaveChanges();
    }

    private Person Reload(int id)
    {
        using PeopleContext context = database.Create();
        return context.People.AsNoTracking().Single(person => person.Id == id);
    }
}
