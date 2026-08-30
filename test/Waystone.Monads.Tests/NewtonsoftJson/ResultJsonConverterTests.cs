namespace Newtonsoft.Json;

using System;
using System.IO;
using Serialization;
using Shouldly;
using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;
using Xunit;

public class ResultJsonConverterTests
{
    private static JsonSerializerSettings Settings() =>
        new JsonSerializerSettings().AddMonadConverters();

    [Fact]
    public void WhenWritingOk_ThenTagItAndNestThePayload() =>
        JsonConvert.SerializeObject(Result.Ok<int, string>(42), Settings())
                   .ShouldBe("{\"$type\":\"ok\",\"value\":42}");

    [Fact]
    public void WhenWritingErr_ThenTagItAndNestThePayload() =>
        JsonConvert.SerializeObject(Result.Err<int, string>("boom"), Settings())
                   .ShouldBe("{\"$type\":\"err\",\"value\":\"boom\"}");

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(42)]
    public void WhenRoundTrippingOk_ThenKeepTheValue(int value)
    {
        JsonSerializerSettings settings = Settings();
        Result<int, string> before = Result.Ok<int, string>(value);

        JsonConvert.DeserializeObject<Result<int, string>>(
                        JsonConvert.SerializeObject(before, settings),
                        settings)
                   .ShouldBe(before);
    }

    [Fact]
    public void WhenRoundTrippingErr_ThenKeepTheError()
    {
        JsonSerializerSettings settings = Settings();
        Result<int, string> before = Result.Err<int, string>("boom");

        JsonConvert.DeserializeObject<Result<int, string>>(
                        JsonConvert.SerializeObject(before, settings),
                        settings)
                   .ShouldBe(before);
    }

    [Fact]
    public void WhenTheDiscriminatorFollowsThePayload_ThenStillReadIt() =>
        JsonConvert.DeserializeObject<Result<int, string>>(
                        "{\"value\":42,\"$type\":\"ok\"}",
                        Settings())
                   .ShouldBe(Result.Ok<int, string>(42));

    [Fact]
    public void WhenRenamingProperties_ThenLeaveTheWireContractAlone()
    {
        JsonSerializerSettings settings = Settings();
        settings.ContractResolver = new CamelCasePropertyNamesContractResolver();

        JsonConvert.SerializeObject(Result.Ok<int, string>(42), settings)
                   .ShouldBe("{\"$type\":\"ok\",\"value\":42}");
    }

    [Fact]
    public void WhenTheErrorIsTheLibrarysOwn_ThenRoundTripIt()
    {
        JsonSerializerSettings settings = Settings();
        Result<int, Error> before =
            Result.Err<int, Error>(new Error("boom.happened", "Boom."));

        JsonConvert.DeserializeObject<Result<int, Error>>(
                        JsonConvert.SerializeObject(before, settings),
                        settings)
                   .ShouldBe(before);
    }

    [Fact]
    public void WhenTheResultIsAModelsProperty_ThenRoundTripIt()
    {
        JsonSerializerSettings settings = Settings();

        JsonConvert.DeserializeObject<Attempt>(
                        JsonConvert.SerializeObject(
                            new Attempt
                            {
                                Outcome = Result.Err<int, string>("boom"),
                            },
                            settings),
                        settings)!
                   .Outcome.ShouldBe(Result.Err<int, string>("boom"));
    }

    [Fact]
    public void WhenTheSerializerWritesItsOwnTypeNames_ThenNestThemBelowTheResults()
    {
        JsonSerializerSettings settings = Settings();
        settings.TypeNameHandling = TypeNameHandling.All;

        string json = JsonConvert.SerializeObject(
            Result.Ok<Box, string>(new Box("crate")),
            settings);

        json.ShouldStartWith("{\"$type\":\"ok\",\"value\":{\"$type\":\"");
    }

    [Theory]
    [InlineData("null")]
    [InlineData("42")]
    [InlineData("[]")]
    [InlineData("\"ok\"")]
    public void WhenThePayloadIsNotAnObject_ThenThrow(string json) =>
        Should.Throw<JsonSerializationException>(
                   () => JsonConvert.DeserializeObject<Result<int, string>>(
                       json,
                       Settings()))
              .Message.ShouldContain("must be a JSON object");

    [Theory]
    [InlineData("{\"value\":42}")]
    [InlineData("{\"$type\":1,\"value\":42}")]
    [InlineData("{\"$type\":null,\"value\":42}")]
    public void WhenTheDiscriminatorIsMissingOrNotAString_ThenThrow(string json) =>
        Should.Throw<JsonSerializationException>(
                   () => JsonConvert.DeserializeObject<Result<int, string>>(
                       json,
                       Settings()))
              .Message.ShouldContain("string \"$type\" property");

    [Fact]
    public void WhenThePayloadPropertyIsMissing_ThenThrow() =>
        Should.Throw<JsonSerializationException>(
                   () => JsonConvert.DeserializeObject<Result<int, string>>(
                       "{\"$type\":\"ok\"}",
                       Settings()))
              .Message.ShouldContain("\"value\" property");

    [Fact]
    public void WhenTheDiscriminatorNamesNoKnownCase_ThenThrow() =>
        Should.Throw<JsonSerializationException>(
                   () => JsonConvert.DeserializeObject<Result<int, string>>(
                       "{\"$type\":\"maybe\",\"value\":42}",
                       Settings()))
              .Message.ShouldContain("\"maybe\" is not a result case");

    [Theory]
    [InlineData("{\"$type\":\"ok\",\"value\":null}")]
    [InlineData("{\"$type\":\"err\",\"value\":null}")]
    public void WhenThePayloadIsNull_ThenThrowRatherThanBuildANullCase(
        string json) =>
        Should.Throw<JsonSerializationException>(
                   () => JsonConvert.DeserializeObject<Result<int, string>>(
                       json,
                       Settings()))
              .Message.ShouldContain("cannot be null");

    [Theory]
    [InlineData("ok")]
    [InlineData("err")]
    public void WhenThePayloadsConverterReadsNull_ThenThrowRatherThanBuildANullCase(
        string discriminator)
    {
        JsonSerializerSettings settings = Settings();
        settings.Converters.Add(new NullReadingBoxConverter());

        Should.Throw<JsonSerializationException>(
                   () => JsonConvert.DeserializeObject<Result<Box, Box>>(
                       $"{{\"$type\":\"{discriminator}\",\"value\":\"crate\"}}",
                       settings))
              .Message.ShouldContain("cannot be null");
    }

    [Fact]
    public void WhenWritingANullReference_ThenWriteNullRatherThanThrow()
    {
        StringWriter text = new();

        new ResultJsonConverter().WriteJson(
            new JsonTextWriter(text),
            null,
            JsonSerializer.CreateDefault());

        text.ToString().ShouldBe("null");
    }

    [Theory]
    [InlineData(typeof(Result<int, string>))]
    [InlineData(typeof(Result<string, Error>))]
    [InlineData(typeof(Ok<int, string>))]
    [InlineData(typeof(Err<int, string>))]
    public void WhenAskedAboutAResult_ThenAgreeToConvertIt(Type type) =>
        new ResultJsonConverter().CanConvert(type).ShouldBeTrue();

    [Theory]
    [InlineData(typeof(string))]
    [InlineData(typeof(int))]
    [InlineData(typeof(Option<int>))]
    [InlineData(typeof(Some<int>))]
    public void WhenAskedAboutAnythingElse_ThenDecline(Type type) =>
        new ResultJsonConverter().CanConvert(type).ShouldBeFalse();

    public sealed class Attempt
    {
        public Result<int, string> Outcome { get; set; } =
            Result.Ok<int, string>(0);
    }

}
