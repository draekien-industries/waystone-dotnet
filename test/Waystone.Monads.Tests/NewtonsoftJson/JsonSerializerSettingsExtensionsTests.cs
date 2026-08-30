namespace Newtonsoft.Json;

using System;
using System.Linq;
using Shouldly;
using Xunit;

public class JsonSerializerSettingsExtensionsTests
{
    [Fact]
    public void WhenAddingTheConverters_ThenReturnTheSameSettingsToChainFrom()
    {
        JsonSerializerSettings settings = new();

        settings.AddMonadConverters().ShouldBeSameAs(settings);
    }

    [Fact]
    public void WhenAddingTheConverters_ThenRegisterTheOptionConverter() =>
        new JsonSerializerSettings().AddMonadConverters()
                                    .Converters
                                    .OfType<OptionJsonConverter>()
                                    .ShouldHaveSingleItem();

    [Fact]
    public void WhenAddingTheConverters_ThenRegisterTheResultConverter() =>
        new JsonSerializerSettings().AddMonadConverters()
                                    .Converters
                                    .OfType<ResultJsonConverter>()
                                    .ShouldHaveSingleItem();

    [Fact]
    public void WhenAddingTheConvertersToNothing_ThenThrow() =>
        Should.Throw<ArgumentNullException>(
                   () => JsonSerializerSettingsExtensions.AddMonadConverters(
                       null!))
              .ParamName.ShouldBe("settings");
}
