namespace Newtonsoft.Json;

using System;
using System.IO;
using System.Reflection;
using Serialization;
using Shouldly;
using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Xunit;

public class OptionJsonConverterTests
{
    private static JsonSerializerSettings Settings() =>
        new JsonSerializerSettings().AddMonadConverters();

    private static JsonSerializerSettings BoxSettings()
    {
        JsonSerializerSettings settings = Settings();
        settings.Converters.Add(new NullReadingBoxConverter());

        return settings;
    }

    private static JsonSerializerSettings SkipNoneSettings()
    {
        JsonSerializerSettings settings = Settings();
        settings.ContractResolver = new SkipNoneContractResolver();

        return settings;
    }

    [Fact]
    public void WhenWritingSome_ThenWriteTheValueAlone() =>
        JsonConvert.SerializeObject(Option.Some(42), Settings()).ShouldBe("42");

    [Fact]
    public void WhenWritingNone_ThenWriteNull() =>
        JsonConvert.SerializeObject(Option.None<int>(), Settings())
                   .ShouldBe("null");

    [Fact]
    public void WhenReadingNull_ThenBuildNoneRatherThanSkippingTheConverter() =>
        JsonConvert.DeserializeObject<Option<string>>("null", Settings())
                   .ShouldBe(Option.None<string>());

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(42)]
    public void WhenRoundTrippingSome_ThenKeepTheValue(int value)
    {
        JsonSerializerSettings settings = Settings();
        Option<int> before = Option.Some(value);

        JsonConvert.DeserializeObject<Option<int>>(
                        JsonConvert.SerializeObject(before, settings),
                        settings)
                   .ShouldBe(before);
    }

    [Fact]
    public void WhenRoundTrippingAStringOption_ThenKeepTheValue()
    {
        JsonSerializerSettings settings = Settings();
        Option<string> before = Option.Some("Ally");

        JsonConvert.DeserializeObject<Option<string>>(
                        JsonConvert.SerializeObject(before, settings),
                        settings)
                   .ShouldBe(before);
    }

    [Fact]
    public void WhenTheValueHasItsOwnConverter_ThenLetItWriteThePayload() =>
        JsonConvert.SerializeObject(
                        Option.Some(new Box("crate")),
                        BoxSettings())
                   .ShouldBe("\"crate\"");

    [Fact]
    public void WhenTheValuesConverterReadsNull_ThenBuildNoneRatherThanASomeOfNull() =>
        JsonConvert.DeserializeObject<Option<Box>>("\"crate\"", BoxSettings())
                   .ShouldBe(Option.None<Box>());

    [Fact]
    public void WhenTheOptionIsNested_ThenCollapseSomeOfNoneToNull() =>
        JsonConvert.SerializeObject(
                        Option.Some(Option.None<int>()),
                        Settings())
                   .ShouldBe("null");

    [Fact]
    public void WhenTheOptionIsNested_ThenReadSomeOfNoneBackAsNone()
    {
        JsonSerializerSettings settings = Settings();

        JsonConvert.DeserializeObject<Option<Option<int>>>(
                        JsonConvert.SerializeObject(
                            Option.Some(Option.None<int>()),
                            settings),
                        settings)
                   .ShouldBe(Option.None<Option<int>>());
    }

    [Fact]
    public void WhenTheOptionIsNestedAroundAValue_ThenSurviveTheRoundTrip()
    {
        JsonSerializerSettings settings = Settings();
        Option<Option<int>> before = Option.Some(Option.Some(1));

        JsonConvert.DeserializeObject<Option<Option<int>>>(
                        JsonConvert.SerializeObject(before, settings),
                        settings)
                   .ShouldBe(before);
    }

    [Fact]
    public void WhenANonePropertyIsWritten_ThenWriteItAsNull() =>
        JsonConvert.SerializeObject(new Person(), Settings())
                   .ShouldBe("{\"Nickname\":null}");

    [Fact]
    public void WhenIgnoringNullsOnWrite_ThenStillWriteANoneProperty()
    {
        JsonSerializerSettings settings =
            new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
            }.AddMonadConverters();

        JsonConvert.SerializeObject(new Person(), settings)
                   .ShouldBe("{\"Nickname\":null}");
    }

    [Fact]
    public void WhenSkippingNonePropertiesThroughTheResolver_ThenOmitThem() =>
        JsonConvert.SerializeObject(new Person(), SkipNoneSettings())
                   .ShouldBe("{}");

    [Fact]
    public void WhenSkippingNonePropertiesThroughTheResolver_ThenKeepSomeOnes() =>
        JsonConvert.SerializeObject(
                        new Person { Nickname = Option.Some("Ally") },
                        SkipNoneSettings())
                   .ShouldBe("{\"Nickname\":\"Ally\"}");

    [Fact]
    public void WhenThePropertyIsAbsent_ThenLeaveTheModelsOwnDefault() =>
        JsonConvert.DeserializeObject<Person>("{}", Settings())!
                   .Nickname.ShouldBe(Option.None<string>());

    [Fact]
    public void WhenWritingANullReference_ThenWriteNullRatherThanThrow()
    {
        StringWriter text = new();

        new OptionJsonConverter().WriteJson(
            new JsonTextWriter(text),
            null,
            JsonSerializer.CreateDefault());

        text.ToString().ShouldBe("null");
    }

    [Theory]
    [InlineData(typeof(Option<int>))]
    [InlineData(typeof(Option<string>))]
    [InlineData(typeof(Some<int>))]
    [InlineData(typeof(None<int>))]
    public void WhenAskedAboutAnOption_ThenAgreeToConvertIt(Type type) =>
        new OptionJsonConverter().CanConvert(type).ShouldBeTrue();

    [Theory]
    [InlineData(typeof(string))]
    [InlineData(typeof(int))]
    [InlineData(typeof(Result<int, string>))]
    [InlineData(typeof(Ok<int, string>))]
    public void WhenAskedAboutAnythingElse_ThenDecline(Type type) =>
        new OptionJsonConverter().CanConvert(type).ShouldBeFalse();

    public sealed class Person
    {
        public Option<string> Nickname { get; set; } = Option.None<string>();
    }


    private sealed class SkipNoneContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(
            MemberInfo member,
            MemberSerialization memberSerialization)
        {
            JsonProperty property =
                base.CreateProperty(member, memberSerialization);

            property.ShouldSerialize = instance =>
                !IsNone(property.ValueProvider?.GetValue(instance));

            return property;
        }

        private static bool IsNone(object? value) =>
            value?.GetType() is { IsGenericType: true } type
         && type.GetGenericTypeDefinition() == typeof(None<>);
    }
}
