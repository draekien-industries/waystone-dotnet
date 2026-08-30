namespace FluentValidation.Extensions;

using System.Threading;
using System.Threading.Tasks;
using Results;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

/// <summary>
/// Extension methods that validate a value into a
/// <see cref="Result{TOk,TErr}" />.
/// </summary>
public static class ValueExtensions
{
    extension<TValue>(TValue value) where TValue : notnull
    {
        /// <summary>
        /// Runs the validator's synchronous rules over a value, returning it as an ok
        /// result if it passes.
        /// </summary>
        /// <remarks>
        /// Throws on a validator that declares asynchronous rules — call
        /// <c>ValidateAsync</c> for those. Only validation failures become an err; an
        /// exception thrown by <paramref name="validator" /> propagates to the caller.
        /// </remarks>
        /// <param name="validator">The implemented <see cref="IValidator{T}" /></param>
        /// <returns>
        /// An ok result containing the value if it is valid, otherwise an err
        /// containing a <see cref="ValidationError" />.
        /// </returns>
        public Result<TValue, Error> Validate(IValidator<TValue> validator) =>
            ToResult(value, validator.Validate(value));

        /// <summary>
        /// Awaits the validator's asynchronous rules over a value, returning it as an
        /// ok result if it passes.
        /// </summary>
        /// <remarks>
        /// Only validation failures become an err. An exception thrown by
        /// <paramref name="validator" /> propagates to the caller, and a cancelled
        /// <paramref name="cancellationToken" /> surfaces as an
        /// <c>OperationCanceledException</c> rather than as an err.
        /// </remarks>
        /// <param name="validator">The implemented <see cref="IValidator{T}" /></param>
        /// <param name="cancellationToken">
        /// Cancels the validator's asynchronous rules. Default:
        /// <see langword="default" />, which never cancels.
        /// </param>
        /// <returns>
        /// An ok result containing the value if it is valid, otherwise an err
        /// containing a <see cref="ValidationError" />.
        /// </returns>
        public async ValueTask<Result<TValue, Error>> ValidateAsync(
            IValidator<TValue> validator,
            CancellationToken cancellationToken = default) =>
            ToResult(
                value,
                await validator.ValidateAsync(value, cancellationToken)
                               .ConfigureAwait(false));
    }

    private static Result<TValue, Error> ToResult<TValue>(
        TValue value,
        ValidationResult validationResult) where TValue : notnull =>
        validationResult.IsValid
            ? Result.Ok<TValue, Error>(value)
            : Result.Err<TValue, Error>(new ValidationError(validationResult));
}
