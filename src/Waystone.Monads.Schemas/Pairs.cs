namespace Waystone.Monads.Schemas;

using System.Collections.Generic;

internal sealed class Pairs<TKey, TValue>
    where TKey : notnull where TValue : notnull
{
    private const string DuplicateMessage =
        "Expected {Path} to parse to a key of its own, but an earlier entry "
      + "already produced {Received}.";

    private readonly ParseContext _context;

    private readonly Dictionary<TKey, TValue> _parsed;

    private readonly List<Violation> _violations = new();

    private bool _complete = true;

    private bool _truncated;

    internal Pairs(int count, ParseContext context)
    {
        _parsed = new Dictionary<TKey, TValue>(count);
        _context = context;
    }

    internal bool IsFull => _truncated;

    internal void Take(
        ParseContext at,
        Outcome<TKey> key,
        Outcome<TValue> value)
    {
        _truncated |= Caps.Gather(_violations, key.Violations);
        _truncated |= Caps.Gather(_violations, value.Violations);

        if (!key.HasValue || !value.HasValue)
        {
            _complete = false;

            return;
        }

        if (_parsed.ContainsKey(key.Value))
        {
            _truncated |= Caps.Gather(
                _violations,
                Violations.One(
                    at,
                    ViolationCodeCatalog.Codes.Duplicate,
                    DuplicateMessage,
                    key.Value));

            _complete = false;

            return;
        }

        _parsed.Add(key.Value, value.Value);
    }

    internal Outcome<IReadOnlyDictionary<TKey, TValue>> ToOutcome()
    {
        if (_truncated)
        {
            _complete = false;
            Caps.Truncate(_violations, _context);
        }

        return Gather.ToOutcome<IReadOnlyDictionary<TKey, TValue>>(
            _complete,
            _parsed,
            _violations);
    }
}
