namespace Waystone.Monads.Options.Extensions;

using System.Threading.Tasks;
using Waystone.SourceGenerators;

/// <summary>
/// Applies <c>Reduce</c> and <c>ReduceAsync</c> to an <see cref="Option{T}" />
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
[GenerateAwaitedMember(nameof(Option<>.Reduce))]
[GenerateAwaitedMember(nameof(Option<>.ReduceAsync))]
public static partial class ReduceExtensions;
