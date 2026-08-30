namespace FluentValidation.Configs;

using System;
using System.Diagnostics.CodeAnalysis;
using Waystone.Monads.Configs;
using Waystone.Monads.Results.Errors;

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
    }

    internal string ValidationErrorCode { get; set; }

    object ISatelliteBuilder.Build() =>
        new MonadValidationOptions(ValidationErrorCode);

    /// <summary>
    /// Sets the <see cref="Error.Code" /> stamped on every
    /// <see cref="ValidationError" /> created while this snapshot is current.
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

    internal static MonadValidationOptionsBuilder For(
        MonadOptionsBuilder builder) =>
        builder.Satellite(
            MonadValidationOptions.Slot,
            static existing => new MonadValidationOptionsBuilder(
                existing as MonadValidationOptions
             ?? MonadValidationOptions.Default));
}
