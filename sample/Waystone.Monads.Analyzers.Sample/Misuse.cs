namespace Waystone.Monads.Analyzers.Sample;

using System;
using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

internal class Misuse
{
    internal Option<int> SomeOfADefault() => Option.Some(0);

    internal Option<Guid> SomeOfAnEmptyGuid() => Option.Some(Guid.Empty);

    internal Option<int> NullInsteadOfNone() => null;

    internal Result<int, string> DefaultInsteadOfErr() =>
        default(Result<int, string>);

    internal Option<int> ZeroThatBecomesNone() => 0;

    internal void NullPassedToAnOptionParameter() => Accept(null!);

    internal void DiscardedResult() => Save();

    private void Accept(Option<int> option) { }

    private Result<int, Error> Save() => Result.Ok<int, Error>(1);
}
