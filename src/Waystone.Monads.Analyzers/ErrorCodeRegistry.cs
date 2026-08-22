namespace Waystone.Monads.Analyzers;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

/// <summary>
/// The committed list of every error code an <c>[ErrorCodeCatalog]</c> enum in the
/// project generates.
/// </summary>
/// <remarks>
/// A project opts in by adding an <c>ErrorCodes.txt</c> as an
/// <c>AdditionalFiles</c> item, the way a project opts into the public API analyzers
/// by adding a <c>PublicAPI.Shipped.txt</c>. Nothing else turns the rules on, and a
/// project without the file never sees them.
/// <para>
/// The file is not generated. A generator cannot write it — <c>RS1035</c> bans file
/// IO in an analyzer or a generator, and for good reason: a build that writes into
/// the source tree is not reproducible and races itself under parallel builds. So the
/// registry is an ordinary committed file, a diagnostic reports when it diverges from
/// the compilation, and a code fix writes the correction through the workspace.
/// </para>
/// </remarks>
public static class ErrorCodeRegistry
{
    /// <summary>The name of the file, matched without regard to its directory.</summary>
    public const string FileName = "ErrorCodes.txt";

    /// <summary>
    /// Whether <paramref name="path" /> names the registry, whatever directory it is
    /// in.
    /// </summary>
    public static bool Matches(string? path) =>
        path is not null
     && string.Equals(NameOf(path), FileName, StringComparison.OrdinalIgnoreCase);

    internal static AdditionalText? Find(ImmutableArray<AdditionalText> files)
    {
        foreach (AdditionalText file in files)
        {
            if (Matches(file.Path)) return file;
        }

        return null;
    }

    /// <summary>
    /// The codes the file lists, in the order it lists them. Blank lines and lines
    /// starting with <c>#</c> are ignored, and a repeated code is kept once.
    /// </summary>
    internal static ImmutableArray<Entry> Parse(SourceText text)
    {
        ImmutableArray<Entry>.Builder entries = ImmutableArray.CreateBuilder<Entry>();

        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (TextLine line in text.Lines)
        {
            string content = line.ToString().Trim();

            if (content.Length == 0 || content[0] == '#') continue;

            if (!seen.Add(content)) continue;

            entries.Add(
                new Entry(
                    content,
                    line.Span,
                    text.Lines.GetLinePositionSpan(line.Span)));
        }

        return entries.ToImmutable();
    }

    /// <summary>
    /// The content the file should have: its leading comment block, then every code
    /// once, ordinally sorted.
    /// </summary>
    /// <remarks>
    /// Sorting means a rename shows up as one removal and one addition rather than as
    /// a reordering of everything after it, which is the whole point of committing the
    /// file. The existing line ending is kept so the rewrite does not turn the file
    /// over from CRLF to LF or back on whoever runs the fix.
    /// <para>
    /// Public because the code fix calls it. The fix recomputes the file from the
    /// compilation rather than reading it off the diagnostic: WM2019 has to be a local
    /// diagnostic for a fix to be offered at all, and a local diagnostic knows only
    /// about its own enum.
    /// </para>
    /// </remarks>
    public static string Render(SourceText existing, IEnumerable<string> codes)
    {
        string newLine = existing.ToString().IndexOf("\r\n", StringComparison.Ordinal)
                      >= 0
            ? "\r\n"
            : "\n";

        var builder = new StringBuilder();

        foreach (TextLine line in existing.Lines)
        {
            string content = line.ToString().Trim();

            if (content.Length == 0 || content[0] != '#') break;

            builder.Append(content).Append(newLine);
        }

        foreach (string code in codes.Distinct(StringComparer.Ordinal)
                                     .OrderBy(code => code, StringComparer.Ordinal))
        {
            builder.Append(code).Append(newLine);
        }

        return builder.ToString();
    }

    private static string NameOf(string path)
    {
        int separator = path.LastIndexOfAny(new[] { '/', '\\' });

        return separator < 0 ? path : path.Substring(separator + 1);
    }

    internal readonly struct Entry
    {
        public Entry(string code, TextSpan span, LinePositionSpan lineSpan)
        {
            Code = code;
            Span = span;
            LineSpan = lineSpan;
        }

        public string Code { get; }

        public TextSpan Span { get; }

        public LinePositionSpan LineSpan { get; }
    }
}
