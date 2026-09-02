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

    [Fact]
    public void GivenAPropertyChild_WhenNesting_ThenJoinWithADot()
    {
        ViolationPath.Root.Append("order")
                    .Nest(ViolationPath.Root.Append("email"))
                    .ToString()
                    .ShouldBe("order.email");
    }

    [Fact]
    public void GivenAnIndexedChild_WhenNesting_ThenJoinWithoutADot()
    {
        ViolationPath.Root.Append("order")
                    .Nest(ViolationPath.Root.AppendIndex(2))
                    .ToString()
                    .ShouldBe("order[2]");
    }

    [Fact]
    public void GivenAKeyedChild_WhenNesting_ThenJoinWithoutADot()
    {
        ViolationPath.Root.Append("rates")
                    .Nest(ViolationPath.Root.AppendKey("AUD"))
                    .ToString()
                    .ShouldBe("rates[\"AUD\"]");
    }

    [Fact]
    public void GivenAPath_WhenRenderedTwice_ThenReturnTheSameText()
    {
        ViolationPath sut = ViolationPath.Root.Append("items")
                                        .AppendIndex(3)
                                        .Append("sku");

        sut.ToString().ShouldBe("items[3].sku");
        sut.ToString().ShouldBe("items[3].sku");
    }

    [Fact]
    public void GivenARootChild_WhenNesting_ThenKeepTheParent()
    {
        ViolationPath parent = ViolationPath.Root.Append("order");

        parent.Nest(ViolationPath.Root).ShouldBe(parent);
    }

    [Fact]
    public void GivenARootParent_WhenNesting_ThenKeepTheChild()
    {
        ViolationPath child = ViolationPath.Root.Append("email");

        ViolationPath.Root.Nest(child).ShouldBe(child);
    }

    [Fact]
    public void GivenADeepChild_WhenNesting_ThenKeepEverySegment()
    {
        ViolationPath.Root.Append("order")
                    .Nest(
                         ViolationPath.Root.Append("items")
                                     .AppendIndex(3)
                                     .Append("sku"))
                    .ToString()
                    .ShouldBe("order.items[3].sku");
    }

    [Fact]
    public void GivenAPropertyLastSegment_WhenRenaming_ThenReplaceIt()
    {
        ViolationPath.Root.Append("order")
                    .Append("email")
                    .Rename("address")
                    .ToString()
                    .ShouldBe("order.address");
    }

    [Fact]
    public void GivenAnIndexedLastSegment_WhenRenaming_ThenAppendTheName()
    {
        ViolationPath.Root.Append("items")
                    .AppendIndex(3)
                    .Rename("sku")
                    .ToString()
                    .ShouldBe("items[3].sku");
    }

    [Fact]
    public void GivenAKeyedLastSegment_WhenRenaming_ThenAppendTheName()
    {
        ViolationPath.Root.Append("rates")
                    .AppendKey("AUD")
                    .Rename("value")
                    .ToString()
                    .ShouldBe("rates[\"AUD\"].value");
    }

    [Fact]
    public void GivenRoot_WhenRenaming_ThenMakeTheNameTheOnlySegment()
    {
        ViolationPath.Root.Rename("sku").ToString().ShouldBe("sku");
    }
}
