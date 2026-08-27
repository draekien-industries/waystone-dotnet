namespace Waystone.Monads.FluentValidation.Configs;

using System.Diagnostics.CodeAnalysis;
using Monads.Configs;
using Monads.Results.Errors;
using Results;

/// <summary>
/// Configuration for converting a <see cref="ValidationErr" /> into an
/// <see cref="Error" />.
/// </summary>
/// <remarks>
/// Attached to a <see cref="MonadOptions" /> snapshot, so it follows whatever
/// options scope is current when <see cref="ValidationErr.ToError" /> reads it.
/// </remarks>
[ExcludeFromCodeCoverage]
public sealed class MonadValidationOptions
{
    internal static readonly int Slot = MonadOptionsSlot.Allocate();

    internal static readonly MonadValidationOptions Default = new(
        "validation.failed",
        "One or more validation errors occurred.");

    internal MonadValidationOptions(
        string validationErrorCode,
        string fallbackValidationErrorMessage)
    {
        ValidationErrorCode = validationErrorCode;
        FallbackValidationErrorMessage = fallbackValidationErrorMessage;
    }

    internal static MonadValidationOptions Global => For(MonadOptions.Global);

    internal static MonadValidationOptions Current => For(MonadOptions.Current);

    internal string ValidationErrorCode { get; }

    internal string FallbackValidationErrorMessage { get; }

    internal static MonadValidationOptions For(MonadOptions options) =>
        options.Satellite<MonadValidationOptions>(Slot) ?? Default;
}
