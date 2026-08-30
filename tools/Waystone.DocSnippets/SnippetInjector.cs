using System.Text.RegularExpressions;
using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;
using Waystone.Monads.Results.Extensions;

namespace Waystone.DocSnippets;

/// <summary>A rewritten page and the snippet keys its slots named.</summary>
/// <param name="Markdown">The page content after every slot was filled.</param>
/// <param name="Keys">Every key a slot asked for, in the order the slots appear.</param>
public sealed record Injection(string Markdown, IReadOnlyList<string> Keys);

/// <summary>Fills the snippet slots in a markdown page from the snippets read out of source.</summary>
public static partial class SnippetInjector
{
    /// <summary>Rewrites every slot in one page.</summary>
    /// <param name="markdown">The page's content.</param>
    /// <param name="snippets">The snippets available, keyed by name.</param>
    /// <param name="pagePath">The page's path, quoted in any error raised.</param>
    /// <returns>
    /// The rewritten page and the keys it asked for, or the first slot that is
    /// unterminated or names a snippet nothing defines. The page comes back
    /// byte-identical when every slot was already current, which is what
    /// <c>--check</c> rests on.
    /// </returns>
    public static Result<Injection, Error> Inject(
        string markdown,
        IReadOnlyDictionary<string, Snippet> snippets,
        string pagePath)
    {
        string[] lines = Lines.Split(markdown);

        return FindSlots(lines, pagePath)
              .AndThen(slots => slots.Select(slot => Fill(slot, snippets, pagePath)).Collect())
              .Map(filled => Render(lines, filled, Lines.NewLineOf(markdown)));
    }

    private static Result<IReadOnlyList<Slot>, Error> FindSlots(string[] lines, string pagePath)
    {
        List<Slot> slots = [];
        bool fenced = false;

        for (int i = 0; i < lines.Length; i++)
        {
            if (Fence().IsMatch(lines[i]))
            {
                fenced = !fenced;

                continue;
            }

            Match start = SlotStart().Match(lines[i]);

            if (fenced || !start.Success)
            {
                continue;
            }

            string key = start.Groups["key"].Value;
            int close = EndOfSlot(lines, i).UnwrapOr(-1);

            if (close < 0)
            {
                return Result.Err<IReadOnlyList<Slot>>(
                    DocSnippetError.UnterminatedSlot.ToError(
                        $"{pagePath}: the slot for '{key}' is never closed. "
                      + "Add <!-- endSnippet --> after it."));
            }

            slots.Add(new Slot(i, key, close));
            i = close;
        }

        return Result.Ok<IReadOnlyList<Slot>>(slots);
    }

    private static Option<int> EndOfSlot(string[] lines, int start)
    {
        for (int i = start + 1; i < lines.Length; i++)
        {
            if (SlotEnd().IsMatch(lines[i]))
            {
                return Option.Some(i);
            }

            if (SlotStart().IsMatch(lines[i]))
            {
                break;
            }
        }

        return Option.None<int>();
    }

    private static Result<FilledSlot, Error> Fill(
        Slot slot,
        IReadOnlyDictionary<string, Snippet> snippets,
        string pagePath) =>
        Lookup(snippets, slot.Key)
           .OkOr(
                DocSnippetError.UnknownSnippet.ToError(
                    $"{pagePath}: no source file defines snippet '{slot.Key}'. "
                  + "Wrap the code in a #region of that name in the sample project, "
                  + "or fix the slot."))
           .Map(snippet => new FilledSlot(slot, snippet));

    private static Option<Snippet> Lookup(
        IReadOnlyDictionary<string, Snippet> snippets,
        string key) =>
        snippets.TryGetValue(key, out Snippet? snippet)
            ? Option.Some(snippet)
            : Option.None<Snippet>();

    private static Injection Render(
        string[] lines,
        IReadOnlyList<FilledSlot> filled,
        string newLine)
    {
        Dictionary<int, FilledSlot> byOpeningLine = filled.ToDictionary(slot => slot.Slot.Open);
        List<string> output = [];

        for (int i = 0; i < lines.Length; i++)
        {
            output.Add(lines[i]);

            if (!byOpeningLine.TryGetValue(i, out FilledSlot? slot))
            {
                continue;
            }

            output.Add($"<!-- source: {slot.Snippet.SourcePath} -->");
            output.Add("```csharp");
            output.AddRange(Lines.Split(slot.Snippet.Body));
            output.Add("```");

            i = slot.Slot.Close;
            output.Add(lines[i]);
        }

        return new Injection(
            string.Join(newLine, output),
            [..filled.Select(slot => slot.Slot.Key)]);
    }

    private sealed record Slot(int Open, string Key, int Close);

    private sealed record FilledSlot(Slot Slot, Snippet Snippet);

    [GeneratedRegex(@"^<!--\s*snippet:\s*(?<key>[a-z0-9]+(-[a-z0-9]+)*)\s*-->\s*$")]
    private static partial Regex SlotStart();

    [GeneratedRegex(@"^<!--\s*endSnippet\s*-->\s*$")]
    private static partial Regex SlotEnd();

    [GeneratedRegex(@"^\s*(```|~~~)")]
    private static partial Regex Fence();
}
