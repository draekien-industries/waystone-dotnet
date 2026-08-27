namespace Waystone.Monads.Options.Extensions;

using Waystone.SourceGenerators;

[GenerateAwaitedReceivers(typeof(Option<>))]
[GenerateAwaitedMember(nameof(Option<>.Zip))]
public static partial class ZipExtensions;
