using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Waystone.Monads.Options;
using Waystone.Monads.Results;

int failures = 0;

Check(
    "Option<string> through the factory alone",
    Expect.Works,
    () => RoundTrip(Option.Some("Ally"), FactoryOnly()));

Check(
    "Option<string> none through the factory alone",
    Expect.Works,
    () => RoundTrip(Option.None<string>(), FactoryOnly()));

Check(
    "Option<Uri> through the factory alone",
    Expect.Works,
    () => RoundTrip(Option.Some(new Uri("https://example.test")), FactoryOnly()));

Check(
    "Result<string, string> ok through the factory alone",
    Expect.Works,
    () => RoundTrip(Result.Ok<string, string>("ok"), FactoryOnly()));

Check(
    "Option<int> through the factory alone",
    Expect.NeedsExplicitRegistration,
    () => RoundTrip(Option.Some(42), FactoryOnly()));

Check(
    "Option<int> none through the factory alone",
    Expect.NeedsExplicitRegistration,
    () => RoundTrip(Option.None<int>(), FactoryOnly()));

Check(
    "Option<double> through the factory alone",
    Expect.NeedsExplicitRegistration,
    () => RoundTrip(Option.Some(1.5), FactoryOnly()));

Check(
    "Result<int, string> ok through the factory alone",
    Expect.NeedsExplicitRegistration,
    () => RoundTrip(Result.Ok<int, string>(42), FactoryOnly()));

Check(
    "Result<Guid, string> ok through the factory alone",
    Expect.NeedsExplicitRegistration,
    () => RoundTrip(Result.Ok<Guid, string>(Guid.NewGuid()), FactoryOnly()));

Check(
    "Option<long> through an explicitly registered converter",
    Expect.Works,
    () => RoundTrip(Option.Some(1L), ExplicitlyRegistered()));

Check(
    "Option<long> none through an explicitly registered converter",
    Expect.Works,
    () => RoundTrip(Option.None<long>(), ExplicitlyRegistered()));

Check(
    "Result<long, string> ok through an explicitly registered converter",
    Expect.Works,
    () => RoundTrip(Result.Ok<long, string>(1L), ExplicitlyRegistered()));

Check(
    "Result<long, string> err through an explicitly registered converter",
    Expect.Works,
    () => RoundTrip(Result.Err<long, string>("boom"), ExplicitlyRegistered()));

Console.WriteLine(
    failures == 0
        ? "NativeAOT behaviour matches what the README documents."
        : $"{failures} NativeAOT check(s) did not behave as documented.");

return failures;

static JsonSerializerOptions FactoryOnly() =>
    new JsonSerializerOptions().AddMonadConverters();

static JsonSerializerOptions ExplicitlyRegistered()
{
    JsonSerializerOptions options = new();
    options.Converters.Add(new OptionJsonConverter<long>());
    options.Converters.Add(new ResultJsonConverter<long, string>());

    return options;
}

static void RoundTrip<T>(T before, JsonSerializerOptions options)
{
    string json = JsonSerializer.Serialize(before, options);
    T after = JsonSerializer.Deserialize<T>(json, options)!;

    if (!Equals(before, after))
    {
        throw new InvalidOperationException(
            $"round trip changed the value: {before} -> {json} -> {after}");
    }
}

void Check(string name, Expect expectation, Action check)
{
    try
    {
        check();

        if (expectation is Expect.NeedsExplicitRegistration)
        {
            Fail(
                name,
                "the factory closed a value-type converter, which the README says it cannot. Re-read the Trimming and NativeAOT section - the workaround may no longer be needed.");

            return;
        }

        Console.WriteLine($"  pass   {name}");
    }
    catch (NotSupportedException exception)
        when (expectation is Expect.NeedsExplicitRegistration)
    {
        Console.WriteLine($"  known  {name}");
        Console.WriteLine($"         {exception.Message}");
    }
    catch (Exception exception)
    {
        Fail(name, $"{exception.GetType().Name}: {exception.Message}");
    }
}

void Fail(string name, string reason)
{
    failures++;
    Console.WriteLine($"  FAIL   {name}");
    Console.WriteLine($"         {reason}");
}

internal enum Expect
{
    Works,
    NeedsExplicitRegistration,
}
