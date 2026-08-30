namespace Waystone.Monads.Shouldly.Analyzers;

using global::Shouldly;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

/// <summary>
/// Pins the assertion names the analyzers write against the assertions that actually
/// exist.
/// </summary>
/// <remarks>
/// The names are string literals in the analyzer assembly, and they have to be: that
/// assembly cannot reference Waystone.Monads.Shouldly, because Waystone.Monads.Shouldly
/// loads it as an analyzer and a project reference back would be a build cycle. So
/// nothing in either build notices when an assertion is renamed and the analyzer keeps
/// naming the old one — the rule would still report, the fix would still apply, and the
/// result would not compile. This test project references both sides and is the only
/// place the two can be compared, which makes it the seam that closes the drift.
/// </remarks>
public class MonadAssertionTests
{
    public static TheoryData<string> Replacements()
    {
        var data = new TheoryData<string>();

        foreach (string name in Named)
        {
            data.Add(name);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Replacements))]
    public void EveryReplacementNamesAnAssertionThatExists(string name) =>
        Declared.ShouldContain(name);

    [Theory]
    [MemberData(nameof(Replacements))]
    public void EveryAssertionHasAnAwaitedForm(string name) =>
        Declared.ShouldContain(MonadAssertion.Awaited(name)!);

    /// <remarks>
    /// The reverse direction. An assertion the package ships and the analyzers never
    /// name is a migration a consumer has to do by hand, and the omission is invisible
    /// from the analyzer side.
    /// </remarks>
    [Fact]
    public void EveryAssertionTheAnalyzersCouldNameIsNamed() =>
        Declared.Where(name => !name.EndsWith("Async"))
           .OrderBy(name => name)
           .ShouldBe(Named.OrderBy(name => name));

    /// <summary>
    /// Every state property maps under both asserted truths, and no pair collides.
    /// </summary>
    /// <remarks>
    /// <c>ShouldBeTrue</c> on <c>IsSome</c> and <c>ShouldBeFalse</c> on <c>IsNone</c>
    /// are the same claim, so the eight combinations collapse onto four assertions with
    /// each named exactly twice. A mapping that named one of them once has an arm
    /// pointing at the wrong half of the monad.
    /// </remarks>
    [Fact]
    public void EveryStateMapsUnderBothTruths()
    {
        string[] properties = ["IsSome", "IsNone", "IsOk", "IsErr"];

        var mapped = from property in properties
                     from asserted in new[] { true, false }
                     select MonadAssertion.StateAssertion(property, asserted);

        mapped.GroupBy(name => name)
           .Select(group => (group.Key, Count: group.Count()))
           .OrderBy(pair => pair.Key)
           .ShouldBe(
                [
                    ("ShouldBeErr", 2), ("ShouldBeNone", 2),
                    ("ShouldBeOk", 2), ("ShouldBeSome", 2),
                ]);
    }

    [Theory]
    [InlineData("Unwrap", true, "ShouldBeSomeValue")]
    [InlineData("Unwrap", false, "ShouldBeOkValue")]
    [InlineData("UnwrapErr", false, "ShouldBeErrValue")]
    public void EveryUnwrapMapsToItsHalf(
        string unwrap,
        bool isOption,
        string expected) =>
        MonadAssertion.ValueAssertion(unwrap, isOption).ShouldBe(expected);

    /// <remarks>
    /// An option has no error half, so <c>UnwrapErr</c> does not compile on one. The
    /// mapping refuses it rather than answering, which is what keeps the rule from
    /// offering a fix for source that cannot exist.
    /// </remarks>
    [Fact]
    public void AnOptionHasNoErrorHalf() =>
        MonadAssertion.ValueAssertion("UnwrapErr", isOption: true)
           .ShouldBeNull();

    [Fact]
    public void AnAssertionThisPackageDoesNotShipHasNoAwaitedForm() =>
        MonadAssertion.Awaited("ShouldBeOfType").ShouldBeNull();

    private static IEnumerable<string> Named =>
        MonadAssertion.Names.Concat(
                new[] { "IsSome", "IsNone", "IsOk", "IsErr" }.SelectMany(
                    property => new[]
                    {
                        MonadAssertion.StateAssertion(property, true)!,
                        MonadAssertion.StateAssertion(property, false)!,
                    }))
           .Concat(
                [
                    MonadAssertion.ValueAssertion("Unwrap", true)!,
                    MonadAssertion.ValueAssertion("Unwrap", false)!,
                    MonadAssertion.ValueAssertion("UnwrapErr", false)!,
                ])
           .Distinct();

    /// <remarks>
    /// Walks nested types as well as the assertion classes themselves. The assertions
    /// are declared in C# <c>extension</c> blocks, which the compiler lowers into a
    /// nested type per block alongside a compatibility static on the container, so a
    /// lookup that read only the container's own methods would depend on a lowering
    /// detail rather than on the surface.
    /// </remarks>
    private static IReadOnlyCollection<string> Declared { get; } =
        new[] { typeof(OptionAssertions), typeof(ResultAssertions) }
           .SelectMany(type => type.GetNestedTypes().Prepend(type))
           .SelectMany(
                type => type.GetMethods(
                    BindingFlags.Public | BindingFlags.Static))
           .Where(method => method.Name.StartsWith("ShouldBe"))
           .Select(method => method.Name)
           .Distinct()
           .ToList();
}
