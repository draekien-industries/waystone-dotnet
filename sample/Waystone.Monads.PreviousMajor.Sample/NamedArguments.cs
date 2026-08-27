namespace Waystone.Monads.PreviousMajor.Sample;

using System.Threading.Tasks;
using Waystone.Monads.Options;
using Waystone.Monads.Options.Extensions;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;
using Waystone.Monads.Results.Extensions;

/// <summary>
/// Every call here names its arguments, which is what makes a parameter rename a
/// source break rather than a cosmetic one. <c>DRA-110</c> renames the extension
/// parameters to match the core members they forward to, so the calls on the
/// extensions are expected to fail as <c>CS1739</c> and the calls on the core
/// members are the control that should not move.
/// </summary>
internal static class NamedArguments
{
    private static readonly Error Refused =
        new Error("order.refused", "refused");

    internal static Result<int, Error> CoreAndThen(Result<int, Error> result) =>
        result.AndThen(createOther: value => Result.Ok<int>(value + 1));

    internal static Option<int> CoreMap(Option<int> option) =>
        option.Map(map: value => value + 1);

    internal static int CoreMatch(Option<int> option) =>
        option.Match(onSome: value => value, onNone: () => 0);

    internal static ValueTask<Result<int, Error>> ExtensionAndThenAsync(
        Result<int, Error> result) =>
        result.AndThenAsync(
            factory: value => Task.FromResult(Result.Ok<int>(value + 1)));

    internal static ValueTask<Option<int>> ExtensionMapAsync(
        Option<int> option) =>
        option.MapAsync(map: value => Task.FromResult(value + 1));

    internal static ValueTask<Result<int, Error>> ExtensionOnTask(
        Task<Result<int, Error>> resultTask) =>
        resultTask.OrElseAsync(
            factory: error => Task.FromResult(Result.Err<int>(Refused)));
}
