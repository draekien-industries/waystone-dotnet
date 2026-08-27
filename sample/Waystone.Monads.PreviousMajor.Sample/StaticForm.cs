namespace Waystone.Monads.PreviousMajor.Sample;

using System.Threading.Tasks;
using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;
using static Waystone.Monads.Options.Option;

/// <summary>
/// Calls that name an extension class rather than reducing to a receiver, plus a
/// <c>using static</c> on the factory class. <c>DRA-111</c> collapses the
/// per-family extension classes, so <c>AndThenExtensions</c> below names a type
/// that will no longer exist and the qualified calls are that issue's inventory.
/// The reduced calls elsewhere in this project are the control: they bind through
/// the namespace and should not move.
/// </summary>
internal static class StaticForm
{
    internal static Option<int> ViaUsingStatic() => Some(5);

    internal static Option<int> ViaUsingStaticNone() => None<int>();

    internal static ValueTask<Result<int, Error>> ViaQualifiedStatic(
        Result<int, Error> result) =>
        AndThenExtensions.AndThenAsync(
            result,
            value => Task.FromResult(Result.Ok<int>(value)));

    /// <summary>
    /// The compatibility static form is the only place a caller can name the
    /// receiver, so it is the only place <c>DRA-110</c>'s receiver rename on
    /// <c>IsSomeAnd</c> is observable at all.
    /// </summary>
    internal static ValueTask<bool> ViaQualifiedReceiverName(
        ValueTask<Option<int>> optionValueTask) =>
        Options.Extensions.IsSomeAndExtensions.IsSomeAndAsync(
            optionValueTask: optionValueTask,
            predicate: value => Task.FromResult(value > 0));

    internal static ValueTask<Result<int, Error>> ViaQualifiedStaticOnTask(
        Task<Result<int, Error>> resultTask) =>
        AndThenExtensions.AndThenAsync(
            resultTask,
            value => Task.FromResult(Result.Ok<int>(value)));
}
