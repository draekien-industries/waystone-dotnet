namespace System.Text.Json;

using System.Linq;
using Serialization;
using Shouldly;
using Xunit;

public class JsonSerializerOptionsExtensionsTests
{
    [Fact]
    public void WhenAddingTheConverters_ThenReturnTheSameOptionsToChainFrom()
    {
        JsonSerializerOptions options = new();

        options.AddMonadConverters().ShouldBeSameAs(options);
    }

    [Fact]
    public void WhenAddingTheConverters_ThenRegisterTheOptionFactory() =>
        new JsonSerializerOptions().AddMonadConverters()
                                   .Converters.OfType<
                                        OptionJsonConverterFactory>()
                                   .ShouldHaveSingleItem();

    [Fact]
    public void WhenAddingTheConvertersToNothing_ThenThrow() =>
        Should.Throw<ArgumentNullException>(
                   () => JsonSerializerOptionsExtensions.AddMonadConverters(
                       null!))
              .ParamName.ShouldBe("options");
}
