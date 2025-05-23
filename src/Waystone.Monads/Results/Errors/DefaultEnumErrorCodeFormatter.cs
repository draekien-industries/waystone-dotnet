﻿namespace Waystone.Monads.Results.Errors;

using System;

internal sealed class DefaultEnumErrorCodeFormatter<T> : IErrorCodeFormatter<T>
    where T : Enum
{
    /// <inheritdoc />
    public string Format(T value) => $"{typeof(T).Name}.{value}";
}
