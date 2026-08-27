namespace Waystone.Monads.Results.Extensions;

using System.Threading.Tasks;
using Waystone.SourceGenerators;

/// <summary>
/// Applies <c>UnwrapOrElse</c> and <c>UnwrapOrElseAsync</c> to a
/// <see cref="Result{TOk,TErr}" /> that is still inside a
/// <see cref="Task{TResult}" /> or <see cref="ValueTask{TResult}" />.
/// </summary>
/// <remarks>
/// Nothing here is hand-written. Every member named below is declared on
/// <see cref="Result{TOk,TErr}" /> itself, and the awaited-receiver generator
/// emits each overload onto a <see cref="Task{TResult}" /> and a
/// <see cref="ValueTask{TResult}" /> receiver.
/// </remarks>
[GenerateAwaitedReceivers(typeof(Result<,>))]
[GenerateAwaitedMember(nameof(Result<,>.UnwrapOrElse))]
[GenerateAwaitedMember(nameof(Result<,>.UnwrapOrElseAsync))]
public static partial class UnwrapOrElseExtensions;
