namespace Waystone.Monads.Options.Extensions;

using Waystone.SourceGenerators;

[GenerateAwaitedReceivers(typeof(Option<>))]
[GenerateAwaitedMember(nameof(Option<>.Expect))]
public static partial class ExpectExtensions;
