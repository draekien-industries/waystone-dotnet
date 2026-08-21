namespace Waystone.Monads.Results.Extensions;

using Waystone.SourceGenerators;

[GenerateAwaitedReceivers(typeof(Result<,>))]
[GenerateAwaitedMember(nameof(Result<,>.Unwrap))]
[GenerateAwaitedMember(nameof(Result<,>.UnwrapErr))]
[GenerateAwaitedMember(nameof(Result<,>.UnwrapOr))]
[GenerateAwaitedMember(nameof(Result<,>.UnwrapOrDefault))]
public static partial class UnwrapExtensions;
