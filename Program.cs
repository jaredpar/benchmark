using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using CompilerBenchmark;
using Microsoft.CodeAnalysis.CSharp;

// _ = BenchmarkRunner.Run(typeof(CompilationBenchmak));
#if NETCOREAPP
_ = BenchmarkRunner.Run(typeof(AnalyzerLoadingBenchmark));
#else
Console.WriteLine("why");
#endif

/*
var b = new CompilationBenchmak();
b.Setup();
b.BuildCompiler();
b.Cleanup();
*/
