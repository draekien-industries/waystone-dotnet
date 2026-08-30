using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Waystone.Monads.Options;

namespace Waystone.Monads.Docs.SystemTextJson.Sample;

/// <summary>packages/system-text-json.md</summary>
internal static class SystemTextJsonPage
{
    internal sealed class Person
    {
        public Option<string> Nickname { get; set; } = Option.None<string>();
    }

    internal static string Install(Person model)
    {
        JsonSerializerOptions options = new();
        options.AddMonadConverters();

        return JsonSerializer.Serialize(model, options);
    }

    internal static void SkipNoneOnTheWire(JsonSerializerOptions options)
    {
        options.TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers = { SkipNoneProperties },
        };

        static void SkipNoneProperties(JsonTypeInfo typeInfo)
        {
            foreach (JsonPropertyInfo property in typeInfo.Properties)
            {
                property.ShouldSerialize = static (_, value) =>
                    value is null
                 || value.GetType() is not { IsGenericType: true } type
                 || type.GetGenericTypeDefinition() != typeof(None<>);
            }
        }
    }

    internal static Option<Option<int>> ANestedNoneCollapses(
        JsonSerializerOptions options)
    {
        Option<Option<int>> before = Option.Some(Option.None<int>());
        string json = JsonSerializer.Serialize(before, options); // "null"
        Option<Option<int>> after =
            JsonSerializer.Deserialize<Option<Option<int>>>(json, options)!;

        // after is None<Option<int>>(), not Some(None<int>())
        return after;
    }

    internal static void RegisterOneConverterAtATime(
        JsonSerializerOptions options)
    {
        options.Converters.Add(new OptionJsonConverter<int>());
        options.Converters.Add(new ResultJsonConverter<int, string>());
    }
}
