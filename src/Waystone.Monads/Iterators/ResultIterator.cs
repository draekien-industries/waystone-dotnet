
namespace Waystone.Monads.Iterators;

using Abstractions;
using Options;
using Primitives;
using Results;

/// <summary>
/// An <see cref="Iterator{T}"/> for a <see cref="Result{TOk,TErr}"/>
/// </summary>
/// <typeparam name="TOk">The ok result value's type</typeparam>
/// <typeparam name="TErr">The err result value's type</typeparam>
public sealed class ResultIterator<TOk, TErr> : Iterator<TOk>
    where TOk : notnull
    where TErr : notnull
{
    private readonly Result<TOk, TErr> _result;

    internal ResultIterator(Result<TOk, TErr> result)
    {
        _result = result;
    }

    /// <inheritdoc/>
    public override bool MoveNext()
    {
        if (++CurrentIndex < 1)
        {
            CurrentItem = _result.GetOk();
            return true;
        }

        CurrentItem = Option.None<TOk>();
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
