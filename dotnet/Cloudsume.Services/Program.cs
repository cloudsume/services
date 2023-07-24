namespace Cloudsume.Services;

using Cloudsume.Services.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Application services.
        builder.Services.AddOptions<LatexOptions>().BindConfiguration("Latex");
        builder.Services.AddOptions<PdfOptions>().BindConfiguration("Pdf");

        // ASP.NET Core services.
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        builder.Services.AddHealthChecks();
        builder.Services.AddControllers();

        // Configure the HTTP request pipeline.
        var app = builder.Build();

        app.UseForwardedHeaders();
        app.MapHealthChecks("/health");
        app.MapControllers();

        // Run server.
        app.Run();
    }
}
