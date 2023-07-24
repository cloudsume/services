namespace Cloudsume.Services;

using System;
using System.IO;

internal sealed class Workspace : IDisposable
{
    private readonly DirectoryInfo directory;
    private bool detached;
    private bool disposed;

    public Workspace()
    {
        var path = System.IO.Path.Join(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());

        this.directory = Directory.CreateDirectory(path);
    }

    public string Path => this.directory.FullName;

    public void Dispose()
    {
        if (this.disposed)
        {
            return;
        }

        if (!this.detached)
        {
            this.directory.Delete(true);
        }

        this.disposed = true;
    }

    public DirectoryInfo Detach()
    {
        this.detached = true;

        return this.directory;
    }

    public DirectoryInfo CreateDirectory(string first, params string[] remaining) => Directory.CreateDirectory(this.GetPathFor(first, remaining));

    public string GetPathFor(string first, params string[] remaining)
    {
        // We required the first part to be non-empty to prevent the result end up with a workspace itself.
        if (first.Length == 0)
        {
            throw new ArgumentException("The value is empty.", nameof(first));
        }

        var args = new string[remaining.Length + 2];
        var i = 0;

        args[i++] = this.Path;
        args[i++] = first;

        foreach (var part in remaining)
        {
            args[i++] = part;
        }

        return System.IO.Path.Join(args);
    }
}
