
namespace Waystone.Monads.Iterators;

using Waystone.Monads.Options;
using Waystone.Monads.Iterators.Abstractions;
using Waystone.Monads.Primitives;

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
    public override (PosInt LowerBound, Option<PosInt> UpperBound) SizeHint() =>
        CurrentIndex switch
        {
            -1 => (1, Option.Some<PosInt>(1)),
            _ => base.SizeHint()
        };
}
