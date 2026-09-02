namespace Waystone.Monads.Schemas;

using System.Collections.Generic;

internal sealed class Entries<T> where T : notnull
{
    private readonly ParseContext _context;

    private readonly T[] _parsed;

    private readonly List<Violation> _violations = new();

    private bool _complete = true;

    private bool _dropped;

    private int _examined;

    internal Entries(int count, ParseContext context)
    {
        _parsed = new T[count];
        _context = context;
    }

    internal bool IsFull => Caps.IsFull(_violations);

    /// <summary>
    /// Whether the report is missing something: a violation that did not fit, or an
    /// entry the caller stopped short of handing over once the list was full.
    /// </summary>
    private bool Truncated => _dropped || _examined < _parsed.Length;

    internal void Take(int index, Outcome<T> outcome)
    {
        _examined++;
        _dropped |= Caps.Gather(_violations, outcome.Violations);

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
        if (Truncated)
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
