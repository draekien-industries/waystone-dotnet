namespace Waystone.Monads.Schemas;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

public sealed class SchemaDictionaryTests
{
    private static readonly ParseContext At = ParseContext.Root.At("rates");

    [Fact]
    public void GivenEveryEntryPasses_WhenParsing_ThenProduceThemAll()
    {
        Outcome<IReadOnlyDictionary<string, int>> outcome =
            Schema.Dictionary(Schema.Text, Schema.Number.Int32)
                  .Evaluate(Rates(("AUD", 1), ("NZD", 2)), At);

        outcome.Violations.ShouldBeEmpty();
        outcome.Value.Count.ShouldBe(2);
        outcome.Value["AUD"].ShouldBe(1);
        outcome.Value["NZD"].ShouldBe(2);
    }

    [Fact]
    public void GivenAnEmptyDictionary_WhenParsing_ThenProduceAnEmptyOne()
    {
        Outcome<IReadOnlyDictionary<string, int>> outcome =
            Schema.Dictionary(Schema.Text, Schema.Number.Int32)
                  .Evaluate(Rates(), At);

        outcome.Violations.ShouldBeEmpty();
        outcome.Value.ShouldBeEmpty();
    }

    [Fact]
    public void GivenAFailingValue_WhenParsing_ThenReportItAtTheKeyedPath()
    {
        Outcome<IReadOnlyDictionary<string, int>> outcome =
            Schema.Dictionary(Schema.Text, Schema.Number.Int32.Positive())
                  .Evaluate(Rates(("AUD", 0)), At);

        Violation violation = outcome.Violations.ShouldHaveSingleItem();
        violation.Path.ToString().ShouldBe("rates[\"AUD\"]");
        violation.Code.ShouldBe(ViolationCodeCatalog.Codes.OutOfRange);
    }

    [Fact]
    public void GivenAFailingKey_WhenParsing_ThenReportItAtTheKeyedPath()
    {
        Outcome<IReadOnlyDictionary<string, int>> outcome =
            Schema.Dictionary(Schema.Text.MinLength(4), Schema.Number.Int32)
                  .Evaluate(Rates(("AUD", 1)), At);

        outcome.Violations.ShouldHaveSingleItem()
               .Path.ToString()
               .ShouldBe("rates[\"AUD\"]");
    }

    [Fact]
    public void GivenAKeyThatProducesNoValue_WhenParsing_ThenProduceNoDictionary()
    {
        Outcome<IReadOnlyDictionary<int, string>> outcome =
            Schema.Dictionary(new Lengths(), Schema.Text)
                  .Evaluate(
                       new Dictionary<string, string> { ["a"] = "x" },
                       At);

        outcome.HasValue.ShouldBeTrue();

        Outcome<IReadOnlyDictionary<int, string>> rejected =
            Schema.Dictionary(new RejectsText(), Schema.Text)
                  .Evaluate(
                       new Dictionary<string, string> { ["a"] = "x" },
                       At);

        rejected.HasValue.ShouldBeFalse();
    }

    [Fact]
    public void GivenTwoKeysThatParseTheSame_WhenParsing_ThenReportADuplicate()
    {
        Outcome<IReadOnlyDictionary<int, string>> outcome =
            Schema.Dictionary(new Lengths(), Schema.Text)
                  .Evaluate(
                       new Dictionary<string, string>
                       {
                           ["a"] = "first",
                           ["b"] = "second",
                       },
                       At);

        outcome.HasValue.ShouldBeFalse();
        Violation violation = outcome.Violations.ShouldHaveSingleItem();
        violation.Code.ShouldBe(ViolationCodeCatalog.Codes.Duplicate);
        violation.Path.ToString().ShouldBe("rates[\"b\"]");
        violation.Message.ShouldContain("already produced 1");
    }

    [Fact]
    public void GivenANullValue_WhenParsing_ThenReportItAsAbsent()
    {
        Outcome<IReadOnlyDictionary<string, string>> outcome =
            Schema.Dictionary(Schema.Text, Schema.Text)
                  .Evaluate(
                       new Dictionary<string, string> { ["AUD"] = null! },
                       At);

        outcome.HasValue.ShouldBeFalse();
        Violation violation = outcome.Violations.ShouldHaveSingleItem();
        violation.Code.ShouldBe(ViolationCodeCatalog.Codes.Incomplete);
        violation.Message.ShouldBe("Expected rates[\"AUD\"] to be present.");
    }

    [Fact]
    public void GivenANumericKey_WhenParsing_ThenQuoteItSoItIsNotAnIndex()
    {
        Outcome<IReadOnlyDictionary<int, string>> outcome =
            Schema.Dictionary(Schema.Number.Int32.Positive(), Schema.Text)
                  .Evaluate(new Dictionary<int, string> { [0] = "x" }, At);

        outcome.Violations.ShouldHaveSingleItem()
               .Path.ToString()
               .ShouldBe("rates[\"0\"]");
    }

    [Fact]
    public async Task GivenAnAsynchronousValueSchema_WhenParsing_ThenAwaitIt()
    {
        Outcome<IReadOnlyDictionary<string, string>> outcome = await Schema
           .Dictionary(Schema.Text, new AsyncRejects<string>())
           .EvaluateAsync(
                new Dictionary<string, string> { ["AUD"] = "x" },
                At,
                TestContext.Current.CancellationToken);

        outcome.HasValue.ShouldBeFalse();

        outcome.Violations.ShouldHaveSingleItem()
               .Path.ToString()
               .ShouldBe("rates[\"AUD\"]");
    }

    [Fact]
    public async Task GivenANullValue_WhenParsingAsynchronously_ThenReportItAsAbsent()
    {
        Outcome<IReadOnlyDictionary<string, string>> outcome = await Schema
           .Dictionary(new AsyncPassThrough<string>(), Schema.Text)
           .EvaluateAsync(
                new Dictionary<string, string> { ["AUD"] = null! },
                At,
                TestContext.Current.CancellationToken);

        outcome.Violations.ShouldHaveSingleItem()
               .Code.ShouldBe(ViolationCodeCatalog.Codes.Incomplete);
    }

    [Fact]
    public async Task GivenEveryEntryPasses_WhenParsingAsynchronously_ThenProduceThem()
    {
        Outcome<IReadOnlyDictionary<string, string>> outcome = await Schema
           .Dictionary(
                new AsyncPassThrough<string>(),
                new AsyncPassThrough<string>())
           .EvaluateAsync(
                new Dictionary<string, string> { ["AUD"] = "x" },
                At,
                TestContext.Current.CancellationToken);

        outcome.Violations.ShouldBeEmpty();
        outcome.Value["AUD"].ShouldBe("x");
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 1)]
    public void GivenACountBound_WhenParsing_ThenBoundTheEntryCount(
        int minimum,
        int expected)
    {
        Schema.Dictionary(Schema.Text, Schema.Number.Int32)
              .MinCount(minimum)
              .Evaluate(Rates(("AUD", 1)), At)
              .Violations.Count.ShouldBe(expected);
    }

    [Fact]
    public void GivenTooManyEntries_WhenBounding_ThenNameTheBound()
    {
        Schema.Dictionary(Schema.Text, Schema.Number.Int32)
              .MaxCount(0)
              .Evaluate(Rates(("AUD", 1)), At)
              .Violations.ShouldHaveSingleItem()
              .Message.ShouldBe("Expected rates to hold at most 0 entries.");

        Schema.Dictionary(Schema.Text, Schema.Number.Int32)
              .MaxCount(1)
              .Evaluate(Rates(("AUD", 1)), At)
              .Violations.ShouldBeEmpty();
    }

    [Fact]
    public void GivenNoSchema_WhenBuildingADictionary_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
                   () => Schema.Dictionary<string, int, string, int>(
                       null!,
                       Schema.Number.Int32))
              .ParamName.ShouldBe("key");

        Should.Throw<ArgumentNullException>(
                   () => Schema.Dictionary<string, int, string, int>(
                       Schema.Text,
                       null!))
              .ParamName.ShouldBe("value");
    }

    [Fact]
    public void GivenNoSchema_WhenAddingACountBound_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
                   () => Absent().MinCount(1))
              .ParamName.ShouldBe("schema");

        Should.Throw<ArgumentNullException>(
                   () => Absent().MaxCount(1))
              .ParamName.ShouldBe("schema");
    }

    [Fact]
    public void GivenAKeyThatRendersAsNull_WhenParsing_ThenUseAnEmptySegment()
    {
        Outcome<IReadOnlyDictionary<Nameless, string>> outcome =
            Schema.Dictionary(Schema.For<Nameless>(), Schema.Text.NotEmpty())
                  .Evaluate(
                       new Dictionary<Nameless, string>
                       {
                           [new Nameless()] = string.Empty,
                       },
                       At);

        outcome.Violations.ShouldHaveSingleItem()
               .Path.ToString()
               .ShouldBe("rates[\"\"]");
    }

    private static Schema<IReadOnlyDictionary<string, int>,
        IReadOnlyDictionary<string, int>> Absent() =>
        null!;

    private sealed class Nameless
    {
        public override string ToString() => null!;
    }

    private static IReadOnlyDictionary<string, int> Rates(
        params (string Code, int Value)[] entries)
    {
        var rates = new Dictionary<string, int>();

        foreach ((string code, int value) in entries) rates[code] = value;

        return rates;
    }
}
