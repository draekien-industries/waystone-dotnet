namespace Waystone.Monads.Options.Extensions;

using System.Threading.Tasks;
using Waystone.SourceGenerators;

/// <summary>
/// Applies <c>IsNoneOr</c> and <c>IsNoneOrAsync</c> to an
/// <see cref="Option{T}" /> that is still inside a <see cref="Task{TResult}" />
/// or <see cref="ValueTask{TResult}" />.
/// </summary>
/// <remarks>
/// Nothing here is hand-written. Every member named below is declared on
/// <see cref="Option{T}" /> itself, and the awaited-receiver generator emits
/// each onto a <see cref="Task{TResult}" /> and a
/// <see cref="ValueTask{TResult}" /> receiver.
/// </remarks>
[GenerateAwaitedReceivers(typeof(Option<>))]
[GenerateAwaitedMember(nameof(Option<>.IsNoneOr))]
[GenerateAwaitedMember(nameof(Option<>.IsNoneOrAsync))]
public static partial class IsNoneOrExtensions;
