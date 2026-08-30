namespace Waystone.DocSnippets;

/// <summary>
/// Line splitting that survives a mixed checkout. Files here are CRLF on Windows
/// and LF on CI, and rewriting one style as the other would make every page look
/// changed in <c>--check</c> even when no snippet moved.
/// </summary>
public static class Lines
{
    /// <summary>Splits on any line ending, discarding the carriage returns.</summary>
    /// <param name="text">The file content to split.</param>
    /// <returns>The lines, without their terminators.</returns>
    public static string[] Split(string text) =>
        text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

    /// <summary>Reports the line ending a file already uses, so a rewrite keeps it.</summary>
    /// <param name="text">The file content to inspect.</param>
    /// <returns><c>"\r\n"</c> if the text contains one anywhere, otherwise <c>"\n"</c>.</returns>
    public static string NewLineOf(string text) =>
        text.Contains("\r\n") ? "\r\n" : "\n";
}
