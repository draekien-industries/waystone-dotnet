namespace Waystone.Monads.Options.Iterators;

using System.Linq;
using JetBrains.Annotations;
using Shouldly;
using Waystone.Monads.Extensions;
using Xunit;

[TestSubject(typeof(OptionIterator<>))]
public sealed class OptionIteratorTests
{
    [Fact]
    public void GivenSome_WhenInvokingNext_ReturnSome()
    {
        Option<int> some = Option.Some(1);
        var result = some.Iter().Next();
        result.ShouldBeSomeValue(1);
    }

    [Fact]
    public void GivenNone_WhenInvokingNext_ReturnNone()
    {
        Option<int> none = Option.None<int>();
        var result = none.Iter().Next();
        result.ShouldBeNone();
    }

    [Fact]
    public void GivenNextInvokedOnce_WhenInvokingNext_ReturnNone()
    {
        Option<int> some = Option.Some(1);
        OptionIterator<int> iter = some.Iter();
        var first = iter.Next();
        var second = iter.Next();

        first.ShouldBeSome();
        second.ShouldBeNone();
    }

    [Fact]
    public void WhenInvokingCount_ThenReturnOne()
    {
        Option<int> some = Option.Some(1);
        var result = some.Iter().Count();
        result.ShouldBe(1);
    }
}