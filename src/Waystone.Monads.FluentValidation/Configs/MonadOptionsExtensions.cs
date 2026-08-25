namespace Waystone.Monads.FluentValidation.Configs;

using System.Diagnostics.CodeAnalysis;
using Monads.Configs;
using Monads.Results.Errors;
using Results;

/// <summary>
/// Extensions for chaining <see cref="MonadValidationOptions" />
/// configuration onto the global <see cref="MonadOptions" />.
/// </summary>
[ExcludeFromCodeCoverage]
public static class MonadOptionsExtensions
{
    /// <summary>
    /// Configures the error code used when converting a
    /// <see cref="ValidationErr" /> into an <see cref="Error" />.
    /// </summary>
    /// <param name="options">
    /// The <see cref="MonadOptions" /> whose validation options
    /// will be configured.
    /// </param>
    /// <param name="errorCode">
    /// The validation error code to use. Default: <c>validation.failed</c>.
    /// </param>
    /// <returns>
    /// The <see cref="MonadValidationOptions" /> for chaining more configurations.
    /// </returns>
    /// <exception cref="System.ArgumentException">
    /// If <paramref name="errorCode" /> is null, empty or whitespace.
    /// </exception>
    public static MonadValidationOptions UseValidationErrorCode(
        this MonadOptions options,
        string errorCode) =>
        MonadValidationOptions.For(options)
           .UseValidationErrorCode(errorCode);


    /// <summary>
    /// Configures the fallback error message used when converting a
    /// <see cref="ValidationErr" /> into an <see cref="Error" /> and the validation
    /// result carries no failure messages.
    /// </summary>
    /// <param name="options">
    /// The <see cref="MonadOptions" /> whose validation options
    /// will be configured.
    /// </param>
    /// <param name="fallbackErrorMessage">
    /// The fallback error message to use. Default:
    /// <c>One or more validation errors occurred.</c>
    /// </param>
    /// <returns>
    /// The <see cref="MonadValidationOptions" /> for chaining more configurations.
    /// </returns>
    /// <exception cref="System.ArgumentException">
    /// If <paramref name="fallbackErrorMessage" /> is null, empty or whitespace.
    /// </exception>
    public static MonadValidationOptions UseFallbackValidationErrorMessage(
        this MonadOptions options,
        string fallbackErrorMessage) =>
        MonadValidationOptions.For(options)
           .UseFallbackValidationErrorMessage(fallbackErrorMessage);
}
