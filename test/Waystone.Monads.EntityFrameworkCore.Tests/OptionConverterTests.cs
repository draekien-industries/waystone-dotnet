namespace Microsoft.EntityFrameworkCore;

using ChangeTracking;
using Shouldly;
using Storage.ValueConversion;
using Waystone.Monads.Options;
using Xunit;

public class ReferenceTypeOptionConverterTests
{
    private static readonly ReferenceTypeOptionConverter<string> Converter = new();

    private static Option<string> FromProvider(string? value) =>
        (Option<string>)Converter.ConvertFromProvider(value)!;

    [Fact]
    public void WhenConvertingASomeToTheProvider_ThenWriteTheHeldValue() =>
        Converter.ConvertToProvider(Option.Some("ally")).ShouldBe("ally");

    [Fact]
    public void WhenConvertingANoneToTheProvider_ThenWriteNull() =>
        Converter.ConvertToProvider(Option.None<string>()).ShouldBeNull();

    [Fact]
    public void GivenAnEmptyString_WhenConvertingToTheProvider_ThenWriteItRatherThanNull() =>
        Converter.ConvertToProvider(Option.Some(string.Empty)).ShouldBe(string.Empty);

    [Fact]
    public void WhenConvertingAValueFromTheProvider_ThenReadASome() =>
        FromProvider("ally").ShouldBeSomeValue("ally");

    [Fact]
    public void WhenConvertingNullFromTheProvider_ThenReadANone() =>
        FromProvider(null).ShouldBeNone();

    [Fact]
    public void GivenAnEmptyString_WhenConvertingFromTheProvider_ThenReadASome() =>
        FromProvider(string.Empty).ShouldBeSomeValue(string.Empty);
}

public class ValueTypeOptionConverterTests
{
    private static readonly ValueTypeOptionConverter<int> Converter = new();

    private static Option<int> FromProvider(int? value) =>
        (Option<int>)Converter.ConvertFromProvider(value)!;

    [Fact]
    public void WhenConvertingASomeToTheProvider_ThenWriteTheHeldValue() =>
        Converter.ConvertToProvider(Option.Some(42)).ShouldBe(42);

    [Fact]
    public void WhenConvertingANoneToTheProvider_ThenWriteNull() =>
        Converter.ConvertToProvider(Option.None<int>()).ShouldBeNull();

    [Fact]
    public void GivenADefaultValue_WhenConvertingToTheProvider_ThenWriteItRatherThanNull() =>
        Converter.ConvertToProvider(Option.Some(0)).ShouldBe(0);

    [Fact]
    public void GivenAFalse_WhenConvertingToTheProvider_ThenWriteItRatherThanNull() =>
        new ValueTypeOptionConverter<bool>()
           .ConvertToProvider(Option.Some(false))
           .ShouldBe(false);

    [Fact]
    public void WhenConvertingAValueFromTheProvider_ThenReadASome() =>
        FromProvider(42).ShouldBeSomeValue(42);

    [Fact]
    public void WhenConvertingNullFromTheProvider_ThenReadANone() =>
        FromProvider(null).ShouldBeNone();

    [Fact]
    public void GivenADefaultValue_WhenConvertingFromTheProvider_ThenReadASome() =>
        FromProvider(0).ShouldBeSomeValue(0);
}

public class OptionValueComparerTests
{
    private static readonly OptionValueComparer<int> Comparer = new();

    [Fact]
    public void GivenTwoSomesHoldingTheSameValue_ThenTheyAreEqual() =>
        Comparer.Equals(Option.Some(1), Option.Some(1)).ShouldBeTrue();

    [Fact]
    public void GivenTwoSomesHoldingDifferentValues_ThenTheyAreNotEqual() =>
        Comparer.Equals(Option.Some(1), Option.Some(2)).ShouldBeFalse();

    [Fact]
    public void GivenASomeAndANone_ThenTheyAreNotEqual() =>
        Comparer.Equals(Option.Some(1), Option.None<int>()).ShouldBeFalse();

    [Fact]
    public void GivenTwoNones_ThenTheyAreEqual() =>
        Comparer.Equals(Option.None<int>(), Option.None<int>()).ShouldBeTrue();

    [Fact]
    public void GivenTwoNulls_ThenTheyAreEqual() =>
        Comparer.Equals(null, null).ShouldBeTrue();

    [Fact]
    public void GivenANullAndASome_ThenTheyAreNotEqual() =>
        Comparer.Equals(null, Option.Some(1)).ShouldBeFalse();

    [Fact]
    public void GivenTwoSomesHoldingTheSameValue_ThenTheirHashCodesMatch() =>
        Comparer.GetHashCode(Option.Some(1))
                .ShouldBe(Comparer.GetHashCode(Option.Some(1)));

    [Fact]
    public void GivenANull_WhenHashing_ThenReturnZero() =>
        Comparer.GetHashCode(null!).ShouldBe(0);

    [Fact]
    public void WhenSnapshotting_ThenReturnTheSameInstance()
    {
        Option<int> option = Option.Some(1);

        Comparer.Snapshot(option).ShouldBeSameAs(option);
    }
}
