namespace FluentValidation;

using System.Collections.Generic;
using Configs;
using Results;
using Shouldly;
using Waystone.Monads.Configs;
using Waystone.Monads.Results.Errors;
using Xunit;

[Collection(GlobalMonadOptionsCollection.Name)]
public class ValidationErrorTests
{
    public ValidationErrorTests() => MonadOptions.Reset();

    private static ValidationError Create(params ValidationFailure[] failures) =>
        new(new ValidationResult(failures));

    [Fact]
    public void WhenCreated_ThenJoinTheFailureMessagesWithASemicolon()
    {
        ValidationError error = Create(
            new ValidationFailure("Property1", "Error message 1."),
            new ValidationFailure("Property2", "Error message 2."));

        error.Code.Value.ShouldBe("validation.failed");
        error.Message.ShouldBe("Error message 1.; Error message 2.");
    }

    [Fact]
    public void WhenCreated_ThenKeepTheFailuresInTheOrderReported()
    {
        var first = new ValidationFailure("Property1", "First.");
        var second = new ValidationFailure("Property2", "Second.");

        ValidationError error = Create(first, second);

        error.Failures.ShouldBe([first, second]);
    }

    [Fact]
    public void
        GivenConfiguredValidationErrorCode_WhenCreated_ThenUseTheConfiguredCode()
    {
        using (MonadOptions.BeginScope(
            options => options.UseValidationErrorCode("custom.validation")))
        {
            Create(new ValidationFailure("Property", "Error message."))
               .Code.Value.ShouldBe("custom.validation");
        }
    }

    [Fact]
    public void GivenAScopeThatEndedAfterCreation_ThenKeepTheCodeItWasCreatedWith()
    {
        ValidationError error;

        using (MonadOptions.BeginScope(
            options => options.UseValidationErrorCode("scoped.validation")))
        {
            error = Create(new ValidationFailure("Property", "Error message."));
        }

        error.Code.Value.ShouldBe("scoped.validation");
    }

    [Fact]
    public void WhenConvertingToDictionary_ThenGroupEveryMessageByItsProperty()
    {
        ValidationError error = Create(
            new ValidationFailure("Property1", "First."),
            new ValidationFailure("Property1", "Second."),
            new ValidationFailure("Property2", "Third."));

        IDictionary<string, string[]> dictionary = error.ToDictionary();

        dictionary.Count.ShouldBe(2);
        dictionary["Property1"].ShouldBe(["First.", "Second."]);
        dictionary["Property2"].ShouldBe(["Third."]);
    }

    [Fact]
    public void
        GivenTwoErrorsWithTheSameMessage_WhenComparing_ThenIgnoreTheFailureInstances()
    {
        ValidationError left =
            Create(new ValidationFailure("Property", "Error message."));

        ValidationError right =
            Create(new ValidationFailure("Property", "Error message."));

        left.Equals(right).ShouldBeTrue();
        (left == right).ShouldBeTrue();
        (left != right).ShouldBeFalse();
        left.GetHashCode().ShouldBe(right.GetHashCode());
    }

    [Fact]
    public void
        GivenTwoErrorsWithDifferentMessages_WhenComparing_ThenTheyAreNotEqual()
    {
        ValidationError left =
            Create(new ValidationFailure("Property", "Error message."));

        ValidationError right =
            Create(new ValidationFailure("Property", "Other message."));

        left.Equals(right).ShouldBeFalse();
        (left == right).ShouldBeFalse();
    }

    [Fact]
    public void
        GivenTwoErrorsWithDifferentCodes_WhenComparing_ThenTheyAreNotEqual()
    {
        ValidationError left =
            Create(new ValidationFailure("Property", "Error message."));

        ValidationError right;

        using (MonadOptions.BeginScope(
            options => options.UseValidationErrorCode("other.validation")))
        {
            right = Create(new ValidationFailure("Property", "Error message."));
        }

        left.Equals(right).ShouldBeFalse();
    }

    [Fact]
    public void GivenNull_WhenComparing_ThenTheyAreNotEqual()
    {
        ValidationError error =
            Create(new ValidationFailure("Property", "Error message."));

        error.Equals(null).ShouldBeFalse();
        (error == null).ShouldBeFalse();
        (error != null).ShouldBeTrue();
    }

    [Fact]
    public void
        GivenAPlainErrorWithTheSameCodeAndMessage_WhenComparing_ThenTheyAreNotEqual()
    {
        ValidationError validationError =
            Create(new ValidationFailure("Property", "Error message."));

        Error plain = new(validationError.Code, validationError.Message);

        validationError.Equals(plain).ShouldBeFalse();
        plain.Equals(validationError).ShouldBeFalse();
    }

    [Fact]
    public void WhenConvertingToString_ThenRenderAsCodeThenMessage()
    {
        ValidationError error =
            Create(new ValidationFailure("Property", "Error message."));

        error.ToString().ShouldBe("[validation.failed] Error message.");
    }
}
