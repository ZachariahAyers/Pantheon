using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using Pantheon.Demo;

namespace Pantheon.Browser;

/// <summary>The browser's entry points into the .NET geometry adapter.</summary>
[SupportedOSPlatform("browser")]
public static partial class BrowserAdapter
{
    [JSExport]
    public static string Generate(string requestJson) => DemoGenerator.GenerateJson(requestJson);

    [JSExport]
    public static string Dxf(string requestJson) => DemoGenerator.GenerateDxf(requestJson);
}

public static class Program
{
    public static void Main()
    {
    }
}
