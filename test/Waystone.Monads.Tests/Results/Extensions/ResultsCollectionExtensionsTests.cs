namespace Waystone.Monads.Results.Extensions;

using JetBrains.Annotations;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

[TestSubject(typeof(ResultsCollectionExtensions))]
public sealed class ResultsCollectionExtensionsTests
{
    private static readonly List<Result<int, string>> Mixed =
        new List<Result<int, string>>
        {
            Result.Ok<int, string>(1),
            Result.Err<int, string>("first"),
            Result.Ok<int, string>(2),
            Result.Err<int, string>("second"),
        };

    [Fact]
    public void GivenMixedResults_WhenFlatten_ThenReturnTheOkValues() =>
        Mixed.Flatten().ShouldBe(new[] { 1, 2 });

    [Fact]
    public void GivenMixedResults_WhenFlattenErr_ThenReturnTheErrors() =>
        Mixed.FlattenErr().ShouldBe(new[] { "first", "second" });

    [Fact]
    public void GivenEmptySequence_WhenFlatten_ThenReturnAnEmptySequence() =>
        new List<Result<int, string>>().Flatten().ShouldBeEmpty();

    [Fact]
    public void GivenEmptySequence_WhenFlattenErr_ThenReturnAnEmptySequence() =>
        new List<Result<int, string>>().FlattenErr().ShouldBeEmpty();

    [Fact]
    public void GivenAllErr_WhenFlatten_ThenReturnAnEmptySequence() =>
        new List<Result<int, string>> { Result.Err<int, string>("failed") }
           .Flatten()
           .ShouldBeEmpty();

    [Fact]
    public void GivenMixedResults_WhenPartition_ThenReturnBothSides()
    {
        (IReadOnlyList<int> oks, IReadOnlyList<string> errs) =
            Mixed.Partition();

        oks.ShouldBe(new[] { 1, 2 });
        errs.ShouldBe(new[] { "first", "second" });
    }

    [Fact]
    public void GivenEmptySequence_WhenPartition_ThenReturnTwoEmptySides()
    {
        (IReadOnlyList<int> oks, IReadOnlyList<string> errs) =
            new List<Result<int, string>>().Partition();

        oks.ShouldBeEmpty();
        errs.ShouldBeEmpty();
    }

    [Fact]
    public void GivenCountingSource_WhenPartition_ThenEnumerateOnce()
    {
        CountingSource source = new CountingSource(Mixed);

        source.Partition();

        source.Enumerations.ShouldBe(1);
    }

    [Fact]
    public void GivenThrowingSource_WhenFlatten_ThenDoNotEnumerateEagerly()
    {
        IEnumerable<int> flattened = ThrowingSource().Flatten();

        flattened.Take(1).ToList().ShouldBe(new[] { 1 });
        Should.Throw<InvalidOperationException>(() => flattened.ToList());
    }

    [Fact]
    public void GivenThrowingSource_WhenFlattenErr_ThenDoNotEnumerateEagerly()
    {
        IEnumerable<string> flattened = ThrowingSource().FlattenErr();

        flattened.Take(1).ToList().ShouldBe(new[] { "first" });
        Should.Throw<InvalidOperationException>(() => flattened.ToList());
    }

    [Fact]
    public void GivenAllOk_WhenCollect_ThenReturnOkOfEveryValueInOrder()
    {
        List<Result<int, string>> results = new List<Result<int, string>>
        {
            Result.Ok<int, string>(1),
            Result.Ok<int, string>(2),
            Result.Ok<int, string>(3),
        };

        results.Collect().Unwrap().ShouldBe(new[] { 1, 2, 3 });
    }

    [Fact]
    public void GivenEmptySequence_WhenCollect_ThenReturnOkOfAnEmptyList() =>
        new List<Result<int, string>>().Collect().Unwrap().ShouldBeEmpty();

    [Fact]
    public void GivenMixedResults_WhenCollect_ThenReturnTheFirstError() =>
        Mixed.Collect().UnwrapErr().ShouldBe("first");

    [Fact]
    public void GivenThrowingSource_WhenCollect_ThenStopAtTheFirstError() =>
        ThrowingSource().Collect().UnwrapErr().ShouldBe("first");

    [Fact]
    public async Task
        GivenAllOkStream_WhenCollectAsync_ThenReturnOkOfEveryValueInOrder()
    {
        Result<IReadOnlyList<int>, string> result =
            await OkStream().CollectAsync();

        result.Unwrap().ShouldBe(new[] { 1, 2, 3 });
    }

    [Fact]
    public async Task
        GivenEmptyStream_WhenCollectAsync_ThenReturnOkOfAnEmptyList()
    {
        Result<IReadOnlyList<int>, string> result =
            await EmptyStream().CollectAsync();

        result.Unwrap().ShouldBeEmpty();
    }

    [Fact]
    public async Task
        GivenThrowingStream_WhenCollectAsync_ThenStopAtTheFirstError()
    {
        Result<IReadOnlyList<int>, string> result =
            await ThrowingStream().CollectAsync();

        result.UnwrapErr().ShouldBe("first");
    }

#pragma warning disable CS1998
    private static async IAsyncEnumerable<Result<int, string>> OkStream()
    {
        yield return Result.Ok<int, string>(1);
        yield return Result.Ok<int, string>(2);
        yield return Result.Ok<int, string>(3);
    }

    private static async IAsyncEnumerable<Result<int, string>> EmptyStream()
    {
        yield break;
    }

    private static async IAsyncEnumerable<Result<int, string>> ThrowingStream()
    {
        yield return Result.Ok<int, string>(1);
        yield return Result.Err<int, string>("first");

        throw new InvalidOperationException("Enumerated too far.");
    }
#pragma warning restore CS1998

    private static IEnumerable<Result<int, string>> ThrowingSource()
    {
        yield return Result.Ok<int, string>(1);
        yield return Result.Err<int, string>("first");

        throw new InvalidOperationException("Enumerated too far.");
    }

    private sealed class CountingSource
        : IEnumerable<Result<int, string>>
    {
        private readonly IEnumerable<Result<int, string>> _source;

        public CountingSource(IEnumerable<Result<int, string>> source) =>
            _source = source;

        public int Enumerations { get; private set; }

        public IEnumerator<Result<int, string>> GetEnumerator()
        {
            Enumerations++;

            return _source.GetEnumerator();
        }

        System.Collections.IEnumerator
            System.Collections.IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}
