namespace Waystone.Monads.Schemas;

using System.Collections.Generic;

internal sealed class Pairs<TKey, TValue>
    where TKey : notnull where TValue : notnull
{
    private const string DuplicateMessage =
        "Expected {Path} to parse to a key of its own, but an earlier entry "
      + "already produced {Received}.";

    private readonly Dictionary<TKey, TValue> _parsed;

    private readonly List<Violation> _violations = new();

    private bool _complete = true;

    internal Pairs(int count)
    {
        _parsed = new Dictionary<TKey, TValue>(count);
    }

    internal void Take(
        ParseContext at,
        Outcome<TKey> key,
        Outcome<TValue> value)
    {
        _violations.AddRange(key.Violations);
        _violations.AddRange(value.Violations);

        if (!key.HasValue || !value.HasValue)
        {
            _complete = false;

            return;
        }

        if (_parsed.ContainsKey(key.Value))
        {
            _violations.Add(
                Violations.Create(
                    at,
                    ViolationCodeCatalog.Codes.Duplicate,
                    DuplicateMessage,
                    key.Value));

            _complete = false;

            return;
        }

        _parsed.Add(key.Value, value.Value);
    }

    internal Outcome<IReadOnlyDictionary<TKey, TValue>> ToOutcome() =>
        Gather.ToOutcome<IReadOnlyDictionary<TKey, TValue>>(
            _complete,
            _parsed,
            _violations);
}
