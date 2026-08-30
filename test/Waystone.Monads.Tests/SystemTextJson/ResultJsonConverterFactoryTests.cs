namespace System.Text.Json.Serialization;

using System.Collections.Generic;
using Shouldly;
using Waystone.Monads.Results;
using Waystone.Monads.Results.Errors;
using Xunit;

public class ResultJsonConverterFactoryTests
{
    private readonly ResultJsonConverterFactory _factory = new();

    [Theory]
    [InlineData(typeof(Result<int, string>))]
    [InlineData(typeof(Result<string, Error>))]
    public void WhenAskedAboutAResult_ThenConvertIt(Type type) =>
        _factory.CanConvert(type).ShouldBeTrue();

    [Theory]
    [InlineData(typeof(string))]
    [InlineData(typeof(int))]
    [InlineData(typeof(Dictionary<int, string>))]
    [InlineData(typeof(Ok<int, string>))]
    [InlineData(typeof(Err<int, string>))]
    public void WhenAskedAboutAnythingElse_ThenDeclineIt(Type type) =>
        _factory.CanConvert(type).ShouldBeFalse();

    [Fact]
    public void WhenCreatingAConverter_ThenCloseItOverBothCaseTypes() =>
        _factory
           .CreateConverter(
                typeof(Result<int, string>),
                new JsonSerializerOptions())
           .ShouldBeOfType<ResultJsonConverter<int, string>>();

    [Fact]
    public void WhenCreatingAConverter_ThenKeepTheCaseTypesInOrder() =>
        _factory
           .CreateConverter(
                typeof(Result<string, int>),
                new JsonSerializerOptions())
           .ShouldBeOfType<ResultJsonConverter<string, int>>();
}
