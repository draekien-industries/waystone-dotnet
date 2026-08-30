namespace Waystone.DocSnippets;

using System;
using System.IO;

internal sealed class TemporaryDirectory : IDisposable
{
    public TemporaryDirectory() =>
        Path = Directory
              .CreateDirectory(
                   System.IO.Path.Combine(
                       System.IO.Path.GetTempPath(),
                       "waystone-docsnippets-" + Guid.NewGuid().ToString("n")))
              .FullName;

    public string Path { get; }

    public string Write(string relativePath, string content)
    {
        string full = System.IO.Path.Combine(Path, relativePath);
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(full)!);
        File.WriteAllText(full, content);

        return full;
    }

    public string Read(string relativePath) =>
        File.ReadAllText(System.IO.Path.Combine(Path, relativePath));

    public void Dispose()
    {
        try
        {
            Directory.Delete(Path, true);
        }
        catch (IOException)
        {
        }
    }
}
