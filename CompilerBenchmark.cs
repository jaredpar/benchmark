using Basic.CompilerLog.Util;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable disable

namespace CompilerBenchmark;

[GcServer(true)]
[SimpleJob(RuntimeMoniker.Net481)]
[SimpleJob(RuntimeMoniker.Net70)]
[SimpleJob(RuntimeMoniker.Net80)]
public class CompilationBenchmak
{
    public List<IDisposable> Disposables = new();

    public CompilationData ConsoleData { get; set; }
    public CompilationData RegexData { get; set; }
    public CompilationData CompilerData { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var path = Util.GetCompilerLogDirectory();
        ConsoleData = Open("console.complog", _ => true);
        RegexData = Open("regex.complog", IsRegexBuild);
        CompilerData = Open("compiler.complog", c => c.ProjectFileName == "Microsoft.CodeAnalysis.csproj" && c.TargetFramework == "net6.0" && c.Kind == CompilerCallKind.Regular);

        CompilationData Open(string complogFileName, Func<CompilerCall, bool> predicate)
        {
            var filePath = Path.Combine(path, complogFileName);
            var reader = CompilerLogReader.Create(filePath);
            Disposables.Add(reader);
            var list = reader.ReadAllCompilerCalls(predicate);
            if (list.Count != 1)
            {
                throw new Exception("Invalid predicate");
            }

            var compilationData = reader.ReadCompilationData(list[0]);

            // This is amortized so the next call in the benhcmark will just get a cached value
            _ = compilationData.GetCompilationAfterGenerators();
            return compilationData;
        }

        // Have to differentiate between the ref and real assembly
        static bool IsRegexBuild(CompilerCall c)
        {
            if (c.ProjectFileName == "System.Text.RegularExpressions.csproj" && c.TargetFramework == "net8.0")
            {
                foreach (var arg in c.Arguments)
                {
                    if (arg.StartsWith("/out") && arg.Contains(@"ref\Debug"))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        Disposables.ForEach(x => x.Dispose());
        Disposables.Clear();
    }

    public static void BuildCore(CompilationData compilationData)
    {
        var emitResult = compilationData.EmitToMemory();
        if (!emitResult.Success)
        {
            foreach (var d in emitResult.Diagnostics)
            {
                Console.WriteLine(d.GetMessage());
            }

            throw new Exception("oops");
        }
    }

    [Benchmark]
    public void BuildConsole() => BuildCore(ConsoleData);

    [Benchmark]
    public void BuildRegex() => BuildCore(RegexData);

    [Benchmark]
    public void BuildCompiler() => BuildCore(CompilerData);
}
