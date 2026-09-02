namespace Waystone.Monads.Schemas;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

public sealed class SchemaListTests
{
    private static readonly ParseContext At = ParseContext.Root.At("lines");

    [Fact]
    public void GivenEveryEntryPasses_WhenParsingAList_ThenProduceThemInOrder()
    {
        Outcome<IReadOnlyList<int>> outcome =
            Schema.List(new Lengths()).Evaluate(new[] { "a", "bb" }, At);

        outcome.Violations.ShouldBeEmpty();
        outcome.Value.ShouldBe(new[] { 1, 2 });
    }

    [Fact]
    public void GivenAnEmptyList_WhenParsing_ThenProduceAnEmptyList()
    {
        Outcome<IReadOnlyList<int>> outcome =
            Schema.List(new Lengths()).Evaluate(Array.Empty<string>(), At);

        outcome.Violations.ShouldBeEmpty();
        outcome.Value.ShouldBeEmpty();
    }

    [Fact]
    public void GivenSeveralBadEntries_WhenParsing_ThenReportEveryOne()
    {
        Outcome<IReadOnlyList<string>> outcome =
            Schema.List(Schema.Text.NotEmpty())
                  .Evaluate(new[] { string.Empty, "ok", string.Empty }, At);

        outcome.Violations.Count.ShouldBe(2);
        outcome.Violations[0].Path.ToString().ShouldBe("lines[0]");
        outcome.Violations[1].Path.ToString().ShouldBe("lines[2]");
    }

    [Fact]
    public void GivenARefinedEntry_WhenParsing_ThenStillProduceTheList()
    {
        Outcome<IReadOnlyList<string>> outcome =
            Schema.List(Schema.Text.NotEmpty())
                  .Evaluate(new[] { string.Empty }, At);

        outcome.HasValue.ShouldBeTrue();
        outcome.Value.ShouldBe(new[] { string.Empty });
    }

    [Fact]
    public void GivenAnEntryThatProducesNoValue_WhenParsing_ThenProduceNoList()
    {
        Outcome<IReadOnlyList<int>> outcome =
            Schema.List(new RejectsText()).Evaluate(new[] { "a" }, At);

        outcome.HasValue.ShouldBeFalse();
        outcome.Violations.ShouldHaveSingleItem();
    }

    [Fact]
    public void GivenOneFailedEntry_WhenParsing_ThenStillRunItsSiblings()
    {
        Outcome<IReadOnlyList<int>> outcome =
            Schema.List(new RejectsText()).Evaluate(new[] { "a", "b" }, At);

        outcome.HasValue.ShouldBeFalse();
        outcome.Violations.Count.ShouldBe(2);
        outcome.Violations[1].Path.ToString().ShouldBe("lines[1]");
    }

    [Fact]
    public void GivenANullEntry_WhenParsing_ThenReportItAsAbsentAtItsIndex()
    {
        Outcome<IReadOnlyList<string>> outcome =
            Schema.List(Schema.Text)
                  .Evaluate(new[] { "a", null!, "c" }, At);

        outcome.HasValue.ShouldBeFalse();
        Violation violation = outcome.Violations.ShouldHaveSingleItem();
        violation.Path.ToString().ShouldBe("lines[1]");
        violation.Code.ShouldBe(ViolationCodeCatalog.Codes.Incomplete);
        violation.Message.ShouldBe("Expected lines[1] to be present.");
    }

    [Fact]
    public void GivenANestedSchema_WhenParsing_ThenNestThePathUnderTheIndex()
    {
        Outcome<IReadOnlyList<string>> outcome =
            Schema.List(Schema.Text.NotEmpty().Named("sku"))
                  .Evaluate(new[] { "ok", string.Empty }, At);

        outcome.Violations.ShouldHaveSingleItem()
               .Path.ToString()
               .ShouldBe("lines[1].sku");
    }

    [Fact]
    public async Task GivenAnAsynchronousItemSchema_WhenParsing_ThenAwaitIt()
    {
        Outcome<IReadOnlyList<string>> outcome = await Schema
           .List(new AsyncRejects<string>())
           .EvaluateAsync(
                new[] { "a", "b" },
                At,
                TestContext.Current.CancellationToken);

        outcome.HasValue.ShouldBeFalse();
        outcome.Violations.Count.ShouldBe(2);
        outcome.Violations[1].Path.ToString().ShouldBe("lines[1]");
    }

    [Fact]
    public async Task GivenANullEntry_WhenParsingAsynchronously_ThenReportItAsAbsent()
    {
        Outcome<IReadOnlyList<string>> outcome = await Schema
           .List(new AsyncPassThrough<string>())
           .EvaluateAsync(
                new[] { "a", null! },
                At,
                TestContext.Current.CancellationToken);

        outcome.Violations.ShouldHaveSingleItem()
               .Code.ShouldBe(ViolationCodeCatalog.Codes.Incomplete);
    }

    [Fact]
    public async Task GivenEveryEntryPasses_WhenParsingAsynchronously_ThenProduceThem()
    {
        Outcome<IReadOnlyList<string>> outcome = await Schema
           .List(new AsyncPassThrough<string>())
           .EvaluateAsync(
                new[] { "a", "b" },
                At,
                TestContext.Current.CancellationToken);

        outcome.Violations.ShouldBeEmpty();
        outcome.Value.ShouldBe(new[] { "a", "b" });
    }

    [Theory]
    [InlineData(2, 0)]
    [InlineData(3, 1)]
    public void GivenAMinimumCount_WhenParsing_ThenBoundTheLength(
        int minimum,
        int expected)
    {
        Schema.List(Schema.Text)
              .MinCount(minimum)
              .Evaluate(new[] { "a", "b" }, At)
              .Violations.Count.ShouldBe(expected);
    }

    [Theory]
    [InlineData(2, 0)]
    [InlineData(1, 1)]
    public void GivenAMaximumCount_WhenParsing_ThenBoundTheLength(
        int maximum,
        int expected)
    {
        Schema.List(Schema.Text)
              .MaxCount(maximum)
              .Evaluate(new[] { "a", "b" }, At)
              .Violations.Count.ShouldBe(expected);
    }

    [Fact]
    public void GivenAListTooShort_WhenBounding_ThenNameTheBound()
    {
        Violation violation = Schema.List(Schema.Text)
                                   .MinCount(3)
                                   .Evaluate(new[] { "a" }, At)
                                   .Violations.ShouldHaveSingleItem();

        violation.Code.ShouldBe(ViolationCodeCatalog.Codes.OutOfRange);
        violation.Message.ShouldBe("Expected lines to hold at least 3 entries.");

        Schema.List(Schema.Text)
              .MaxCount(0)
              .Evaluate(new[] { "a" }, At)
              .Violations.ShouldHaveSingleItem()
              .Message.ShouldBe("Expected lines to hold at most 0 entries.");
    }

    [Fact]
    public void GivenBadEntriesAndABadCount_WhenParsing_ThenReportBoth()
    {
        Schema.List(Schema.Text.NotEmpty())
              .MinCount(5)
              .Evaluate(new[] { string.Empty, string.Empty }, At)
              .Violations.Count.ShouldBe(3);
    }

    [Fact]
    public void GivenNoItemSchema_WhenBuildingAList_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
                   () => Schema.List<string, string>(null!))
              .ParamName.ShouldBe("item");
    }

    [Fact]
    public void GivenNoSchema_WhenAddingACountBound_ThenThrow()
    {
        Should.Throw<ArgumentNullException>(
                   () => ((Schema<IReadOnlyList<string>, IReadOnlyList<string>>)
                       null!).MinCount(1))
              .ParamName.ShouldBe("schema");

        Should.Throw<ArgumentNullException>(
                   () => ((Schema<IReadOnlyList<string>, IReadOnlyList<string>>)
                       null!).MaxCount(1))
              .ParamName.ShouldBe("schema");
    }

    /// <summary>
    /// The bound has to cost one comparison, not one parse per entry. Bounding a
    /// list after parsing it means an over-long payload still does all the work,
    /// which is the whole thing the bound is set to prevent.
    /// </summary>
    [Fact]
    public void GivenAListPastTheBound_WhenParsing_ThenParseNoEntryAtAll()
    {
        var item = new Counting<string>(new PassThrough<string>());

        Outcome<IReadOnlyList<string>> outcome = Schema.List(item)
           .MaxCount(2)
           .Evaluate(new[] { "a", "b", "c" }, At);

        item.Evaluations.ShouldBe(0);

        outcome.Violations.ShouldHaveSingleItem()
               .Message.ShouldBe("Expected lines to hold at most 2 entries.");
    }

    [Fact]
    public void GivenAListWithinTheBound_WhenParsing_ThenParseEveryEntry()
    {
        var item = new Counting<string>(new PassThrough<string>());

        Schema.List(item)
              .MaxCount(3)
              .Evaluate(new[] { "a", "b", "c" }, At)
              .Violations.ShouldBeEmpty();

        item.Evaluations.ShouldBe(3);
    }

    [Fact]
    public async Task
        GivenAListPastTheBound_WhenParsingAsynchronously_ThenParseNoEntryAtAll()
    {
        var item = new Counting<string>(new AsyncPassThrough<string>());

        Outcome<IReadOnlyList<string>> outcome = await Schema.List(item)
           .MaxCount(2)
           .EvaluateAsync(
                new[] { "a", "b", "c" },
                At,
                TestContext.Current.CancellationToken);

        item.Evaluations.ShouldBe(0);
        outcome.Violations.ShouldHaveSingleItem();
    }

    /// <summary>
    /// Without a bound set, a list of bad entries would otherwise report one
    /// violation per entry, each holding the value it rejected. The report stops
    /// at the cap and says that it did, so a caller never reads a partial report
    /// as a complete one.
    /// </summary>
    [Fact]
    public void GivenMoreBadEntriesThanTheCap_WhenParsing_ThenStopAndSaySo()
    {
        var entries = new string[Caps.ViolationsPerNode * 4];

        for (var index = 0; index < entries.Length; index++)
        {
            entries[index] = string.Empty;
        }

        Outcome<IReadOnlyList<string>> outcome =
            Schema.List(Schema.Text.NotEmpty()).Evaluate(entries, At);

        outcome.Violations.Count.ShouldBe(Caps.ViolationsPerNode + 1);

        Violation last = outcome.Violations[Caps.ViolationsPerNode];

        last.Code.ShouldBe(ViolationCodeCatalog.Codes.Truncated);
        last.Path.ToString().ShouldBe("lines");

        last.Message.ShouldBe("Stopped after 64 problems; there are more.");
    }

    /// <summary>
    /// The boundary the cap is easiest to get wrong at. Reaching the cap on the last
    /// entry loses nothing, so claiming "there are more" would be a lie — and it
    /// would also fail a list whose entries only needed refining.
    /// </summary>
    [Fact]
    public void GivenExactlyTheCapOfBadEntries_WhenParsing_ThenReportNoTruncation()
    {
        var entries = new string[Caps.ViolationsPerNode];

        for (var index = 0; index < entries.Length; index++)
        {
            entries[index] = string.Empty;
        }

        Outcome<IReadOnlyList<string>> outcome =
            Schema.List(Schema.Text.NotEmpty()).Evaluate(entries, At);

        outcome.Violations.Count.ShouldBe(Caps.ViolationsPerNode);

        foreach (Violation violation in outcome.Violations)
        {
            violation.Code.ShouldNotBe(ViolationCodeCatalog.Codes.Truncated);
        }
    }

    [Fact]
    public void GivenMoreBadEntriesThanTheCap_WhenParsing_ThenStopParsingToo()
    {
        var item = new Counting<string>(new Rejects<string>());
        var entries = new string[Caps.ViolationsPerNode * 4];

        for (var index = 0; index < entries.Length; index++)
        {
            entries[index] = "x";
        }

        Schema.List(item).Evaluate(entries, At);

        item.Evaluations.ShouldBe(Caps.ViolationsPerNode);
    }

    [Fact]
    public void GivenFewerBadEntriesThanTheCap_WhenParsing_ThenReportNoTruncation()
    {
        Outcome<IReadOnlyList<string>> outcome =
            Schema.List(Schema.Text.NotEmpty())
                  .Evaluate(new[] { string.Empty, string.Empty }, At);

        outcome.Violations.Count.ShouldBe(2);

        foreach (Violation violation in outcome.Violations)
        {
            violation.Code.ShouldNotBe(ViolationCodeCatalog.Codes.Truncated);
        }
    }
}
