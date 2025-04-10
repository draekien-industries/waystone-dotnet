namespace Waystone.Monads.FluentValidation.Results;

using System.Collections.Generic;
using global::FluentValidation.Results;
using Options;

/// <summary>The errors that were encountered while running a validator</summary>
public sealed class ValidationErr
{
    private readonly ValidationResult _validationResult;

    private ValidationErr(ValidationResult validationResult)
    {
        _validationResult = validationResult;
    }

    /// <inheritdoc cref="ValidationResult.Errors" />
    public List<ValidationFailure> Errors => _validationResult.Errors;

    /// <inheritdoc cref="ValidationResult.RuleSetsExecuted" />
    public string[] RuleSetsExecuted => _validationResult.RuleSetsExecuted;

    /// <summary>
    /// Creates an option that may contain a validation err depending on the
    /// state of the validation result
    /// </summary>
    /// <param name="validationResult">The <see cref="ValidationResult" /></param>
    /// <returns>
    /// A some containing the validation err if the validation result is
    /// invalid. Otherwise, none.
    /// </returns>
    public static Option<ValidationErr>
        Create(ValidationResult? validationResult) =>
        validationResult is { IsValid: false }
            ? Option.Some(new ValidationErr(validationResult))
            : Option.None<ValidationErr>();

    /// <summary>
    /// Converts the <see cref="ValidationErr" /> back to a
    /// <see cref="ValidationResult" />
    /// </summary>
    /// <returns>The <see cref="ValidationResult" /></returns>
    public ValidationResult AsValidationResult() => _validationResult;

    /// <inheritdoc cref="ValidationResult.ToDictionary" />
    public IDictionary<string, string[]> ToDictionary() =>
        _validationResult.ToDictionary();

    /// <inheritdoc cref="ValidationResult.ToString()" />
    public override string ToString() => _validationResult.ToString();
}
