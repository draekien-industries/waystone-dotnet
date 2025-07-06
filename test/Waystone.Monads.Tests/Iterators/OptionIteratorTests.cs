namespace Waystone.Monads.Iterators;

using System;
using System.Linq;
using JetBrains.Annotations;
using Shouldly;
using Waystone.Monads.Extensions;
using Waystone.Monads.Iterators.Primitives;
using Waystone.Monads.Options;
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

    [Fact]
    public void WhenInvokingSizeHint_ThenReturnExpected()
    {
        Option<int> some = Option.Some(1);
        var iter = some.Iter();
        var sizeHint = iter.SizeHint();
        sizeHint.LowerBound.ShouldBe((PosInt)1);
        sizeHint.UpperBound.ShouldBeSomeValue(1);

        iter.Next();
        sizeHint = iter.SizeHint();
        sizeHint.LowerBound.ShouldBe((PosInt)0);
        sizeHint.UpperBound.ShouldBeNone();
    }

    [Fact]
    public void WhenInvokingNth_ThenReturnExpected()
    {
        Option<int> some = Option.Some(1);
        var iter = some.Iter();
        iter.Nth(0).ShouldBeSomeValue(1);
        iter.Nth(0).ShouldBeNone();
    }
}
