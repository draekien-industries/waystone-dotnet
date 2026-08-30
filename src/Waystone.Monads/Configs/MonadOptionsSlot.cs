namespace Waystone.Monads.Configs;

using System.Threading;

internal static class MonadOptionsSlot
{
    private static int _next = -1;

    internal static int Count => Volatile.Read(ref _next) + 1;

    internal static int Allocate() => Interlocked.Increment(ref _next);

    internal static T? At<T>(object?[] satellites, int slot)
        where T : class =>
        (uint)slot < (uint)satellites.Length ? (T?)satellites[slot] : null;
}
