namespace Waystone.Monads.Results.Extensions;

using System.Threading.Tasks;
using AutoFixture;
using Fixtures;
using FluentValidation.Results;
using FluentValidation.Results.Extensions;
using global::FluentValidation;
using global::FluentValidation.Results;
using Shouldly;
using Xunit;

public sealed class ValueExtensionsTests
{
    private readonly Fixture _fixture;

    public ValueExtensionsTests() => _fixture = new Fixture();

    [Fact]
    public void
        GivenValidClass_WhenInvokingValidate_ThenReturnOk()
    {
        var value = _fixture.Create<TestClass>();

        Result<TestClass, ValidationErr> result =
            value.Validate(new TestValidator());

        result.IsOk.ShouldBeTrue();
        result.Unwrap().ShouldBe(value);
    }

    [Fact]
    public void GivenInvalidClass_WhenInvokingValidate_ThenReturnError()
    {
        var value = new TestClass();

        Result<TestClass, ValidationErr> result =
            value.Validate(new TestValidator());

        result.IsErr.ShouldBeTrue();
        result.UnwrapErr().Errors.Count.ShouldBe(1);
        result.UnwrapErr().RuleSetsExecuted.Length.ShouldBe(1);
        result.UnwrapErr().ToDictionary().ShouldContainKey("Value");

        result.UnwrapErr()
           .ToString()
           .ShouldBe("'Value' must not be empty.");

        result.UnwrapErr()
           .AsValidationResult()
           .ShouldBeOfType<ValidationResult>();
    }

    [Fact]
    public async Task
        GivenValidClass_WhenInvokingValidateAsync_ThenReturnOk()
    {
        var value = _fixture.Create<TestClass>();

        Result<TestClass, ValidationErr> result =
            await value.ValidateAsync(
                new TestValidator(),
                TestContext.Current.CancellationToken);

        result.IsOk.ShouldBeTrue();
        result.Unwrap().ShouldBe(value);
    }

    [Fact]
    public async Task
        GivenInvalidClass_WhenInvokingValidateAsync_ThenReturnError()
    {
        var value = new TestClass();

        Result<TestClass, ValidationErr> result =
            await value.ValidateAsync(
                new TestValidator(),
                TestContext.Current.CancellationToken);

        result.IsErr.ShouldBeTrue();
        result.UnwrapErr().Errors.Count.ShouldBe(1);
        result.UnwrapErr().RuleSetsExecuted.Length.ShouldBe(1);
        result.UnwrapErr().ToDictionary().ShouldContainKey("Value");

        result.UnwrapErr()
           .ToString()
           .ShouldBe("'Value' must not be empty.");

        result.UnwrapErr()
           .AsValidationResult()
           .ShouldBeOfType<ValidationResult>();
    }

    private class TestValidator : AbstractValidator<TestClass>
    {
        public TestValidator()
        {
            RuleFor(x => x.Value).NotEmpty();
        }
    }
}
