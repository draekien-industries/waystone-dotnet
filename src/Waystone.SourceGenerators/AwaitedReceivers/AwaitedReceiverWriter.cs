namespace Waystone.SourceGenerators.AwaitedReceivers;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

/// <summary>
/// Writes the generated receiver shapes. Line endings are normalised to <c>\n</c> so
/// the emitted source does not vary by build platform.
/// </summary>
internal static class AwaitedReceiverWriter
{
    private const string Task = "global::System.Threading.Tasks.Task";
    private const string ValueTask = "global::System.Threading.Tasks.ValueTask";

    public static string Emit(
        INamedTypeSymbol target,
        IEnumerable<AwaitedMember> members,
        Compilation compilation)
    {
        var writer = new StringBuilder();

        writer.AppendLine("#nullable enable");
        writer.AppendLine();
        writer.AppendLine(
            $"namespace {target.ContainingNamespace.ToDisplayString()};");
        writer.AppendLine();
        writer.AppendLine($"public static partial class {target.Name}");
        writer.AppendLine("{");

        var first = true;

        foreach (IGrouping<string, AwaitedMember> group in members.GroupBy(Key))
        {
            foreach (string wrapper in new[] { Task, ValueTask })
            {
                if (!first) writer.AppendLine();

                first = false;

                EmitBlock(writer, group.ToImmutableArray(), wrapper, compilation);
            }
        }

        writer.Append('}');
        writer.AppendLine();

        return writer.Replace("\r\n", "\n").ToString();
    }

    private static string Key(AwaitedMember member) =>
        member.ReceiverType.ToDisplayString(Display.Format)
      + " "
      + member.ReceiverParameterName;

    private static void EmitBlock(
        StringBuilder writer,
        ImmutableArray<AwaitedMember> group,
        string wrapper,
        Compilation compilation)
    {
        AwaitedMember head = group[0];
        string receiver = head.ReceiverType.ToDisplayString(Display.Format);
        string parameterName = Identifiers.Escape(head.ReceiverParameterName + "Task");

        writer.AppendLine(
            $"    extension{TypeParameters.Render(head.BlockTypeParameters)}({wrapper}<{receiver}> {parameterName})");

        foreach (string constraint in TypeParameters.Constraints(head.BlockTypeParameters))
        {
            writer.AppendLine($"        {constraint}");
        }

        writer.AppendLine("    {");

        for (var i = 0; i < group.Length; i++)
        {
            if (i > 0) writer.AppendLine();

            EmitMember(writer, group[i], parameterName, compilation);
        }

        writer.AppendLine("    }");
    }

    private static void EmitMember(
        StringBuilder writer,
        AwaitedMember member,
        string receiverParameterName,
        Compilation compilation)
    {
        foreach (string line in DocComments.Render(
                     member.Source,
                     member.ReceiverType,
                     compilation,
                     member.SummaryOverride))
        {
            writer.AppendLine($"        {line}");
        }

        string name = member.Source.Name.EndsWith("Async")
            ? member.Source.Name
            : member.Source.Name + "Async";

        (string returnType, string statement) = Invocation(member);

        writer.AppendLine(
            $"        public async {returnType} {name}{TypeParameters.Render(member.MemberTypeParameters)}({RenderParameters(member.Parameters)})");

        foreach (string constraint in
                 TypeParameters.Constraints(member.MemberTypeParameters))
        {
            writer.AppendLine($"            {constraint}");
        }

        writer.AppendLine("        {");
        writer.AppendLine(
            $"            {member.ReceiverType.ToDisplayString(Display.Format)} {Identifiers.Escape(member.ReceiverParameterName)} = await {receiverParameterName}.ConfigureAwait(false);");
        writer.AppendLine();
        writer.AppendLine($"            {statement}");
        writer.AppendLine("        }");
    }

    private static (string ReturnType, string Statement) Invocation(AwaitedMember member)
    {
        var call =
            $"{Identifiers.Escape(member.ReceiverParameterName)}.{member.Source.Name}{TypeParameters.Render(member.MemberTypeParameters)}({RenderArguments(member.Parameters)})";

        ITypeSymbol returns = member.Source.ReturnType;

        if (member.Source.ReturnsVoid) return (ValueTask, $"{call};");

        if (Awaitables.IsAwaitable(returns))
        {
            ITypeSymbol? awaited = Awaitables.Unwrap(returns);

            return awaited is null
                ? (ValueTask, $"await {call}.ConfigureAwait(false);")
                : ($"{ValueTask}<{awaited.ToDisplayString(Display.Format)}>",
                    $"return await {call}.ConfigureAwait(false);");
        }

        return ($"{ValueTask}<{returns.ToDisplayString(Display.Format)}>",
            $"return {call};");
    }

    private static string RenderParameters(ImmutableArray<IParameterSymbol> parameters) =>
        string.Join(", ", parameters.Select(RenderParameter));

    private static string RenderParameter(IParameterSymbol parameter)
    {
        var rendered = new StringBuilder();

        if (parameter.IsParams) rendered.Append("params ");

        rendered.Append(RefKindPrefix(parameter.RefKind))
                .Append(parameter.Type.ToDisplayString(Display.Format))
                .Append(' ')
                .Append(Identifiers.Escape(parameter.Name));

        if (parameter.HasExplicitDefaultValue)
        {
            rendered.Append(" = ").Append(Identifiers.Default(parameter));
        }

        return rendered.ToString();
    }

    private static string RenderArguments(ImmutableArray<IParameterSymbol> parameters) =>
        string.Join(
            ", ",
            parameters.Select(
                static p =>
                    RefKindPrefix(p.RefKind) + Identifiers.Escape(p.Name)));

    private static string RefKindPrefix(RefKind kind) =>
        kind switch
        {
            RefKind.Ref => "ref ",
            RefKind.Out => "out ",
            RefKind.In => "in ",
            _ => string.Empty,
        };
}
