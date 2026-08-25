namespace Waystone.Monads.FluentValidation.Configs;

using System;
using System.Diagnostics.CodeAnalysis;
using Monads.Configs;
using Monads.Results.Errors;
using Results;

/// <summary>
/// Configuration for converting a <see cref="ValidationErr" /> into an
/// <see cref="Error" />.
/// </summary>
/// <remarks>
/// Registered as a satellite of <see cref="MonadOptions" />, so it follows whatever
/// options scope is current when <see cref="ValidationErr.ToError" /> reads it.
/// </remarks>
[ExcludeFromCodeCoverage]
public sealed class MonadValidationOptions : IMonadOptionsSatellite
{
    private MonadValidationOptions()
    {
        ValidationErrorCode = "validation.failed";
        FallbackValidationErrorMessage =
            "One or more validation errors occurred.";
    }

    internal static MonadValidationOptions Global => For(MonadOptions.Global);

    internal static MonadValidationOptions Current =>
        For(MonadOptions.Current);

    internal static MonadValidationOptions For(MonadOptions options) =>
        options.Satellite(() => new MonadValidationOptions());

    IMonadOptionsSatellite IMonadOptionsSatellite.Clone() =>
        new MonadValidationOptions
        {
            ValidationErrorCode = ValidationErrorCode,
            FallbackValidationErrorMessage = FallbackValidationErrorMessage,
        };


    internal string ValidationErrorCode { get; set; }


    internal string FallbackValidationErrorMessage { get; set; }

    /// <summary>
    /// Sets the error code that <see cref="ValidationErr.ToError" /> stamps on the
    /// <see cref="Error" /> it produces.
    /// </summary>
    /// <param name="errorCode">
    /// The validation error code to use. Default: <c>validation.failed</c>.
    /// </param>
    /// <returns>This instance, for chaining more configurations.</returns>
    /// <exception cref="ArgumentException">
    /// If <paramref name="errorCode" /> is null, empty or whitespace.
    /// </exception>
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
    /// Sets the message <see cref="ValidationErr.ToError" /> uses when the validation
    /// result carries no failure messages of its own.
    /// </summary>
    /// <param name="fallbackErrorMessage">
    /// The fallback error message to use. Default:
    /// <c>One or more validation errors occurred.</c>
    /// </param>
    /// <returns>This instance, for chaining more configurations.</returns>
    /// <exception cref="ArgumentException">
    /// If <paramref name="fallbackErrorMessage" /> is null, empty or whitespace.
    /// </exception>
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
