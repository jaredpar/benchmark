using Basic.CompilerLog.Util;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.FlowAnalysis;
#nullable disable

namespace CompilerBenchmark;

[GcServer(true)]
public class RefAndImplTimes
{
    public List<IDisposable> Disposables = new();

    public MemoryStream ConsoleComplogStream { get; set; }
    public MemoryStream CompilerComplogStream { get; set; }

    public enum Kind
    {
        Impl,
        Ref,
        Both
    }

    [GlobalSetup]
    public void Setup()
    {
        ConsoleComplogStream = Util.GetCompilerLogStream("console.complog");
        CompilerComplogStream = Util.GetCompilerLogStream("compiler.complog");
    }

    public static void Build(MemoryStream stream, Func<CompilerCall, bool> predicate, Kind kind)
    {
        stream.Position = 0;
        using var reader = CompilerLogReader.Create(stream, leaveOpen: true, BasicAnalyzerHostOptions.None);
        var compilerCall = reader
            .ReadAllCompilerCalls(predicate)
            .Single();
        var compilationData = reader.ReadCompilationData(compilerCall);
        var compilation = compilationData.GetCompilationAfterGenerators();
        var emitOptions = compilationData
            .EmitOptions
            .WithDebugInformationFormat(DebugInformationFormat.Pdb);

        MemoryStream assemblyStream = new MemoryStream();
        MemoryStream metadataStream = null;

        switch (kind)
        {
            case Kind.Impl:
                metadataStream = new MemoryStream();
                emitOptions = emitOptions.WithEmitMetadataOnly(false);
                break;
            case Kind.Ref:
                emitOptions = emitOptions.WithEmitMetadataOnly(true);
                break;
            case Kind.Both:
                emitOptions = emitOptions.WithEmitMetadataOnly(false);
                metadataStream = new MemoryStream();
                break;
            default:
                throw new Exception("oops");
        }

        var result = compilation.Emit(
            assemblyStream,
            pdbStream: null,
            xmlDocumentationStream: null,
            compilationData.EmitData.Win32ResourceStream,
            compilationData.EmitData.Resources,
            emitOptions,
            debugEntryPoint: null,
            sourceLinkStream: null,
            embeddedTexts: null,
            metadataPEStream: metadataStream);
        if (!result.Success)
        {
            throw new Exception("compilation failed");
        }
    }

    public static bool IsCompilerLog(CompilerCall cc) => cc.TargetFramework == "net7.0" && cc.ProjectFileName == "Microsoft.CodeAnalysis.csproj";

    [Benchmark]
    public void BuildConsoleRef() => Build(ConsoleComplogStream, _ => true, Kind.Ref);

    [Benchmark]
    public void BuildConsoleImpl() => Build(ConsoleComplogStream, _ => true, Kind.Impl);

    [Benchmark]
    public void BuildConsoleBoth() => Build(ConsoleComplogStream, _ => true, Kind.Both);

    [Benchmark]
    public void BuildCompilerRef() => Build(CompilerComplogStream, IsCompilerLog, Kind.Ref);

    [Benchmark]
    public void BuildCompilerImpl() => Build(CompilerComplogStream, IsCompilerLog, Kind.Impl);

    [Benchmark]
    public void BuildCompilerBoth() => Build(CompilerComplogStream, IsCompilerLog, Kind.Both);
}
