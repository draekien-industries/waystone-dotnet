namespace Waystone.Monads.Schemas;

using System;
using System.Collections.Generic;
using System.Globalization;

internal static class DeclaredMembers<T> where T : struct, Enum
{
    private static readonly bool CountsAsUnsignedLong =
        Enum.GetUnderlyingType(typeof(T)) == typeof(ulong);

    private static readonly HashSet<T> Declared = Read();

    private static readonly ulong DeclaredBits = ReadBits();

    private static readonly bool IsFlags =
        typeof(T).IsDefined(typeof(FlagsAttribute), false);

    internal static Schema<T, T> Instance { get; } =
        Schema.For<T>()
              .Check(
                   static value => IsFlags
                       ? IsMadeOfDeclaredBits(value)
                       : Declared.Contains(value),
                   ViolationCode.Mismatched,
                   "Expected {Path} to be a recognised value, but got {Received}.");

    /// <summary>
    /// Accepts any combination of the bits the enum declares, which is what
    /// <c>[Flags]</c> says its values are for. A bit no member declares still
    /// fails, so an unrecognised value is caught either way.
    /// </summary>
    /// <remarks>
    /// Zero is the exception and is checked against the declared members instead. It
    /// is made of no bits at all, so the bit test accepts it from every flags enum,
    /// including one with no zero-valued member — and zero is exactly what a
    /// deserialiser produces for a field the payload left out.
    /// </remarks>
    private static bool IsMadeOfDeclaredBits(T value)
    {
        ulong bits = ToBits(value);

        return bits == 0
            ? Declared.Contains(value)
            : (bits & ~DeclaredBits) == 0;
    }

    /// <summary>
    /// Reads the value as a bit pattern. A signed enum can declare a negative
    /// member, so the conversion goes through <c>long</c> and reinterprets rather
    /// than through <c>ulong</c>, which would throw on one.
    /// </summary>
    private static ulong ToBits(T value) =>
        CountsAsUnsignedLong
            ? Convert.ToUInt64(value, CultureInfo.InvariantCulture)
            : unchecked((ulong)Convert.ToInt64(
                value,
                CultureInfo.InvariantCulture));

    private static ulong ReadBits()
    {
        var bits = 0UL;

        foreach (T value in Declared)
        {
            bits |= ToBits(value);
        }

        return bits;
    }

    private static HashSet<T> Read()
    {
        var declared = new HashSet<T>();

        foreach (T value in (T[])Enum.GetValues(typeof(T)))
        {
            declared.Add(value);
        }

        return declared;
    }
}
