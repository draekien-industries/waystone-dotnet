namespace Waystone.DocSnippets;

using Shouldly;
using System.Collections.Generic;
using Waystone.Monads.Results.Errors;
using Xunit;

public sealed class SnippetInjectorTests
{
    private static readonly IReadOnlyDictionary<string, Snippet> Snippets =
        new Dictionary<string, Snippet>
        {
            ["roll-initiative"] = new(
                "roll-initiative",
                "var order = party.Roll();",
                "sample/Party.cs"),
            ["short-rest"] = new("short-rest", "party.Rest();", "sample/Party.cs"),
        };

    [Fact]
    public void FillsAnEmptySlot() =>
        Inject(
                """
                Before.

                <!-- snippet: roll-initiative -->
                <!-- endSnippet -->

                After.
                """)
           .ShouldBe(
                """
                Before.

                <!-- snippet: roll-initiative -->
                <!-- source: sample/Party.cs -->
                ```csharp
                var order = party.Roll();
                ```
                <!-- endSnippet -->

                After.
                """);

    [Fact]
    public void ReplacesWhateverTheSlotHeldBefore() =>
        Inject(
                """
                <!-- snippet: short-rest -->
                <!-- source: sample/Old.cs -->
                ```csharp
                party.TakeANap();
                ```
                <!-- endSnippet -->
                """)
           .ShouldBe(
                """
                <!-- snippet: short-rest -->
                <!-- source: sample/Party.cs -->
                ```csharp
                party.Rest();
                ```
                <!-- endSnippet -->
                """);

    [Fact]
    public void LeavesACurrentPageByteIdentical()
    {
        string page = Inject(
            """
            <!-- snippet: short-rest -->
            <!-- endSnippet -->
            """);

        Inject(page).ShouldBe(page);
    }

    [Fact]
    public void FillsEverySlotOnAPage() =>
        SnippetInjector
           .Inject(
                """
                <!-- snippet: roll-initiative -->
                <!-- endSnippet -->
                <!-- snippet: short-rest -->
                <!-- endSnippet -->
                """,
                Snippets,
                "page.md")
           .ShouldBeOk()
           .Keys.ShouldBe(["roll-initiative", "short-rest"]);

    [Fact]
    public void KeepsTheLineEndingsThePageAlreadyUsed() =>
        Inject("<!-- snippet: short-rest -->\r\n<!-- endSnippet -->")
           .ShouldBe(
                "<!-- snippet: short-rest -->\r\n"
              + "<!-- source: sample/Party.cs -->\r\n"
              + "```csharp\r\n"
              + "party.Rest();\r\n"
              + "```\r\n"
              + "<!-- endSnippet -->");

    [Fact]
    public void LeavesAPageWithNoSlotsAlone() =>
        Inject("Just prose.").ShouldBe("Just prose.");

    [Fact]
    public void RejectsASlotNoSourceFileDefines()
    {
        Error error = Failure(
            """
            <!-- snippet: counterspell -->
            <!-- endSnippet -->
            """);

        error.Code.ShouldBe(DocSnippetError.UnknownSnippet.ToErrorCode());
        error.Message.ShouldContain("no source file defines snippet 'counterspell'");
    }

    [Fact]
    public void RejectsAnUnclosedSlot()
    {
        Error error = Failure("<!-- snippet: short-rest -->");

        error.Code.ShouldBe(DocSnippetError.UnterminatedSlot.ToErrorCode());
        error.Message.ShouldContain("the slot for 'short-rest' is never closed");
    }

    [Fact]
    public void RejectsASlotClosedOnlyByTheNextSlot() =>
        Failure(
                """
                <!-- snippet: short-rest -->
                <!-- snippet: roll-initiative -->
                <!-- endSnippet -->
                """)
           .Message.ShouldContain("the slot for 'short-rest' is never closed");

    [Fact]
    public void LeavesASlotShownInsideAFencedBlockAlone()
    {
        string page =
            """
            To add one, write this:

            ```
            <!-- snippet: short-rest -->
            <!-- endSnippet -->
            ```

            Then run the tool.
            """;

        Inject(page).ShouldBe(page);
    }

    [Fact]
    public void StillFillsASlotAfterAFencedBlockCloses() =>
        Inject(
                """
                ```
                <!-- snippet: roll-initiative -->
                ```

                <!-- snippet: short-rest -->
                <!-- endSnippet -->
                """)
           .ShouldBe(
                """
                ```
                <!-- snippet: roll-initiative -->
                ```

                <!-- snippet: short-rest -->
                <!-- source: sample/Party.cs -->
                ```csharp
                party.Rest();
                ```
                <!-- endSnippet -->
                """);

    private static string Inject(string markdown) =>
        SnippetInjector.Inject(markdown, Snippets, "page.md").ShouldBeOk().Markdown;

    private static Error Failure(string markdown) =>
        SnippetInjector.Inject(markdown, Snippets, "page.md").ShouldBeErr();
}
