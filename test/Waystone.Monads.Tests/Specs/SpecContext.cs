namespace Waystone.Monads.Specs;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waystone.Monads.Results.Errors;

public sealed class SpecContext
{
    public const string MapSlot = "Map";
    public const string ElseSlot = "Else";
    public const string FlatMapSlot = "FlatMap";
    public const string AsyncOkSlot = "AsyncOkDelegate";
    public const string SyncOkSlot = "SyncOkDelegate";
    public const string AsyncErrorSlot = "AsyncErrorDelegate";
    public const string SyncErrorSlot = "SyncErrorDelegate";

    private const string OutcomeSlot = "Outcome";
    private const string ErrorSlot = "Error";
    private const string ExceptionSlot = "Exception";

    private readonly Dictionary<string, object?> slots =
        new Dictionary<string, object?>();

    public Exception CapturedException => Slot<Exception>(ExceptionSlot);

    public Error Error
    {
        get => Slot<Error>(ErrorSlot);
        set => SetSlot(value, ErrorSlot);
    }

    public T Subject<T>() => Slot<T>(typeof(T).FullName!);

    public void SetSubject<T>(T value) => SetSlot(value, typeof(T).FullName!);

    public T Outcome<T>() => Slot<T>(OutcomeSlot);

    public void SetOutcome<T>(T value) => SetSlot(value, OutcomeSlot);

    public T Slot<T>(string slot)
    {
        if (slots.TryGetValue(slot, out object? value))
        {
            return (T)value!;
        }

        throw new KeyNotFoundException(
            "The scenario did not set the '" + slot + "' slot.");
    }

    public void SetSlot<T>(T value, string slot) => slots[slot] = value;

    public async Task CaptureAsync<T>(Func<Task<T>> operation)
    {
        try
        {
            SetOutcome(await operation().ConfigureAwait(false));
        }
        catch (Exception exception)
        {
            SetSlot(exception, ExceptionSlot);
        }
    }
}
