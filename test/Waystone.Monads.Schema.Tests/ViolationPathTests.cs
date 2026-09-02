namespace Waystone.Monads.Schemas;

using Shouldly;
using Xunit;

public sealed class ViolationPathTests
{
    [Fact]
    public void GivenRoot_WhenRendering_ThenReturnEmptyString()
    {
        ViolationPath.Root.ToString().ShouldBe(string.Empty);
        ViolationPath.Root.IsRoot.ShouldBeTrue();
    }

    [Fact]
    public void GivenRoot_WhenAppendingProperty_ThenOmitTheLeadingSeparator()
    {
        ViolationPath sut = ViolationPath.Root.Append("sku");

        sut.ToString().ShouldBe("sku");
        sut.IsRoot.ShouldBeFalse();
    }

    [Fact]
    public void GivenProperty_WhenAppendingProperty_ThenSeparateWithADot()
    {
        ViolationPath sut = ViolationPath.Root.Append("line").Append("sku");

        sut.ToString().ShouldBe("line.sku");
    }

    [Fact]
    public void GivenPropertyAndIndex_WhenAppendingProperty_ThenRenderTheAccessAsWritten()
    {
        ViolationPath sut = ViolationPath.Root.Append("items")
                                         .AppendIndex(3)
                                         .Append("sku");

        sut.ToString().ShouldBe("items[3].sku");
    }

    [Fact]
    public void GivenProperty_WhenAppendingKey_ThenQuoteTheKey()
    {
        ViolationPath sut = ViolationPath.Root.Append("rates").AppendKey("AUD");

        sut.ToString().ShouldBe("rates[\"AUD\"]");
    }

    [Fact]
    public void GivenIndexAndKey_WhenRendering_ThenKeepThemDistinguishable()
    {
        ViolationPath indexed = ViolationPath.Root.Append("rates").AppendIndex(1);
        ViolationPath keyed = ViolationPath.Root.Append("rates").AppendKey("1");

        indexed.ToString().ShouldBe("rates[1]");
        keyed.ToString().ShouldBe("rates[\"1\"]");
        indexed.Equals(keyed).ShouldBeFalse();
    }

    [Fact]
    public void GivenTwoPathsBuiltTheSameWay_WhenComparing_ThenTheyAreEqual()
    {
        ViolationPath left = ViolationPath.Root.Append("items").AppendIndex(3);
        ViolationPath right = ViolationPath.Root.Append("items").AppendIndex(3);

        left.Equals(right).ShouldBeTrue();
        left.Equals((object)right).ShouldBeTrue();
        left.GetHashCode().ShouldBe(right.GetHashCode());
    }

    [Fact]
    public void GivenDifferentPaths_WhenComparing_ThenTheyAreNotEqual()
    {
        ViolationPath sut = ViolationPath.Root.Append("sku");

        sut.Equals(ViolationPath.Root.Append("SKU")).ShouldBeFalse();
    }

    [Fact]
    public void GivenNull_WhenComparing_ThenItIsNotEqual()
    {
        ViolationPath sut = ViolationPath.Root.Append("sku");

        sut.Equals(null).ShouldBeFalse();
        sut.Equals((object?)null).ShouldBeFalse();
    }

    [Fact]
    public void GivenAnotherType_WhenComparing_ThenItIsNotEqual()
    {
        ViolationPath.Root.Equals("").ShouldBeFalse();
    }

    [Fact]
    public void GivenAppend_WhenCalled_ThenTheReceiverIsUnchanged()
    {
        ViolationPath sut = ViolationPath.Root.Append("items");

        sut.Append("sku").ToString().ShouldBe("items.sku");
        sut.ToString().ShouldBe("items");
    }
}
