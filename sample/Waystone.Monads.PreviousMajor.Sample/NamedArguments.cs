namespace Waystone.Monads.PreviousMajor.Sample;

using System.Threading.Tasks;
using Waystone.Monads.Options;
using Waystone.Monads.Options.Extensions;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;
using Waystone.Monads.Results.Extensions;

/// <summary>
/// Every call here names its arguments, which is what makes a parameter rename a
/// source break rather than a cosmetic one. <c>DRA-110</c> settles one naming
/// convention across both the core members and the extensions that forward to
/// them, so calls on both sides are expected to fail as <c>CS1739</c>.
/// </summary>
/// <remarks>
/// The <c>Unchanged</c> members at the end are the control. <c>Match</c> keeps
/// <c>onSome</c>, <c>onNone</c>, <c>onOk</c> and <c>onErr</c>, because those are
/// branch handlers rather than factories; <c>Try</c> keeps <c>factory</c>,
/// because its delegate is the operation rather than a fallback value; and
/// <c>Map</c> keeps <c>map</c>. A <c>CS1739</c> against any of them means the
/// rename reached further than it was meant to.
/// </remarks>
internal static class NamedArguments
{
    private static readonly Error Refused =
        new Error("order.refused", "refused");

    internal static int CoreUnwrapOrElse(Option<int> option) =>
        option.UnwrapOrElse(@else: () => 0);

    internal static int CoreMapOr(Option<int> option) =>
        option.MapOr(@default: 0, map: value => value);

    internal static int CoreMapOrElse(Option<int> option) =>
        option.MapOrElse(createDefault: () => 0, map: value => value);

    internal static Option<int> CoreOrElse(Option<int> option) =>
        option.OrElse(createElse: Option.None<int>);

    internal static Result<int, Error> CoreAndThen(Result<int, Error> result) =>
        result.AndThen(createOther: value => Result.Ok<int>(value + 1));

    internal static Result<int, Error> CoreResultOrElse(
        Result<int, Error> result) =>
        result.OrElse(createOther: error => Result.Ok<int>(0));

    internal static int CoreResultUnwrapOr(Result<int, Error> result) =>
        result.UnwrapOr(@default: 0);

    internal static int CoreResultUnwrapOrElse(Result<int, Error> result) =>
        result.UnwrapOrElse(onErr: error => 0);

    internal static int CoreResultMapOrElse(Result<int, Error> result) =>
        result.MapOrElse(createDefault: error => 0, map: value => value);

    internal static ValueTask<Result<int, Error>> ExtensionAndThenAsync(
        Result<int, Error> result) =>
        result.AndThenAsync(
            factory: value => Task.FromResult(Result.Ok<int>(value + 1)));

    internal static ValueTask<Result<int, Error>> ExtensionOnTask(
        Task<Result<int, Error>> resultTask) =>
        resultTask.OrElseAsync(
            factory: error => Task.FromResult(Result.Err<int>(Refused)));

    /// <summary>
    /// A generated awaited receiver takes its parameter names from the core
    /// member it forwards to, so renaming the core renames this call site too
    /// without anyone editing the family.
    /// </summary>
    internal static ValueTask<int> GeneratedUnwrapOrAsync(
        Task<Result<int, Error>> resultTask) =>
        resultTask.UnwrapOrAsync(@default: 0);

    internal static int UnchangedOptionMatch(Option<int> option) =>
        option.Match(onSome: value => value, onNone: () => 0);

    internal static int UnchangedResultMatch(Result<int, Error> result) =>
        result.Match(onOk: value => value, onErr: error => 0);

    internal static Result<int, Error> UnchangedTry() =>
        Result.Try(factory: () => 1);

    internal static Option<int> UnchangedMap(Option<int> option) =>
        option.Map(map: value => value + 1);
}
