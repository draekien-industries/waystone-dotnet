namespace Waystone.Monads.Schemas;

using System.Collections.Generic;

internal sealed class Entries<T> where T : notnull
{
    private readonly ParseContext _context;

    private readonly T[] _parsed;

    private readonly List<Violation> _violations = new();

    private bool _complete = true;

    private bool _truncated;

    internal Entries(int count, ParseContext context)
    {
        _parsed = new T[count];
        _context = context;
    }

    internal bool IsFull => _truncated;

    internal void Take(int index, Outcome<T> outcome)
    {
        _truncated |= Caps.Gather(_violations, outcome.Violations);

        if (outcome.HasValue)
        {
            _parsed[index] = outcome.Value;
        }
        else
        {
            _complete = false;
        }
    }

    internal Outcome<IReadOnlyList<T>> ToOutcome()
    {
        if (_truncated)
        {
            _complete = false;
            Caps.Truncate(_violations, _context);
        }

        return Gather.ToOutcome<IReadOnlyList<T>>(
            _complete,
            _parsed,
            _violations);
    }
}
