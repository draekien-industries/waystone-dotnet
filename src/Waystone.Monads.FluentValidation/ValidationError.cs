namespace FluentValidation;

using System.Collections.Generic;
using Configs;
using Results;
using Waystone.Monads.Results.Errors;

/// <summary>The <see cref="Error" /> a validator's failures produce.</summary>
/// <remarks>
/// Only <c>Validate</c> and <c>ValidateAsync</c> construct one, and only from a
/// validation result that already failed, so <see cref="Failures" /> is never
/// empty. It is an <see cref="Error" /> rather than something convertible to one,
/// so a validation step composes with every other step in a
/// <see cref="Waystone.Monads.Results.Result{TOk,TErr}" /> chain without a <c>MapErr</c> at
/// the seam. Recover the detail by pattern matching:
/// <code>
/// if (error is ValidationError validationError)
/// {
///     return ValidationProblem(validationError.ToDictionary());
/// }
/// </code>
/// <para>
/// <see cref="Error.Code" /> and <see cref="Error.Message" /> are both fixed at
/// construction, so the ambient <see cref="MonadValidationOptions" /> scope that
/// matters is the one the validation ran in, not the one that later reads the
/// error. Two instances are equal when those two match; <see cref="Failures" />
/// takes no part in equality, since the message is derived from it.
/// </para>
/// </remarks>
public sealed record ValidationError : Error
{
    private readonly ValidationResult _validationResult;

    internal ValidationError(ValidationResult validationResult) : base(
        new ErrorCode(MonadValidationOptions.Current.ValidationErrorCode),
        validationResult.ToString("; "))
    {
        _validationResult = validationResult;
    }

    /// <summary>Gets the failures the validator reported, in the order it reported them.</summary>
    /// <remarks>
    /// Never empty. The list is the one the validator produced and nothing else
    /// holds it, but a <see cref="ValidationFailure" /> is itself mutable — editing
    /// one changes what <see cref="ToDictionary" /> reports and leaves
    /// <see cref="Error.Message" /> untouched, since that was rendered at
    /// construction.
    /// </remarks>
    public IReadOnlyList<ValidationFailure> Failures => _validationResult.Errors;

    /// <inheritdoc cref="ValidationResult.ToDictionary" />
    /// <remarks>
    /// Shaped for a model-state or problem-details payload. Builds a fresh
    /// dictionary on every call rather than caching one, so hold the result if you
    /// need it twice.
    /// </remarks>
    public IDictionary<string, string[]> ToDictionary() =>
        _validationResult.ToDictionary();

    /// <summary>
    /// Checks whether another <see cref="ValidationError" /> reports the same code
    /// and message.
    /// </summary>
    /// <remarks>
    /// <see cref="Failures" /> is deliberately excluded.
    /// <see cref="Error.Message" /> is rendered from it, so comparing both would
    /// only add reference equality over a list and make two errors describing the
    /// same failures compare unequal.
    /// </remarks>
    /// <param name="other">The error to compare against. Null is never equal.</param>
    /// <returns>
    /// True if both errors are <see cref="ValidationError" /> and their
    /// <see cref="Error.Code" /> and <see cref="Error.Message" /> match; false
    /// otherwise.
    /// </returns>
    public bool Equals(ValidationError? other) => base.Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => base.GetHashCode();

    /// <summary>Returns the error rendered as <c>[code] message</c>.</summary>
    /// <remarks>
    /// Keeps <see cref="Error" />'s rendering rather than the one a record would
    /// otherwise synthesise, which would print every property including
    /// <see cref="Failures" />.
    /// </remarks>
    /// <returns>
    /// The <see cref="Error.Code" /> in square brackets, a space, then the joined
    /// failure messages.
    /// </returns>
    public override string ToString() => base.ToString();
}
