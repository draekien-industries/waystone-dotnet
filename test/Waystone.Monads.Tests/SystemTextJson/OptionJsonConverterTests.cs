namespace System.Text.Json.Serialization;

using Metadata;
using Shouldly;
using Waystone.Monads.Options;
using Xunit;

public class OptionJsonConverterTests
{
    private static JsonSerializerOptions Options() =>
        new JsonSerializerOptions().AddMonadConverters();

    private static JsonSerializerOptions BoxOptions()
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new NullReadingBoxConverter());

        return options.AddMonadConverters();
    }

    private static JsonSerializerOptions SkipNoneOptions() =>
        new JsonSerializerOptions
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { SkipNoneProperties },
            },
        }.AddMonadConverters();

    [Fact]
    public void WhenWritingSome_ThenEmitTheValueWithNoWrapper()
    {
        string json = JsonSerializer.Serialize(
            Option.Some("Ally"),
            Options());

        json.ShouldBe("\"Ally\"");
    }

    [Fact]
    public void WhenWritingNone_ThenEmitNull()
    {
        string json = JsonSerializer.Serialize(
            Option.None<string>(),
            Options());

        json.ShouldBe("null");
    }

    [Fact]
    public void WhenReadingNull_ThenReturnNone()
    {
        Option<string> option =
            JsonSerializer.Deserialize<Option<string>>("null", Options())!;

        option.ShouldBe(Option.None<string>());
    }

    [Fact]
    public void WhenRoundTrippingSomeOfAReferenceType_ThenKeepTheValue()
    {
        JsonSerializerOptions options = Options();
        Option<string> before = Option.Some("Ally");

        Option<string> after = JsonSerializer.Deserialize<Option<string>>(
            JsonSerializer.Serialize(before, options),
            options)!;

        after.ShouldBe(before);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(42)]
    public void WhenRoundTrippingSomeOfAValueType_ThenKeepTheValue(int value)
    {
        JsonSerializerOptions options = Options();
        Option<int> before = Option.Some(value);

        Option<int> after = JsonSerializer.Deserialize<Option<int>>(
            JsonSerializer.Serialize(before, options),
            options)!;

        after.ShouldBe(before);
    }

    [Fact]
    public void WhenRoundTrippingNoneOfAValueType_ThenKeepTheNone()
    {
        JsonSerializerOptions options = Options();

        Option<int> after = JsonSerializer.Deserialize<Option<int>>(
            JsonSerializer.Serialize(Option.None<int>(), options),
            options)!;

        after.ShouldBe(Option.None<int>());
    }

    [Fact]
    public void WhenTheValueConverterReturnsNull_ThenReturnNoneRatherThanThrow()
    {
        Option<Box> option = JsonSerializer.Deserialize<Option<Box>>(
            "\"anything\"",
            BoxOptions())!;

        option.ShouldBe(Option.None<Box>());
    }

    [Fact]
    public void WhenTheValueHasItsOwnConverter_ThenWriteThroughIt()
    {
        string json = JsonSerializer.Serialize(
            Option.Some(new Box("Ally")),
            BoxOptions());

        json.ShouldBe("\"Ally\"");
    }

    [Fact]
    public void WhenWritingSomeOfNone_ThenCollapseToTheSameNullAsNone()
    {
        JsonSerializerOptions options = Options();

        JsonSerializer.Serialize(Option.Some(Option.None<int>()), options)
                      .ShouldBe(
                           JsonSerializer.Serialize(
                               Option.None<Option<int>>(),
                               options));
    }

    [Fact]
    public void WhenReadingBackSomeOfNone_ThenLoseTheOuterSome()
    {
        JsonSerializerOptions options = Options();

        Option<Option<int>> after =
            JsonSerializer.Deserialize<Option<Option<int>>>(
                JsonSerializer.Serialize(
                    Option.Some(Option.None<int>()),
                    options),
                options)!;

        after.ShouldBe(Option.None<Option<int>>());
    }

    [Fact]
    public void WhenNestingSomeInsideSome_ThenRoundTripUnharmed()
    {
        JsonSerializerOptions options = Options();
        Option<Option<int>> before = Option.Some(Option.Some(1));

        Option<Option<int>> after =
            JsonSerializer.Deserialize<Option<Option<int>>>(
                JsonSerializer.Serialize(before, options),
                options)!;

        after.ShouldBe(before);
    }

    [Fact]
    public void WhenAPropertyIsNone_ThenWriteItAsNullRatherThanOmitIt()
    {
        string json = JsonSerializer.Serialize(
            new Person { Nickname = Option.None<string>() },
            Options());

        json.ShouldBe("{\"Nickname\":null}");
    }

    [Fact]
    public void WhenIgnoringNullsOnWrite_ThenStillWriteANoneProperty()
    {
        JsonSerializerOptions options = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
        options.AddMonadConverters();

        string json = JsonSerializer.Serialize(
            new Person { Nickname = Option.None<string>() },
            options);

        json.ShouldBe("{\"Nickname\":null}");
    }

    [Fact]
    public void WhenSkippingNonePropertiesThroughTheResolver_ThenOmitThem()
    {
        string json = JsonSerializer.Serialize(
            new Person { Nickname = Option.None<string>() },
            SkipNoneOptions());

        json.ShouldBe("{}");
    }

    [Fact]
    public void WhenSkippingNonePropertiesThroughTheResolver_ThenKeepSomeOnes()
    {
        string json = JsonSerializer.Serialize(
            new Person { Nickname = Option.Some("Ally") },
            SkipNoneOptions());

        json.ShouldBe("{\"Nickname\":\"Ally\"}");
    }

    private static void SkipNoneProperties(JsonTypeInfo typeInfo)
    {
        foreach (JsonPropertyInfo property in typeInfo.Properties)
        {
            property.ShouldSerialize = static (_, value) =>
                value is null || !IsNone(value);
        }
    }

    private static bool IsNone(object value) =>
        value.GetType() is { IsGenericType: true } type
     && type.GetGenericTypeDefinition() == typeof(None<>);

    [Fact]
    public void WhenAPropertyIsAbsent_ThenKeepTheModelsOwnDefault()
    {
        Person person = JsonSerializer.Deserialize<Person>("{}", Options())!;

        person.Nickname.ShouldBe(Option.None<string>());
    }

    [Fact]
    public void WhenAskedWhetherToHandleNull_ThenSayYesSoNoneCanBeBuilt() =>
        new OptionJsonConverter<string>().HandleNull.ShouldBeTrue();

    public sealed class Box
    {
        public Box(string name) => Name = name;

        public string Name { get; }
    }

    public sealed class Person
    {
        public Option<string> Nickname { get; set; } = Option.None<string>();
    }

    private sealed class NullReadingBoxConverter : JsonConverter<Box>
    {
        public override Box Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            reader.Skip();

            return null!;
        }

        public override void Write(
            Utf8JsonWriter writer,
            Box value,
            JsonSerializerOptions options) =>
            writer.WriteStringValue(value.Name);
    }
}
