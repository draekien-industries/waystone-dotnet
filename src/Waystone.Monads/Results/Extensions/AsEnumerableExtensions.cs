namespace Waystone.Monads.Results.Extensions;

using Waystone.SourceGenerators;

[GenerateAwaitedReceivers(typeof(Result<,>))]
[GenerateAwaitedMember(nameof(Result<,>.AsEnumerable))]
public static partial class AsEnumerableExtensions;
