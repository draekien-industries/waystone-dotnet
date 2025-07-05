namespace Waystone.Monads.FluentValidation.Results;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using global::FluentValidation.Results;
using Options;
using Waystone.Monads.FluentValidation.Configs;
using Waystone.Monads.Results.Errors;

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

    /// <summary>
    /// Converts the <see cref="ValidationErr" /> to an <see cref="Error" />.
    /// </summary>
    /// <remarks>
    /// Uses the options configured in <see cref="MonadValidationOptions"/> to determine\
    /// the error code and the fallback error message (for when there are no errors in the validation result).
    /// </remarks>
    /// <returns>The created <see cref="Error"/></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public Error ToError()
    {
        Debug.Assert(_validationResult.IsValid is false, "Validation Result should never be valid here.");

        ErrorCode errorCode = new(MonadValidationOptions.Global.ValidationErrorCode);

        string errorMessage = Errors.Count > 0
            ? string.Join("; ", Errors.Select(e => e.ErrorMessage.TrimEnd('.')))
            : MonadValidationOptions.Global.FallbackValidationErrorMessage;

        return new Error(errorCode, $"{errorMessage};");
    }

    /// <inheritdoc cref="ValidationResult.ToString()" />
    public override string ToString() => _validationResult.ToString();
}
