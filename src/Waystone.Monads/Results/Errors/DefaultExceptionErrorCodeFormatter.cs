namespace Waystone.Monads.Results.Errors;

using System;

[Obsolete]
internal sealed class DefaultExceptionErrorCodeFormatter<T>
    : IErrorCodeFormatter<T> where T : Exception
{
    private const string Prefix = "Err.";
    private const string NameOfException = nameof(Exception);

    /// <inheritdoc />
    public string Format(T value)
    {
        string name = typeof(T).Name;

        return name switch
        {
            NameOfException => $"{Prefix}{NameOfException}",
            var _ when name.EndsWith(
                    NameOfException,
                    StringComparison.OrdinalIgnoreCase) =>
                $"{Prefix}{name[..^NameOfException.Length]}",
            var _ => $"{Prefix}{name}",
        };
    }
}
