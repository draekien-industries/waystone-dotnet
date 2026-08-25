namespace Waystone.Monads.FluentValidation.Results.Extensions;

using System.Threading;
using System.Threading.Tasks;
using global::FluentValidation;
using global::FluentValidation.Results;
using Monads.Results;

/// <summary>
/// Extension methods that validate a value into a
/// <see cref="Result{TOk,TErr}" />.
/// </summary>
public static class ValueExtensions
{
    /// <summary>
    /// Runs the validator's synchronous rules over a value, returning it as an ok
    /// result if it passes.
    /// </summary>
    /// <remarks>
    /// Only validation failures become an err. An exception thrown by
    /// <paramref name="validator" /> propagates to the caller.
    /// </remarks>
    /// <param name="value">The value that needs to be validated</param>
    /// <param name="validator">The implemented <see cref="IValidator{T}" /></param>
    /// <typeparam name="TValue">The value's type</typeparam>
    /// <returns>
    /// An ok result containing the value if it is valid, otherwise an err
    /// containing a <see cref="ValidationErr" />.
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
    /// Awaits the validator's asynchronous rules over a value, returning it as an ok
    /// result if it passes.
    /// </summary>
    /// <remarks>
    /// Only validation failures become an err. An exception thrown by
    /// <paramref name="validator" /> propagates to the caller, and a cancelled
    /// <paramref name="cancellationToken" /> surfaces as an
    /// <c>OperationCanceledException</c> rather than as an err.
    /// </remarks>
    /// <param name="value">The value that needs to be validated</param>
    /// <param name="validator">The implemented <see cref="IValidator{T}" /></param>
    /// <param name="cancellationToken">
    /// Cancels the validator's asynchronous rules. Default:
    /// <see langword="default" />, which never cancels.
    /// </param>
    /// <typeparam name="TValue">The value's type</typeparam>
    /// <returns>
    /// An ok result containing the value if it is valid, otherwise an err
    /// containing a <see cref="ValidationErr" />.
    /// </returns>
    public static async Task<Result<TValue, ValidationErr>>
        ValidateAsync<TValue>(
            this TValue value,
            IValidator<TValue> validator,
            CancellationToken cancellationToken = default)
        where TValue : notnull
    {
        ValidationResult? validationResult =
            await validator.ValidateAsync(value, cancellationToken)
               .ConfigureAwait(false);

        return ValidationErr.Create(validationResult)
                            .Match(
                                 Result.Err<TValue, ValidationErr>,
                                 () => Result.Ok<TValue, ValidationErr>(value));
    }
}
