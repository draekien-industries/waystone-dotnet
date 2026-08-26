namespace Waystone.Monads.Options.Extensions;

using Waystone.SourceGenerators;

[GenerateAwaitedReceivers(typeof(Option<>))]
[GenerateAwaitedMember(nameof(Option<>.OkOr))]
public static partial class OkOrExtensions;
