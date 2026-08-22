namespace Waystone.Monads.Options.Extensions;

using Waystone.SourceGenerators;

[GenerateAwaitedReceivers(typeof(Option<>))]
[GenerateAwaitedMember(nameof(Option<>.Unwrap))]
[GenerateAwaitedMember(nameof(Option<>.UnwrapOr))]
[GenerateAwaitedMember(nameof(Option<>.UnwrapOrDefault))]
public static partial class UnwrapExtensions;
