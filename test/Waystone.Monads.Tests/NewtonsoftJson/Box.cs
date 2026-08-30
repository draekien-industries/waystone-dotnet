namespace Newtonsoft.Json;

using System;

internal sealed class Box
{
    public Box(string name) => Name = name;

    public string Name { get; }
}

internal sealed class NullReadingBoxConverter : JsonConverter<Box>
{
    public override Box? ReadJson(
        JsonReader reader,
        Type objectType,
        Box? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer) =>
        null;

    public override void WriteJson(
        JsonWriter writer,
        Box? value,
        JsonSerializer serializer) =>
        writer.WriteValue(value!.Name);
}
