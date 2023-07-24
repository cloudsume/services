namespace Cloudsume.Services.Controllers;

using System.ComponentModel.DataAnnotations;
using System.Globalization;
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
[Route("pdf")]
public sealed class PdfController : ControllerBase
{
    private readonly PdfOptions options;

    public PdfController(IOptions<PdfOptions> options)
    {
        this.options = options.Value;
    }

    [HttpPost("jobs/render")]
    public async Task<IActionResult> CreateRenderJobAsync([FromQuery, Range(1, 10000)] int? size, CancellationToken cancellationToken = default)
    {
        // Prepare to launch renderer.
        using var workspace = new Workspace();
        var launcher = ProcessLauncher.For(this.options.Renderer)
            .WithWorkingDirectory(workspace.Path)
            .AddArgument("-jpeg");
        var extension = ".jpg";

        if (size != null)
        {
            launcher.AddArguments("-scale-to", size.Value.ToString(CultureInfo.InvariantCulture));
        }

        launcher.AddArgument("-");
        launcher.AddArgument("result");

        // Launch renderer.
        var output = new StringBuilder(0, 1024 * 100);
        var status = await launcher.ExecuteAsync(this.Request.Body, output, cancellationToken);

        if (status != 0)
        {
            return this.File(Encoding.UTF8.GetBytes(output.ToString()), "text/x.command-output", false);
        }

        // Response.
        return new WorkspaceResult(workspace.Detach(), async (context, workspace) =>
        {
            // Set up header.
            var response = context.Response;

            response.ContentType = "application/x-tar";

            // Write results.
            await using var writer = new ArchiveBuilder(response.Body, true);

            for (var i = 1; ; i++)
            {
                // Check if next page exists.
                var name = $"result-{i.ToString(CultureInfo.InvariantCulture)}{extension}";
                var path = Path.Join(workspace.FullName, name);

                if (!System.IO.File.Exists(path))
                {
                    break;
                }

                await using var file = System.IO.File.OpenRead(path);

                // Write entry.
                var item = new UstarItem(PrePosixType.RegularFile, new($"./{(i - 1).ToString(CultureInfo.InvariantCulture)}{extension}"))
                {
                    Content = file,
                    Size = file.Length,
                };

                await writer.WriteItemAsync(item, null, context.RequestAborted);
            }

            await writer.CompleteAsync(context.RequestAborted);
        });
    }
}
