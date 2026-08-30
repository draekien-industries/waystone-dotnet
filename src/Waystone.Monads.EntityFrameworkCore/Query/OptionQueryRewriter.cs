namespace Microsoft.EntityFrameworkCore.Query;

using System;
using System.Linq.Expressions;
using System.Reflection;
using Waystone.Monads.Options;

internal sealed class OptionQueryRewriter : ExpressionVisitor
{
    private static readonly MethodInfo EfProperty =
        typeof(EF).GetMethod(nameof(EF.Property))!;

    protected override Expression VisitBinary(BinaryExpression node)
    {
        if (node.NodeType is not (ExpressionType.Equal or ExpressionType.NotEqual))
        {
            return base.VisitBinary(node);
        }

        if (TryRewriteComparison(node.NodeType, node.Left, node.Right, out Expression? rewritten)
         || TryRewriteComparison(node.NodeType, node.Right, node.Left, out rewritten))
        {
            return rewritten;
        }

        ThrowIfComparingAnUnreadableOption(node.Left, node.Right);
        ThrowIfComparingAnUnreadableOption(node.Right, node.Left);

        return base.VisitBinary(node);
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Member.Name is not (nameof(Option<int>.IsSome) or nameof(Option<int>.IsNone))
         || node.Expression is not MemberExpression source
         || !IsOption(source.Type))
        {
            return base.VisitMember(node);
        }

        Expression column = Column(source);
        ConstantExpression @null = Expression.Constant(null, column.Type);

        return node.Member.Name == nameof(Option<int>.IsSome)
            ? Expression.NotEqual(column, @null)
            : Expression.Equal(column, @null);
    }

    private static bool TryRewriteComparison(
        ExpressionType comparison,
        Expression optionSide,
        Expression valueSide,
        out Expression rewritten)
    {
        rewritten = null!;

        if (optionSide is not MemberExpression source
         || !IsOption(source.Type)
         || !TryReadOption(valueSide, out object? held, out object? option))
        {
            return false;
        }

        if (held is null)
        {
            Expression column = Column(source);
            ConstantExpression @null = Expression.Constant(null, column.Type);

            rewritten = comparison == ExpressionType.Equal
                ? Expression.Equal(column, @null)
                : Expression.NotEqual(column, @null);

            return true;
        }

        Expression parameter = Expression.Constant(option, source.Type);

        rewritten = comparison == ExpressionType.Equal
            ? Expression.Equal(optionSide, parameter)
            : Expression.NotEqual(optionSide, parameter);

        return true;
    }

    private static void ThrowIfComparingAnUnreadableOption(
        Expression optionSide,
        Expression valueSide)
    {
        if (optionSide is not MemberExpression source || !IsOption(source.Type))
        {
            return;
        }

        throw new InvalidOperationException(
            $"Cannot translate the comparison of '{source.Member.DeclaringType?.Name}."
          + $"{source.Member.Name}' against a value this query cannot read. A "
          + "captured option becomes a SQL parameter before translation, so one "
          + "compiled query serves both a some and a none, and a none would "
          + "compare against NULL and match no row rather than the rows that "
          + "hold none. Write the option inline instead - "
          + $"'{source.Member.Name} == Option.Some(value)' or "
          + $"'{source.Member.Name} == Option.None<T>()' - or compare the column "
          + $"directly with 'EF.Property<T?>(entity, \"{source.Member.Name}\")'. "
          + $"The unreadable side was '{valueSide}'.");
    }

    private static bool TryReadOption(
        Expression node,
        out object? held,
        out object? option)
    {
        held = null;
        option = null;

        if (!IsOption(node.Type) || !IsClosed(node))
        {
            return false;
        }

        option = Expression.Lambda(node).Compile().DynamicInvoke();

        if (option is null)
        {
            return true;
        }

        Type declared = node.Type;

        if (!(bool)declared.GetProperty(nameof(Option<int>.IsSome))!
                          .GetValue(option)!)
        {
            return true;
        }

        held = declared.GetMethod(nameof(Option<int>.Unwrap), Type.EmptyTypes)!
                       .Invoke(option, null);

        return true;
    }

    private static bool IsClosed(Expression node) =>
        new ParameterFinder().Search(node);

    private sealed class ParameterFinder : ExpressionVisitor
    {
        private bool found;

        public bool Search(Expression node)
        {
            found = false;
            Visit(node);
            return !found;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            found = true;
            return node;
        }
    }

    private static Expression Column(MemberExpression source)
    {
        Type held = source.Type.GetGenericArguments()[0];
        Type provider = held.IsValueType
            ? typeof(Nullable<>).MakeGenericType(held)
            : held;

        return Expression.Call(
            EfProperty.MakeGenericMethod(provider),
            source.Expression!,
            Expression.Constant(source.Member.Name));
    }

    private static bool IsOption(Type type)
    {
        for (Type? candidate = type; candidate is not null; candidate = candidate.BaseType)
        {
            if (candidate.IsGenericType
             && candidate.GetGenericTypeDefinition() == typeof(Option<>))
            {
                return true;
            }
        }

        return false;
    }
}
