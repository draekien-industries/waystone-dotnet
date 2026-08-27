namespace Waystone.Monads.FluentValidation.Configs;

using System;
using System.Diagnostics.CodeAnalysis;
using Monads.Configs;
using Monads.Results.Errors;
using Results;

/// <summary>Assembles the <see cref="MonadValidationOptions" /> for one snapshot.</summary>
/// <remarks>
/// Reached through the <see cref="MonadOptionsBuilder" /> extension methods in
/// <see cref="MonadOptionsBuilderExtensions" /> rather than constructed.
/// </remarks>
[ExcludeFromCodeCoverage]
public sealed class MonadValidationOptionsBuilder : ISatelliteBuilder
{
    private MonadValidationOptionsBuilder(MonadValidationOptions source)
    {
        ValidationErrorCode = source.ValidationErrorCode;
        FallbackValidationErrorMessage = source.FallbackValidationErrorMessage;
    }

    internal string ValidationErrorCode { get; set; }

    internal string FallbackValidationErrorMessage { get; set; }

    object ISatelliteBuilder.Build() =>
        new MonadValidationOptions(
            ValidationErrorCode,
            FallbackValidationErrorMessage);

    /// <summary>
    /// Sets the error code that <see cref="ValidationErr.ToError" /> stamps on the
    /// <see cref="Error" /> it produces.
    /// </summary>
    /// <param name="errorCode">
    /// The validation error code to use. Default: <c>validation.failed</c>.
    /// </param>
    /// <returns>This builder, for chaining more configurations.</returns>
    /// <exception cref="ArgumentException">
    /// If <paramref name="errorCode" /> is null, empty or whitespace.
    /// </exception>
    public MonadValidationOptionsBuilder UseValidationErrorCode(string errorCode)
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
    /// <returns>This builder, for chaining more configurations.</returns>
    /// <exception cref="ArgumentException">
    /// If <paramref name="fallbackErrorMessage" /> is null, empty or whitespace.
    /// </exception>
    public MonadValidationOptionsBuilder UseFallbackValidationErrorMessage(
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

    internal static MonadValidationOptionsBuilder For(
        MonadOptionsBuilder builder) =>
        builder.Satellite(
            MonadValidationOptions.Slot,
            static existing => new MonadValidationOptionsBuilder(
                existing as MonadValidationOptions
             ?? MonadValidationOptions.Default));
}
