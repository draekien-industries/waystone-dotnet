namespace Waystone.Monads.Schemas;

using System.Collections.Generic;

internal static class Caps
{
    internal const string TruncatedMessage =
        "Stopped after {Expected} problems; there are more.";

    /// <summary>
    /// How many violations one list or dictionary gathers before it stops and says
    /// so. Sixty-four is well past where a human stops reading a report, and past
    /// that point the report is for a machine, which branches on
    /// <see cref="Violation.Code" /> rather than reading every message. It is also
    /// the nesting depth <c>System.Text.Json</c> defaults to, so the number is
    /// already familiar to anyone who has tuned a parser limit.
    /// </summary>
    /// <remarks>
    /// Fixed rather than configurable, because nobody has asked for another number
    /// and making it configurable later breaks no one.
    /// </remarks>
    internal const int ViolationsPerNode = 64;

    /// <summary>
    /// Whether the gathered list has no room left, which is the signal to stop
    /// examining entries rather than a claim that anything was lost.
    /// </summary>
    internal static bool IsFull(List<Violation> violations) =>
        violations.Count >= ViolationsPerNode;

    /// <summary>
    /// Adds what a nested outcome reported, up to the cap, and reports whether any
    /// of it had to be left out.
    /// </summary>
    /// <remarks>
    /// Reaching the cap is not the same as losing something: a report that fills the
    /// last slot exactly has lost nothing, and saying otherwise would append
    /// "there are more" to a report that lists every problem there is.
    /// </remarks>
    internal static bool Gather(
        List<Violation> violations,
        IReadOnlyList<Violation> reported)
    {
        var index = 0;

        for (; index < reported.Count && !IsFull(violations); index++)
        {
            violations.Add(reported[index]);
        }

        return index < reported.Count;
    }

    internal static void Truncate(
        List<Violation> violations,
        ParseContext context)
    {
        violations.Add(
            Violations.Create(
                context,
                ViolationCodeCatalog.ToErrorCode(ViolationCode.Truncated),
                TruncatedMessage,
                expected: ViolationsPerNode));
    }
}
