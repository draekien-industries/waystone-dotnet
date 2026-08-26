namespace Waystone.Monads.Fixtures;

using System;

internal sealed class NoScope : IDisposable
{
    internal static readonly NoScope Instance = new();

    public void Dispose()
    { }
}
