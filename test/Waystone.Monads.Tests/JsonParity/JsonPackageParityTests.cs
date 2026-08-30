namespace Waystone.Monads.Tests.JsonParity;

using System.Text.Json;
using Newtonsoft.Json;
using Options;
using Results;
using Results.Errors;
using Shouldly;
using Xunit;
using Stj = System.Text.Json;

public class JsonPackageParityTests
{
    private static readonly JsonSerializerOptions StjOptions =
        new JsonSerializerOptions().AddMonadConverters();

    private static readonly JsonSerializerSettings NetSettings =
        new JsonSerializerSettings().AddMonadConverters();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(42)]
    public void WhenTheOptionHoldsANumber_ThenBothPackagesAgree(int value) =>
        ShouldAgree(Option.Some(value));

    [Fact]
    public void WhenTheOptionHoldsAString_ThenBothPackagesAgree() =>
        ShouldAgree(Option.Some("Ally"));

    [Fact]
    public void WhenTheOptionIsNone_ThenBothPackagesAgree() =>
        ShouldAgree(Option.None<int>());

    [Fact]
    public void WhenTheOptionIsNoneOfAString_ThenBothPackagesAgree() =>
        ShouldAgree(Option.None<string>());

    [Fact]
    public void WhenTheOptionIsNested_ThenBothPackagesCollapseItTheSameWay()
    {
        Option<Option<int>> before = Option.Some(Option.None<int>());

        Stj.JsonSerializer.Serialize(before, StjOptions).ShouldBe("null");
        JsonConvert.SerializeObject(before, NetSettings).ShouldBe("null");

        ShouldAgree(Option.None<Option<int>>());
    }

    [Fact]
    public void WhenTheResultIsOk_ThenBothPackagesAgree() =>
        ShouldAgree(Result.Ok<int, string>(42));

    [Fact]
    public void WhenTheResultIsErr_ThenBothPackagesAgree() =>
        ShouldAgree(Result.Err<int, string>("boom"));

    [Fact]
    public void WhenTheResultCarriesTheLibrarysOwnError_ThenBothPackagesAgree() =>
        ShouldAgree(
            Result.Err<int, Error>(new Error("boom.happened", "Boom.")));

    [Fact]
    public void WhenTheResultHoldsAnOption_ThenBothPackagesAgree() =>
        ShouldAgree(Result.Ok<Option<int>, string>(Option.Some(7)));

    [Fact]
    public void WhenTheResultHoldsANone_ThenBothPackagesAgree() =>
        ShouldAgree(Result.Ok<Option<int>, string>(Option.None<int>()));

    [Fact]
    public void WhenTheMonadsAreAModelsProperties_ThenBothPackagesAgree() =>
        ShouldAgree(
            new Registration
            {
                Nickname = Option.Some("Ally"),
                Outcome = Result.Err<int, string>("boom"),
            });

    [Fact]
    public void WhenTheModelsOptionIsNone_ThenBothPackagesWriteTheProperty() =>
        ShouldAgree(new Registration());

    private static void ShouldAgree<T>(T value)
        where T : notnull
    {
        string stj = Stj.JsonSerializer.Serialize(value, StjOptions);
        string net = JsonConvert.SerializeObject(value, NetSettings);

        net.ShouldBe(stj);

        JsonConvert.DeserializeObject<T>(stj, NetSettings).ShouldBe(value);
        Stj.JsonSerializer.Deserialize<T>(net, StjOptions).ShouldBe(value);
    }

    public sealed class Registration
    {
        public Option<string> Nickname { get; set; } = Option.None<string>();

        public Result<int, string> Outcome { get; set; } =
            Result.Ok<int, string>(0);

        public override bool Equals(object? obj) =>
            obj is Registration other
         && Nickname.Equals(other.Nickname)
         && Outcome.Equals(other.Outcome);

        public override int GetHashCode() =>
            Nickname.GetHashCode() ^ Outcome.GetHashCode();
    }
}
