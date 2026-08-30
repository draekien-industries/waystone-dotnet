using Waystone.Monads.Results.Errors;

namespace Spellcraft;

/// <summary>
/// source-generation/code-format.md. The page reuses one enum name across both
/// its unformatted and formatted examples. One assembly cannot declare the same
/// enum twice, so the formatted example gets a catalog of its own here.
/// </summary>
[ErrorCodeCatalog(Format = "spell.{member:kebab}")]
public enum SpellErrorCode
{
    NotFound,
    AlreadyPrepared,
}

internal static class CodeFormatPage
{
    internal static void WhatTheFormatProduces()
    {
        _ = SpellErrorCodeCatalog.Names.NotFound;       // "spell.not-found"
        _ = SpellErrorCodeCatalog.Names.AlreadyPrepared; // "spell.already-prepared"
    }

    // The page also shows an assembly-wide default:
    //
    //     [assembly: ErrorCodeFormat("{enum:kebab}/{member:kebab}")]
    //
    // It is not applied here. It would change every catalog in this project,
    // including the one on the error code catalogs page, and that page's own
    // sample asserts the unformatted "SpellErrorCode.NotFound".
}
