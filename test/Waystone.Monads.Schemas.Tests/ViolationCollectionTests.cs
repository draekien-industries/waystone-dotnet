namespace Waystone.Monads.Schemas;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Shouldly;
using Waystone.Monads.Results.Errors;
using Xunit;

public sealed class ViolationCollectionTests
{
    private static Violation At(string path, ErrorCode code, string message) =>
        new(
            path.Length == 0 ? ViolationPath.Root : ViolationPath.Root.Append(path),
            code,
            message);

    private static ViolationCollection Sut() => new(
        new[]
        {
            At("email", ViolationCodeCatalog.Codes.Malformed, "Not an email."),
            At("email", ViolationCodeCatalog.Codes.OutOfRange, "Too long."),
            At("total", ViolationCodeCatalog.Codes.OutOfRange, "Not positive."),
            At("", ViolationCodeCatalog.Codes.Conflicting, "End before start."),
        });

    [Fact]
    public void GivenNull_WhenConstructing_ThenThrow() =>
        Should.Throw<ArgumentNullException>(() => new ViolationCollection(null!));

    [Fact]
    public void GivenNoViolations_WhenConstructing_ThenThrow() =>
        Should.Throw<ArgumentException>(
            () => new ViolationCollection(Array.Empty<Violation>()));

    [Fact]
    public void GivenViolations_WhenReadingCount_ThenReturnHowManyWereGiven()
    {
        Sut().Count.ShouldBe(4);
    }

    [Fact]
    public void GivenViolations_WhenIndexing_ThenKeepDeclarationOrder()
    {
        ViolationCollection sut = Sut();

        sut[0].Message.ShouldBe("Not an email.");
        sut[3].Message.ShouldBe("End before start.");
    }

    [Fact]
    public void GivenViolations_WhenEnumerating_ThenYieldEveryOneInOrder()
    {
        Sut().Select(violation => violation.Message)
             .ShouldBe(
                  new[]
                  {
                      "Not an email.",
                      "Too long.",
                      "Not positive.",
                      "End before start.",
                  });
    }

    [Fact]
    public void GivenViolations_WhenEnumeratingAsIEnumerable_ThenYieldTheSameItems()
    {
        var enumerated = new List<object?>();

        foreach (object? item in (IEnumerable)Sut()) enumerated.Add(item);

        enumerated.Count.ShouldBe(4);
    }

    [Fact]
    public void GivenViolationsAtSeveralPaths_WhenGroupingByPath_ThenKeyOnTheRenderedPath()
    {
        IReadOnlyDictionary<string, IReadOnlyList<Violation>> grouped =
            Sut().ByPath();

        grouped.Count.ShouldBe(3);
        grouped["email"].Count.ShouldBe(2);
        grouped["total"].Count.ShouldBe(1);
        grouped[string.Empty].Count.ShouldBe(1);
        grouped["email"][0].Message.ShouldBe("Not an email.");
    }

    [Fact]
    public void GivenViolationsOfSeveralKinds_WhenGroupingByCode_ThenKeyOnTheErrorCode()
    {
        IReadOnlyDictionary<ErrorCode, IReadOnlyList<Violation>> grouped =
            Sut().ByCode();

        grouped.Count.ShouldBe(3);
        grouped[ViolationCodeCatalog.Codes.OutOfRange].Count.ShouldBe(2);
        grouped.ContainsKey(ViolationCodeCatalog.Codes.Duplicate).ShouldBeFalse();
    }

    [Fact]
    public void GivenADomainCode_WhenGroupingByCode_ThenGroupItAlongsideTheBuiltInOnes()
    {
        var sut = new ViolationCollection(
            new[]
            {
                At("lines", new ErrorCode("order.too_many"), "Too many lines."),
                At("email", ViolationCodeCatalog.Codes.Malformed, "Not an email."),
            });

        sut.ByCode()[new ErrorCode("order.too_many")].Count.ShouldBe(1);
    }

    [Fact]
    public void GivenViolations_WhenRenderingAsProblemDetails_ThenMapEachPathToItsMessages()
    {
        IDictionary<string, string[]> rendered = Sut().ToDictionary();

        rendered.Count.ShouldBe(3);
        rendered["email"].ShouldBe(new[] { "Not an email.", "Too long." });
        rendered[string.Empty].ShouldBe(new[] { "End before start." });
    }

    [Fact]
    public void GivenTwoCalls_WhenGrouping_ThenReturnSeparateDictionaries()
    {
        ViolationCollection sut = Sut();

        sut.ByPath().ShouldNotBeSameAs(sut.ByPath());
        sut.ByCode().ShouldNotBeSameAs(sut.ByCode());
        sut.ToDictionary().ShouldNotBeSameAs(sut.ToDictionary());
    }
}
