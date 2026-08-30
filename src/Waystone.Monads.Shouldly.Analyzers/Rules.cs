namespace Waystone.Monads.Shouldly.Analyzers;

using Microsoft.CodeAnalysis;

internal static class Rules
{
    private const string Usage = nameof(Usage);

    /// <remarks>
    /// <para>
    /// Carries no <c>Unnecessary</c> tag, against the shape of most WM2 rules that
    /// have a fix. The tag fades the reported span in an IDE, which reads as "this
    /// line can go" — but the fix replaces the assertion rather than removing it, and
    /// the span reported here is the whole assertion. Fading it would advertise
    /// deleting a test's only check.
    /// </para>
    /// <para>
    /// Overlaps deliberately with <c>WM2001</c> on the <c>Unwrap</c> shape, which
    /// reports the panicking call itself. The spans differ — <c>WM2001</c> reports the
    /// method name and this rule the whole assertion — and applying this fix resolves
    /// both, because the rewrite is what removes the <c>Unwrap</c>. Suppressing either
    /// one to silence the pair would leave a consumer who is not using this package
    /// with no signal at all.
    /// </para>
    /// </remarks>
    public static readonly DiagnosticDescriptor RawAssertion = Idiom(
        "WMS2001",
        "Prefer a monad assertion over asserting on its parts",
        "'{0}' is read off the '{1}' first, so a failure names that value rather than the monad's state. Use '{2}' instead.",
        "Reading IsSome or IsOk yields a bool and Unwrap yields the contained value, so the assertion that follows never sees the monad. A failing test then reports that True was expected, or throws from Unwrap before the assertion runs, instead of naming the None or Err it found. The assertions in Waystone.Monads.Shouldly take the monad itself and report both its state and its contents.");

    /// <remarks>
    /// Scoped to an await of <c>Task&lt;T&gt;</c> or <c>ValueTask&lt;T&gt;</c> written
    /// directly, and to an assertion whose result nothing else reads. Both exclusions
    /// exist because the fix moves the <c>await</c> outward, and outside those two
    /// cases the move changes what is awaited: a <c>ConfigureAwait</c> receiver has no
    /// assertion declared on it, and a chained member access would bind to the
    /// assertion's task rather than its value.
    /// </remarks>
    public static readonly DiagnosticDescriptor ParenthesisedAwaitAssertion =
        Idiom(
            "WMS2002",
            "Prefer the awaited assertion over awaiting the receiver",
            "'{0}' runs on an await wrapped in parentheses. Use 'await' with '{1}', which is declared on the task itself.",
            "Member access binds tighter than await, so asserting on a task's result forces the await into parentheses. Waystone.Monads.Shouldly declares every assertion on Task and ValueTask receivers as well, so the parentheses are not needed.");

    private static DiagnosticDescriptor Idiom(
        string id,
        string title,
        string messageFormat,
        string description,
        params string[] tags) =>
        Create(
            id,
            title,
            messageFormat,
            description,
            Usage,
            DiagnosticSeverity.Info,
            true,
            tags);

    private static DiagnosticDescriptor Create(
        string id,
        string title,
        string messageFormat,
        string description,
        string category,
        DiagnosticSeverity severity,
        bool enabledByDefault,
        params string[] tags) =>
        new DiagnosticDescriptor(
            id,
            title,
            messageFormat,
            category,
            severity,
            enabledByDefault,
            description,
            "https://draekien-industries.wpei.me/using-the-library/analyzer-rules#"
          + id.ToLowerInvariant(),
            tags);
}
