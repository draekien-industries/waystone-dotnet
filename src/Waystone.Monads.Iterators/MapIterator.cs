namespace Waystone.Monads.Iterators;

using Abstractions;
using Options;

/// <summary>
/// Represents an iterator that applies a transformation function to each
/// element of the source iterator and yields the transformed elements.
/// </summary>
/// <typeparam name="TIn">
/// The type of the elements in the source iterator. Must be
/// a non-nullable type.
/// </typeparam>
/// <typeparam name="TOut">
/// The type of the elements produced by the transformation
/// function. Must be a non-nullable type.
/// </typeparam>
public sealed class MapIterator<TIn, TOut> : Iterator<TOut>
    where TIn : notnull where TOut : notnull
{
    private readonly Func<TIn, TOut> _map;
    private readonly IIterator<TIn> _source;

    /// <summary>
    /// Represents an iterator that applies a transformation function to each
    /// element of the source iterator and yields the transformed elements.
    /// </summary>
    /// <typeparam name="TIn">
    /// The type of the elements in the source iterator. Must be
    /// a non-nullable type.
    /// </typeparam>
    /// <typeparam name="TOut">
    /// The type of the elements produced by the transformation
    /// function. Must be a non-nullable type.
    /// </typeparam>
    public MapIterator(IIterator<TIn> source, Func<TIn, TOut> map)
    {
        _source = source;
        _map = map;
    }


    /// <inheritdoc />
    public override Option<TOut> Next() => _source.Next().Map(_map);

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        _source.Dispose();
    }
}
