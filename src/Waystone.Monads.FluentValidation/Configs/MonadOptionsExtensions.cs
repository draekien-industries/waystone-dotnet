namespace Waystone.Monads.FluentValidation.Configs;

using System.Diagnostics.CodeAnalysis;
using Waystone.Monads.Configs;
using Waystone.Monads.FluentValidation.Results;
using Waystone.Monads.Results.Errors;

/// <summary>
/// Extensions for chaining <see cref="MonadValidationOptions"/> configuration onto the global <see cref="MonadOptions"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public static class MonadOptionsExtensions
{
    /// <summary>
    /// Configures the error code that will be used when converting a <see cref="ValidationErr"/>
    /// into an <see cref="Error"/>
    /// </summary>
    /// <remarks>The default error code is `validation.failed`.</remarks>
    /// <param name="_">The <see cref="MonadOptions"/> to chain the method from.</param>
    /// <param name="errorCode">The validation error code to use.</param>
    /// <returns>The instance of <see cref="MonadValidationOptions"/> for chaining more configurations.</returns>
    public static MonadValidationOptions UseValidationErrorCode(
        this MonadOptions _,
        string errorCode) => MonadValidationOptions.Global.UseValidationErrorCode(errorCode);


    /// <summary>
    /// Configures the fallback error message that will be used when converting a <see cref="ValidationErr"/>
    /// into an <see cref="Error"/> if the validation error does not have a specific message set.
    /// </summary>
    /// <remarks>The default fallback error message is `One or more validation errors occurred.`</remarks>
    /// <param name="_">The <see cref="MonadOptions"/> to chain the method from.</param>
    /// <param name="fallbackErrorMessage">The fallback error message to use.</param>
    /// <returns>The instance of <see cref="MonadValidationOptions"/> for chaining more configurations.</returns>
    public static MonadValidationOptions UseFallbackValidationErrorMessage(
        this MonadOptions _,
        string fallbackErrorMessage) =>
        MonadValidationOptions.Global.UseFallbackValidationErrorMessage(fallbackErrorMessage);
}