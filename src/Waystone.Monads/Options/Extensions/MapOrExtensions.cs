namespace Waystone.Monads.Options.Extensions;

using System.Threading.Tasks;
using Waystone.SourceGenerators;

/// <summary>
/// Applies <c>MapOr</c> and <c>MapOrAsync</c> to an <see cref="Option{T}" />
/// that is still inside a <see cref="Task{TResult}" /> or
/// <see cref="ValueTask{TResult}" />.
/// </summary>
/// <remarks>
/// Nothing here is hand-written. Every member named below is declared on
/// <see cref="Option{T}" /> itself, and the awaited-receiver generator emits
/// each overload onto a <see cref="Task{TResult}" /> and a
/// <see cref="ValueTask{TResult}" /> receiver.
/// </remarks>
[GenerateAwaitedReceivers(typeof(Option<>))]
[GenerateAwaitedMember(nameof(Option<>.MapOr))]
[GenerateAwaitedMember(nameof(Option<>.MapOrAsync))]
public static partial class MapOrExtensions;
