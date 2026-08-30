namespace Microsoft.EntityFrameworkCore.Diagnostics;

using System.Linq.Expressions;
using Query;
using Waystone.Monads.Options;

/// <summary>
/// Rewrites the <see cref="Option{T}" /> operations in a query into the column
/// comparisons Entity Framework Core can turn into SQL.
/// </summary>
/// <remarks>
/// Register this through
/// <see cref="DbContextOptionsBuilderExtensions.UseWaystoneOptionQueries" />
/// rather than adding it by hand. Without it, <c>IsSome</c> and a comparison
/// against an <c>Option.Some(…)</c> both throw at translation time, because a
/// value converter tells Entity Framework Core how to store an option and
/// nothing about how to read one back in SQL.
/// <para>
/// This is an interceptor rather than a replacement for the query translation
/// preprocessor. Replacing that service would drop whatever preprocessing your
/// provider does — a relational provider substitutes its own — so the rewrite
/// runs beside the pipeline instead of in place of part of it.
/// </para>
/// </remarks>
internal sealed class OptionQueryInterceptor : IQueryExpressionInterceptor
{
    /// <summary>
    /// Rewrites the options in a query before Entity Framework Core compiles it.
    /// </summary>
    /// <param name="queryExpression">The query about to be compiled.</param>
    /// <param name="eventData">Context for the compilation, which this ignores.</param>
    /// <returns>The query with its option operations rewritten.</returns>
    /// <exception cref="System.InvalidOperationException">
    /// If the query compares an option property against something this rewrite
    /// cannot read, such as a second column. Failing here is better than
    /// emitting a comparison against <c>NULL</c> that matches no row.
    /// </exception>
    public Expression QueryCompilationStarting(
        Expression queryExpression,
        QueryExpressionEventData eventData) =>
        new OptionQueryRewriter().Visit(queryExpression);
}
