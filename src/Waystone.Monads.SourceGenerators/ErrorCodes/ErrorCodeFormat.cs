namespace Waystone.Monads.SourceGenerators.ErrorCodes;

using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// A parsed error code format: the literal text and placeholders of something like
/// <c>"order.{member:kebab}"</c>.
/// </summary>
internal sealed class ErrorCodeFormat
{
    public const string Default = "{enum}.{member}";

    public const string EnumPlaceholder = "enum";
    public const string MemberPlaceholder = "member";

    private readonly List<Segment> _segments;

    private ErrorCodeFormat(List<Segment> segments)
    {
        _segments = segments;
    }

    /// <summary>Whether any segment substitutes the member name.</summary>
    public bool UsesMember
    {
        get
        {
            foreach (Segment segment in _segments)
            {
                if (segment.Placeholder == MemberPlaceholder) return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Parses <paramref name="format" />, or returns the reason it is not a usable
    /// format.
    /// </summary>
    public static bool TryParse(
        string? format,
        out ErrorCodeFormat? parsed,
        out string? error)
    {
        parsed = null;
        error = null;

        if (format is null || format.Length == 0)
        {
            error = "the format is empty";

            return false;
        }

        var segments = new List<Segment>();
        var literal = new StringBuilder();
        var i = 0;

        while (i < format.Length)
        {
            char current = format[i];

            if (current == '{' && i + 1 < format.Length && format[i + 1] == '{')
            {
                literal.Append('{');
                i += 2;

                continue;
            }

            if (current == '}' && i + 1 < format.Length && format[i + 1] == '}')
            {
                literal.Append('}');
                i += 2;

                continue;
            }

            if (current == '}')
            {
                error = $"'}}' at position {i} closes nothing";

                return false;
            }

            if (current != '{')
            {
                literal.Append(current);
                i++;

                continue;
            }

            int close = format.IndexOf('}', i);

            if (close < 0)
            {
                error = $"the placeholder opened at position {i} is not closed";

                return false;
            }

            string body = format.Substring(i + 1, close - i - 1);

            if (!TryParsePlaceholder(body, out string? name, out string? transform, out error))
            {
                return false;
            }

            if (literal.Length > 0)
            {
                segments.Add(Segment.Literal(literal.ToString()));
                literal.Clear();
            }

            segments.Add(Segment.For(name!, transform));
            i = close + 1;
        }

        if (literal.Length > 0) segments.Add(Segment.Literal(literal.ToString()));

        parsed = new ErrorCodeFormat(segments);

        return true;
    }

    /// <summary>Applies the format to a concrete enum and member name.</summary>
    public string Apply(string enumName, string memberName)
    {
        var builder = new StringBuilder();

        foreach (Segment segment in _segments)
        {
            builder.Append(
                segment.Placeholder switch
                {
                    null => segment.Text!,
                    EnumPlaceholder => Casing.Apply(enumName, segment.Transform),
                    _ => Casing.Apply(memberName, segment.Transform),
                });
        }

        return builder.ToString();
    }

    /// <summary>
    /// Renders the format as a C# expression for a value that is not a declared
    /// member, where the member's name is only known at run time.
    /// </summary>
    /// <remarks>
    /// Every part that does not depend on the member is folded to a literal here, so
    /// the emitted expression is a concatenation of constants around one
    /// <c>ToString()</c>. The member's own transform is dropped rather than emitted:
    /// an undeclared value renders as its digits, and all four transforms are the
    /// identity on digits, so applying one would cost a call and change nothing.
    /// </remarks>
    public string ApplyToUndeclared(string enumName, string valueExpression)
    {
        var parts = new List<string>();
        var literal = new StringBuilder();

        foreach (Segment segment in _segments)
        {
            if (segment.Placeholder == MemberPlaceholder)
            {
                if (literal.Length > 0)
                {
                    parts.Add(Quote(literal.ToString()));
                    literal.Clear();
                }

                parts.Add(valueExpression);

                continue;
            }

            literal.Append(
                segment.Placeholder is null
                    ? segment.Text!
                    : Casing.Apply(enumName, segment.Transform));
        }

        if (literal.Length > 0) parts.Add(Quote(literal.ToString()));

        return parts.Count == 0 ? "\"\"" : string.Join(" + ", parts);
    }

    private static bool TryParsePlaceholder(
        string body,
        out string? name,
        out string? transform,
        out string? error)
    {
        name = null;
        transform = null;
        error = null;

        int colon = body.IndexOf(':');

        string rawName = colon < 0 ? body : body.Substring(0, colon);

        transform = colon < 0 ? null : body.Substring(colon + 1);

        if (rawName != EnumPlaceholder && rawName != MemberPlaceholder)
        {
            error =
                $"'{{{body}}}' is not a placeholder. Use '{{{EnumPlaceholder}}}' or '{{{MemberPlaceholder}}}'";

            return false;
        }

        if (transform is not null && !Casing.IsKnown(transform))
        {
            error =
                $"'{transform}' is not a casing. Use {Casing.Known}";

            return false;
        }

        name = rawName;

        return true;
    }

    private static string Quote(string text) =>
        "\"" + text.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";

    private readonly struct Segment
    {
        private Segment(string? text, string? placeholder, string? transform)
        {
            Text = text;
            Placeholder = placeholder;
            Transform = transform;
        }

        public string? Text { get; }

        public string? Placeholder { get; }

        public string? Transform { get; }

        public static Segment Literal(string text) => new Segment(text, null, null);

        public static Segment For(string placeholder, string? transform) =>
            new Segment(null, placeholder, transform);
    }
}

internal static class Casing
{
    public const string Kebab = "kebab";
    public const string Snake = "snake";
    public const string Lower = "lower";
    public const string Upper = "upper";

    public const string Known = "'kebab', 'snake', 'lower' or 'upper'";

    public static bool IsKnown(string transform) =>
        transform is Kebab or Snake or Lower or Upper;

    public static string Apply(string identifier, string? transform) =>
        transform switch
        {
            null => identifier,
            Lower => identifier.ToLowerInvariant(),
            Upper => identifier.ToUpperInvariant(),
            Kebab => Join(identifier, '-'),
            _ => Join(identifier, '_'),
        };

    /// <summary>
    /// Splits an identifier into words and rejoins them lowercased.
    /// </summary>
    /// <remarks>
    /// A boundary falls where a lowercase or digit meets an uppercase
    /// (<c>NotFound</c>), before the last uppercase of a run that runs into a
    /// lowercase (<c>HTTPNotFound</c> gives <c>http-not-found</c>, not
    /// <c>h-t-t-p-not-found</c>), and between letters and digits
    /// (<c>Error404</c> gives <c>error-404</c>). Existing separators are treated as
    /// boundaries too, so an identifier that already carries one does not double it.
    /// </remarks>
    private static string Join(string identifier, char separator)
    {
        var builder = new StringBuilder(identifier.Length + 4);

        for (var i = 0; i < identifier.Length; i++)
        {
            char current = identifier[i];

            if (current is '_' or '-')
            {
                Separate(builder, separator);

                continue;
            }

            if (i > 0 && IsBoundary(identifier, i)) Separate(builder, separator);

            builder.Append(char.ToLowerInvariant(current));
        }

        return builder.ToString().Trim(separator);
    }

    private static bool IsBoundary(string identifier, int i)
    {
        char current = identifier[i];
        char previous = identifier[i - 1];

        if (char.IsDigit(current) != char.IsDigit(previous)
         && (char.IsLetterOrDigit(current) && char.IsLetterOrDigit(previous)))
        {
            return true;
        }

        if (!char.IsUpper(current)) return false;

        if (!char.IsUpper(previous)) return true;

        return i + 1 < identifier.Length && char.IsLower(identifier[i + 1]);
    }

    private static void Separate(StringBuilder builder, char separator)
    {
        if (builder.Length > 0 && builder[builder.Length - 1] != separator)
        {
            builder.Append(separator);
        }
    }
}
