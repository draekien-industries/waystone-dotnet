namespace Waystone.Monads.Options.Extensions;

using System.Threading.Tasks;
using Waystone.SourceGenerators;

/// <summary>
/// Applies <c>Map</c> and <c>MapAsync</c> to an <see cref="Option{T}" /> that
/// is still inside a <see cref="Task{TResult}" /> or
/// <see cref="ValueTask{TResult}" />.
/// </summary>
/// <remarks>
/// Nothing here is hand-written. Both members named below are declared on
/// <see cref="Option{T}" /> itself, and the awaited-receiver generator emits
/// every overload of each onto a <see cref="Task{TResult}" /> and a
/// <see cref="ValueTask{TResult}" /> receiver.
/// </remarks>
[GenerateAwaitedReceivers(typeof(Option<>))]
[GenerateAwaitedMember(nameof(Option<>.Map))]
[GenerateAwaitedMember(nameof(Option<>.MapAsync))]
public static partial class MapExtensions;
