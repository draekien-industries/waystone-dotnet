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
            error.Length.ShouldBe(6);
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
    public void GivenBothCases_WhenSwitchingAsAStatement_ThenNoArmIsNeededForNeither()
    {
        DescribeByStatement(Result.Ok<int, string>(42)).ShouldBe("ok 42");

        DescribeByStatement(Result.Err<int, string>("broken"))
           .ShouldBe("err broken");
    }

    private static string Describe(Result<int, string> result) =>
        result switch
        {
            Ok<int, string>(var value) => $"ok {value}",
            Err<int, string>(var error) => $"err {error}",
            _ => throw new InvalidOperationException(
                "Result has no third case."),
        };

    private static string DescribeByStatement(Result<int, string> result)
    {
        switch (result)
        {
            case Ok<int, string>(var value):
                return $"ok {value}";
            case Err<int, string>(var error):
                return $"err {error}";
        }

        throw new InvalidOperationException("Result has no third case.");
    }
}
