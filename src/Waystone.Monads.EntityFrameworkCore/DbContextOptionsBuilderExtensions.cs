namespace Microsoft.EntityFrameworkCore;

using System;
using Diagnostics;
using Waystone.Monads.Options;

/// <summary>
/// Adds the query support for <see cref="Option{T}" /> properties to
/// <see cref="DbContextOptionsBuilder" />.
/// </summary>
public static class DbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Teaches queries to read an <see cref="Option{T}" /> property, so
    /// <c>IsSome</c> and a comparison against an inline option translate to SQL
    /// instead of throwing.
    /// </summary>
    /// <remarks>
    /// This is the companion to
    /// <see cref="ModelBuilderExtensions.UseWaystoneOptionConversions" />, and
    /// the two are separate calls because they configure different things — one
    /// the model, the other the services. Storing an option works without this
    /// call. Querying one mostly does not.
    /// <para>
    /// Four forms translate once this is on:
    /// <c>option.IsSome</c> and <c>option.IsNone</c> become
    /// <c>IS NOT NULL</c> and <c>IS NULL</c>; a comparison against an inline
    /// <c>Option.Some(value)</c> becomes a comparison against the value, and one
    /// against an inline <c>Option.None&lt;T&gt;()</c> becomes <c>IS NULL</c>.
    /// </para>
    /// <para>
    /// A captured option works too. The rewrite runs before Entity Framework
    /// Core turns a captured value into a SQL parameter, so it can still read
    /// the option and emit the right comparison — a captured none becomes
    /// <c>IS NULL</c> rather than a comparison against <c>NULL</c> that would
    /// match no row.
    /// </para>
    /// <para>
    /// This is added as an interceptor. It does not replace the query
    /// translation preprocessor, which would discard whatever preprocessing
    /// your provider does.
    /// </para>
    /// </remarks>
    /// <param name="optionsBuilder">The options being built.</param>
    /// <returns>The same builder, so calls can be chained.</returns>
    /// <example>
    /// <code>
    /// services.AddDbContext&lt;AppDbContext&gt;(
    ///     options => options.UseSqlite(connectionString)
    ///                       .UseWaystoneOptionQueries());
    /// </code>
    /// </example>
    public static DbContextOptionsBuilder UseWaystoneOptionQueries(
        this DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder is null)
        {
            throw new ArgumentNullException(nameof(optionsBuilder));
        }

        return optionsBuilder.AddInterceptors(new OptionQueryInterceptor());
    }
}
