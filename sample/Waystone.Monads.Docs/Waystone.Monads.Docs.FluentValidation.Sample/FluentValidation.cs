using FluentValidation;
using FluentValidation.Configs;
using FluentValidation.Extensions;
using Waystone.Monads.Configs;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;
using Waystone.Monads.Results.Extensions;

namespace Waystone.Monads.Docs.FluentValidation.Sample;

/// <summary>packages/fluent-validation.md</summary>
internal static class FluentValidationPage
{
    internal sealed record UserInput(int Range, string Search);

    internal sealed class UserInputValidator : AbstractValidator<UserInput>
    {
        public UserInputValidator()
        {
            RuleFor(x => x.Range).GreaterThan(0);
            RuleFor(x => x.Search).NotEmpty();
        }
    }

    internal static Result<UserInput, Error> Validate()
    {
        UserInput input = new(1, "bob");

        return input.Validate(new UserInputValidator());
    }

    internal static async Task<Result<UserInput, Error>> ValidateAsync(
        UserInput input,
        CancellationToken cancellationToken) =>
        await input.ValidateAsync(new UserInputValidator(), cancellationToken);

    internal static Result<UserInput, Error> Chained(UserInput input) =>
        input.Validate(new UserInputValidator())
             .AndThen(Normalise);

    internal static async Task<Result<UserInput, Error>> ChainedAsync(
        UserInput input,
        CancellationToken cancellationToken) =>
        await input.ValidateAsync(new UserInputValidator(), cancellationToken)
                   .AndThenAsync(Save);

    internal static IDictionary<string, string[]>? ReadTheFailures(Error error) =>
        error is ValidationError validationError
            ? validationError.ToDictionary()
            : null;

    internal static void ConfigureTheErrorCode()
    {
        MonadOptions.Configure(
            options => options.UseValidationErrorCode("input.invalid"));
    }

    // The page also shows UseValidationErrorCode chained after UseLogger. That
    // sample is not here because it cannot be: UseLogger ships in
    // Waystone.Monads.Extensions.Logging, and the page's install section names
    // only Waystone.Monads.FluentValidation. A reader who copies it does not
    // compile. This project references what the page tells you to install and
    // nothing else, which is what makes that visible. The companion packages
    // layer fixes the page.

    internal static void OverrideForOneBlock()
    {
        using (MonadOptions.BeginScope(
                   options => options.UseValidationErrorCode("debug.validation")))
        {
            // errors created in here carry "debug.validation"
        }
    }

    private static Result<UserInput, Error> Normalise(UserInput input) =>
        Result.Ok<UserInput, Error>(input);

    private static ValueTask<Result<UserInput, Error>> Save(UserInput input) =>
        new(Result.Ok<UserInput, Error>(input));
}
