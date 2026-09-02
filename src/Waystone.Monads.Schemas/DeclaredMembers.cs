namespace Waystone.Monads.Schemas;

using System;
using System.Collections.Generic;

internal static class DeclaredMembers<T> where T : struct, Enum
{
    private static readonly HashSet<T> Declared = Read();

    internal static Schema<T, T> Instance { get; } =
        Schema.For<T>()
              .Check(
                   static value => Declared.Contains(value),
                   ViolationCode.Mismatched,
                   "Expected {Path} to be a recognised value, but got {Received}.");

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
