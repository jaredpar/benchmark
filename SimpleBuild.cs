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
public class SimpleBuild
{
    public MemoryStream ConsoleComplogStream { get; set; }
    public MemoryStream CompilerComplogStream { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        ConsoleComplogStream = Util.GetCompilerLogStream("console.complog");
        CompilerComplogStream = Util.GetCompilerLogStream("compiler.complog");
    }

    public static void Build(MemoryStream stream, Func<CompilerCall, bool> predicate)
    {
        stream.Position = 0;
        using var reader = CompilerLogReader.Create(stream, leaveOpen: true, BasicAnalyzerHostOptions.None);
        var compilerCall = reader
            .ReadAllCompilerCalls(predicate)
            .Single();
        var compilationData = reader.ReadCompilationData(compilerCall);
        var emitResult = compilationData.EmitToMemory();
        if (!emitResult.Success)
        {
            throw new Exception("oops");
        }
    }

    public static bool IsCompilerLog(CompilerCall cc) => cc.TargetFramework == "net7.0" && cc.ProjectFileName == "Microsoft.CodeAnalysis.csproj";

    [Benchmark]
    public void BuildConsole() => Build(ConsoleComplogStream, _ => true);

    [Benchmark]
    public void BuildCompiler() => Build(CompilerComplogStream, IsCompilerLog);
}
