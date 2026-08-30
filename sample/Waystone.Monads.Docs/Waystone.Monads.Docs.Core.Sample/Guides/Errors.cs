using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

namespace Waystone.Monads.Docs.Core.Sample.Guides;

/// <summary>guides/errors.md</summary>
internal static class ErrorsGuide
{
    // The page opens with a sketch of the ErrorCode and Error declarations
    // themselves. That block is the library's own shape written without
    // bodies, so there is nothing here to compile -- the real declarations are
    // in src/Waystone.Monads.

    internal static class SpellErrorCodes
    {
        internal static readonly ErrorCode ComponentMissing = new("component.missing");
        internal static readonly ErrorCode SigilMalformed = new("sigil.malformed");
        internal static readonly ErrorCode LevelOutOfRange = new("level.out_of_range");
    }

    internal static void ACodeFromACatalog()
    {
        ErrorCode code = SpellErrorsCatalog.Codes.SigilMalformed;
        //        ^? "SpellErrors.SigilMalformed"

        _ = code;
    }

    internal static void AnErrorFromACatalogFactory()
    {
        Error error = SpellErrorsCatalog.Errors.SigilMalformed(
            "Failed to parse the sigil as a rune sequence");
        //    ^? Code: "SpellErrors.SigilMalformed", Message: "Failed to parse…"

        _ = error;
    }

    internal static void ErrorCodeFromException()
    {
        try
        {
            // do work
        }
        catch (ScryingFailedException e)
        {
            var errorCode = ErrorCode.FromException(e); // "ScryingFailed"

            _ = errorCode;
        }
    }

    internal static void BuildingAnError()
    {
        Error malformedUri = new(
            SpellErrorCodes.SigilMalformed,
            "Expected an absolute sigil but received a relative one");

        Error unparseable = new(
            SpellErrorCodes.SigilMalformed,
            "Failed to parse the sigil as a rune sequence");

        _ = (malformedUri, unparseable);
    }

    internal static Result<int, Error> AnErrorFromACatalog() =>
        Result.Err<int>(
            SpellErrorsCatalog.Errors.SigilMalformed(
                "Failed to parse the sigil as a rune sequence"));

    internal static void ErrorFromException()
    {
        try
        {
            // do work
        }
        catch (ScryingFailedException e)
        {
            var error = Error.FromException(e);
            //  ^? Code: "ScryingFailed", Message: e.Message

            _ = error;
        }
    }
}

/// <summary>guides/errors.md — the catalog the page marks up</summary>
[ErrorCodeCatalog]
public enum SpellErrors
{
    ComponentMissing = 1,
    SigilMalformed = 2,
    LevelOutOfRange = 3,
}

internal sealed class ScryingFailedException : Exception;
