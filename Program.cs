using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using CompilerBenchmark;
using Microsoft.CodeAnalysis.CSharp;

// _ = BenchmarkRunner.Run(typeof(CompilationBenchmak));
// _ = BenchmarkRunner.Run(typeof(RefAndImplTimes).Assembly);
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

/*
var b = new CompilationBenchmak();
b.Setup();
b.BuildCompiler();
b.Cleanup();
*/
