#nullable enable

namespace Waystone.Monads.Analyzers.Sample;

using System;
using Waystone.Monads.Options;
using Waystone.Monads.Options.Extensions;
using Waystone.Monads.Results;

internal class Idioms
{
    internal Option<string> MaybeNullPassedToSome(string? value) =>
        Option.Some(value);

    internal Option<string> ProjectionMayReturnNull(Option<int> option) =>
        option.Map(Describe);

    internal Option<string> ProjectionMayReturnNullFromALambda(
        Option<int> option) =>
        option.Map(value => Describe(value));

    private static string? Describe(int value) =>
        value > 0 ? value.ToString() : null;

    internal int Panics(Option<int> option) => option.Unwrap();

    internal int Expects(Option<int> option) => option.Expect("checked");

    internal Result<int, string> ThrowsFromAResultMember(int value)
    {
        if (value > 0)
        {
            return Result.Ok<int, string>(value);
        }

        throw new InvalidOperationException("not positive");
    }

    internal int GuardsThenUnwraps(Option<int> option)
    {
        if (option.IsSome)
        {
            return option.Unwrap();
        }

        return 0;
    }

    internal bool ChecksThenUnwraps(Option<int> option) =>
        option.IsSome && option.Unwrap() > 2;

    internal Option<int> MapsThenFlattens(Option<int> option) =>
        option.Map(value => Option.Some(value * 2)).Flatten();

    internal int UnwrapsOrADefault(Option<int> option) => option.UnwrapOr(0);

    internal bool ComparesToNull(Option<int> option) => option == null;

    internal Option<Option<int>> Nested() => Option.None<Option<int>>();

    internal Result<string, string> IdenticalTypeArguments() =>
        Result.Ok<string, string>("value");

    internal bool DeclaresACase(Some<int> some) => some.IsSome;

    internal string? NullableAlongsideOption(int id) => null;

    internal int UnwrapsOrDefaultOnAStruct(Option<int> option) =>
        option.UnwrapOrDefault();
}
