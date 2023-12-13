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

            var name = Path.Combine(path, "complog");
            if (Directory.Exists(name))
            {
                return name;
            }

            path = Path.GetDirectoryName(path);
        } while (true);
    }

    public static string GetCompilerLog(string name)   
    {
        var dir = GetCompilerLogDirectory();
        return Path.Combine(dir, name);
    }

    public static MemoryStream GetCompilerLogStream(string name)   
    {
        var filePath = GetCompilerLog(name);
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var stream = new MemoryStream();
        fileStream.CopyTo(stream);
        return stream;
    }
}
