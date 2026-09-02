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
    /// What one list or dictionary has gathered, and whether that report is missing
    /// anything.
    /// </summary>
    /// <remarks>
    /// Held by both accumulators rather than written into each. A list and a
    /// dictionary differ in what an entry is, not in how a full report is decided,
    /// and the two halves of the decision are easy to conflate: being full is the
    /// signal to stop examining entries, while only a violation that did not fit or
    /// an entry never examined makes the report incomplete. Getting that wrong
    /// appends "there are more" to a report that already lists everything.
    /// </remarks>
    internal sealed class Report
    {
        private readonly ParseContext _context;

        private readonly int _total;

        private readonly List<Violation> _violations = new();

        private bool _dropped;

        private int _examined;

        internal Report(int total, ParseContext context)
        {
            _total = total;
            _context = context;
        }

        /// <summary>
        /// Whether there is no room left. The signal to stop examining entries, not
        /// a claim that anything has been lost.
        /// </summary>
        internal bool IsFull => _violations.Count >= ViolationsPerNode;

        /// <summary>
        /// Whether the report is missing something: a violation that did not fit, or
        /// an entry the caller stopped short of handing over once it was full.
        /// </summary>
        internal bool Truncated => _dropped || _examined < _total;

        /// <summary>Records that one more entry was handed over.</summary>
        internal void Examined() => _examined++;

        /// <summary>Adds what a nested outcome reported, up to the cap.</summary>
        internal void Take(IReadOnlyList<Violation> reported)
        {
            var index = 0;

            for (; index < reported.Count && !IsFull; index++)
            {
                _violations.Add(reported[index]);
            }

            _dropped |= index < reported.Count;
        }

        /// <summary>
        /// The gathered violations, with the truncation violation appended if
        /// anything is missing. Call once.
        /// </summary>
        internal List<Violation> Close()
        {
            if (Truncated)
            {
                _violations.Add(
                    Violations.Create(
                        _context,
                        ViolationCodeCatalog.ToErrorCode(ViolationCode.Truncated),
                        TruncatedMessage,
                        expected: ViolationsPerNode));
            }

            return _violations;
        }
    }
}
