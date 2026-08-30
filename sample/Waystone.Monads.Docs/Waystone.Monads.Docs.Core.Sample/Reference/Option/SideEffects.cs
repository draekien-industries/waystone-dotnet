using Waystone.Monads.Options;

namespace Waystone.Monads.Docs.Core.Sample.Reference.OptionApi;

/// <summary>reference/option/side-effects.md</summary>
internal static class OptionSideEffects
{
    internal static void Inspect()
    {
        Option<string> maybeName = Option.Some("Geladon");
        maybeName.Inspect(name => Console.WriteLine(name.Length));
    }

    internal static void ToStringShowsOnlyTheState()
    {
        _ = Option.Some("Vex'ahlia").ToString(); // "Some { IsSome = True, IsNone = False }"
        _ = Option.None<string>().ToString();    // "None { IsSome = False, IsNone = True }"
    }
}
