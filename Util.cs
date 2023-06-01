using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CompilerBenchmark;

internal static class Util
{
    public static string GetCompilerLogDirectory()
    {
        var path = Environment.CurrentDirectory;
        do
        {
            if (path is null)
            {
                throw new Exception("Cannot find compiler resource directory");
            }

            if (File.Exists(Path.Combine(path, "compiler.complog")))
            {
                break;
            }

            path = Path.GetDirectoryName(path);
        } while (true);

        if (path is null)
        {
            throw new Exception("Could not find compiler logs");
        }

        return path;
    }

    public static string GetConsoleCompilerLog()
    {
        var dir = GetCompilerLogDirectory();
        var name = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "console.complog"
            : "console-linux.complog";
        return Path.Combine(dir, name);
    }
}
