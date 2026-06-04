using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FiscalOS.Domain.LegislationIngestion;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Define_Legislation_Ingestion_Pipeline_Contracts
{
    // Test-only doubles used to validate the contract shapes and semantics.
    // Not production implementations (no orchestration logic, repository, or
    // source acquisition), per FOS-0034 scope.
    private sealed class AppendTraceStage : ILegislationIngestionStage
    {
        public AppendTraceStage(IngestionStage stage) => Stage = stage;

        public IngestionStage Stage { get; }

        public Task<IngestionContext> ExecuteAsync(IngestionContext context, CancellationToken cancellationToken = default)
        {
            var trace = new List<IngestionTraceEntry>(context.Trace)
            {
                new(Stage, IngestionStatus.Succeeded, default, $"executed {Stage}")
            };

            return Task.FromResult(context with { Trace = trace });
        }
    }

    private sealed class SequentialPipeline : ILegislationIngestionPipeline
    {
        private readonly IReadOnlyList<ILegislationIngestionStage> _stages;

        public SequentialPipeline(params ILegislationIngestionStage[] stages) => _stages = stages;

        public async Task<IngestionResult> RunAsync(IngestionContext context, CancellationToken cancellationToken = default)
        {
            var current = context;
            foreach (var stage in _stages)
            {
                current = await stage.ExecuteAsync(current, cancellationToken);
            }

            return new IngestionResult(current.BatchId, IngestionStatus.Succeeded, current.Trace);
        }
    }

    private static IngestionContext Context() =>
        new(new IngestionBatchId("BATCH-1"),
            Array.Empty<RawLegislationDocument>(),
            Array.Empty<IngestionTraceEntry>());

    [Fact]
    public async Task Stage_executes_and_returns_an_updated_context()
    {
        ILegislationIngestionStage stage = new AppendTraceStage(IngestionStage.Normalization);
        var context = Context();

        var updated = await stage.ExecuteAsync(context);

        Assert.Equal(IngestionStage.Normalization, stage.Stage);
        Assert.Single(updated.Trace);
        // Original context is unchanged (immutable).
        Assert.Empty(context.Trace);
    }

    [Fact]
    public async Task Pipeline_runs_stages_and_produces_a_result()
    {
        ILegislationIngestionPipeline pipeline = new SequentialPipeline(
            new AppendTraceStage(IngestionStage.Normalization),
            new AppendTraceStage(IngestionStage.CuratedPromotion));
        var context = Context();

        var result = await pipeline.RunAsync(context);

        Assert.Equal("BATCH-1", result.BatchId.Value);
        Assert.Equal(IngestionStatus.Succeeded, result.Status);
        Assert.Equal(2, result.Trace.Count);
    }
}
