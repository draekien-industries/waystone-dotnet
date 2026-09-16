namespace Vagrant.Appraisal.Tests;

using Shouldly;
using Vagrant.SharedKernel;
using Xunit;

public sealed class SpecimenTests
{
    private static readonly Enchantment Elvenkind = new(
        "Cloak of Elvenkind",
        "advantage on stealth, disadvantage to see you");

    [Fact]
    public void HandedIn_keeps_what_it_was_given()
    {
        SpecimenId id = SpecimenId.New();
        PatronId patron = PatronId.New();

        Specimen specimen = Specimen.HandedIn(
            id,
            patron,
            "a grey cloak, well worn",
            new Aura(15),
            Elvenkind);

        specimen.Id.ShouldBe(id);
        specimen.HandedInBy.ShouldBe(patron);
        specimen.Description.ShouldBe("a grey cloak, well worn");
        specimen.Aura.ShouldBe(new Aura(15));
    }

    [Fact]
    public void HandedIn_trims_the_description()
    {
        HandedIn(15, "  a grey cloak  ").Description.ShouldBe("a grey cloak");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void HandedIn_refuses_a_blank_description(string description)
    {
        Should.Throw<ArgumentException>(
            () => Specimen.HandedIn(
                SpecimenId.New(),
                PatronId.New(),
                description,
                new Aura(15),
                Elvenkind));
    }

    [Fact]
    public void An_item_nobody_has_examined_yet_gives_up_nothing()
    {
        HandedIn(15).Enchantment.ShouldBeNone();
    }

    [Fact]
    public void A_check_that_meets_the_aura_names_the_enchantment()
    {
        Specimen specimen = HandedIn(15);

        specimen.Identify(new ArcanaCheck(15));

        specimen.Enchantment.ShouldBeSomeValue(Elvenkind);
    }

    [Fact]
    public void A_check_that_falls_short_leaves_the_shop_none_the_wiser()
    {
        Specimen specimen = HandedIn(15);

        specimen.Identify(new ArcanaCheck(14));

        specimen.Enchantment.ShouldBeNone();
    }

    [Fact]
    public void A_second_look_after_a_failure_can_still_succeed()
    {
        Specimen specimen = HandedIn(15);

        specimen.Identify(new ArcanaCheck(14));
        specimen.Identify(new ArcanaCheck(15));

        specimen.Enchantment.ShouldBeSomeValue(Elvenkind);
    }

    [Fact]
    public void A_bad_roll_cannot_unlearn_what_the_shop_already_knows()
    {
        Specimen specimen = HandedIn(15);

        specimen.Identify(new ArcanaCheck(15));
        specimen.Identify(new ArcanaCheck(-4));

        specimen.Enchantment.ShouldBeSomeValue(Elvenkind);
    }

    [Fact]
    public void Identifying_an_item_the_shop_has_already_read_changes_nothing()
    {
        Specimen specimen = HandedIn(15);

        specimen.Identify(new ArcanaCheck(15));
        specimen.Identify(new ArcanaCheck(30));

        specimen.Enchantment.ShouldBeSomeValue(Elvenkind);
    }

    [Fact]
    public void An_item_carries_its_enchantment_before_anyone_reads_it()
    {
        HandedIn(15).Carries.ShouldBe(Elvenkind);
    }

    private static Specimen HandedIn(uint obscurity, string description = "a grey cloak") =>
        Specimen.HandedIn(
            SpecimenId.New(),
            PatronId.New(),
            description,
            new Aura(obscurity),
            Elvenkind);
}
