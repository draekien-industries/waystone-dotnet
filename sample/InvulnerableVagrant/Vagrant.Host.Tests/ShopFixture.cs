namespace Vagrant.Host.Tests;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Vagrant.Host.Infrastructure;

/// <summary>A shop of its own, on its own SQLite file, seeded at startup.</summary>
/// <remarks>
/// <para>
/// A file rather than an in-memory database. The host runs
/// <see cref="ShopDatabase.OpenAsync" /> against the real provider, so the value
/// converters, the complex property and the seed all have to work for a test to reach an
/// endpoint at all — which is most of what these tests are for.
/// </para>
/// <para>
/// This wraps <see cref="WebApplicationFactory{TEntryPoint}" /> rather than deriving
/// from it. That type implements both <see cref="IDisposable" /> and
/// <see cref="IAsyncDisposable" />, and xUnit v3 fails a class fixture that implements
/// both — as a cleanup failure after every test has already passed.
/// </para>
/// </remarks>
public sealed class ShopFixture : IAsyncDisposable
{
    private readonly string _file = Path.Combine(
        Path.GetTempPath(),
        $"vagrant-{Guid.CreateVersion7():N}.db");

    private readonly ShopFactory _shop;

    /// <summary>Opens a shop no other test shares.</summary>
    public ShopFixture()
    {
        _shop = new ShopFactory(_file);
    }

    /// <summary>Opens a connection to this shop.</summary>
    /// <returns>A client the caller disposes.</returns>
    public HttpClient CreateClient() => _shop.CreateClient();

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await _shop.DisposeAsync().ConfigureAwait(false);

        // The pool holds the files open after the host has gone, and Windows refuses to
        // delete a file with a handle on it.
        SqliteConnection.ClearAllPools();

        // A glob rather than a list. Each bounded context keeps its own file beside the
        // configured one, so the set to delete grows every time a context is added and a
        // named list would silently leave the newest behind.
        string directory = Path.GetDirectoryName(_file)!;
        string stem = Path.GetFileNameWithoutExtension(_file);

        foreach (string leftover in Directory.EnumerateFiles(directory, $"{stem}*"))
        {
            File.Delete(leftover);
        }
    }

    private sealed class ShopFactory(string file) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting(
                $"ConnectionStrings:{ShopDatabase.ConnectionName}",
                $"Data Source={file}");
        }
    }
}
