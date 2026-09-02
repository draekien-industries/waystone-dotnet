namespace Waystone.Monads.Schemas.Internal.Reporting;

using System.Text;
using Waystone.Monads.Results.Errors;

internal static class MessageTemplate
{
    internal const string Redacted = "***";

    private const string AbsentReceived = "null";

    internal static string Render(
        string template,
        ViolationPath path,
        ErrorCode code,
        object? received,
        object? expected,
        bool isSensitive)
    {
        if (template.IndexOf('{') < 0) return template;

        var builder = new StringBuilder(template.Length);
        var index = 0;

        while (index < template.Length)
        {
            if (template[index] != '{')
            {
                builder.Append(template[index]);
                index++;
                continue;
            }

            int close = template.IndexOf('}', index + 1);

            if (close < 0)
            {
                builder.Append(template, index, template.Length - index);
                break;
            }

            string? replacement = Resolve(
                template.Substring(index + 1, close - index - 1),
                path,
                code,
                received,
                expected,
                isSensitive);

            if (replacement is null)
            {
                builder.Append(template, index, close - index + 1);
            }
            else
            {
                builder.Append(replacement);
            }

            index = close + 1;
        }

        return builder.ToString();
    }

    private static string? Resolve(
        string token,
        ViolationPath path,
        ErrorCode code,
        object? received,
        object? expected,
        bool isSensitive) =>
        token switch
        {
            "Path" => path.ToString(),
            "Received" => isSensitive
                ? Redacted
                : received?.ToString() ?? AbsentReceived,
            "Expected" => expected?.ToString(),
            "Code" => code.Value,
            _ => null,
        };
}
