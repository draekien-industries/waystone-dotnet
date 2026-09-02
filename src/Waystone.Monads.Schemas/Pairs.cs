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

    private readonly int _count;

    private bool _dropped;

    private int _examined;

    internal Pairs(int count, ParseContext context)
    {
        _parsed = new Dictionary<TKey, TValue>(count);
        _count = count;
        _context = context;
    }

    internal bool IsFull => Caps.IsFull(_violations);

    /// <summary>
    /// Whether the report is missing something: a violation that did not fit, or an
    /// entry the caller stopped short of handing over once the list was full.
    /// </summary>
    private bool Truncated => _dropped || _examined < _count;

    internal void Take(
        ParseContext at,
        Outcome<TKey> key,
        Outcome<TValue> value)
    {
        _examined++;
        _dropped |= Caps.Gather(_violations, key.Violations);
        _dropped |= Caps.Gather(_violations, value.Violations);

        if (!key.HasValue || !value.HasValue)
        {
            _complete = false;

            return;
        }

        if (_parsed.ContainsKey(key.Value))
        {
            _dropped |= Caps.Gather(
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
        if (Truncated)
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
