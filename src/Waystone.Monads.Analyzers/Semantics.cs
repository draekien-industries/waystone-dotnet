namespace Waystone.Monads.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public static class Semantics
{
    public static readonly ImmutableHashSet<string> UnwrapNames =
        ImmutableHashSet.Create(
            "Unwrap",
            "UnwrapAsync",
            "UnwrapErr",
            "UnwrapErrAsync");

    public static readonly ImmutableHashSet<string> ExpectNames =
        ImmutableHashSet.Create(
            "Expect",
            "ExpectAsync",
            "ExpectErr",
            "ExpectErrAsync");

    public static readonly ImmutableHashSet<string> PanickingNames =
        UnwrapNames.Union(ExpectNames);

    public static readonly ImmutableHashSet<string> StateNames =
        ImmutableHashSet.Create("IsSome", "IsNone", "IsOk", "IsErr");

    private static readonly Dictionary<string, string> WellKnownDefaults =
        new Dictionary<string, string>
        {
            ["System.Guid"] = "Empty",
            ["System.DateTime"] = "MinValue",
            ["System.DateTimeOffset"] = "MinValue",
            ["System.TimeSpan"] = "Zero",
            ["System.IntPtr"] = "Zero",
            ["System.UIntPtr"] = "Zero",
        };

    public static IOperation Unconverted(IOperation operation)
    {
        while (operation is IConversionOperation conversion)
        {
            operation = conversion.Operand;
        }

        return operation;
    }

    public static bool IsDefaultValue(IOperation operation)
    {
        var value = Unconverted(operation);

        if (value is IDefaultValueOperation)
        {
            return true;
        }

        if (value.ConstantValue.HasValue)
        {
            object? constant = value.ConstantValue.Value;

            return constant is null || IsZero(constant);
        }

        return IsWellKnownDefault(value);
    }

    public static bool IsZeroConstant(object? constant) =>
        constant is not null && IsZero(constant);

    public static string DefaultOf(ITypeSymbol type)
    {
        if (WellKnownDefaults.TryGetValue(
                type.ToDisplayString(),
                out string? wellKnown))
        {
            return Display(type) + "." + wellKnown;
        }

        if (type.TypeKind == TypeKind.Enum)
        {
            var zero = ZeroMemberOf(type);

            return zero is null
                ? "default(" + Display(type) + ")"
                : Display(type) + "." + zero.Name;
        }

        switch (type.SpecialType)
        {
            case SpecialType.System_Boolean:
                return "false";
            case SpecialType.System_Char:
                return "'\\0'";
            case SpecialType.System_SByte:
            case SpecialType.System_Byte:
            case SpecialType.System_Int16:
            case SpecialType.System_UInt16:
            case SpecialType.System_Int32:
            case SpecialType.System_UInt32:
            case SpecialType.System_Int64:
            case SpecialType.System_UInt64:
            case SpecialType.System_Single:
            case SpecialType.System_Double:
            case SpecialType.System_Decimal:
                return "0";
            default:
                return "default(" + Display(type) + ")";
        }
    }

    private static IFieldSymbol? ZeroMemberOf(ITypeSymbol type) =>
        type.GetMembers()
           .OfType<IFieldSymbol>()
           .FirstOrDefault(
                field => field.HasConstantValue
                      && IsZeroConstant(field.ConstantValue));

    public static bool IsMaybeNull(IOperation operation)
    {
        var value = Unconverted(operation);

        if (value.Type is null || value.Type.IsValueType)
        {
            return false;
        }

        if (value.SemanticModel is null)
        {
            return false;
        }

        return value.SemanticModel.GetTypeInfo(value.Syntax)
                    .Nullability.FlowState
            == NullableFlowState.MaybeNull;
    }

    public static IOperation? ReceiverOf(IInvocationOperation invocation)
    {
        if (invocation.Instance is not null)
        {
            return invocation.Instance;
        }

        if (invocation.Arguments.Length == 0)
        {
            return null;
        }

        var first = invocation.Arguments[0].Value;

        return invocation.Syntax is InvocationExpressionSyntax
            {
                Expression: MemberAccessExpressionSyntax access,
            }
            && access.Expression.Span == first.Syntax.Span
                ? first
                : null;
    }

    public static ISymbol? ReferencedSymbol(IOperation? operation)
    {
        if (operation is null)
        {
            return null;
        }

        return Unconverted(operation) switch
        {
            ILocalReferenceOperation local => local.Local,
            IParameterReferenceOperation parameter => parameter.Parameter,
            IFieldReferenceOperation field => field.Field,
            IPropertyReferenceOperation property => property.Property,
            _ => null,
        };
    }

    public static bool ContainsPanickingCallOn(
        IOperation root,
        ISymbol instance,
        MonadSymbols symbols) =>
        PanickingCallsOn(root, instance, symbols).Any();

    public static IEnumerable<IInvocationOperation> PanickingCallsOn(
        IOperation root,
        ISymbol instance,
        MonadSymbols symbols) =>
        root.DescendantsAndSelf()
           .OfType<IInvocationOperation>()
           .Where(
                invocation =>
                    PanickingNames.Contains(invocation.TargetMethod.Name)
                 && symbols.IsMonadInvocation(invocation)
                 && SymbolEqualityComparer.Default.Equals(
                        ReferencedSymbol(ReceiverOf(invocation)),
                        instance));

    public static Location NameLocationOf(IInvocationOperation invocation)
    {
        if (invocation.Syntax is not InvocationExpressionSyntax invoked)
        {
            return invocation.Syntax.GetLocation();
        }

        return invoked.Expression is MemberAccessExpressionSyntax memberAccess
            ? memberAccess.Name.GetLocation()
            : invoked.Expression.GetLocation();
    }

    public static string Display(ITypeSymbol type) =>
        type.WithNullableAnnotation(NullableAnnotation.None)
           .ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

    public static ITypeSymbol NonNullable(ITypeSymbol type) =>
        type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T
     && type is INamedTypeSymbol { TypeArguments.Length: 1 } nullable
            ? nullable.TypeArguments[0]
            : type;

    public static bool IsNullable(ITypeSymbol type) =>
        type.NullableAnnotation == NullableAnnotation.Annotated
     || type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;

    public static (ISymbol? Instance, string? Member) StateCheck(
        IOperation operation,
        MonadSymbols symbols)
    {
        if (Unconverted(operation) is not IPropertyReferenceOperation property
         || !StateNames.Contains(property.Property.Name)
         || !symbols.IsMonad(property.Property.ContainingType))
        {
            return (null, null);
        }

        return (ReferencedSymbol(property.Instance), property.Property.Name);
    }

    public static bool IsDeclarationTypePosition(SyntaxNode node)
    {
        var current = node;

        while (current.Parent is QualifiedNameSyntax
            or AliasQualifiedNameSyntax
            or NullableTypeSyntax
            or TupleElementSyntax
            or TupleTypeSyntax
            or ArrayTypeSyntax)
        {
            current = current.Parent;
        }

        return current.Parent switch
        {
            ParameterSyntax parameter => parameter.Type == current,
            MethodDeclarationSyntax method => method.ReturnType == current,
            LocalFunctionStatementSyntax local => local.ReturnType == current,
            PropertyDeclarationSyntax property => property.Type == current,
            VariableDeclarationSyntax variable => variable.Type == current,
            DelegateDeclarationSyntax @delegate =>
                @delegate.ReturnType == current,
            TypeArgumentListSyntax list => list.Parent is not null
             && IsDeclarationTypePosition(list.Parent),
            _ => false,
        };
    }

    public static Location TypeLocationOf(ISymbol symbol)
    {
        var reference = symbol.DeclaringSyntaxReferences.FirstOrDefault();

        var syntax = reference?.GetSyntax();

        var type = syntax switch
        {
            MethodDeclarationSyntax method => method.ReturnType,
            PropertyDeclarationSyntax property => property.Type,
            ParameterSyntax parameter => parameter.Type,
            VariableDeclaratorSyntax
            {
                Parent: VariableDeclarationSyntax declaration,
            } => declaration.Type,
            _ => null,
        };

        return type?.GetLocation()
            ?? symbol.Locations.FirstOrDefault()
            ?? Location.None;
    }

    private static bool IsWellKnownDefault(IOperation operation)
    {
        var member = operation switch
        {
            IFieldReferenceOperation field => (ISymbol)field.Field,
            IPropertyReferenceOperation property => property.Property,
            _ => null,
        };

        if (member?.ContainingType is null)
        {
            return false;
        }

        return WellKnownDefaults.TryGetValue(
                   member.ContainingType.ToDisplayString(),
                   out string? name)
            && name == member.Name;
    }

    private static bool IsZero(object constant) =>
        constant switch
        {
            bool value => !value,
            char value => value == '\0',
            sbyte value => value == 0,
            byte value => value == 0,
            short value => value == 0,
            ushort value => value == 0,
            int value => value == 0,
            uint value => value == 0U,
            long value => value == 0L,
            ulong value => value == 0UL,
            float value => value == 0F,
            double value => value == 0D,
            decimal value => value == 0M,
            _ => false,
        };
}
