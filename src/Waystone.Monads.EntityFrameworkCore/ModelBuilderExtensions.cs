namespace Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ChangeTracking;
using Metadata;
using Storage.ValueConversion;
using Waystone.Monads.Options;

/// <summary>
/// Adds the model-wide registration of the <see cref="Option{T}" /> converters
/// to <see cref="ModelBuilder" />.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Maps every <see cref="Option{T}" /> property in the model onto a single
    /// nullable column, whatever the type held.
    /// </summary>
    /// <remarks>
    /// Call this from <c>OnModelCreating</c>, after
    /// <c>base.OnModelCreating(modelBuilder)</c> and after any entity
    /// configuration of your own, since it reads the entity types already in the
    /// model. Calling it twice is harmless; the second sweep reapplies the same
    /// configuration.
    /// <para>
    /// Prefer this over <c>ConfigureConventions</c>, which cannot express the
    /// open generic <c>Option&lt;&gt;</c> — its <c>Properties&lt;T&gt;()</c>
    /// takes closed types only, so it would need one line per <c>T</c> you use
    /// and would silently miss any you forget. This sweep finds them all.
    /// </para>
    /// <para>
    /// It also marks each converted property optional. Without that the provider
    /// emits a <c>NOT NULL</c> column, because the model property is a
    /// non-nullable reference type, and saving a none option then fails at the
    /// database.
    /// </para>
    /// <para>
    /// A property already configured with a converter of your own is left alone,
    /// so a hand-written registration survives the sweep whichever order the two
    /// run in.
    /// </para>
    /// </remarks>
    /// <param name="modelBuilder">The model being built.</param>
    /// <returns>The same builder, so calls can be chained.</returns>
    /// <example>
    /// <code>
    /// protected override void OnModelCreating(ModelBuilder modelBuilder)
    /// {
    ///     base.OnModelCreating(modelBuilder);
    ///
    ///     modelBuilder.UseWaystoneOptionConversions();
    /// }
    /// </code>
    /// </example>
    public static ModelBuilder UseWaystoneOptionConversions(
        this ModelBuilder modelBuilder)
    {
        if (modelBuilder is null)
        {
            throw new ArgumentNullException(nameof(modelBuilder));
        }

        Dictionary<Type, Conversion> conversions = [];

        foreach (IMutableEntityType entityType in
                 modelBuilder.Model.GetEntityTypes().ToList())
        {
            Type clrType = entityType.ClrType;

            foreach (PropertyInfo property in clrType.GetProperties(
                         BindingFlags.Public | BindingFlags.Instance))
            {
                if (!IsOption(property.PropertyType)
                 || property.GetIndexParameters().Length > 0
                 || entityType.FindProperty(property.Name)?.GetValueConverter()
                 is not null)
                {
                    continue;
                }

                Type held = property.PropertyType.GetGenericArguments()[0];

                if (!conversions.TryGetValue(held, out Conversion conversion))
                {
                    conversion = new Conversion(
                        CreateConverter(held),
                        CreateComparer(held));
                    conversions.Add(held, conversion);
                }

                modelBuilder.Entity(clrType)
                            .Property(property.Name)
                            .HasConversion(conversion.Converter, conversion.Comparer)
                            .IsRequired(false);
            }
        }

        return modelBuilder;
    }

    private static bool IsOption(Type type) =>
        type.IsGenericType
     && type.GetGenericTypeDefinition() == typeof(Option<>);

    private static ValueConverter CreateConverter(Type held)
    {
        Type converter = held.IsValueType
            ? typeof(ValueTypeOptionConverter<>)
            : typeof(ReferenceTypeOptionConverter<>);

        return (ValueConverter)Activator.CreateInstance(
            converter.MakeGenericType(held))!;
    }

    private static ValueComparer CreateComparer(Type held) =>
        (ValueComparer)Activator.CreateInstance(
            typeof(OptionValueComparer<>).MakeGenericType(held))!;

    private readonly record struct Conversion(
        ValueConverter Converter,
        ValueComparer Comparer);
}
