namespace Waystone.Monads.SourceGenerators.ErrorCodes;

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

internal static class ErrorCodeProviderWriter
{
    public const string ErrorCodeStringsClass = "ErrorCodeStrings";
    public const string ErrorCodesClass = "ErrorCodes";
    public const string ErrorsClass = "Errors";

    private const string ErrorCodeType =
        "global::Waystone.Monads.Results.Errors.ErrorCode";

    private const string ErrorType =
        "global::Waystone.Monads.Results.Errors.Error";

    public static string Emit(
        INamedTypeSymbol enumType,
        string providerName,
        IReadOnlyList<IFieldSymbol> members)
    {
        string qualified =
            enumType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        string @namespace = enumType.ContainingNamespace.IsGlobalNamespace
            ? string.Empty
            : enumType.ContainingNamespace.ToDisplayString();

        var writer = new Writer();

        var depth = 0;

        if (@namespace.Length > 0)
        {
            writer.Line(0, $"namespace {@namespace}");
            writer.Line(0, "{");
            depth = 1;
        }

        writer.Line(
            depth,
            $"/// <summary>The error codes declared by <c>{qualified}</c>.</summary>");

        writer.Line(
            depth,
            $"{AccessibilityOf(enumType)} static partial class {providerName}");

        writer.Line(depth, "{");

        WriteStrings(writer, depth + 1, qualified, enumType.Name, members);
        writer.Blank();
        WriteCodes(writer, depth + 1, qualified, members);
        writer.Blank();
        WriteErrors(writer, depth + 1, qualified, members);
        writer.Blank();

        WriteLookup(
            writer,
            depth + 1,
            qualified,
            members,
            "string",
            "ToErrorCodeString",
            "error code string",
            member => $"{ErrorCodeStringsClass}.{member.Name}",
            $"{ErrorCodeType}.FromEnum(value).Value");

        writer.Blank();

        WriteLookup(
            writer,
            depth + 1,
            qualified,
            members,
            ErrorCodeType,
            "ToErrorCode",
            "error code",
            member => $"{ErrorCodesClass}.{member.Name}",
            $"{ErrorCodeType}.FromEnum(value)");

        writer.Blank();
        WriteToError(writer, depth + 1, qualified);

        writer.Line(depth, "}");

        if (@namespace.Length > 0) writer.Line(0, "}");

        return writer.ToString();
    }

    private static void WriteStrings(
        Writer writer,
        int depth,
        string qualified,
        string enumName,
        IReadOnlyList<IFieldSymbol> members)
    {
        writer.Line(
            depth,
            $"/// <summary>The error code string of every <c>{qualified}</c> member.</summary>");

        writer.Line(depth, $"public static class {ErrorCodeStringsClass}");
        writer.Line(depth, "{");

        for (var i = 0; i < members.Count; i++)
        {
            if (i > 0) writer.Blank();

            string name = members[i].Name;

            writer.Line(
                depth + 1,
                $"/// <summary>The error code string of <c>{qualified}.{name}</c>.</summary>");

            writer.Line(
                depth + 1,
                $"public const string {name} = \"{enumName}.{name}\";");
        }

        writer.Line(depth, "}");
    }

    private static void WriteCodes(
        Writer writer,
        int depth,
        string qualified,
        IReadOnlyList<IFieldSymbol> members)
    {
        writer.Line(
            depth,
            $"/// <summary>The error code of every <c>{qualified}</c> member.</summary>");

        writer.Line(depth, $"public static class {ErrorCodesClass}");
        writer.Line(depth, "{");

        for (var i = 0; i < members.Count; i++)
        {
            if (i > 0) writer.Blank();

            string name = members[i].Name;

            writer.Line(
                depth + 1,
                $"/// <summary>The error code of <c>{qualified}.{name}</c>.</summary>");

            writer.Line(
                depth + 1,
                $"public static readonly {ErrorCodeType} {name} = new {ErrorCodeType}({ErrorCodeStringsClass}.{name});");
        }

        writer.Line(depth, "}");
    }

    private static void WriteErrors(
        Writer writer,
        int depth,
        string qualified,
        IReadOnlyList<IFieldSymbol> members)
    {
        writer.Line(
            depth,
            $"/// <summary>Creates an error carrying the error code of a <c>{qualified}</c> member.</summary>");

        writer.Line(depth, $"public static class {ErrorsClass}");
        writer.Line(depth, "{");

        for (var i = 0; i < members.Count; i++)
        {
            if (i > 0) writer.Blank();

            string name = members[i].Name;

            writer.Line(
                depth + 1,
                $"/// <summary>Creates an error carrying the error code of <c>{qualified}.{name}</c>.</summary>");

            writer.Line(
                depth + 1,
                "/// <param name=\"message\">The message describing this occurrence of the error.</param>");

            writer.Line(depth + 1, "/// <returns>The created error.</returns>");

            writer.Line(
                depth + 1,
                $"public static {ErrorType} {name}(string message)");

            writer.Line(depth + 1, "{");

            writer.Line(
                depth + 2,
                $"return new {ErrorType}({ErrorCodesClass}.{name}, message);");

            writer.Line(depth + 1, "}");
        }

        writer.Line(depth, "}");
    }

    private static void WriteLookup(
        Writer writer,
        int depth,
        string qualified,
        IReadOnlyList<IFieldSymbol> members,
        string returnType,
        string methodName,
        string noun,
        Func<IFieldSymbol, string> result,
        string fallback)
    {
        writer.Line(
            depth,
            $"/// <summary>Gets the {noun} of a <c>{qualified}</c> value.</summary>");

        writer.Line(
            depth,
            $"/// <param name=\"value\">The value to read the {noun} of.</param>");

        writer.Line(
            depth,
            $"/// <returns>The {noun}, or the runtime error code of a value that is not a declared member.</returns>");

        writer.Line(
            depth,
            $"public static {returnType} {methodName}(this {qualified} value)");

        writer.Line(depth, "{");
        writer.Line(depth + 1, "switch (value)");
        writer.Line(depth + 1, "{");

        foreach (IFieldSymbol member in members)
        {
            writer.Line(depth + 2, $"case {qualified}.{member.Name}:");
            writer.Line(depth + 3, $"return {result(member)};");
        }

        writer.Line(depth + 2, "default:");
        writer.Line(depth + 3, $"return {fallback};");
        writer.Line(depth + 1, "}");
        writer.Line(depth, "}");
    }

    private static void WriteToError(Writer writer, int depth, string qualified)
    {
        writer.Line(
            depth,
            $"/// <summary>Creates an error carrying the error code of a <c>{qualified}</c> value.</summary>");

        writer.Line(
            depth,
            "/// <param name=\"value\">The value to read the error code of.</param>");

        writer.Line(
            depth,
            "/// <param name=\"message\">The message describing this occurrence of the error.</param>");

        writer.Line(depth, "/// <returns>The created error.</returns>");

        writer.Line(
            depth,
            $"public static {ErrorType} ToError(this {qualified} value, string message)");

        writer.Line(depth, "{");

        writer.Line(
            depth + 1,
            $"return new {ErrorType}(ToErrorCode(value), message);");

        writer.Line(depth, "}");
    }

    private static string AccessibilityOf(INamedTypeSymbol enumType) =>
        enumType.DeclaredAccessibility == Accessibility.Public
            ? "public"
            : "internal";

    private sealed class Writer
    {
        private readonly StringBuilder _builder = new StringBuilder();

        public void Line(int depth, string text)
        {
            _builder.Append(new string(' ', depth * 4));
            _builder.Append(text);
            _builder.Append('\n');
        }

        public void Blank() => _builder.Append('\n');

        public override string ToString() => _builder.ToString();
    }
}
