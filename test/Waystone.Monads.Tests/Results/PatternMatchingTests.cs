namespace Waystone.Monads.Results;

using System;
using JetBrains.Annotations;
using Shouldly;
using Xunit;

[TestSubject(typeof(Result<,>))]
public sealed class PatternMatchingTests
{
    [Fact]
    public void GivenAnOk_WhenMatchingAPositionalPattern_ThenBindTheValue()
    {
        Result<int, string> result = Result.Ok<int, string>(42);

        if (result is Ok<int, string>(var value))
        {
            value.ShouldBe(42);
        }
        else
        {
            Assert.Fail("The positional pattern did not match an Ok.");
        }
    }

    [Fact]
    public void GivenAnErr_WhenMatchingAPositionalPattern_ThenBindTheError()
    {
        Result<int, string> result = Result.Err<int, string>("broken");

        if (result is Err<int, string>(var error))
        {
            error.ShouldBe("broken");
        }
        else
        {
            Assert.Fail("The positional pattern did not match an Err.");
        }
    }

    [Fact]
    public void GivenAnOk_WhenMatchingAnErrPositionalPattern_ThenDoNotMatch()
    {
        Result<int, string> result = Result.Ok<int, string>(42);

        (result is Err<int, string>(_)).ShouldBeFalse();
        (result is Ok<int, string>(_)).ShouldBeTrue();
    }

    [Fact]
    public void GivenBothCases_WhenSwitchingOverThePair_ThenEachArmIsReached()
    {
        Describe(Result.Ok<int, string>(42)).ShouldBe("ok 42");
        Describe(Result.Err<int, string>("broken")).ShouldBe("err broken");
    }

    [Fact]
    public void GivenAnOk_WhenTryingToUnwrap_ThenReturnTrueAndTheValue()
    {
        Result<int, string> result = Result.Ok<int, string>(42);

        result.TryUnwrap(out var value).ShouldBeTrue();
        value.ShouldBe(42);
    }

    [Fact]
    public void GivenAnErr_WhenTryingToUnwrap_ThenReturnFalseAndTheDefault()
    {
        Result<int, string> result = Result.Err<int, string>("broken");

        result.TryUnwrap(out var value).ShouldBeFalse();
        value.ShouldBe(0);
    }

    [Fact]
    public void GivenAnErr_WhenTryingToUnwrapTheError_ThenReturnTrueAndTheError()
    {
        Result<int, string> result = Result.Err<int, string>("broken");

        result.TryUnwrapErr(out var error).ShouldBeTrue();
        error.ShouldBe("broken");
    }

    [Fact]
    public void GivenAnOk_WhenTryingToUnwrapTheError_ThenReturnFalseAndNull()
    {
        Result<int, string> result = Result.Ok<int, string>(42);

        result.TryUnwrapErr(out var error).ShouldBeFalse();
        error.ShouldBeNull();
    }

    [Fact]
    public void GivenAnOk_WhenTryUnwrapSucceeds_ThenTheValueIsNotNullable()
    {
        Result<string, string> result = Result.Ok<string, string>("abc");

        if (!result.TryUnwrap(out var value))
        {
            Assert.Fail("TryUnwrap did not succeed for an Ok.");

            return;
        }

        value.Length.ShouldBe(3);
    }

    [Fact]
    public void GivenAnErr_WhenTryUnwrapErrSucceeds_ThenTheErrorIsNotNullable()
    {
        Result<string, string> result = Result.Err<string, string>("broken");

        if (!result.TryUnwrapErr(out var error))
        {
            Assert.Fail("TryUnwrapErr did not succeed for an Err.");

            return;
        }

        error.Length.ShouldBe(6);
    }

    private static string Describe(Result<int, string> result) =>
        result switch
        {
            Ok<int, string>(var value) => $"ok {value}",
            Err<int, string>(var error) => $"err {error}",
            _ => throw new InvalidOperationException(
                "Result has no third case."),
        };
}
