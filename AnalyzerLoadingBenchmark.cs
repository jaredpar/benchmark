#if NETCOREAPP
using Basic.CompilerLog.Util;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace CompilerBenchmark;
#nullable disable

[GcServer(true)]
[SimpleJob(RuntimeMoniker.Net70)]
public class AnalyzerLoadingBenchmark
{
    internal string AnalyzerDirectory { get; set; }
    internal List<string> AnalyzerFiles { get; set; }
    internal List<byte[]> AnalyzerBytes { get; set; }
    internal List<string> TempDirectories { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        AnalyzerDirectory = Path.Combine(Path.GetTempPath(), nameof(AnalyzerLoadingBenchmark), Guid.NewGuid().ToString());
        AnalyzerFiles = new();
        AnalyzerBytes = new();
        TempDirectories = new()
        {
            AnalyzerDirectory
        };

        Directory.CreateDirectory(AnalyzerDirectory);
        using var reader = CompilerLogReader.Create(Path.Combine(Util.GetCompilerLogDirectory(), "console.complog"));
        var call = reader.ReadCompilerCall(0);
        foreach (var tuple in reader.ReadAnalyzerFileInfo(call))
        {
            var filePath = Path.Combine(AnalyzerDirectory, Path.GetFileName(tuple.FilePath));
            File.WriteAllBytes(filePath, tuple.ImageBytes);
            AnalyzerFiles.Add(filePath);
            AnalyzerBytes.Add(tuple.ImageBytes);
        }
    }

    [Benchmark]
    public void LoadViaPath()
    {
        var context = new AssemblyLoadContext(name: "LoadViaPath", isCollectible: true);
        foreach (var filePath in AnalyzerFiles)
        {
            context.LoadFromAssemblyPath(filePath);
        }
        context.Unload();
    }

    [Benchmark]
    public void LoadViaPathShadow()
    {
        var dest = Path.Combine(Path.GetTempPath(), nameof(AnalyzerLoadingBenchmark), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dest);
        TempDirectories.Add(dest);

        var context = new AssemblyLoadContext(name: "LoadViaPath", isCollectible: true);
        foreach (var filePath in AnalyzerFiles)
        {
            var destFilePath = Path.Combine(dest, Path.GetFileName(filePath));
            File.Copy(filePath, destFilePath);
            context.LoadFromAssemblyPath(destFilePath);
        }
        context.Unload();
    }

    [Benchmark]
    public void LoadViaStream()
    {
        var context = new AssemblyLoadContext(name: "LoadViaStream", isCollectible: true);
        foreach (var array in AnalyzerBytes)
        {
            var stream = new MemoryStream(array, 0, count: array.Length, writable: false, publiclyVisible: true);
            context.LoadFromStream(stream);
        }
        context.Unload();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        foreach (var dir in TempDirectories)
        {
            try
            {
                Directory.Delete(dir, recursive: true);
            }
            catch { }
        }
    }
}
#endif
