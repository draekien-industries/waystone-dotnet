namespace Waystone.DocSnippets;

using Shouldly;
using System.Collections.Generic;
using System.Linq;
using Waystone.Monads.Results.Errors;
using Xunit;

public sealed class SnippetReaderTests
{
    [Fact]
    public void ReadsANamedRegion()
    {
        Snippet snippet = Single(
            """
            class Party
            {
                #region roll-initiative
                var order = party.Roll();
                #endregion
            }
            """);

        snippet.Key.ShouldBe("roll-initiative");
        snippet.Body.ShouldBe("var order = party.Roll();");
        snippet.SourcePath.ShouldBe("Party.cs");
    }

    [Fact]
    public void StripsTheIndentationTheSourceCarried()
    {
        Single(
                """
                    #region cast-fireball
                    if (target is not null)
                    {
                        target.Burn();
                    }
                    #endregion
                """)
           .Body.ShouldBe(
                """
                if (target is not null)
                {
                    target.Burn();
                }
                """.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void JoinsABodyWithNewlinesWhateverTheSourceUsed()
    {
        Single("#region cast-fireball\r\ntarget.Burn();\r\nrunes.Spend();\r\n#endregion")
           .Body.ShouldBe("target.Burn();\nrunes.Spend();");
    }

    [Fact]
    public void DropsBlankLinesAtBothEnds()
    {
        Single(
                """
                #region short-rest

                party.Rest();

                #endregion
                """)
           .Body.ShouldBe("party.Rest();");
    }

    [Fact]
    public void KeepsBlankLinesInTheMiddle()
    {
        Single(
                """
                #region long-rest
                party.Rest();

                party.Wake();
                #endregion
                """)
           .Body.ShouldBe("party.Rest();\n\nparty.Wake();");
    }

    [Fact]
    public void YieldsAnEmptyBodyForAnEmptyRegion()
    {
        Single(
                """
                #region nothing-here
                #endregion
                """)
           .Body.ShouldBe("");
    }

    [Fact]
    public void IgnoresARegionWhoseNameIsNotASnippetKey()
    {
        Read(
                """
                #region Fields
                private int hitPoints;
                #endregion
                """)
           .ShouldBeEmpty();
    }

    [Fact]
    public void ReadsASnippetNestedInsideAPlainRegion()
    {
        Single(
                """
                #region Fields
                #region hit-points
                private int hitPoints;
                #endregion
                #endregion
                """)
           .Body.ShouldBe("private int hitPoints;");
    }

    [Fact]
    public void ReadsEveryRegionInOneFile()
    {
        Read(
                """
                #region first
                a();
                #endregion
                #region second
                b();
                #endregion
                """)
           .Select(snippet => snippet.Key)
           .ShouldBe(["first", "second"]);
    }

    [Fact]
    public void ToleratesATrailingCommentOnTheEndRegion()
    {
        Single(
                """
                #region wild-shape
                druid.Shift();
                #endregion wild-shape
                """)
           .Body.ShouldBe("druid.Shift();");
    }

    [Fact]
    public void RejectsARegionOpenedInsideASnippet()
    {
        Error error = SnippetReader
                     .Read(
                          """
                          #region counterspell
                          #region inner
                          x();
                          #endregion
                          #endregion
                          """,
                          "Party.cs")
                     .ShouldBeErr();

        error.Code.ShouldBe(DocSnippetError.NestedRegion.ToErrorCode());
        error.Message.ShouldContain("'inner' opens inside snippet 'counterspell'");
    }

    [Fact]
    public void RejectsAnUnterminatedSnippet()
    {
        Error error = SnippetReader
                     .Read(
                          """
                          #region hunters-mark
                          ranger.Mark(target);
                          """,
                          "Ranger.cs")
                     .ShouldBeErr();

        error.Code.ShouldBe(DocSnippetError.UnterminatedRegion.ToErrorCode());
        error.Message.ShouldContain("'hunters-mark' is never closed");
    }

    private static IReadOnlyList<Snippet> Read(string source) =>
        SnippetReader.Read(source, "Party.cs").ShouldBeOk();

    private static Snippet Single(string source) =>
        Read(source).ShouldHaveSingleItem();
}
