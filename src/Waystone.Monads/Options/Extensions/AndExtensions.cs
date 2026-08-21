namespace Waystone.Monads.Options.Extensions;

using Waystone.SourceGenerators;

[GenerateAwaitedReceivers(typeof(Option<>))]
[GenerateAwaitedMember(nameof(Option<>.And))]
public static partial class AndExtensions;
