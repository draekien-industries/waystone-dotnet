namespace Waystone.Monads;

using Options;
using Results;
using Shouldly;
using System;
using System.Collections.Generic;
using Xunit;

public sealed class CaseDispatchTests
{
    public static TheoryData<Option<int>> Options() =>
        new TheoryData<Option<int>> { Option.Some(1), Option.None<int>() };

    public static TheoryData<Result<int, string>> Results() =>
        new TheoryData<Result<int, string>>
        {
            Result.Ok<int, string>(1),
            Result.Err<int, string>("failed"),
        };

    [Theory]
    [MemberData(nameof(Options))]
    public void OptionAndMatchesTheMatchExpressedOriginal(Option<int> option) =>
        option.And(Option.Some("other"))
           .ShouldBe(option.Match(_ => Option.Some("other"), Option.None<string>));

    [Theory]
    [MemberData(nameof(Options))]
    public void OptionMapOrDefaultMatchesTheMatchExpressedOriginal(
        Option<int> option) =>
        option.MapOrDefault(value => value + 1)
           .ShouldBe(option.Match(value => value + 1, () => default(int)));

    [Theory]
    [MemberData(nameof(Options))]
    public void OptionAsEnumerableMatchesTheMatchExpressedOriginal(
        Option<int> option) =>
        option.AsEnumerable()
           .ShouldBe(
                option.Match<IEnumerable<int>>(
                    value => new[] { value },
                    Array.Empty<int>));

    [Theory]
    [MemberData(nameof(Options))]
    public void OptionReduceMatchesTheMatchExpressedOriginal(Option<int> option)
    {
        foreach (var other in new[] { Option.Some(2), Option.None<int>() })
        {
            option.Reduce(other, (a, b) => a + b)
               .ShouldBe(
                    option.Match(
                        value => other.Match<Option<int>>(
                            otherValue => value + otherValue,
                            () => value),
                        () => other));
        }
    }

    [Theory]
    [MemberData(nameof(Results))]
    public void ResultMapOrDefaultMatchesTheMatchExpressedOriginal(
        Result<int, string> result) =>
        result.MapOrDefault(value => value + 1)
           .ShouldBe(result.Match(value => value + 1, _ => default(int)));

    [Theory]
    [MemberData(nameof(Results))]
    public void ResultAsEnumerableMatchesTheMatchExpressedOriginal(
        Result<int, string> result) =>
        result.AsEnumerable()
           .ShouldBe(
                result.Match<IEnumerable<int>>(
                    value => new[] { value },
                    _ => Array.Empty<int>()));

    [Theory]
    [MemberData(nameof(Options))]
    public void EveryOptionCaseStillRoundTripsThroughMatch(Option<int> option) =>
        option.Match(Option.Some, Option.None<int>).ShouldBe(option);

    [Theory]
    [MemberData(nameof(Results))]
    public void EveryResultCaseStillRoundTripsThroughMatch(
        Result<int, string> result) =>
        result.Match(Result.Ok<int, string>, Result.Err<int, string>)
           .ShouldBe(result);
}
