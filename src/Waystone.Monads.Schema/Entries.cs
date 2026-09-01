namespace Waystone.Monads.Schemas;

using System.Collections.Generic;

internal sealed class Entries<T> where T : notnull
{
    private readonly T[] _parsed;

    private readonly List<Violation> _violations = new();

    private bool _complete = true;

    internal Entries(int count)
    {
        _parsed = new T[count];
    }

    internal void Take(int index, Outcome<T> outcome)
    {
        _violations.AddRange(outcome.Violations);

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
        Gather.ToOutcome<IReadOnlyList<T>>(_complete, _parsed, _violations);
}
