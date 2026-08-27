namespace Waystone.Monads.Results.Extensions;

using Waystone.SourceGenerators;

[GenerateAwaitedReceivers(typeof(Result<,>))]
[GenerateAwaitedMember(
    nameof(Result<,>.GetErr),
    Summary = "Awaits the result and returns an option holding its error.")]
public static partial class GetErrExtensions;
