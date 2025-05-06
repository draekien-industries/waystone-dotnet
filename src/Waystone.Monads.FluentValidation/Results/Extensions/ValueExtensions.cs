namespace Waystone.Monads.FluentValidation.Results.Extensions;

using System.Threading;
using System.Threading.Tasks;
using global::FluentValidation;
using global::FluentValidation.Results;
using Monads.Results;

/// <summary>Extension methods for <see cref="ValidationResult" /></summary>
public static class ValueExtensions
{
    /// <summary>
    /// Validates a value, returning a result containing the value if it is
    /// valid, otherwise a validation err
    /// </summary>
    /// <param name="value">The value that needs to be validated</param>
    /// <param name="validator">The implemented <see cref="IValidator{T}" /></param>
    /// <typeparam name="TValue">The value's type</typeparam>
    /// <returns>
    /// An ok result containing the value if it is valid, otherwise a
    /// <see cref="ValidationErr" />
    /// </returns>
    public static Result<TValue, ValidationErr> Validate<TValue>(
        this TValue value,
        IValidator<TValue> validator) where TValue : notnull
    {
        ValidationResult? validationResult = validator.Validate(value);

        return ValidationErr.Create(validationResult)
                            .Match(
                                 Result.Err<TValue, ValidationErr>,
                                 () => Result.Ok<TValue, ValidationErr>(value));
    }

    /// <summary>
    /// Validates a value asynchronously, returning a result containing the
    /// value if it is valid, otherwise a validation err
    /// </summary>
    /// <param name="value">The value that needs to be validated</param>
    /// <param name="validator">The implemented <see cref="IValidator{T}" /></param>
    /// <param name="cancellationToken">An optional cancellation token</param>
    /// <typeparam name="TValue">The value's type</typeparam>
    /// <returns>
    /// An ok result containing the value if it is valid, otherwise a
    /// <see cref="ValidationErr" />
    /// </returns>
    public static async Task<Result<TValue, ValidationErr>>
        ValidateAsync<TValue>(
            this TValue value,
            IValidator<TValue> validator,
            CancellationToken cancellationToken = default)
        where TValue : notnull
    {
        ValidationResult? validationResult =
            await validator.ValidateAsync(value, cancellationToken);

        return ValidationErr.Create(validationResult)
                            .Match(
                                 Result.Err<TValue, ValidationErr>,
                                 () => Result.Ok<TValue, ValidationErr>(value));
    }
}
