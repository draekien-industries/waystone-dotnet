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
    /// Configures the error code that will be used when converting a
    /// <see cref="ValidationErr" /> into an <see cref="Error" />
    /// </summary>
    /// <remarks>The default error code is `validation.failed`.</remarks>
    /// <param name="options">
    /// The <see cref="MonadOptions" /> whose validation options
    /// will be configured.
    /// </param>
    /// <param name="errorCode">The validation error code to use.</param>
    /// <returns>
    /// The instance of <see cref="MonadValidationOptions" /> for chaining
    /// more configurations.
    /// </returns>
    public static MonadValidationOptions UseValidationErrorCode(
        this MonadOptions options,
        string errorCode) =>
        MonadValidationOptions.For(options)
           .UseValidationErrorCode(errorCode);


    /// <summary>
    /// Configures the fallback error message that will be used when
    /// converting a <see cref="ValidationErr" /> into an <see cref="Error" /> if the
    /// validation error does not have a specific message set.
    /// </summary>
    /// <remarks>
    /// The default fallback error message is `One or more validation errors
    /// occurred.`
    /// </remarks>
    /// <param name="options">
    /// The <see cref="MonadOptions" /> whose validation options
    /// will be configured.
    /// </param>
    /// <param name="fallbackErrorMessage">The fallback error message to use.</param>
    /// <returns>
    /// The instance of <see cref="MonadValidationOptions" /> for chaining
    /// more configurations.
    /// </returns>
    public static MonadValidationOptions UseFallbackValidationErrorMessage(
        this MonadOptions options,
        string fallbackErrorMessage) =>
        MonadValidationOptions.For(options)
           .UseFallbackValidationErrorMessage(fallbackErrorMessage);
}
