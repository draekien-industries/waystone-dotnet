namespace Waystone.Monads.Primitives;

using System;

/// <summary>
/// A positive integer value
/// </summary>
public readonly struct PosInt
{
    private readonly int _value;

    /// <summary>
    /// Creates a new instance of <see cref="PosInt"/>
    /// </summary>
    /// <param name="value">The integer value</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The value is less than zero.
    /// </exception>
    public PosInt(int value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Value must be greater than zero.");

        _value = value;
    }

    /// <summary>
    /// Implicitly converts a <see cref="PosInt"/> to an <see cref="int"/>.
    /// </summary>
    /// <param name="posInt">The positive integer to convert</param>
    public static implicit operator int(PosInt posInt) => posInt._value;

    /// <summary>
    /// Implicitly converts an <see cref="int"/> to a <see cref="PosInt"/>.
    /// </summary>
    /// <param name="value">The integer to convert</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The value is less than zero.
    /// </exception>
    public static implicit operator PosInt(int value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => _value.ToString();
}