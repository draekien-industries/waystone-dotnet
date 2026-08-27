namespace Shouldly;

using System.Text;
using Waystone.Monads.Options;
using Waystone.Monads.Results;

internal static class MonadMessage
{
    internal static string Build(
        string? actualExpression,
        string expected,
        string actual,
        string? customMessage)
    {
        StringBuilder builder = new StringBuilder()
                               .Append(actualExpression ?? "actual")
                               .Append("\n    should be ")
                               .Append(expected)
                               .Append("\n    but was\n")
                               .Append(actual);

        if (!string.IsNullOrWhiteSpace(customMessage))
        {
            builder.Append("\n\nAdditional Info:\n    ").Append(customMessage);
        }

        return builder.ToString();
    }

    internal static string Describe<T>(Option<T> option)
        where T : notnull =>
        option.IsNone ? "None" : $"Some({Value(option.Unwrap())})";

    internal static string Describe<TOk, TErr>(Result<TOk, TErr> result)
        where TOk : notnull
        where TErr : notnull =>
        result.IsOk
            ? $"Ok({Value(result.Unwrap())})"
            : $"Err({Value(result.UnwrapErr())})";

    internal static string Some(object value) => $"Some({Value(value)})";

    internal static string Ok(object value) => $"Ok({Value(value)})";

    internal static string Err(object value) => $"Err({Value(value)})";

    private static string Value(object value) =>
        value is string text ? $"\"{text}\"" : $"{value}";
}
