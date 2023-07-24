namespace Cloudsume.Services.Controllers;

using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cloudsume.Services.ActionResults;
using Cloudsume.Services.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TapeArchive;
using Ultima.Extensions.OS;

[ApiController]
[Route("latex")]
public sealed class LatexController : ControllerBase
{
    private readonly LatexOptions options;

    public LatexController(IOptions<LatexOptions> options)
    {
        this.options = options.Value;
    }

    [HttpPost("jobs")]
    public async Task<IActionResult> CreateJobAsync(CancellationToken cancellationToken = default)
    {
        using var workspace = new Workspace();

        // Extract request.
        await using var request = new TapeArchive(this.Request.Body, true);

        await using (var files = request.ReadAsync(cancellationToken).GetAsyncEnumerator())
        {
            for (; ;)
            {
                try
                {
                    if (!await files.MoveNextAsync())
                    {
                        break;
                    }
                }
                catch (IOException)
                {
                    return this.BadRequest();
                }

                var file = files.Current;
                var path = file.Name.ToFileSystemPath(workspace.Path);

                if (file.IsRegularFile)
                {
                    await using var local = System.IO.File.Create(path);
                    await file.Content.CopyToAsync(local, cancellationToken);
                }
                else if (file.IsDirectory)
                {
                    Directory.CreateDirectory(path);
                }
                else
                {
                    return this.BadRequest();
                }
            }
        }

        // Launch LaTeX compiler.
        var results = workspace.CreateDirectory("output");
        var output = new StringBuilder(100000, 1024 * 1024);
        var status = await ProcessLauncher.For(this.options.Compilers["Xetex"])
            .AddArgument("-halt-on-error")
            .AddArgument("-output-directory=" + results.FullName)
            .AddArgument(workspace.GetPathFor("main.tex"))
            .WithWorkingDirectory(workspace.Path)
            .ExecuteAsync(Stream.Null, output, cancellationToken);

        if (status != 0)
        {
            return this.File(Encoding.UTF8.GetBytes(output.ToString()), "text/x.command-output", false);
        }

        // Send PDF.
        return new WorkspaceResult(workspace.Detach(), async (context, workspace) =>
        {
            // Open PDF.
            var path = Path.Join(results.FullName, "main.pdf");
            await using var file = System.IO.File.OpenRead(path);

            // Set response headers.
            var response = context.Response;

            response.ContentType = "application/pdf";
            response.ContentLength = file.Length;

            // Write body.
            await file.CopyToAsync(response.Body, context.RequestAborted);
        });
    }
}
