namespace FluentValidation.Configs;

using System.Diagnostics.CodeAnalysis;
using Waystone.Monads.Configs;
using Waystone.Monads.Results.Errors;

/// <summary>Configuration for the <see cref="Error" /> a validation failure produces.</summary>
/// <remarks>
/// Attached to a <see cref="MonadOptions" /> snapshot, so a
/// <see cref="ValidationError" /> takes its code from whatever options scope the
/// validation ran in.
/// </remarks>
[ExcludeFromCodeCoverage]
public sealed class MonadValidationOptions
{
    internal static readonly int Slot = MonadOptionsSlot.Allocate();

    internal static readonly MonadValidationOptions Default =
        new("validation.failed");

    internal MonadValidationOptions(string validationErrorCode)
    {
        ValidationErrorCode = validationErrorCode;
    }

    internal static MonadValidationOptions Global => For(MonadOptions.Global);

    internal static MonadValidationOptions Current => For(MonadOptions.Current);

    internal string ValidationErrorCode { get; }

    internal static MonadValidationOptions For(MonadOptions options) =>
        options.Satellite<MonadValidationOptions>(Slot) ?? Default;
}
