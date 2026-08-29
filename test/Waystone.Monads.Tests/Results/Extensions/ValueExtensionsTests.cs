namespace Waystone.Monads.Results.Extensions;

using System.Threading.Tasks;
using AutoFixture;
using Errors;
using Fixtures;
using FluentValidation.Results;
using FluentValidation.Results.Extensions;
using global::FluentValidation;
using Shouldly;
using Xunit;

public sealed class ValueExtensionsTests
{
    private readonly Fixture _fixture;

    public ValueExtensionsTests() => _fixture = new Fixture();

    [Fact]
    public void GivenValidClass_WhenInvokingValidate_ThenReturnOk()
    {
        var value = _fixture.Create<TestClass>();

        Result<TestClass, Error> result = value.Validate(new TestValidator());

        result.ShouldBeOkValue(value);
    }

    [Fact]
    public void GivenInvalidClass_WhenInvokingValidate_ThenReturnValidationError()
    {
        var value = new TestClass();

        Result<TestClass, Error> result = value.Validate(new TestValidator());

        var error = result.ShouldBeErr().ShouldBeOfType<ValidationError>();

        error.Code.Value.ShouldBe("validation.failed");
        error.Message.ShouldBe("'Value' must not be empty.");
        error.Failures.Count.ShouldBe(1);
        error.ToDictionary().ShouldContainKey("Value");
    }

    [Fact]
    public async Task GivenValidClass_WhenInvokingValidateAsync_ThenReturnOk()
    {
        var value = _fixture.Create<TestClass>();

        Result<TestClass, Error> result = await value.ValidateAsync(
            new TestValidator(),
            TestContext.Current.CancellationToken);

        result.ShouldBeOkValue(value);
    }

    [Fact]
    public async Task
        GivenInvalidClass_WhenInvokingValidateAsync_ThenReturnValidationError()
    {
        var value = new TestClass();

        Result<TestClass, Error> result = await value.ValidateAsync(
            new TestValidator(),
            TestContext.Current.CancellationToken);

        var error = result.ShouldBeErr().ShouldBeOfType<ValidationError>();

        error.Code.Value.ShouldBe("validation.failed");
        error.Message.ShouldBe("'Value' must not be empty.");
        error.Failures.Count.ShouldBe(1);
        error.ToDictionary().ShouldContainKey("Value");
    }

    private class TestValidator : AbstractValidator<TestClass>
    {
        public TestValidator()
        {
            RuleFor(x => x.Value).NotEmpty();
        }
    }
}
