namespace Cloudsume.Services.ActionResults;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

internal sealed class WorkspaceResult : ActionResult
{
    private readonly Func<HttpContext, DirectoryInfo, Task> response;

    public WorkspaceResult(DirectoryInfo directory, Func<HttpContext, DirectoryInfo, Task> response)
    {
        this.Directory = directory;
        this.response = response;
    }

    public DirectoryInfo Directory { get; }

    public override async Task ExecuteResultAsync(ActionContext context)
    {
        try
        {
            await this.response(context.HttpContext, this.Directory);
        }
        catch (OperationCanceledException)
        {
            context.HttpContext.Abort();
        }
        finally
        {
            this.Directory.Delete(true);
        }
    }
}
