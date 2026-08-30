namespace Microsoft.EntityFrameworkCore;

using System;
using Microsoft.Data.Sqlite;
using Waystone.Monads.Options;

public sealed class Person
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public Option<string> Nickname { get; set; } = Option.None<string>();

    public Option<int> Age { get; set; } = Option.None<int>();

    public Option<string> this[int index] => Option.None<string>();
}

public class PeopleContext : DbContext
{
    public PeopleContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<Person> People => Set<Person>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.UseWaystoneOptionConversions();
    }
}

public sealed class SqliteDatabase : IDisposable
{
    private readonly SqliteConnection connection;

    public SqliteDatabase()
    {
        connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        using PeopleContext context = Create();
        context.Database.EnsureCreated();
    }

    public PeopleContext Create() =>
        new(new DbContextOptionsBuilder<PeopleContext>()
            .UseSqlite(connection)
            .UseWaystoneOptionQueries()
            .Options);

    public string ReadColumnType(string column, int id)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            $"select typeof(\"{column}\") from \"People\" where \"Id\" = $id";
        command.Parameters.AddWithValue("$id", id);
        return (string)command.ExecuteScalar()!;
    }

    public void Dispose() => connection.Dispose();
}
