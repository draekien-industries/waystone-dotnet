namespace Waystone.Monads.FluentValidation.Configs;

using System.Diagnostics.CodeAnalysis;
using Monads.Configs;
using Monads.Results.Errors;
using Results;

/// <summary>
/// Extensions for chaining <see cref="MonadValidationOptions" /> configuration
/// onto a <see cref="MonadOptionsBuilder" />.
/// </summary>
[ExcludeFromCodeCoverage]
public static class MonadOptionsBuilderExtensions
{
    /// <summary>
    /// Configures the <see cref="Error.Code" /> stamped on every
    /// <see cref="ValidationError" />.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="MonadOptionsBuilder" /> whose validation options will be
    /// configured.
    /// </param>
    /// <param name="errorCode">
    /// The validation error code to use. Default: <c>validation.failed</c>.
    /// </param>
    /// <returns>
    /// The <see cref="MonadValidationOptionsBuilder" /> for chaining more
    /// configurations.
    /// </returns>
    /// <exception cref="System.ArgumentException">
    /// If <paramref name="errorCode" /> is null, empty or whitespace.
    /// </exception>
    public static MonadValidationOptionsBuilder UseValidationErrorCode(
        this MonadOptionsBuilder builder,
        string errorCode) =>
        MonadValidationOptionsBuilder.For(builder)
                                     .UseValidationErrorCode(errorCode);
}
