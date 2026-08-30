namespace Microsoft.EntityFrameworkCore;

using System;
using ChangeTracking;
using Metadata;
using Shouldly;
using Storage.ValueConversion;
using Waystone.Monads.Options;
using Xunit;

public class ModelBuilderExtensionsTests
{
    [Fact]
    public void GivenANullBuilder_ThenThrow() =>
        Should.Throw<ArgumentNullException>(
            () => ((ModelBuilder)null!).UseWaystoneOptionConversions());

    [Fact]
    public void WhenSweeping_ThenReturnTheSameBuilder()
    {
        ModelBuilder builder = new();

        builder.UseWaystoneOptionConversions().ShouldBeSameAs(builder);
    }

    [Fact]
    public void GivenAReferenceTypeOption_ThenUseTheReferenceTypeConverter() =>
        Property(nameof(Person.Nickname))
           .GetValueConverter()
           .ShouldBeOfType<ReferenceTypeOptionConverter<string>>();

    [Fact]
    public void GivenAValueTypeOption_ThenUseTheValueTypeConverter() =>
        Property(nameof(Person.Age))
           .GetValueConverter()
           .ShouldBeOfType<ValueTypeOptionConverter<int>>();

    [Fact]
    public void GivenAnOptionProperty_ThenUseTheOptionComparer() =>
        Property(nameof(Person.Nickname))
           .GetValueComparer()
           .ShouldBeOfType<OptionValueComparer<string>>();

    [Fact]
    public void GivenAnOptionProperty_ThenMarkItOptional() =>
        Property(nameof(Person.Age)).IsNullable.ShouldBeTrue();

    [Fact]
    public void GivenAnOptionProperty_ThenGiveItTheHeldTypesColumn() =>
        Property(nameof(Person.Age))
           .GetValueConverter()!
           .ProviderClrType.ShouldBe(typeof(int?));

    [Fact]
    public void GivenAPropertyThatIsNotAnOption_ThenLeaveItAlone()
    {
        IReadOnlyProperty name = Property(nameof(Person.Name));

        name.GetValueConverter().ShouldBeNull();
        name.IsNullable.ShouldBeFalse();
    }

    [Fact]
    public void GivenAnOptionIndexer_ThenLeaveItAlone() =>
        SweptModel.FindEntityType(typeof(Person))!
                  .FindProperty("Item")
                  .ShouldBeNull();

    [Fact]
    public void GivenAPropertyAlreadyConfigured_ThenKeepTheExistingConverter()
    {
        Build<PreconfiguredContext>()
           .FindEntityType(typeof(Person))!
           .FindProperty(nameof(Person.Nickname))!
           .GetValueConverter()
           .ShouldBeOfType<CustomConverter>();
    }

    [Fact]
    public void WhenSweptTwice_ThenTheModelIsUnchanged()
    {
        Build<TwiceSweptContext>()
           .FindEntityType(typeof(Person))!
           .FindProperty(nameof(Person.Age))!
           .GetValueConverter()
           .ShouldBeOfType<ValueTypeOptionConverter<int>>();
    }

    private static readonly IModel SweptModel = Build<PeopleContext>();

    private static IModel Build<TContext>()
        where TContext : PeopleContext
    {
        using PeopleContext context = (TContext)Activator.CreateInstance(
            typeof(TContext),
            new DbContextOptionsBuilder<TContext>()
               .UseSqlite("DataSource=:memory:")
               .Options)!;

        return context.Model;
    }

    private static IReadOnlyProperty Property(string name) =>
        SweptModel.FindEntityType(typeof(Person))!.FindProperty(name)!;

    private sealed class PreconfiguredContext : PeopleContext
    {
        public PreconfiguredContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>()
                        .Property(person => person.Nickname)
                        .HasConversion(
                            new CustomConverter(),
                            new OptionValueComparer<string>())
                        .IsRequired(false);

            base.OnModelCreating(modelBuilder);
        }
    }

    private sealed class TwiceSweptContext : PeopleContext
    {
        public TwiceSweptContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.UseWaystoneOptionConversions();
        }
    }

    private sealed class CustomConverter : ValueConverter<Option<string>, string?>
    {
        public CustomConverter()
            : base(
                option => option.UnwrapOrDefault(),
                value => value == null
                    ? Option.None<string>()
                    : Option.Some(value))
        {
        }
    }
}
