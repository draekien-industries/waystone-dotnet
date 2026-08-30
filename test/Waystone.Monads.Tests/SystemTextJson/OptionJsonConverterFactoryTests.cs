namespace System.Text.Json.Serialization;

using System.Collections.Generic;
using Shouldly;
using Waystone.Monads.Options;
using Xunit;

public class OptionJsonConverterFactoryTests
{
    private readonly OptionJsonConverterFactory _factory = new();

    [Theory]
    [InlineData(typeof(Option<int>))]
    [InlineData(typeof(Option<string>))]
    [InlineData(typeof(Option<Option<int>>))]
    public void WhenAskedAboutAnOption_ThenConvertIt(Type type) =>
        _factory.CanConvert(type).ShouldBeTrue();

    [Theory]
    [InlineData(typeof(string))]
    [InlineData(typeof(int))]
    [InlineData(typeof(List<int>))]
    [InlineData(typeof(Some<int>))]
    [InlineData(typeof(None<int>))]
    public void WhenAskedAboutAnythingElse_ThenDeclineIt(Type type) =>
        _factory.CanConvert(type).ShouldBeFalse();

    [Fact]
    public void WhenCreatingAConverter_ThenCloseItOverTheOptionsValueType() =>
        _factory.CreateConverter(typeof(Option<int>), new JsonSerializerOptions())
                .ShouldBeOfType<OptionJsonConverter<int>>();

    [Fact]
    public void WhenCreatingAConverterForAReferenceType_ThenCloseItToo() =>
        _factory
           .CreateConverter(typeof(Option<string>), new JsonSerializerOptions())
           .ShouldBeOfType<OptionJsonConverter<string>>();
}
