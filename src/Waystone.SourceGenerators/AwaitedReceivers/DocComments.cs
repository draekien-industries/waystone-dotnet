namespace Waystone.SourceGenerators.AwaitedReceivers;

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;

internal static class DocComments
{
    private const int MaxInheritDocHops = 4;

    /// <summary>
    /// The source member's documentation with every tag but <c>&lt;summary&gt;</c>
    /// forwarded verbatim, and the established await phrasing prepended to the summary.
    /// </summary>
    /// <remarks>
    /// Empty when the source member has no usable documentation and
    /// <paramref name="summaryOverride" /> is <see langword="null" />, so the
    /// generated member is emitted with no doc comment rather than an empty one.
    /// Passing an override is what documents such a member.
    /// </remarks>
    public static IEnumerable<string> Render(
        IMethodSymbol source,
        ITypeSymbol receiverType,
        Compilation compilation,
        string? summaryOverride)
    {
        XElement? member = Load(source, compilation, 0);

        if (member is null)
        {
            if (summaryOverride is null) return [];

            member = new XElement("member");
        }

        XElement? summary = member.Element("summary");

        if (summaryOverride is not null)
        {
            if (summary is null)
            {
                summary = new XElement("summary");
                member.AddFirst(summary);
            }

            summary.ReplaceNodes(new XText($"\n{summaryOverride.Trim()}\n"));
        }
        else if (summary is not null)
        {
            PrependAwaitPhrasing(summary, receiverType);
        }

        return member.Elements()
                     .SelectMany(
                          element => element
                                    .ToString(SaveOptions.DisableFormatting)
                                    .Split('\n'))
                     .Select(static line => line.Trim())
                     .Select(static line => line.Length == 0 ? "///" : "/// " + line);
    }

    /// <summary>
    /// The documentation comment ID for <paramref name="type" />. The ID form is used
    /// rather than the source form because the generated file carries no using
    /// directives, so a source-form cref would not resolve.
    /// </summary>
    public static string Cref(ITypeSymbol type) =>
        DocumentationCommentId.CreateDeclarationId(type.OriginalDefinition)
     ?? type.Name;

    /// <summary>
    /// The member's documentation, following the <c>&lt;inheritdoc /&gt;</c> the compiler
    /// leaves on an extension block's compatibility static form back to the declaration
    /// that carries the real text.
    /// </summary>
    /// <remarks>
    /// Null when the member has no comment, when its XML does not parse, and when a
    /// lone <c>&lt;inheritdoc /&gt;</c> has no <c>cref</c> or one that does not
    /// resolve to a method — a missing comment and an unfollowable one are the same
    /// outcome to the caller. The chain is followed at most
    /// <see cref="MaxInheritDocHops" /> times.
    /// </remarks>
    private static XElement? Load(
        IMethodSymbol source,
        Compilation compilation,
        int depth)
    {
        string? xml = source.GetDocumentationCommentXml();

        if (string.IsNullOrWhiteSpace(xml)) return null;

        XElement member;

        try
        {
            member = XElement.Parse(xml!, LoadOptions.PreserveWhitespace);
        }
        catch (XmlException)
        {
            return null;
        }

        if (depth >= MaxInheritDocHops) return member;

        List<XElement> tags = member.Elements().ToList();

        if (tags.Count != 1 || tags[0].Name.LocalName != "inheritdoc") return member;

        if (tags[0].Attribute("cref")?.Value is not { } cref) return null;

        return DocumentationCommentId.GetFirstSymbolForDeclarationId(cref, compilation)
        is IMethodSymbol inherited
            ? Load(inherited, compilation, depth + 1)
            : null;
    }

    private static void PrependAwaitPhrasing(XElement summary, ITypeSymbol receiverType)
    {
        if (summary.FirstNode is XText leading)
        {
            leading.Value = Decapitalise(leading.Value);

            if (!StartsWithWhitespace(leading.Value))
            {
                leading.Value = "\n" + leading.Value;
            }
        }

        if (summary.LastNode is XText trailing && !EndsWithWhitespace(trailing.Value))
        {
            trailing.Value += "\n";
        }

        summary.AddFirst(
            new XText("\nAsynchronously awaits the "),
            new XElement("see", new XAttribute("cref", Cref(receiverType))),
            new XText(" then"));
    }

    private static bool StartsWithWhitespace(string text) =>
        text.Length > 0 && char.IsWhiteSpace(text[0]);

    private static bool EndsWithWhitespace(string text) =>
        text.Length > 0 && char.IsWhiteSpace(text[text.Length - 1]);

    private static string Decapitalise(string text)
    {
        for (var i = 0; i < text.Length; i++)
        {
            if (char.IsWhiteSpace(text[i])) continue;

            if (!char.IsUpper(text[i])) return text;

            var isAcronym = i + 1 < text.Length && char.IsUpper(text[i + 1]);

            return isAcronym
                ? text
                : text.Substring(0, i)
                + char.ToLowerInvariant(text[i])
                + text.Substring(i + 1);
        }

        return text;
    }
}
