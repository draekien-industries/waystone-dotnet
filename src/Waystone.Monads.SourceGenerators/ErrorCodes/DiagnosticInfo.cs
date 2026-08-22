namespace Waystone.Monads.SourceGenerators.ErrorCodes;

using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

internal sealed record DiagnosticInfo(
    DiagnosticDescriptor Descriptor,
    string? FilePath,
    TextSpan Span,
    LinePositionSpan LineSpan,
    EquatableArray<string> MessageArgs)
{
    public static DiagnosticInfo Create(
        DiagnosticDescriptor descriptor,
        Location location,
        params string[] messageArgs)
    {
        FileLinePositionSpan mapped = location.GetLineSpan();

        return new DiagnosticInfo(
            descriptor,
            location.SourceTree?.FilePath,
            location.SourceSpan,
            mapped.Span,
            new EquatableArray<string>(messageArgs));
    }

    public Diagnostic ToDiagnostic()
    {
        Location location = FilePath is null
            ? Location.None
            : Location.Create(FilePath, Span, LineSpan);

        return Diagnostic.Create(
            Descriptor,
            location,
            MessageArgs.Values.Cast<object?>().ToArray());
    }
}

internal readonly struct EquatableArray<T>(T[] values)
    : IEquatable<EquatableArray<T>>
    where T : IEquatable<T>
{
    public T[] Values { get; } = values;

    public bool Equals(EquatableArray<T> other)
    {
        if (Values.Length != other.Values.Length) return false;

        for (var i = 0; i < Values.Length; i++)
        {
            if (!Values[i].Equals(other.Values[i])) return false;
        }

        return true;
    }

    public override bool Equals(object? obj) =>
        obj is EquatableArray<T> other && Equals(other);

    public override int GetHashCode()
    {
        int hash = Values.Length;

        foreach (T value in Values) hash = (hash * 397) ^ value.GetHashCode();

        return hash;
    }
}
