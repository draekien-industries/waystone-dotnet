namespace Waystone.Monads.Results.Extensions;

using Waystone.SourceGenerators;

[GenerateAwaitedReceivers(typeof(Result<,>))]
[GenerateAwaitedMember(
    nameof(Result<,>.GetOk),
    Summary = "Awaits the result and returns an option holding its success value.")]
public static partial class GetOkExtensions;
