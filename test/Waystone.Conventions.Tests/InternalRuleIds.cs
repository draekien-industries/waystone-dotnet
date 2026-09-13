namespace Waystone.Conventions.Tests;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Linq;

internal readonly record struct Citation(string Member, string RuleId);

internal static class InternalRuleIds
{
    private const string InternalNamespace = "Waystone.Internal.";

    private static readonly Regex Pattern =
        new(@"\b(?:WA|WSG)\d{4}\b", RegexOptions.CultureInvariant);

    public static IReadOnlyList<Citation> In(string documentationXml) =>
        Scan(documentationXml, exempt: false);

    public static IReadOnlyList<Citation> ExemptedIn(string documentationXml) =>
        Scan(documentationXml, exempt: true);

    private static IReadOnlyList<Citation> Scan(string documentationXml, bool exempt)
    {
        List<Citation> citations = [];

        foreach (XElement member in XDocument.Parse(documentationXml)
                                             .Descendants("member"))
        {
            string name = member.Attribute("name")?.Value ?? string.Empty;

            if (DeclaredInternally(name) != exempt)
            {
                continue;
            }

            foreach (Match match in Pattern.Matches(member.Value))
            {
                citations.Add(new Citation(name, match.Value));
            }
        }

        return citations;
    }

    private static bool DeclaredInternally(string memberName)
    {
        int colon = memberName.IndexOf(':');

        return colon >= 0
            && memberName[(colon + 1)..]
                  .StartsWith(InternalNamespace, StringComparison.Ordinal);
    }
}
