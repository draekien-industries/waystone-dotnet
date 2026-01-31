namespace Waystone.WideLogEvents;

using System;
using System.Threading;
using Monads.Options;

public abstract class AsyncLocalScoped<T> where T : class
{
    private static readonly AsyncLocal<Option<T>> Scoped = new()
    {
        Value = Option.None<T>(),
    };

    internal static Option<T> ScopedValue => Scoped.Value;

    internal static Scope BeginScope(T value)
    {
        if (value is null) throw new ArgumentNullException(nameof(value));

        return new Scope(value);
    }

    internal readonly struct Scope : IDisposable
    {
        private readonly Option<T> _previous;

        public Scope(T value)
        {
            _previous = ScopedValue;

            Scoped.Value = Option.FromNullable(value);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Scoped.Value = _previous;
        }
    }
}
