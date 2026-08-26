namespace Waystone.Monads.Observability.Sample;

using System.Globalization;
using Options;
using Results;
using Results.Errors;

internal static class PriceFeed
{
    private static readonly Dictionary<string, string> Quotes = new()
    {
        ["WAY"] = "42.50",
        ["MON"] = "not a number",
    };

    internal static Option<decimal> Read(string symbol) =>
        Option.Try(() => Parse(symbol));

    internal static Result<decimal, Error> Fetch(string symbol) =>
        Result.Try(() => Parse(symbol));

    private static decimal Parse(string symbol) =>
        Quotes.TryGetValue(symbol, out string? quote)
            ? decimal.Parse(quote, CultureInfo.InvariantCulture)
            : throw new KeyNotFoundException($"no quote for {symbol}");
}
