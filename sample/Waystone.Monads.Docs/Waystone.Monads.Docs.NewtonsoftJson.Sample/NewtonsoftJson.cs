using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Waystone.Monads.Options;

namespace Waystone.Monads.Docs.NewtonsoftJson.Sample;

/// <summary>packages/newtonsoft-json.md</summary>
internal static class NewtonsoftJsonPage
{
    internal sealed class Character
    {
        public Option<string> Nickname { get; set; } = Option.None<string>();
    }

    internal static string Install(Character model)
    {
        JsonSerializerSettings settings =
            new JsonSerializerSettings().AddMonadConverters();

        return JsonConvert.SerializeObject(model, settings);
    }

    internal static void SkipNoneOnTheWire(JsonSerializerSettings settings)
    {
        settings.ContractResolver = new SkipNoneContractResolver();
    }

    internal static Option<Option<int>> ANestedNoneCollapses(
        JsonSerializerSettings settings)
    {
        Option<Option<int>> before = Option.Some(Option.None<int>());
        string json = JsonConvert.SerializeObject(before, settings); // "null"
        Option<Option<int>> after =
            JsonConvert.DeserializeObject<Option<Option<int>>>(json, settings)!;

        // after is None<Option<int>>(), not Some(None<int>())
        return after;
    }
}

internal sealed class SkipNoneContractResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(
        MemberInfo member,
        MemberSerialization memberSerialization)
    {
        JsonProperty property = base.CreateProperty(member, memberSerialization);

        property.ShouldSerialize = instance =>
            property.ValueProvider?.GetValue(instance)?.GetType() is not
                { IsGenericType: true } type
         || type.GetGenericTypeDefinition() != typeof(None<>);

        return property;
    }
}
