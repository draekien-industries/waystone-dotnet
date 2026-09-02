namespace Waystone.Monads.Schemas.Internal.Structures;

using System.Collections.Generic;

internal sealed class Entries<T> where T : notnull
{
    private readonly T[] _parsed;

    private readonly Caps.Report _report;

    private bool _complete = true;

    internal Entries(int count, ParseContext context)
    {
        _parsed = new T[count];
        _report = new Caps.Report(count, context);
    }

    internal bool IsFull => _report.IsFull;

    internal void Take(int index, Outcome<T> outcome)
    {
        _report.Examined();
        _report.Take(outcome.Violations);

        if (outcome.HasValue)
        {
            _parsed[index] = outcome.Value;
        }
        else
        {
            _complete = false;
        }
    }

    internal Outcome<IReadOnlyList<T>> ToOutcome() =>
        Gather.ToOutcome<IReadOnlyList<T>>(
            _complete && !_report.Truncated,
            _parsed,
            _report.Close());
}
