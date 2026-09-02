namespace Waystone.Monads.Schemas;

internal readonly struct ParseContext
{
    private ParseContext(ViolationPath path, bool isSensitive)
    {
        Path = path;
        IsSensitive = isSensitive;
    }

    public static ParseContext Root { get; } =
        new(ViolationPath.Root, false);

    public ViolationPath Path { get; }

    public bool IsSensitive { get; }

    public ParseContext At(string property) =>
        new(Path.Append(property), IsSensitive);

    public ParseContext AtIndex(int index) =>
        new(Path.AppendIndex(index), IsSensitive);

    public ParseContext AtKey(string key) =>
        new(Path.AppendKey(key), IsSensitive);

    public ParseContext Renamed(string property) =>
        new(Path.Rename(property), IsSensitive);

    public ParseContext AsSensitive() =>
        IsSensitive ? this : new ParseContext(Path, true);
}
