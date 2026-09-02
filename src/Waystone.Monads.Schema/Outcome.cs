namespace Waystone.Monads.Schemas;

using System;
using System.Collections.Generic;
using Waystone.Monads.Results;

internal sealed class Outcome<T> where T : notnull
{
    private static readonly IReadOnlyList<Violation> NoViolations =
        Array.Empty<Violation>();

    private readonly T _value;

    private Outcome(bool hasValue, T value, IReadOnlyList<Violation> violations)
    {
        HasValue = hasValue;
        _value = value;
        Violations = violations;
    }

    public bool HasValue { get; }

    public IReadOnlyList<Violation> Violations { get; }

    public T Value => HasValue
        ? _value
        : throw new InvalidOperationException(
              "This outcome carries no value. Check HasValue first.");

    public static Outcome<T> Passed(T value) =>
        new(true, value, NoViolations);

    public static Outcome<T> Failed(IReadOnlyList<Violation> violations) =>
        new(false, default!, Require(violations));

    public static Outcome<T> Refined(
        T value,
        IReadOnlyList<Violation> violations) =>
        new(true, value, Require(violations));

    public Outcome<T> WithViolations(IReadOnlyList<Violation> violations) =>
        HasValue ? Refined(_value, violations) : Failed(violations);

    public Outcome<TNext> WithValue<TNext>(TNext value) where TNext : notnull =>
        Violations.Count == 0
            ? Outcome<TNext>.Passed(value)
            : Outcome<TNext>.Refined(value, Violations);

    public Result<T, SchemaViolation> ToResult() =>
        Violations.Count == 0
            ? Result.Ok<T, SchemaViolation>(_value)
            : Result.Err<T, SchemaViolation>(
                  new SchemaViolation(new ViolationCollection(Violations)));

    private static IReadOnlyList<Violation> Require(
        IReadOnlyList<Violation> violations)
    {
        if (violations is null)
        {
            throw new ArgumentNullException(nameof(violations));
        }

        if (violations.Count == 0)
        {
            throw new ArgumentException(
                "An outcome that reports a failure needs at least one violation.",
                nameof(violations));
        }

        return violations;
    }
}
