namespace Waystone.SourceGenerators.AsyncSurface;

using Microsoft.CodeAnalysis;

internal static class Rules
{
    /// <summary>
    /// Reported at the declaration, not the call site, because the declaration is
    /// the only place the correction is known. A caller who trips over this sees
    /// <c>CS0411</c> instead, which names neither <c>ValueTask</c> nor the
    /// parameter — the diagnostic quality that made this rule worth writing.
    /// </summary>
    public static readonly DiagnosticDescriptor TaskReturningMonad = new(
        "WSG0004",
        "Do not return a Task of Option or Result",
        "'{0}' returns '{1}', so a chain ending in it cannot be passed as a step "
      + "to 'AndThenAsync' or 'OrElseAsync'; return '{2}' instead",
        "Usage",
        DiagnosticSeverity.Error,
        true,
        "A ValueTask is not convertible to a Task, so a member declared with Task "
      + "compiles cleanly here and fails only where a caller tries to compose it. "
      + "The step-shaped delegate parameters take ValueTask, which makes every "
      + "Task-returning member of this library a link that cannot be chained.");
}
