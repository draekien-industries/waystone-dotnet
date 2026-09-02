#if NET8_0_OR_GREATER
namespace Waystone.Monads.Schemas;

using System;

public abstract partial class Schema
{
    /// <summary>Gets a schema accepting any calendar date, as a base for date rules.</summary>
    /// <value>
    /// A schema that produces its input unchanged. Chain <c>Before</c> or
    /// <c>After</c> to bound it.
    /// </value>
    /// <remarks>
    /// <para>
    /// A date of birth, an invoice date, an expiry — anything where a time of day
    /// would be noise and a time zone would be a bug. Reach for
    /// <see cref="Timestamp" /> instead when the value marks a moment rather than a
    /// day.
    /// </para>
    /// <para>
    /// <b>Not available on netstandard2.0</b>, because <see cref="DateOnly" /> is
    /// .NET 6 and later only. It is the one member of this package that a
    /// .NET Framework consumer cannot reach; everything else compiles for both
    /// targets. Model a date as a <see cref="DateTimeOffset" /> at midnight there,
    /// or as a domain type reached through <c>Transform</c>.
    /// </para>
    /// </remarks>
    public static Schema<DateOnly, DateOnly> Date { get; } = For<DateOnly>();
}
#endif
