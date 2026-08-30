namespace Waystone.Monads.Tests;

using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shouldly;
using Waystone.Monads.Options;
using Waystone.Monads.Results;
using Xunit;

/// <summary>
/// Verify that System.Text.Json and Newtonsoft.Json converters produce and consume identical wire formats.
/// This ensures cross-package round-trip serialization is safe: serialize with one package, deserialize with the other.
/// </summary>
public class CrossPackageWireFormatTests
{
    private static System.Text.Json.JsonSerializerOptions StjOptions() =>
        new System.Text.Json.JsonSerializerOptions().AddMonadConverters();

    private static JsonSerializerSettings JsonNetSettings() =>
        new JsonSerializerSettings().AddMonadConverters();

    public class OptionWireFormat
    {
        [Fact]
        public void WhenSerializingOptionWithStj_AndDeserializingWithJsonNet_ThenValueRoundTrips()
        {
            System.Text.Json.JsonSerializerOptions stj = StjOptions();
            JsonSerializerSettings jsonNet = JsonNetSettings();

            Option<int> original = Option.Some(42);
            string wire = System.Text.Json.JsonSerializer.Serialize(original, stj);

            Option<int> deserialized =
                JsonConvert.DeserializeObject<Option<int>>(wire, jsonNet)!;

            deserialized.ShouldBe(original);
        }

        [Fact]
        public void WhenSerializingOptionWithJsonNet_AndDeserializingWithStj_ThenValueRoundTrips()
        {
            System.Text.Json.JsonSerializerOptions stj = StjOptions();
            JsonSerializerSettings jsonNet = JsonNetSettings();

            Option<int> original = Option.Some(42);
            string wire = JsonConvert.SerializeObject(original, jsonNet);

            Option<int> deserialized =
                System.Text.Json.JsonSerializer.Deserialize<Option<int>>(wire, stj)!;

            deserialized.ShouldBe(original);
        }

        [Fact]
        public void WhenSerializingNoneWithStj_AndDeserializingWithJsonNet_ThenNoneRoundTrips()
        {
            System.Text.Json.JsonSerializerOptions stj = StjOptions();
            JsonSerializerSettings jsonNet = JsonNetSettings();

            Option<int> original = Option.None<int>();
            string wire = System.Text.Json.JsonSerializer.Serialize(original, stj);

            Option<int> deserialized =
                JsonConvert.DeserializeObject<Option<int>>(wire, jsonNet)!;

            deserialized.ShouldBe(original);
        }

        [Fact]
        public void WhenSerializingNoneWithJsonNet_AndDeserializingWithStj_ThenNoneRoundTrips()
        {
            System.Text.Json.JsonSerializerOptions stj = StjOptions();
            JsonSerializerSettings jsonNet = JsonNetSettings();

            Option<int> original = Option.None<int>();
            string wire = JsonConvert.SerializeObject(original, jsonNet);

            Option<int> deserialized =
                System.Text.Json.JsonSerializer.Deserialize<Option<int>>(wire, stj)!;

            deserialized.ShouldBe(original);
        }

        [Fact]
        public void WhenSerializingStringOption_BothPackagesProduceIdenticalWire()
        {
            System.Text.Json.JsonSerializerOptions stj = StjOptions();
            JsonSerializerSettings jsonNet = JsonNetSettings();

            Option<string> value = Option.Some("hello");

            string stjWire = System.Text.Json.JsonSerializer.Serialize(value, stj);
            string jsonNetWire = JsonConvert.SerializeObject(value, jsonNet);

            stjWire.ShouldBe(jsonNetWire);
        }
    }

    public class ResultWireFormat
    {
        [Fact]
        public void WhenSerializingResultOkWithStj_AndDeserializingWithJsonNet_ThenValueRoundTrips()
        {
            System.Text.Json.JsonSerializerOptions stj = StjOptions();
            JsonSerializerSettings jsonNet = JsonNetSettings();

            Result<int, string> original = Result.Ok<int, string>(42);
            string wire = System.Text.Json.JsonSerializer.Serialize(original, stj);

            Result<int, string> deserialized =
                JsonConvert.DeserializeObject<Result<int, string>>(wire, jsonNet)!;

            deserialized.ShouldBe(original);
        }

        [Fact]
        public void WhenSerializingResultOkWithJsonNet_AndDeserializingWithStj_ThenValueRoundTrips()
        {
            System.Text.Json.JsonSerializerOptions stj = StjOptions();
            JsonSerializerSettings jsonNet = JsonNetSettings();

            Result<int, string> original = Result.Ok<int, string>(42);
            string wire = JsonConvert.SerializeObject(original, jsonNet);

            Result<int, string> deserialized =
                System.Text.Json.JsonSerializer.Deserialize<Result<int, string>>(wire, stj)!;

            deserialized.ShouldBe(original);
        }

        [Fact]
        public void WhenSerializingResultErrWithStj_AndDeserializingWithJsonNet_ThenErrorRoundTrips()
        {
            System.Text.Json.JsonSerializerOptions stj = StjOptions();
            JsonSerializerSettings jsonNet = JsonNetSettings();

            Result<int, string> original = Result.Err<int, string>("boom");
            string wire = System.Text.Json.JsonSerializer.Serialize(original, stj);

            Result<int, string> deserialized =
                JsonConvert.DeserializeObject<Result<int, string>>(wire, jsonNet)!;

            deserialized.ShouldBe(original);
        }

        [Fact]
        public void WhenSerializingResultErrWithJsonNet_AndDeserializingWithStj_ThenErrorRoundTrips()
        {
            System.Text.Json.JsonSerializerOptions stj = StjOptions();
            JsonSerializerSettings jsonNet = JsonNetSettings();

            Result<int, string> original = Result.Err<int, string>("boom");
            string wire = JsonConvert.SerializeObject(original, jsonNet);

            Result<int, string> deserialized =
                System.Text.Json.JsonSerializer.Deserialize<Result<int, string>>(wire, stj)!;

            deserialized.ShouldBe(original);
        }

        [Fact]
        public void WhenSerializingResult_BothPackagesProduceIdenticalWire()
        {
            System.Text.Json.JsonSerializerOptions stj = StjOptions();
            JsonSerializerSettings jsonNet = JsonNetSettings();

            Result<int, string> okValue = Result.Ok<int, string>(42);
            Result<int, string> errValue = Result.Err<int, string>("error");

            string stjOkWire = System.Text.Json.JsonSerializer.Serialize(okValue, stj);
            string jsonNetOkWire = JsonConvert.SerializeObject(okValue, jsonNet);
            stjOkWire.ShouldBe(jsonNetOkWire);

            string stjErrWire = System.Text.Json.JsonSerializer.Serialize(errValue, stj);
            string jsonNetErrWire = JsonConvert.SerializeObject(errValue, jsonNet);
            stjErrWire.ShouldBe(jsonNetErrWire);
        }

        [Fact]
        public void WhenSerializingComplexResult_BothPackagesProduceIdenticalWire()
        {
            System.Text.Json.JsonSerializerOptions stj = StjOptions();
            JsonSerializerSettings jsonNet = JsonNetSettings();

            Result<Person, Error> value = Result.Ok<Person, Error>(
                new Person { Name = "Alice", Age = 30 });

            string stjWire = System.Text.Json.JsonSerializer.Serialize(value, stj);
            string jsonNetWire = JsonConvert.SerializeObject(value, jsonNet);

            // Both should have the same structure: {"$type":"ok","value":{...}}
            JObject stjParsed = JObject.Parse(stjWire);
            JObject jsonNetParsed = JObject.Parse(jsonNetWire);

            // Verify structure is identical
            stjParsed["$type"]!.Value<string>().ShouldBe("ok");
            jsonNetParsed["$type"]!.Value<string>().ShouldBe("ok");

            // Verify payload nesting is identical
            JObject stjPayload = (JObject)stjParsed["value"]!;
            JObject jsonNetPayload = (JObject)jsonNetParsed["value"]!;

            stjPayload["Name"]!.Value<string>().ShouldBe("Alice");
            jsonNetPayload["Name"]!.Value<string>().ShouldBe("Alice");
            stjPayload["Age"]!.Value<int>().ShouldBe(30);
            jsonNetPayload["Age"]!.Value<int>().ShouldBe(30);
        }
    }

    public class NestedMonads
    {
        [Fact]
        public void WhenSerializingSomeNone_BothPackagesProduceIdenticalWire()
        {
            System.Text.Json.JsonSerializerOptions stj = StjOptions();
            JsonSerializerSettings jsonNet = JsonNetSettings();

            Option<Option<int>> value = Option.Some(Option.Some(42));

            string stjWire = System.Text.Json.JsonSerializer.Serialize(value, stj);
            string jsonNetWire = JsonConvert.SerializeObject(value, jsonNet);

            stjWire.ShouldBe(jsonNetWire);
        }

        [Fact]
        public void WhenSerializingSomeOfNone_BothPackagesProduceIdenticalWire()
        {
            System.Text.Json.JsonSerializerOptions stj = StjOptions();
            JsonSerializerSettings jsonNet = JsonNetSettings();

            Option<Option<int>> value = Option.Some(Option.None<int>());

            string stjWire = System.Text.Json.JsonSerializer.Serialize(value, stj);
            string jsonNetWire = JsonConvert.SerializeObject(value, jsonNet);

            stjWire.ShouldBe(jsonNetWire);
        }

        [Fact]
        public void WhenSerializingResultInOption_BothPackagesProduceIdenticalWire()
        {
            System.Text.Json.JsonSerializerOptions stj = StjOptions();
            JsonSerializerSettings jsonNet = JsonNetSettings();

            Option<Result<int, string>> value = Option.Some(Result.Ok<int, string>(42));

            string stjWire = System.Text.Json.JsonSerializer.Serialize(value, stj);
            string jsonNetWire = JsonConvert.SerializeObject(value, jsonNet);

            stjWire.ShouldBe(jsonNetWire);
        }
    }

    private class Person
    {
        public string Name { get; set; } = string.Empty;

        public int Age { get; set; }
    }

    private class Error
    {
        public string Message { get; set; } = string.Empty;

        public override bool Equals(object? obj) =>
            obj is Error other && Message == other.Message;

        public override int GetHashCode() => Message.GetHashCode();
    }
}
