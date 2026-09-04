namespace Waystone.Monads.Schemas;

using Shouldly;
using Xunit;

/// <summary>
/// The scan behind <c>Schema.Text.Email</c>. Written against the parser rather
/// than the rule because the subset it accepts is the decision, and a rule test
/// would only show that some values pass.
/// </summary>
public sealed class EmailAddressTests
{
    [Theory]
    [InlineData("ada@example.com")]
    [InlineData("ada.lovelace@example.co.uk")]
    [InlineData("ada+quests@example.com")]
    [InlineData("a!#$%&'*+-/=?^_`{|}~b@example.com")]
    [InlineData("ADA@EXAMPLE.COM")]
    [InlineData("ada@xn--bcher-kva.example")]
    [InlineData("ada@host-1.example")]
    [InlineData("1@2")]
    public void GivenAWellFormedAddress_WhenScanning_ThenAcceptIt(string value) =>
        EmailAddress.IsWellFormed(value).ShouldBeTrue();

    /// <summary>
    /// A real address on an internal network, and the one accepted case a reader
    /// is most likely to be surprised by.
    /// </summary>
    [Fact]
    public void GivenASingleLabelHost_WhenScanning_ThenAcceptIt() =>
        EmailAddress.IsWellFormed("root@localhost").ShouldBeTrue();

    [Theory]
    [InlineData("", "empty")]
    [InlineData("ada.example.com", "no at sign")]
    [InlineData("@example.com", "nothing before the at sign")]
    [InlineData("ada@", "nothing after it")]
    [InlineData(".ada@example.com", "leading dot in the local part")]
    [InlineData("ada.@example.com", "trailing dot in the local part")]
    [InlineData("a..b@example.com", "doubled dot in the local part")]
    [InlineData("(ada)@example.com", "comment syntax opening the local part")]
    [InlineData("ada lovelace@example.com", "space in the local part")]
    [InlineData("ada\"s@example.com", "quote in the local part")]
    [InlineData("adá@example.com", "non-ascii in the local part")]
    [InlineData("ada@b@example.com", "at sign in the local part")]
    [InlineData("ada@.example.com", "leading dot in the host")]
    [InlineData("ada@-example.com", "leading hyphen in the host")]
    [InlineData("ada@example.com.", "trailing dot in the host")]
    [InlineData("ada@example.com-", "trailing hyphen in the host")]
    [InlineData("ada@example..com", "doubled dot in the host")]
    [InlineData("ada@example-.com", "hyphen before a dot in the host")]
    [InlineData("ada@example.-com", "hyphen after a dot in the host")]
    [InlineData("ada@exam ple.com", "space in the host")]
    [InlineData("ada@exampl€.com", "non-ascii in the host")]
    [InlineData("ada@[127.0.0.1]", "bracketed address literal")]
    public void GivenAMalformedAddress_WhenScanning_ThenRejectIt(
        string value,
        string because) =>
        EmailAddress.IsWellFormed(value).ShouldBeFalse(because);

    [Fact]
    public void GivenALocalPartOverSixtyFour_WhenScanning_ThenRejectIt() =>
        EmailAddress.IsWellFormed(new string('a', 65) + "@example.com")
                    .ShouldBeFalse();

    [Fact]
    public void GivenALocalPartOfExactlySixtyFour_WhenScanning_ThenAcceptIt() =>
        EmailAddress.IsWellFormed(new string('a', 64) + "@example.com")
                    .ShouldBeTrue();

    [Fact]
    public void GivenAnAddressOverTwoFiftyFour_WhenScanning_ThenRejectIt() =>
        EmailAddress.IsWellFormed(
                         new string('a', 64) + "@" + new string('b', 190))
                    .ShouldBeFalse();

    /// <summary>
    /// The longest address the scan accepts, so the bound itself is exercised
    /// rather than only the value past it.
    /// </summary>
    [Fact]
    public void GivenAnAddressOfExactlyTwoFiftyFour_WhenScanning_ThenAcceptIt() =>
        EmailAddress.IsWellFormed(
                         new string('a', 64) + "@" + new string('b', 189))
                    .ShouldBeTrue();
}
