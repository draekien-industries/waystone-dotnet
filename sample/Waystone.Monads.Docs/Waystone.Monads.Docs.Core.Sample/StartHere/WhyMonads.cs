using Waystone.Monads.Options;
using Waystone.Monads.Options.Extensions;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;
using Waystone.Monads.Results.Extensions;

namespace Waystone.Monads.Docs.Core.Sample.StartHere;

/// <summary>start-here/why-monads.md</summary>
internal static class WhyMonads
{
    internal sealed record Ritual(decimal Components);

    internal sealed record SpellEffect(string Message);

    internal sealed record Spellbook(Option<string> Patron);

    internal sealed record Character(Spellbook Spellbook);

    internal sealed class FailedToPrepareRitualException : Exception;

    // The page writes `_logger` without saying what it is. It is scenery in
    // both samples, so this project stubs it rather than taking a reference on
    // Microsoft.Extensions.Logging that no page tells a reader to install.
    private interface ILogger
    {
        void LogWarning(Exception exception, string message);

        void LogWarning(Error error);
    }

    private static readonly ILogger _logger = null!;

    internal static SpellEffect TheWayWithoutMonads(decimal? components)
    {
        if (components is null)
        {
            return Cantrip();
        }

        try
        {
            Ritual? ritual = PrepareRitualOrNull(components.Value);

            return ritual is not null
                ? CastOrThrow(ritual)
                : Cantrip();
        }
        catch (FailedToPrepareRitualException ex) // An exception for a valid use case
        {
            _logger.LogWarning(ex, "Failed to prepare the ritual");
            return Cantrip();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cast the spell");
            throw;
        }
    }

    internal static string TheCallSiteWithoutMonads()
    {
        SpellEffect effect = TheWayWithoutMonads(10); // can be an effect, or throw

        return effect?.Message ?? "Something went wrong"; // errors potentially lost
    }

    internal static Result<SpellEffect, Error> CastSpell(Option<decimal> components) =>
        components.AndThen(PrepareRitual) // If components are Some, prepare the ritual
                  .Map(Cast)              // If the ritual is Some, cast it
                  .Transpose()            // Turn the Option<Result> into a Result<Option>
                  .InspectErr(error => _logger.LogWarning(error))  // If Err, log a warning
                  .Map(effect => effect.UnwrapOrElse(Cantrip));    // If Ok, take the effect, or the cantrip

    internal static string TheCallSiteWithMonads() =>
        CastSpell(Option.Some(10.0m))
            .Match(
                onOk: effect => effect.Message,
                onErr: error => error.Message); // always the first error in the pipeline

    internal static Option<string> ChainingTwoMaps(string name) =>
        FindCharacter(name)
            .Map(c => c.Spellbook)
            .AndThen(s => s.Patron);

    internal static string ReadingTheAnswerOut(Option<string> patron) =>
        patron.Match(
            some => some,
            () => "[No patron]");

    private static Option<Ritual> PrepareRitual(decimal components) =>
        Option.Some(new Ritual(components));

    private static Result<SpellEffect, Error> Cast(Ritual ritual) =>
        Result.Ok<SpellEffect, Error>(new SpellEffect("The fireball lands"));

    private static Ritual? PrepareRitualOrNull(decimal components) => new(components);

    private static SpellEffect CastOrThrow(Ritual ritual) => new("The fireball lands");

    private static Option<Character> FindCharacter(string name) =>
        Option.Some(new Character(new Spellbook(Option.Some("The Raven Queen"))));

    private static SpellEffect Cantrip() => new("You cast a cantrip instead");
}
