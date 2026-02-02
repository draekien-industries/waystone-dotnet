namespace Waystone.WideLogEvents;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class ReservedPropertyNames
{
    public const string Outcome = nameof(Outcome);
    public const string HttpRequest = nameof(HttpRequest);
    public const string HttpResponse = nameof(HttpResponse);

    private static readonly HashSet<string> ReservedNames = new(
        typeof(ReservedPropertyNames)
           .GetFields(
                BindingFlags.Public
              | BindingFlags.Static
              | BindingFlags.FlattenHierarchy)
           .Where(f =>
                f is { IsLiteral: true, IsInitOnly: false }
             && f.FieldType == typeof(string))
           .Select(f => (string)f.GetRawConstantValue()),
        StringComparer.OrdinalIgnoreCase);

    internal static bool IsReserved(string propertyName) =>
        !string.IsNullOrWhiteSpace(propertyName)
     && ReservedNames.Contains(propertyName);
}
