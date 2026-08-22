namespace Waystone.Monads.Results.Extensions;

using Waystone.SourceGenerators;

[GenerateAwaitedReceivers(typeof(Result<,>))]
[GenerateAwaitedMember(nameof(Result<,>.Expect))]
[GenerateAwaitedMember(nameof(Result<,>.ExpectErr))]
public static partial class ExpectExtensions;
