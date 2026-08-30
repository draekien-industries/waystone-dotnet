namespace Waystone.DocSnippets;

using Shouldly;
using Xunit;

public sealed class LinesTests
{
    [Theory]
    [InlineData("a\nb")]
    [InlineData("a\r\nb")]
    [InlineData("a\rb")]
    public void SplitsOnAnyLineEnding(string text) =>
        Lines.Split(text).ShouldBe(["a", "b"]);

    [Fact]
    public void SplitsTextWithNoLineEndingIntoOneLine() =>
        Lines.Split("a").ShouldBe(["a"]);

    [Fact]
    public void ReportsWindowsLineEndingsWhenAnyArePresent() =>
        Lines.NewLineOf("a\r\nb\nc").ShouldBe("\r\n");

    [Fact]
    public void ReportsUnixLineEndingsOtherwise() =>
        Lines.NewLineOf("a\nb").ShouldBe("\n");
}
