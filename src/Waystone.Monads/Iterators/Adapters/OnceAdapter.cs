namespace Waystone.Monads.Iterators.Adapters;

using Abstractions;
using Options;

/// <summary>
/// An <see cref="Iterator{T}" /> that limits the iteration to a single
/// element. Subsequent calls to iterate past the first element will yield none.
/// </summary>
/// <typeparam name="T">
/// The type of elements the iterator processes, which must be
/// non-nullable.
/// </typeparam>
public sealed class OnceAdapter<T> : Iterator<T> where T : notnull
{
    private readonly IIterator<T> _source;
    private bool _hasBeenCalled;

    /// <summary>Creates a new instance of the <see cref="OnceAdapter{T}" /></summary>
    /// <param name="source">The source iterator</param>
    public OnceAdapter(IIterator<T> source)
    {
        _source = source;
        _hasBeenCalled = false;
    }

    /// <inheritdoc />
    public override Option<T> Next()
    {
        if (_hasBeenCalled) return Option.None<T>();
        _hasBeenCalled = true;
        return _source.Next();
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        _source.Dispose();
    }
}
