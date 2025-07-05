namespace Waystone.Monads.FluentValidation.Results;

using System.Collections.Generic;
using global::FluentValidation.Results;
using Shouldly;
using Waystone.Monads.Extensions;
using Xunit;

public class ValidationErrTests
{
    [Fact]
    public void GivenInvalidValidationResult_WhenCreatingValidationErr_ThenReturnSome()
    {
        var validationResult = new ValidationResult([
            new ValidationFailure("Property", "Error message")
        ]);

        var result = ValidationErr.Create(validationResult);

        result.ShouldBeSome();
        var validationErr = result.Unwrap();
        validationErr.AsValidationResult().ShouldBe(validationResult);
    }

    [Fact]
    public void GivenValidValidationResult_WhenCreatingValidationErr_ThenReturnNone()
    {
        var validationResult = new ValidationResult();

        var result = ValidationErr.Create(validationResult);

        result.ShouldBeNone();
    }

    [Fact]
    public void WhenConvertingToError_ThenReturnExpectedErrorProperties()
    {
        var validationResult = new ValidationResult([
            new ValidationFailure("Property1", "Error message 1."),
            new ValidationFailure("Property2", "Error message 2.")
        ]);

        var validationErr = ValidationErr.Create(validationResult);
        var error = validationErr.Unwrap().ToError();

        error.Code.Value.ShouldBe("validation.failed"); ;
        error.Message.ShouldBe("Error message 1; Error message 2;");
    }
}