namespace Waystone.Monads.Schemas;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Waystone.Monads.Results.Errors;

/// <summary>Holds every reason one parse failed, in the order the schema found them.</summary>
/// <remarks>
/// <para>
/// Never empty. A parse that found nothing wrong produces no collection at all,
/// so code reading one does not have to treat "failed with no violations" as a
/// case.
/// </para>
/// <para>
/// The order is the schema's declaration order, field by field, which is the
/// order a reader of the schema would expect rather than the order the checks
/// happened to run. Grouping through <see cref="ByPath" /> or <see cref="ByCode" />
/// preserves it inside each group.
/// </para>
/// <para>
/// Named for the suffix .NET requires on a public type implementing a collection
/// interface, not because a shorter plural would read worse.
/// </para>
/// </remarks>
public sealed class ViolationCollection : IReadOnlyList<Violation>
{
    private readonly IReadOnlyList<Violation> _violations;

    internal ViolationCollection(IReadOnlyList<Violation> violations)
    {
        if (violations is null) throw new ArgumentNullException(nameof(violations));

        if (violations.Count == 0)
        {
            throw new ArgumentException(
                "A violation collection reports a failure, so it cannot be empty.",
                nameof(violations));
        }

        _violations = violations;
    }

    /// <summary>Gets how many violations the parse reported.</summary>
    /// <remarks>Always one or more.</remarks>
    public int Count => _violations.Count;

    /// <summary>Gets the violation at a position in the schema's declaration order.</summary>
    /// <param name="index">
    /// The zero-based position. Must be below <see cref="Count" />.
    /// </param>
    /// <returns>The violation at that position.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// If <paramref name="index" /> is negative or at least <see cref="Count" />.
    /// </exception>
    public Violation this[int index] => _violations[index];

    /// <inheritdoc />
    public IEnumerator<Violation> GetEnumerator() => _violations.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>Groups the violations by the place each one is about.</summary>
    /// <remarks>
    /// Keyed by the rendered path, so a root-level violation lands under the empty
    /// string. Builds a fresh dictionary on every call rather than caching one, so
    /// hold the result if you need it twice.
    /// </remarks>
    /// <returns>
    /// Each distinct path, against the violations at it in declaration order.
    /// </returns>
    public IReadOnlyDictionary<string, IReadOnlyList<Violation>> ByPath() =>
        _violations.GroupBy(violation => violation.Path.ToString(), StringComparer.Ordinal)
                   .ToDictionary(
                        group => group.Key,
                        group => (IReadOnlyList<Violation>)group.ToArray(),
                        StringComparer.Ordinal);

    /// <summary>Groups the violations by what kind of failure each one is.</summary>
    /// <remarks>
    /// Use this to answer questions about the failure as a whole — whether
    /// anything was merely absent, say, as against malformed. Keyed by
    /// <see cref="ErrorCode" /> rather than <see cref="ViolationCode" />, so a
    /// schema's own codes group alongside the built-in ones; look a built-in one up
    /// through <c>ViolationCodeCatalog.Codes</c>. Builds a fresh dictionary on every
    /// call.
    /// </remarks>
    /// <returns>
    /// Each code that occurred, against the violations carrying it in declaration
    /// order. A code that did not occur is absent rather than present and empty.
    /// </returns>
    public IReadOnlyDictionary<ErrorCode, IReadOnlyList<Violation>> ByCode() =>
        _violations.GroupBy(violation => violation.Code)
                   .ToDictionary(
                        group => group.Key,
                        group => (IReadOnlyList<Violation>)group.ToArray());

    /// <summary>Renders the violations as a problem-details payload.</summary>
    /// <remarks>
    /// The shape ASP.NET Core's <c>ValidationProblem</c> and model state expect:
    /// each path against its messages. Matches what the FluentValidation companion
    /// package produces, so a handler written for one accepts the other. Builds a
    /// fresh dictionary on every call.
    /// </remarks>
    /// <returns>Each distinct rendered path, against its messages in declaration order.</returns>
    public IDictionary<string, string[]> ToDictionary() =>
        ByPath().ToDictionary(
            entry => entry.Key,
            entry => entry.Value.Select(violation => violation.Message).ToArray(),
            StringComparer.Ordinal);
}
