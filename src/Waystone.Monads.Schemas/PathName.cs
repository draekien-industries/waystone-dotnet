namespace Waystone.Monads.Schemas;

internal static class PathName
{
    internal const string Fallback = "value";

    internal static string From(string? expression)
    {
        if (expression is null) return Fallback;

        int dot = expression.LastIndexOf('.');
        string tail = dot < 0 ? expression : expression.Substring(dot + 1);

        if (tail.Length == 0) return Fallback;

        return char.IsUpper(tail[0])
            ? char.ToLowerInvariant(tail[0]) + tail.Substring(1)
            : tail;
    }
}
