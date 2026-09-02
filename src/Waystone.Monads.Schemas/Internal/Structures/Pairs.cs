namespace Waystone.Monads.Schemas.Internal.Structures;

using System.Collections.Generic;

internal sealed class Pairs<TKey, TValue>
    where TKey : notnull where TValue : notnull
{
    private const string DuplicateMessage =
        "Expected {Path} to parse to a key of its own, but an earlier entry "
      + "already produced {Received}.";

    private readonly Dictionary<TKey, TValue> _parsed;

    private readonly Caps.Report _report;

    private bool _complete = true;

    internal Pairs(int count, ParseContext context)
    {
        _parsed = new Dictionary<TKey, TValue>(count);
        _report = new Caps.Report(count, context);
    }

    internal bool IsFull => _report.IsFull;

    internal void Take(
        ParseContext at,
        Outcome<TKey> key,
        Outcome<TValue> value)
    {
        _report.Examined();
        _report.Take(key.Violations);
        _report.Take(value.Violations);

        if (!key.HasValue || !value.HasValue)
        {
            _complete = false;

            return;
        }

        if (_parsed.ContainsKey(key.Value))
        {
            _report.Take(
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

    internal Outcome<IReadOnlyDictionary<TKey, TValue>> ToOutcome() =>
        Gather.ToOutcome<IReadOnlyDictionary<TKey, TValue>>(
            _complete && !_report.Truncated,
            _parsed,
            _report.Close());
}
