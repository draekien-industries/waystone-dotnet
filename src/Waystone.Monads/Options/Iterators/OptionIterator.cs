namespace Waystone.Monads.Options.Iterators;

using Waystone.Monads.Iterators;

/// <summary>
/// An <see cref="Iterator{T}"/> for an <see cref="Option{T}"/>
/// </summary>
/// <typeparam name="T">The type of the value contained within the <see cref="Option{T}"/></typeparam>
public sealed class OptionIterator<T> : Iterator<T>
    where T : notnull
{
    private readonly Option<T> _option;

    internal OptionIterator(Option<T> option)
    {
        _option = option;
    }

    /// <inheritdoc/>
    public override bool MoveNext()
    {
        if (++CurrentIndex < 1)
        {
            CurrentItem = _option;
            return true;
        }

        CurrentItem = Option.None<T>();
        return false;
    }

    /// <inheritdoc/>
    public override (int LowerBound, Option<int> UpperBound) SizeHint() =>
        CurrentIndex switch
        {
            -1 => (1, Option.Some(1)),
            _ => base.SizeHint()
        };
}
