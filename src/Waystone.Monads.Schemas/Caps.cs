namespace Waystone.Monads.Schemas;

using System.Collections.Generic;

internal static class Caps
{
    internal const string TruncatedMessage =
        "Stopped reporting {Path} after {Expected} problems; there are more.";

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
    /// Adds what a nested outcome reported, up to the cap, and reports whether the
    /// cap has now been reached.
    /// </summary>
    internal static bool Gather(
        List<Violation> violations,
        IReadOnlyList<Violation> reported)
    {
        for (var index = 0;
             index < reported.Count
          && violations.Count < ViolationsPerNode;
             index++)
        {
            violations.Add(reported[index]);
        }

        return violations.Count >= ViolationsPerNode;
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
