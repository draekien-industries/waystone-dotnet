namespace System.Text.Json.Serialization;

using Shouldly;
using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;
using Xunit;

public class ResultJsonConverterTests
{
    private static JsonSerializerOptions Options() =>
        new JsonSerializerOptions().AddMonadConverters();

    [Fact]
    public void WhenWritingOk_ThenTagItAndNestThePayload() =>
        JsonSerializer.Serialize(Result.Ok<int, string>(42), Options())
                      .ShouldBe("{\"$type\":\"ok\",\"value\":42}");

    [Fact]
    public void WhenWritingErr_ThenTagItAndNestThePayload() =>
        JsonSerializer.Serialize(Result.Err<int, string>("boom"), Options())
                      .ShouldBe("{\"$type\":\"err\",\"value\":\"boom\"}");

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(42)]
    public void WhenRoundTrippingOk_ThenKeepTheValue(int value)
    {
        JsonSerializerOptions options = Options();
        Result<int, string> before = Result.Ok<int, string>(value);

        Result<int, string> after =
            JsonSerializer.Deserialize<Result<int, string>>(
                JsonSerializer.Serialize(before, options),
                options)!;

        after.ShouldBe(before);
    }

    [Fact]
    public void WhenRoundTrippingErr_ThenKeepTheError()
    {
        JsonSerializerOptions options = Options();
        Result<int, string> before = Result.Err<int, string>("boom");

        Result<int, string> after =
            JsonSerializer.Deserialize<Result<int, string>>(
                JsonSerializer.Serialize(before, options),
                options)!;

        after.ShouldBe(before);
    }

    [Fact]
    public void WhenTheDiscriminatorFollowsThePayload_ThenStillReadIt() =>
        JsonSerializer.Deserialize<Result<int, string>>(
                           "{\"value\":42,\"$type\":\"ok\"}",
                           Options())
                      .ShouldBe(Result.Ok<int, string>(42));

    [Fact]
    public void WhenRenamingProperties_ThenLeaveTheWireContractAlone()
    {
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = new UpperCasePolicy(),
        };

        JsonSerializer.Serialize(
                           Result.Ok<int, string>(42),
                           options.AddMonadConverters())
                      .ShouldBe("{\"$type\":\"ok\",\"value\":42}");
    }

    [Fact]
    public void WhenTheErrorIsTheLibrarysOwn_ThenRoundTripIt()
    {
        JsonSerializerOptions options = Options();
        Result<int, Error> before =
            Result.Err<int, Error>(new Error("boom.happened", "Boom."));

        Result<int, Error> after =
            JsonSerializer.Deserialize<Result<int, Error>>(
                JsonSerializer.Serialize(before, options),
                options)!;

        after.ShouldBe(before);
    }

    [Fact]
    public void WhenThePayloadIsPolymorphic_ThenNestItsOwnDiscriminator() =>
        JsonSerializer.Serialize(
                           Result.Ok<Animal, string>(new Cat { Name = "Tom" }),
                           Options())
                      .ShouldBe(
                           "{\"$type\":\"ok\",\"value\":{\"$type\":\"cat\",\"Name\":\"Tom\"}}");

    [Fact]
    public void WhenThePayloadIsPolymorphic_ThenRoundTripItsConcreteType()
    {
        JsonSerializerOptions options = Options();

        Result<Animal, string> after =
            JsonSerializer.Deserialize<Result<Animal, string>>(
                JsonSerializer.Serialize(
                    Result.Ok<Animal, string>(new Cat { Name = "Tom" }),
                    options),
                options)!;

        after.Unwrap().ShouldBeOfType<Cat>().Name.ShouldBe("Tom");
    }

    [Theory]
    [InlineData("null")]
    [InlineData("42")]
    [InlineData("[]")]
    [InlineData("\"ok\"")]
    public void WhenThePayloadIsNotAnObject_ThenThrow(string json) =>
        Should.Throw<JsonException>(
                   () => JsonSerializer.Deserialize<Result<int, string>>(
                       json,
                       Options()))
              .Message.ShouldContain("must be a JSON object");

    [Theory]
    [InlineData("{\"value\":42}")]
    [InlineData("{\"$type\":1,\"value\":42}")]
    [InlineData("{\"$type\":null,\"value\":42}")]
    public void WhenTheDiscriminatorIsMissingOrNotAString_ThenThrow(string json) =>
        Should.Throw<JsonException>(
                   () => JsonSerializer.Deserialize<Result<int, string>>(
                       json,
                       Options()))
              .Message.ShouldContain("string \"$type\" property");

    [Fact]
    public void WhenThePayloadPropertyIsMissing_ThenThrow() =>
        Should.Throw<JsonException>(
                   () => JsonSerializer.Deserialize<Result<int, string>>(
                       "{\"$type\":\"ok\"}",
                       Options()))
              .Message.ShouldContain("\"value\" property");

    [Fact]
    public void WhenTheDiscriminatorNamesNoKnownCase_ThenThrow() =>
        Should.Throw<JsonException>(
                   () => JsonSerializer.Deserialize<Result<int, string>>(
                       "{\"$type\":\"maybe\",\"value\":42}",
                       Options()))
              .Message.ShouldContain("\"maybe\" is not a result case");

    [Theory]
    [InlineData("{\"$type\":\"ok\",\"value\":null}")]
    [InlineData("{\"$type\":\"err\",\"value\":null}")]
    public void WhenAReferenceTypePayloadIsNull_ThenThrowRatherThanBuildANullCase(
        string json) =>
        Should.Throw<JsonException>(
                   () => JsonSerializer.Deserialize<Result<string, string>>(
                       json,
                       Options()))
              .Message.ShouldContain("cannot be null");

    [Fact]
    public void WhenAValueTypePayloadIsNull_ThenLetTheSerializerRejectItFirst() =>
        Should.Throw<JsonException>(
            () => JsonSerializer.Deserialize<Result<int, string>>(
                "{\"$type\":\"ok\",\"value\":null}",
                Options()));

    [Fact]
    public void WhenThePayloadIsAnOption_ThenReadANullValueAsNone() =>
        JsonSerializer.Deserialize<Result<Option<int>, string>>(
                          "{\"$type\":\"ok\",\"value\":null}",
                          Options())
                      .ShouldBe(
                           Result.Ok<Option<int>, string>(Option.None<int>()));

    [Fact]
    public void WhenTheOkCaseHoldsANone_ThenRoundTripIt()
    {
        JsonSerializerOptions options = Options();
        Result<Option<int>, string> before =
            Result.Ok<Option<int>, string>(Option.None<int>());

        JsonSerializer.Deserialize<Result<Option<int>, string>>(
                          JsonSerializer.Serialize(before, options),
                          options)
                      .ShouldBe(before);
    }

    [Fact]
    public void WhenAskedWhetherToHandleNull_ThenSayYesSoNullCanBeRejected() =>
        new ResultJsonConverter<int, string>().HandleNull.ShouldBeTrue();

    [JsonDerivedType(typeof(Cat), "cat")]
    [JsonDerivedType(typeof(Dog), "dog")]
    public abstract class Animal
    {
        public string Name { get; set; } = string.Empty;
    }

    public sealed class Cat : Animal;

    public sealed class Dog : Animal;

    private sealed class UpperCasePolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name) =>
            name.ToUpperInvariant();
    }
}
