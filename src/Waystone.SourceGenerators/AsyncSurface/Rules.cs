namespace Waystone.SourceGenerators.AsyncSurface;

using Microsoft.CodeAnalysis;

internal static class Rules
{
    /// <summary>
    /// Sits beside <see cref="TaskReturningMonad" /> and is the half that carries
    /// the intent: only a delegate returning one of this library's own monads is
    /// converted, because only this library's chains produce that shape. A delegate
    /// returning an arbitrary type keeps <c>Task</c>, so
    /// <c>MapAsync(client.GetStringAsync)</c> still binds.
    /// </summary>
    public static readonly DiagnosticDescriptor TaskReturningStepDelegate = new(
        "WSG0003",
        "Do not take a delegate returning a Task of Option or Result",
        "Parameter '{0}' of '{1}' takes a delegate returning '{2}', so an "
      + "async chain cannot be passed to it by name; take a delegate "
      + "returning '{3}'",
        "Usage",
        DiagnosticSeverity.Error,
        true,
        "A method group returning a ValueTask does not convert to a delegate "
      + "parameter declared with Task, and the compiler reports that as CS0411 "
      + "against the call site rather than the declaration. So a parameter "
      + "declared with Task compiles cleanly here and fails only for the "
      + "caller who tries to reuse a chain.");

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
