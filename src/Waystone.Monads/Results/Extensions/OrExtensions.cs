namespace Waystone.Monads.Results.Extensions;

using Waystone.SourceGenerators;

[GenerateAwaitedReceivers(typeof(Result<,>))]
[GenerateAwaitedMember(nameof(Result<,>.Or))]
public static partial class OrExtensions;
