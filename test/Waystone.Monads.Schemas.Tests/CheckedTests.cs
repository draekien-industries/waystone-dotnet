namespace Waystone.Monads.Schemas;

using Shouldly;
using Xunit;

public sealed class CheckedTests
{
    [Fact]
    public void GivenInstance_WhenComparingAgainstDefault_ThenTheyAreTheSameValue()
    {
        Checked.Instance.ShouldBe(default(Checked));
    }

    [Fact]
    public void GivenTwoValues_WhenComparing_ThenTheyAreAlwaysEqual()
    {
        Checked left = Checked.Instance;
        Checked right = default;

        left.Equals(right).ShouldBeTrue();
        left.Equals((object)right).ShouldBeTrue();
        (left == right).ShouldBeTrue();
        (left != right).ShouldBeFalse();
        left.GetHashCode().ShouldBe(right.GetHashCode());
    }

    [Fact]
    public void GivenAnotherType_WhenComparing_ThenItIsNotEqual()
    {
        Checked.Instance.Equals("checked").ShouldBeFalse();
        Checked.Instance.Equals(null).ShouldBeFalse();
    }

    [Fact]
    public void GivenAValue_WhenRendering_ThenReturnTheFixedText()
    {
        Checked.Instance.ToString().ShouldBe("checked");
    }
}
