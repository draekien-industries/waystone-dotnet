namespace Vagrant.Staffing.Tests;

using Microsoft.Extensions.Time.Testing;
using Shouldly;
using Waystone.Monads.Results.Errors;
using Xunit;

// A clerk holds one errand at a time. Every expectation below is that rule, not a
// reading of what the code did.
public sealed class ClerkTests
{
    private static readonly DateTimeOffset Noon =
        new(2026, 9, 16, 12, 0, 0, TimeSpan.Zero);
    private static readonly FakeTimeProvider Clock = new(Noon);
    private static readonly Errand Cloak =
        new(new ErrandSubject(Guid.CreateVersion7()));

    [Fact]
    public void A_clerk_keeps_the_name_they_were_taken_on_under()
    {
        Clerk.Hired(ClerkId.New(), "Pumat Prime").Name.ShouldBe("Pumat Prime");
    }

    [Fact]
    public void Hired_trims_the_name()
    {
        Clerk.Hired(ClerkId.New(), "  Pumat Sol  ").Name.ShouldBe("Pumat Sol");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void A_clerk_with_no_name_is_refused(string name)
    {
        Should.Throw<ArgumentException>(() => Clerk.Hired(ClerkId.New(), name));
    }

    [Fact]
    public void A_new_clerk_is_holding_nothing()
    {
        Hired().Engagement.ShouldBeNone();
    }

    [Fact]
    public void A_clerk_given_an_errand_is_holding_it()
    {
        Clerk clerk = Hired();

        Assignment taken = clerk.Take(Cloak, Clock).ShouldBeOk();

        taken.Errand.ShouldBe(Cloak);
        taken.Clerk.ShouldBe(clerk.Id);
        taken.Since.ShouldBe(Noon);
        clerk.Engagement.ShouldBeSomeValue(taken);
    }

    [Fact]
    public void A_clerk_already_holding_something_cannot_take_another()
    {
        Clerk clerk = Hired();
        clerk.Take(Cloak, Clock).ShouldBeOk();

        Error error = clerk
                     .Take(new Errand(new ErrandSubject(Guid.CreateVersion7())), Clock)
                     .ShouldBeErr();

        error.Code.Value.ShouldBe("vagrant.staffing.clerk_already_engaged");
    }

    [Fact]
    public void A_refused_errand_leaves_the_first_one_in_hand()
    {
        Clerk clerk = Hired();
        Assignment first = clerk.Take(Cloak, Clock).ShouldBeOk();

        clerk.Take(new Errand(new ErrandSubject(Guid.CreateVersion7())), Clock)
             .ShouldBeErr();

        clerk.Engagement.ShouldBeSomeValue(first);
    }

    [Fact]
    public void A_released_clerk_is_holding_nothing()
    {
        Clerk clerk = Hired();
        clerk.Take(Cloak, Clock).ShouldBeOk();

        clerk.Release();

        clerk.Engagement.ShouldBeNone();
    }

    [Fact]
    public void A_released_clerk_can_take_another_errand()
    {
        Clerk clerk = Hired();
        clerk.Take(Cloak, Clock).ShouldBeOk();
        clerk.Release();

        clerk.Take(new Errand(new ErrandSubject(Guid.CreateVersion7())), Clock)
             .ShouldBeOk();
    }

    // Releasing a clerk who is holding nothing is the state the caller wanted.
    [Fact]
    public void Releasing_a_free_clerk_is_not_a_failure()
    {
        Clerk clerk = Hired();

        clerk.Release();

        clerk.Engagement.ShouldBeNone();
    }

    [Fact]
    public void Two_clerks_answering_to_the_same_name_are_still_two_clerks()
    {
        Clerk one = Clerk.Hired(ClerkId.New(), "Pumat Sol");
        Clerk two = Clerk.Hired(ClerkId.New(), "Pumat Sol");

        one.Take(Cloak, Clock).ShouldBeOk();

        two.Engagement.ShouldBeNone();
        two.Id.ShouldNotBe(one.Id);
    }

    private static Clerk Hired() => Clerk.Hired(ClerkId.New(), "Pumat Sol");
}
