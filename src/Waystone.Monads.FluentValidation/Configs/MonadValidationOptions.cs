namespace Waystone.Monads.FluentValidation.Configs;

using System;
using System.Diagnostics.CodeAnalysis;
using Monads.Results.Errors;
using Results;

/// <summary>
/// Global configuration options for the Waystone.Monads.FluentValidation
/// library.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class MonadValidationOptions
{
    private static readonly Lazy<MonadValidationOptions> Singleton =
        new(() => new MonadValidationOptions());

    private MonadValidationOptions()
    {
        ValidationErrorCode = "validation.failed";
        FallbackValidationErrorMessage =
            "One or more validation errors occurred.";
    }

    internal static MonadValidationOptions Global => Singleton.Value;


    internal string ValidationErrorCode { get; set; }


    internal string FallbackValidationErrorMessage { get; set; }

    /// <summary>
    /// Configures the error code that will be used when converting a
    /// <see cref="ValidationErr" /> into an <see cref="Error" />
    /// </summary>
    /// <remarks>The default error code is `validation.failed`.</remarks>
    /// <param name="errorCode">The validation error code to use.</param>
    /// <returns>
    /// The instance of <see cref="MonadValidationOptions" /> for chaining
    /// more configurations.
    /// </returns>
    public MonadValidationOptions UseValidationErrorCode(string errorCode)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
        {
            throw new ArgumentException(
                "Error code cannot be null or whitespace.",
                nameof(errorCode));
        }

        ValidationErrorCode = errorCode;
        return this;
    }

    /// <summary>
    /// Configures the fallback error message that will be used when
    /// converting a <see cref="ValidationErr" /> into an <see cref="Error" /> if the
    /// validation error does not have a specific message set.
    /// </summary>
    /// <remarks>
    /// The default fallback error message is `One or more validation errors
    /// occurred.`
    /// </remarks>
    /// <param name="fallbackErrorMessage">The fallback error message to use.</param>
    /// <returns>
    /// The instance of <see cref="MonadValidationOptions" /> for chaining
    /// more configurations.
    /// </returns>
    public MonadValidationOptions UseFallbackValidationErrorMessage(
        string fallbackErrorMessage)
    {
        if (string.IsNullOrWhiteSpace(fallbackErrorMessage))
        {
            throw new ArgumentException(
                "Fallback error message cannot be null or whitespace.",
                nameof(fallbackErrorMessage));
        }

        FallbackValidationErrorMessage = fallbackErrorMessage;
        return this;
    }
}
