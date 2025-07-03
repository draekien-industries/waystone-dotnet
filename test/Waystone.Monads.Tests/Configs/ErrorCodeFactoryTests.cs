namespace Waystone.Monads.Configs;

using System;
using Shouldly;
using Xunit;

public sealed class ErrorCodeFactoryTests
{
    private readonly ErrorCodeFactory _sut;

    public ErrorCodeFactoryTests()
    {
        _sut = new ErrorCodeFactory();
    }

    public enum TestErrorCodes
    {
        Failure
    }

    [Fact]
    public void WhenInvokingFromEnum_ThenReturnExpectedCode()
    {
        var result = _sut.FromEnum(TestErrorCodes.Failure);
        result.Value.ShouldBe($"{nameof(TestErrorCodes)}.Failure");
    }

    [Fact]
    public void GivenNamedException_WhenInvokingFromException_ThenStripExceptionSuffixFromCode()
    {
        InvalidOperationException exception = new();
        var result = _sut.FromException(exception);
        result.Value.ShouldBe("InvalidOperation");
    }

    [Fact]
    public void GivenBaseException_WhenInvokingFromException_ThenReturnException()
    {
        Exception exception = new();
        var result = _sut.FromException(exception);
        result.Value.ShouldBe(nameof(Exception));
    }

    public class NonConforming : Exception { }

    [Fact]
    public void GivenNonConformingException_WhenInvokingFromException_ThenReturnExceptionName()
    {
        NonConforming exception = new();
        var result = _sut.FromException(exception);
        result.Value.ShouldBe(nameof(NonConforming));
    }
}