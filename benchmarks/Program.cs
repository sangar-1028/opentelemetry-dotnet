﻿using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Benchmarks.Tracing;
using BenchmarkSdk.Tracing;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Trace.Configuration;
using OpenTelemetry.Trace.Sampler;

namespace BenchmarkSdk
{
    [MemoryDiagnoser]
    public class OpenTelemetrySdkBenchmarks
    {
        private readonly ITracer alwaysSampleTracer;
        private readonly ITracer neverSampleTracer;
        private readonly ITracer noopTracer;

        public OpenTelemetrySdkBenchmarks()
        {
            alwaysSampleTracer = new Tracer(new NoopProcessor(), new TracerConfiguration(Samplers.AlwaysSample), Resource.Empty);
            neverSampleTracer = new Tracer(new NoopProcessor(), new TracerConfiguration(Samplers.NeverSample), Resource.Empty);
            noopTracer = NoopTracer.Instance;
        }

        [Benchmark]
        public void CreateSpan_Sampled() => SpanCreationScenarios.CreateSpan(alwaysSampleTracer);

        [Benchmark]
        public void CreateSpan_Attributes_Sampled() => SpanCreationScenarios.CreateSpan_Attributes(alwaysSampleTracer);

        [Benchmark]
        public void CreateSpan_Propagate_Sampled() => SpanCreationScenarios.CreateSpan_Propagate(alwaysSampleTracer);

        [Benchmark]
        public void CreateSpan_Attributes_NotSampled() => SpanCreationScenarios.CreateSpan_Attributes(neverSampleTracer);

        [Benchmark(Baseline = true)]
        public void CreateSpan_Noop() => SpanCreationScenarios.CreateSpan(noopTracer);

        [Benchmark]
        public void CreateSpan_Attributes_Noop() => SpanCreationScenarios.CreateSpan_Attributes(noopTracer);

        [Benchmark]
        public void CreateSpan_Propagate_Noop() => SpanCreationScenarios.CreateSpan_Propagate(noopTracer);

        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<OpenTelemetrySdkBenchmarks>();
        }
    }
}
