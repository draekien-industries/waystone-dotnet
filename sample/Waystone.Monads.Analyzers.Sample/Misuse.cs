namespace Waystone.Monads.Analyzers.Sample;

using System.Threading.Tasks;
using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;

internal class Misuse
{
    internal Option<string> SomeOfANull() => Option.Some(default(string)!);

    internal Option<int> NullInsteadOfNone() => null;

    internal Result<int, string> DefaultInsteadOfErr() =>
        default(Result<int, string>);

    internal void NullPassedToAnOptionParameter() => Accept(null!);

    internal void DiscardedResult() => Save();

#nullable enable
    internal Option<int>? NullableOption() => null;
#nullable restore

    private void Accept(Option<int> option) { }

    internal async Task DiscardedResultBehindConfigureAwait() =>
        await SaveAsync().ConfigureAwait(false);

    private Result<int, Error> Save() => Result.Ok<int, Error>(1);

    private Task<Result<int, Error>> SaveAsync() =>
        Task.FromResult(Result.Ok<int, Error>(1));
}
