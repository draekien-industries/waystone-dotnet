namespace Waystone.DocSnippets;

/// <summary>Path shaping for values that end up in published markdown.</summary>
public static class Paths
{
    /// <summary>Rewrites a path with forward slashes whatever platform produced it.</summary>
    /// <param name="path">A path from the file system, possibly using backslashes.</param>
    /// <returns>The same path, safe to commit — a page rendered on Windows reads the same as one rendered on CI.</returns>
    public static string Forward(string path) =>
        path.Replace(Path.DirectorySeparatorChar, '/');
}
