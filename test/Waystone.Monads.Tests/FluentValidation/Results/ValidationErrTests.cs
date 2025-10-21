namespace Waystone.Monads.FluentValidation.Results;

using global::FluentValidation.Results;
using Monads.Extensions;
using Options;
using Shouldly;
using Xunit;

public class ValidationErrTests
{
    [Fact]
    public void
        GivenInvalidValidationResult_WhenCreatingValidationErr_ThenReturnSome()
    {
        var validationResult = new ValidationResult(
        [
            new ValidationFailure("Property", "Error message"),
        ]);

        Option<ValidationErr> result = ValidationErr.Create(validationResult);

        result.ShouldBeSome();
        ValidationErr validationErr = result.Unwrap();
        validationErr.AsValidationResult().ShouldBe(validationResult);
    }

    [Fact]
    public void
        GivenValidValidationResult_WhenCreatingValidationErr_ThenReturnNone()
    {
        var validationResult = new ValidationResult();

        Option<ValidationErr> result = ValidationErr.Create(validationResult);

        result.ShouldBeNone();
    }

    [Fact]
    public void WhenConvertingToError_ThenReturnExpectedErrorProperties()
    {
        var validationResult = new ValidationResult(
        [
            new ValidationFailure("Property1", "Error message 1."),
            new ValidationFailure("Property2", "Error message 2."),
        ]);

        Option<ValidationErr> validationErr =
            ValidationErr.Create(validationResult);
        var error = validationErr.Unwrap().ToError();

        error.Code.Value.ShouldBe("validation.failed");
        error.Message.ShouldBe("Error message 1; Error message 2;");
    }
}
