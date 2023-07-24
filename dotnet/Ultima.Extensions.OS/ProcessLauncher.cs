namespace Ultima.Extensions.OS;

using System.Diagnostics;
using System.Text;

public sealed class ProcessLauncher
{
    private ProcessLauncher(string file)
    {
        this.File = file;
        this.Arguments = new List<string>();
    }

    public string File { get; set; }

    public string? WorkingDirectory { get; set; }

    public ICollection<string> Arguments { get; set; }

    public static ProcessLauncher For(string file) => new(file);

    public ProcessLauncher WithWorkingDirectory(string? path)
    {
        this.WorkingDirectory = path;
        return this;
    }

    public ProcessLauncher AddArgument(string argument)
    {
        this.Arguments.Add(argument);
        return this;
    }

    public ProcessLauncher AddArguments(params string[] arguments)
    {
        foreach (var argument in arguments)
        {
            this.Arguments.Add(argument);
        }

        return this;
    }

    public async Task<int> ExecuteAsync(Stream input, StringBuilder output, CancellationToken cancellationToken = default)
    {
        // Prepare to launch.
        using var process = new Process();
        var mutex = new object();
        var full = false;

        process.StartInfo.FileName = this.File;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.RedirectStandardInput = true;
        process.OutputDataReceived += OnOutput;
        process.ErrorDataReceived += OnOutput;

        if (this.WorkingDirectory != null)
        {
            process.StartInfo.WorkingDirectory = this.WorkingDirectory;
        }

        foreach (var argument in this.Arguments)
        {
            process.StartInfo.ArgumentList.Add(argument);
        }

        // Launch.
        process.Start();

        try
        {
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Stream input.
            await input.CopyToAsync(process.StandardInput.BaseStream, cancellationToken);
            process.StandardInput.Close();

            // Wait til exit.
            await process.WaitForExitAsync(cancellationToken);
        }
        catch
        {
            process.Kill(true);
            await process.WaitForExitAsync();
            throw;
        }

        return process.ExitCode;

        void OnOutput(object sender, DataReceivedEventArgs e)
        {
            lock (mutex)
            {
                if (full || e.Data == null)
                {
                    return;
                }

                try
                {
                    output.AppendLine(e.Data);
                }
                catch (ArgumentOutOfRangeException)
                {
                    full = true;
                }
            }
        }
    }
}
