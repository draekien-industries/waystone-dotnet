namespace Waystone.Monads.FluentValidation.Results;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Configs;
using global::FluentValidation.Results;
using Monads.Results.Errors;
using Options;

/// <summary>The failures a validator reported for one value.</summary>
/// <remarks>
/// An instance only exists for an invalid <see cref="ValidationResult" />;
/// <see cref="Create" /> returns none for a valid one, so <see cref="Errors" /> is
/// never empty in practice.
/// </remarks>
public sealed class ValidationErr
{
    private readonly ValidationResult _validationResult;

    private ValidationErr(ValidationResult validationResult)
    {
        _validationResult = validationResult;
    }

    /// <inheritdoc cref="ValidationResult.Errors" />
    /// <remarks>
    /// The list is the wrapped <see cref="ValidationResult" />'s own, not a copy.
    /// Mutating it changes what <see cref="AsValidationResult" />,
    /// <see cref="ToDictionary" /> and <see cref="ToError" /> report.
    /// </remarks>
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

    /// <summary>Converts the <see cref="ValidationErr" /> to an <see cref="Error" />.</summary>
    /// <remarks>
    /// Reads <see cref="MonadValidationOptions" /> from the ambient scope at the
    /// moment of the call, not from when this instance was created, so the error
    /// code and the fallback message follow whatever scope the call sits in.
    /// <para>
    /// The message is every failure's message joined with <c>"; "</c>, each stripped
    /// of a trailing full stop, with a single <c>";"</c> appended. When the
    /// validation result carries no failures the configured fallback message is used
    /// instead.
    /// </para>
    /// </remarks>
    /// <returns>The created <see cref="Error" />.</returns>
    public Error ToError()
    {
        Debug.Assert(
            _validationResult.IsValid is false,
            "Validation Result should never be valid here.");

        MonadValidationOptions options = MonadValidationOptions.Current;

        ErrorCode errorCode = new(options.ValidationErrorCode);

        string errorMessage = Errors.Count > 0
            ? string.Join("; ", Errors.Select(e => e.ErrorMessage.TrimEnd('.')))
            : options.FallbackValidationErrorMessage;

        return new Error(errorCode, $"{errorMessage};");
    }

    /// <inheritdoc cref="ValidationResult.ToString()" />
    public override string ToString() => _validationResult.ToString();
}
