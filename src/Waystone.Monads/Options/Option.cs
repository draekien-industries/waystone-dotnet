﻿namespace Waystone.Monads.Options;

using System;
using System.Threading.Tasks;

/// <summary>Static functions for <see cref="Option{T}" /></summary>
public static class Option
{
    /// <summary>
    /// Binds the result of a <paramref name="factory" /> into an
    /// <see cref="Option{T}" />
    /// </summary>
    /// <param name="factory">
    /// A method which when executed will produce the value of
    /// the <see cref="Option{T}" />
    /// </param>
    /// <param name="onError">
    /// Optional. Provides access to any exceptions the factory
    /// throws. Not providing a callback will mean the exception gets swallowed.
    /// </param>
    /// <typeparam name="T">The factory return value's type</typeparam>
    /// <returns>
    /// A <see cref="Some{T}" /> if the factory executes successfully,
    /// otherwise a <see cref="None{T}" />
    /// </returns>
    public static Option<T> Bind<T>(
        Func<T> factory,
        Action<Exception>? onError = null)
        where T : notnull
    {
        try
        {
            T value = factory();
            return Some(value);
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex);
            return None<T>();
        }
    }

    /// <summary>
    /// Binds the result of an <paramref name="asyncFactory" /> into an
    /// <see cref="Option{T}" />
    /// </summary>
    /// <param name="asyncFactory">
    /// An asynchronous method which when awaited will
    /// produce the value for the <see cref="Option{T}" />
    /// </param>
    /// <param name="onError">
    /// Optional. Provides access to any exceptions the factory
    /// throws. Not providing a callback will mean the exception gets swallowed.
    /// </param>
    /// <typeparam name="T">The async factory return type</typeparam>
    /// <returns>
    /// A <see cref="Some{T}" /> if the factory succeeds, otherwise a
    /// <see cref="None{T}" />
    /// </returns>
    public static async Task<Option<T>> Bind<T>(
        Func<Task<T>> asyncFactory,
        Action<Exception>? onError = null) where T : notnull
    {
        try
        {
            T value = await asyncFactory();
            return Some(value);
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex);
            return None<T>();
        }
    }

    /// <summary>Creates a <see cref="Some{T}" /></summary>
    /// <param name="value">The value of the <see cref="Some{T}" /></param>
    /// <typeparam name="T">The option value's type.</typeparam>
    /// <returns>An <see cref="Option{T}" />.</returns>
    public static Option<T> Some<T>(T value) where T : notnull =>
        new Some<T>(value);

    /// <summary>Creates a <see cref="None{T}" /></summary>
    /// <typeparam name="T">The option value's type.</typeparam>
    /// <returns>An <see cref="Option{T}" />.</returns>
    public static Option<T> None<T>() where T : notnull => new None<T>();
}
