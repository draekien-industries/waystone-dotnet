namespace Waystone.Monads.Fixtures;

using System;
using System.Diagnostics;

/// <summary>
/// Forwards <see cref="IObserver{T}.OnNext" /> to a delegate and ignores
/// completion and errors.
/// </summary>
/// <remarks>
/// Exists because <see cref="DiagnosticListener" /> takes an
/// <see cref="IObserver{T}" /> rather than an <see cref="Action{T}" />, and every
/// test subscribing to the library's listener wants only the one callback.
/// Swallowing <see cref="IObserver{T}.OnError" /> is safe here, because a
/// <see cref="DiagnosticListener" /> never calls it.
/// </remarks>
/// <typeparam name="T">The type of value being observed.</typeparam>
/// <param name="onNext">The delegate each observed value is handed to.</param>
public sealed class Observer<T>(Action<T> onNext) : IObserver<T>
{
    public void OnCompleted()
    { }

    public void OnError(Exception error)
    { }

    public void OnNext(T value)
    {
        onNext(value);
    }
}
