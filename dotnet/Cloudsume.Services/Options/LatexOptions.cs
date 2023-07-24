namespace Cloudsume.Services.Options;

using System.Collections.Generic;

public sealed class LatexOptions
{
    public Dictionary<string, string> Compilers { get; set; } = new();
}
