namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

public static partial class OptionOfTAsyncExtensions
{
    /// <summary>
    /// Zips the current option with another option using the provided
    /// function.
    /// </summary>
    /// <typeparam name="TSelf">The type of the value in the current option.</typeparam>
    /// <typeparam name="TOther">The type of the value in the other option.</typeparam>
    /// <typeparam name="TOut">The return type of the zip function.</typeparam>
    /// <param name="self">The current option.</param>
    /// <param name="other">The option to zip with.</param>
    /// <param name="zip">The function that will perform the zipping.</param>
    /// <returns>
    /// If the current option is <see cref="Some{T}" /> and
    /// <paramref name="other" /> is <see cref="Some{T}" />, this method returns
    /// <c>Some&lt;TOut&gt;</c> where <c>TOut</c> is the result of applying
    /// <paramref name="zip" /> to the values of both options. Otherwise,
    /// <c>None&lt;TOut&gt;</c> is returned.
    /// </returns>
    public static Task<Option<TOut>> ZipWithAsync<TSelf, TOther, TOut>(
        this Option<TSelf> self,
        Option<TOther> other,
        Func<TSelf, TOther, Task<TOut>> zip)
        where TSelf : notnull
        where TOther : notnull
        where TOut : notnull =>
        self.FlatMapAsync(s => other.MapAsync(o => zip(s, o)));

    /// <summary>
    /// Zips the current option with another option using the provided
    /// function.
    /// </summary>
    /// <typeparam name="TSelf">The type of the value in the current option.</typeparam>
    /// <typeparam name="TOther">The type of the value in the other option.</typeparam>
    /// <typeparam name="TOut">The return type of the zip function.</typeparam>
    /// <param name="self">The current option.</param>
    /// <param name="other">The option to zip with.</param>
    /// <param name="zip">The function that will perform the zipping.</param>
    /// <returns>
    /// If the current option is <see cref="Some{T}" /> and
    /// <paramref name="other" /> is <see cref="Some{T}" />, this method returns
    /// <c>Some&lt;TOut&gt;</c> where <c>TOut</c> is the result of applying
    /// <paramref name="zip" /> to the values of both options. Otherwise,
    /// <c>None&lt;TOut&gt;</c> is returned.
    /// </returns>
    public static Task<Option<TOut>> ZipWithAsync<TSelf, TOther, TOut>(
        this Task<Option<TSelf>> self,
        Option<TOther> other,
        Func<TSelf, TOther, Task<TOut>> zip)
        where TSelf : notnull
        where TOther : notnull
        where TOut : notnull =>
        self.FlatMapAsync(s => other.MapAsync(o => zip(s, o)));

    /// <summary>
    /// Zips the current option with another option using the provided
    /// function.
    /// </summary>
    /// <typeparam name="TSelf">The type of the value in the current option.</typeparam>
    /// <typeparam name="TOther">The type of the value in the other option.</typeparam>
    /// <typeparam name="TOut">The return type of the zip function.</typeparam>
    /// <param name="self">The current option.</param>
    /// <param name="other">The option to zip with.</param>
    /// <param name="zip">The function that will perform the zipping.</param>
    /// <returns>
    /// If the current option is <see cref="Some{T}" /> and
    /// <paramref name="other" /> is <see cref="Some{T}" />, this method returns
    /// <c>Some&lt;TOut&gt;</c> where <c>TOut</c> is the result of applying
    /// <paramref name="zip" /> to the values of both options. Otherwise,
    /// <c>None&lt;TOut&gt;</c> is returned.
    /// </returns>
    public static ValueTask<Option<TOut>> ZipWithAsync<TSelf, TOther, TOut>(
        this Option<TSelf> self,
        Option<TOther> other,
        Func<TSelf, TOther, ValueTask<TOut>> zip)
        where TSelf : notnull
        where TOther : notnull
        where TOut : notnull =>
        self.FlatMapAsync(s => other.MapAsync(o => zip(s, o)));

    /// <summary>
    /// Zips the current option with another option using the provided
    /// function.
    /// </summary>
    /// <typeparam name="TSelf">The type of the value in the current option.</typeparam>
    /// <typeparam name="TOther">The type of the value in the other option.</typeparam>
    /// <typeparam name="TOut">The return type of the zip function.</typeparam>
    /// <param name="self">The current option.</param>
    /// <param name="other">The option to zip with.</param>
    /// <param name="zip">The function that will perform the zipping.</param>
    /// <returns>
    /// If the current option is <see cref="Some{T}" /> and
    /// <paramref name="other" /> is <see cref="Some{T}" />, this method returns
    /// <c>Some&lt;TOut&gt;</c> where <c>TOut</c> is the result of applying
    /// <paramref name="zip" /> to the values of both options. Otherwise,
    /// <c>None&lt;TOut&gt;</c> is returned.
    /// </returns>
    public static ValueTask<Option<TOut>> ZipWithAsync<TSelf, TOther, TOut>(
        this ValueTask<Option<TSelf>> self,
        Option<TOther> other,
        Func<TSelf, TOther, ValueTask<TOut>> zip)
        where TSelf : notnull
        where TOther : notnull
        where TOut : notnull =>
        self.FlatMapAsync(s => other.MapAsync(o => zip(s, o)));
}
