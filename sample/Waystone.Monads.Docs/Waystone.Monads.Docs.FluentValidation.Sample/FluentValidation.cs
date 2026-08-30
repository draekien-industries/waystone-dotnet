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
    internal sealed record SpellInput(int Range, string Name);

    internal sealed class SpellInputValidator : AbstractValidator<SpellInput>
    {
        public SpellInputValidator()
        {
            RuleFor(x => x.Range).GreaterThan(0);
            RuleFor(x => x.Name).NotEmpty();
        }
    }

    internal static Result<SpellInput, Error> Validate()
    {
        SpellInput input = new(120, "Fireball");

        return input.Validate(new SpellInputValidator());
    }

    internal static async Task<Result<SpellInput, Error>> ValidateAsync(
        SpellInput input,
        CancellationToken cancellationToken) =>
        await input.ValidateAsync(new SpellInputValidator(), cancellationToken);

    internal static Result<SpellInput, Error> Chained(SpellInput input) =>
        input.Validate(new SpellInputValidator())
             .AndThen(Normalise);

    internal static async Task<Result<SpellInput, Error>> ChainedAsync(
        SpellInput input,
        CancellationToken cancellationToken) =>
        await input.ValidateAsync(new SpellInputValidator(), cancellationToken)
                   .AndThenAsync(Inscribe);

    internal static IDictionary<string, string[]>? ReadTheFailures(Error error) =>
        error is ValidationError validationError
            ? validationError.ToDictionary()
            : null;

    internal static void ConfigureTheErrorCode()
    {
        MonadOptions.Configure(
            options => options.UseValidationErrorCode("spell.invalid"));
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
                   options => options.UseValidationErrorCode("debug.spell")))
        {
            // errors created in here carry "debug.spell"
        }
    }

    private static Result<SpellInput, Error> Normalise(SpellInput input) =>
        Result.Ok<SpellInput, Error>(input);

    private static ValueTask<Result<SpellInput, Error>> Inscribe(SpellInput input) =>
        new(Result.Ok<SpellInput, Error>(input));
}
