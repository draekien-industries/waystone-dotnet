using Waystone.Monads.Results;
using Waystone.Monads.Results.Extensions;

namespace Waystone.Monads.Docs.Core.Sample.Reference.ResultApi;

/// <summary>reference/result/collections.md</summary>
internal static class ResultCollections
{
    internal sealed record Report(int Count);

    internal static void Flatten()
    {
        List<Result<int, string>> results =
        [
            Result.Ok<int, string>(1),
            Result.Err<int, string>("bad"),
            Result.Ok<int, string>(3),
        ];

        IEnumerable<int> values = results.Flatten();
        //               ^? [1, 3]

        IEnumerable<string> errors = results.FlattenErr();
        //                  ^? ["bad"]

        _ = (values, errors);
    }

    internal static void Partition()
    {
        List<Result<int, string>> results =
        [
            Result.Ok<int, string>(1),
            Result.Err<int, string>("bad"),
            Result.Ok<int, string>(3),
        ];

        (IReadOnlyList<int> oks, IReadOnlyList<string> errs) = results.Partition();
        //                  ^? [1, 3]              ^? ["bad"]

        _ = (oks, errs);
    }

    internal static Result<Report, IReadOnlyList<string>> PartitionAtABoundary(
        IEnumerable<string> items)
    {
        var (succeeded, failed) = items.Select(Validate).Partition();

        if (failed.Count > 0)
        {
            return Result.Err<Report, IReadOnlyList<string>>(failed);
        }

        return Result.Ok<Report, IReadOnlyList<string>>(new Report(succeeded.Count));
    }

    internal static void Collect()
    {
        List<Result<int, string>> results =
        [
            Result.Ok<int, string>(1),
            Result.Ok<int, string>(3),
        ];

        Result<IReadOnlyList<int>, string> all = results.Collect();
        //                                 ^? Ok([1, 3])

        _ = all;
    }

    internal static void CollectStopsAtTheFirstFailure()
    {
        List<Result<int, string>> withAFailure =
        [
            Result.Ok<int, string>(1),
            Result.Err<int, string>("bad"),
            Result.Err<int, string>("worse"),
        ];

        Result<IReadOnlyList<int>, string> all = withAFailure.Collect();
        //                                 ^? Err("bad")

        _ = all;
    }

    internal static async Task CollectAsync(
        IAsyncEnumerable<Result<int, string>> stream,
        CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<int>, string> all =
            await stream.CollectAsync(cancellationToken);

        _ = all;
    }

    internal static void AsEnumerable()
    {
        Result<int, string> result = Result.Ok<int, string>(1);

        IEnumerable<int> sequence = result.AsEnumerable();
        //               ^? [1], and [] for an Err

        _ = sequence;
    }

    private static Result<int, string> Validate(string item) =>
        item.Length > 0
            ? Result.Ok<int, string>(item.Length)
            : Result.Err<int, string>("empty");
}
