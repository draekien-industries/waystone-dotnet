namespace Waystone.Monads.FluentValidation.Results;

using Configs;
using global::FluentValidation.Results;
using Monads.Configs;
using Monads.Results.Errors;
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

    [Fact]
    public void
        GivenConfiguredValidationErrorCode_WhenConvertingToError_ThenUseConfiguredCode()
    {
        using (MonadOptions.BeginScope(
            options => options.UseValidationErrorCode("custom.validation")))
        {
            Error error = ValidationErr.Create(
                    new ValidationResult(
                    [
                        new ValidationFailure("Property", "Error message."),
                    ]))
               .Unwrap()
               .ToError();

            error.Code.Value.ShouldBe("custom.validation");
        }
    }

    [Fact]
    public void
        WhenConfiguringThroughMonadOptions_ThenTheValidationOptionsAreUpdated()
    {
        string originalCode = MonadValidationOptions.Global.ValidationErrorCode;

        string originalMessage =
            MonadValidationOptions.Global.FallbackValidationErrorMessage;

        try
        {
            MonadOptions.Configure(
                options => options.UseValidationErrorCode("chained.validation")
                   .UseFallbackValidationErrorMessage("chained fallback."));

            MonadValidationOptions.Global.ValidationErrorCode.ShouldBe(
                "chained.validation");

            MonadValidationOptions.Global.FallbackValidationErrorMessage
               .ShouldBe("chained fallback.");
        }
        finally
        {
            MonadValidationOptions.Global
               .UseValidationErrorCode(originalCode)
               .UseFallbackValidationErrorMessage(originalMessage);
        }
    }
}
