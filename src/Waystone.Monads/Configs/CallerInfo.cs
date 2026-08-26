namespace Waystone.Monads.Configs;

#if !DEBUG
using System.Diagnostics;
#endif

/// <summary>Describes the call site whose exception a <c>Try</c> method caught.</summary>
/// <remarks>
/// The library builds this from the compiler-supplied
/// <see cref="System.Runtime.CompilerServices.CallerMemberNameAttribute" />,
/// <see cref="System.Runtime.CompilerServices.CallerArgumentExpressionAttribute" />
/// and <see cref="System.Runtime.CompilerServices.CallerLineNumberAttribute" />
/// values at the <c>Try</c> or <c>TryAsync</c> call, then reports it on the
/// <see cref="Diagnostics.ExceptionHandled" /> diagnostic event and hands it to
/// any logger registered through
/// <see cref="MonadOptions.UseExceptionLogger" />. Do not supply these values
/// yourself.
/// </remarks>
/// <param name="MemberName">The member that called <c>Try</c>.</param>
/// <param name="ArgumentExpression">
/// The source text of the delegate argument passed to <c>Try</c>.
/// </param>
/// <param name="LineNumber">
/// The line in the caller's file where the <c>Try</c> call appears.
/// </param>
#if !DEBUG
[DebuggerStepThrough]
#endif
public record CallerInfo(
    string MemberName,
    string ArgumentExpression,
    int LineNumber);
