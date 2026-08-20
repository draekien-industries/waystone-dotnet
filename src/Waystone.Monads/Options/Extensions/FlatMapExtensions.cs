namespace Waystone.Monads.Options.Extensions;

using System;
using System.Threading.Tasks;

public static class FlatMapExtensions
{
    extension<T>(Option<T> option) where T : notnull
    {
        /// <summary>
        /// Projects the inner value of the current <see cref="Option{T}" /> into another
        /// <see cref="Option{TOut}" /> asynchronously using a provided mapping function.
        /// </summary>
        /// <typeparam name="TOut">The type of the resulting <see cref="Option{TOut}" />.</typeparam>
        /// <param name="map">
        /// A function that asynchronously maps the inner value of the current
        /// <see cref="Option{T}" /> to another <see cref="Option{TOut}" />.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> of <see cref="Option{TOut}" />, containing
        /// the mapped
        /// value if the original <see cref="Option{T}" /> was <see cref="Some{T}" />, or
        /// <see cref="None{T}" /> otherwise.
        /// </returns>
        [Obsolete(
            "Use AndThenAsync instead. This method will be removed in v6 of Waystone.Monads.")]
        public ValueTask<Option<TOut>> FlatMapAsync<TOut>(
            Func<T, Task<Option<TOut>>> map)
            where TOut : notnull =>
            option.AndThenAsync(map);
    }

    extension<T>(Task<Option<T>> optionTask) where T : notnull
    {
        /// <summary>
        /// Projects the inner value of the current <see cref="Task{TResult}" /> containing
        /// an
        /// <see cref="Option{T}" /> into another <see cref="Option{TOut}" />
        /// asynchronously using a provided mapping function.
        /// </summary>
        /// <typeparam name="TOut">The type of the resulting <see cref="Option{TOut}" />.</typeparam>
        /// <param name="map">
        /// A function that asynchronously maps the inner value of the current
        /// <see cref="Option{T}" /> to another <see cref="Option{TOut}" />.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> containing the <see cref="Option{TOut}" />, with
        /// the mapped value if the original <see cref="Option{T}" /> was
        /// <see cref="Some{T}" />, or
        /// <see cref="None{T}" /> otherwise.
        /// </returns>
        [Obsolete(
            "Use AndThenAsync instead. This method will be removed in v6 of Waystone.Monads.")]
        public ValueTask<Option<TOut>> FlatMapAsync<TOut>(
            Func<T, Task<Option<TOut>>> map)
            where TOut : notnull =>
            optionTask.AndThenAsync(map);

        /// <summary>
        /// Projects the inner value of the current <see cref="Task{TResult}" /> containing
        /// an <see cref="Option{T}" /> into another <see cref="Option{TOut}" />
        /// asynchronously
        /// using a provided mapping function.
        /// </summary>
        /// <typeparam name="TOut">The type of the resulting <see cref="Option{TOut}" />.</typeparam>
        /// <param name="map">
        /// A function that asynchronously maps the inner value of the current
        /// <see cref="Option{T}" /> to another <see cref="Option{TOut}" />.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> of <see cref="Option{TOut}" />, containing the
        /// mapped
        /// value if the original <see cref="Option{T}" /> was <see cref="Some{T}" />, or
        /// <see cref="None{T}" /> otherwise.
        /// </returns>
        [Obsolete(
            "Use AndThenAsync instead. This method will be removed in v6 of Waystone.Monads.")]
        public ValueTask<Option<TOut>> FlatMapAsync<TOut>(
            Func<T, Option<TOut>> map)
            where TOut : notnull =>
            optionTask.AndThenAsync(map);
    }

    extension<T>(ValueTask<Option<T>> optionTask) where T : notnull
    {
        /// <summary>
        /// Projects the inner value of the current <see cref="Option{T}" /> into another
        /// <see cref="Option{TOut}" /> asynchronously using a provided mapping function.
        /// </summary>
        /// <typeparam name="TOut">The type of the resulting <see cref="Option{TOut}" />.</typeparam>
        /// <param name="map">
        /// A function that asynchronously maps the inner value of the current
        /// <see cref="Option{T}" /> to another <see cref="Option{TOut}" />.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> of <see cref="Option{TOut}" />, containing
        /// the mapped value if the original <see cref="Option{T}" /> was
        /// <see cref="Some{T}" />, or
        /// <see cref="None{T}" /> otherwise.
        /// </returns>
        [Obsolete(
            "Use AndThenAsync instead. This method will be removed in v6 of Waystone.Monads.")]
        public ValueTask<Option<TOut>> FlatMapAsync<TOut>(
            Func<T, Task<Option<TOut>>> map)
            where TOut : notnull =>
            optionTask.AndThenAsync(map);

        /// <summary>
        /// Projects the inner value of the current <see cref="Option{T}" /> into another
        /// <see cref="Option{TOut}" /> asynchronously using a provided mapping function.
        /// </summary>
        /// <typeparam name="TOut">The type of the resulting <see cref="Option{TOut}" />.</typeparam>
        /// <param name="map">
        /// A function that asynchronously maps the inner value of the current
        /// <see cref="Option{T}" /> to another <see cref="Option{TOut}" />.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}" /> of <see cref="Option{TOut}" />, containing
        /// the mapped value if the original <see cref="Option{T}" /> was
        /// <see cref="Some{T}" />,
        /// or <see cref="None{T}" /> otherwise.
        /// </returns>
        [Obsolete(
            "Use AndThenAsync instead. This method will be removed in v6 of Waystone.Monads.")]
        public ValueTask<Option<TOut>> FlatMapAsync<TOut>(
            Func<T, Option<TOut>> map)
            where TOut : notnull =>
            optionTask.AndThenAsync(map);
    }
}
