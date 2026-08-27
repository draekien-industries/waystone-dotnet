namespace Waystone.Monads.Options.Extensions;

using System.Threading.Tasks;
using Waystone.SourceGenerators;

/// <summary>
/// Applies <c>MapOrNull</c> and <c>MapOrNullAsync</c> to an
/// <see cref="Option{T}" /> that is still inside a <see cref="Task{TResult}" />
/// or <see cref="ValueTask{TResult}" />.
/// </summary>
/// <remarks>
/// Nothing here is hand-written. Every member named below is declared on
/// <see cref="Option{T}" /> itself, and the awaited-receiver generator emits
/// each overload onto a <see cref="Task{TResult}" /> and a
/// <see cref="ValueTask{TResult}" /> receiver.
/// </remarks>
[GenerateAwaitedReceivers(typeof(Option<>))]
[GenerateAwaitedMember(nameof(Option<>.MapOrNull))]
[GenerateAwaitedMember(nameof(Option<>.MapOrNullAsync))]
public static partial class MapOrNullExtensions;
