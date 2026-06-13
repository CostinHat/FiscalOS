using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
using FiscalOS.Domain.LegalReferences;
using FiscalOS.LegalCore;
using FiscalOS.Runtime.LegalReferences;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Run_Legal_Reference_Resolution_Implementation
{
    private sealed class RecordingStage : ILegalReferenceResolutionStage
    {
        private readonly string _marker;
        private readonly List<string> _executionOrder;

        public RecordingStage(string marker, List<string> executionOrder)
        {
            _marker = marker;
            _executionOrder = executionOrder;
        }

        public string Name => $"record-{_marker}";

        public Task<LegalReferenceResolutionContext> ExecuteAsync(
            LegalReferenceResolutionContext context,
            CancellationToken cancellationToken = default)
        {
            _executionOrder.Add(_marker);
            return Task.FromResult(context with
            {
                Results = context.Results.Concat(new[] { Unresolved(Address(Seg("Marker", _marker))) }).ToList(),
            });
        }
    }

    private sealed class StubResolutionPipeline : ILegalReferenceResolutionPipeline
    {
        private readonly IReadOnlyList<ResolutionResult> _results;

        public StubResolutionPipeline(IReadOnlyList<ResolutionResult> results) => _results = results;

        public LegalReferenceResolutionContext? ReceivedContext { get; private set; }

        public Task<LegalReferenceResolutionContext> RunAsync(
            LegalReferenceResolutionContext context,
            CancellationToken cancellationToken = default)
        {
            ReceivedContext = context;
            return Task.FromResult(context with { Results = _results });
        }
    }

    private sealed class InMemoryLegalReferenceResolutionRepository : ILegalReferenceResolutionRepository
    {
        private readonly Dictionary<LegalReference, ResolutionResult> _store = new();

        public Task StoreAsync(LegalReference query, ResolutionResult result, CancellationToken cancellationToken = default)
        {
            _store[query] = result;
            return Task.CompletedTask;
        }

        public Task<ResolutionResult?> GetAsync(LegalReference query, CancellationToken cancellationToken = default)
            => Task.FromResult<ResolutionResult?>(_store.TryGetValue(query, out var result) ? result : null);

        public Task<IReadOnlyList<ResolutionResult>> GetByStatusAsync(ResolutionStatus status, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ResolutionResult>>(_store.Values.Where(result => result.Status == status).ToList());

        public Task<IReadOnlyList<UnresolvedReference>> GetUnresolvedAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<UnresolvedReference>>(
                _store.Values.Where(result => result.Unresolved is not null).Select(result => result.Unresolved!).ToList());
    }

    private static ReferenceSegment Seg(string kind, string value) => new(kind, value);

    private static LegalReference Address(params ReferenceSegment[] segments) => new(segments);

    private static FullyQualifiedLegalReference Fq(string document, params ReferenceSegment[] segments) =>
        new(new LegalDocumentReference(document), new LegalReference(segments));

    private static ResolutionResult Resolved(FullyQualifiedLegalReference reference) =>
        new(ResolutionStatus.Resolved, new[] { new ResolutionCandidate(reference) }, Unresolved: null);

    private static ResolutionResult Unresolved(LegalReference query) =>
        new(ResolutionStatus.Unresolved, Array.Empty<ResolutionCandidate>(), new UnresolvedReference(query, "test"));

    private static readonly LegalReference QueryParagraph = Address(Seg("Article", "47"), Seg("Paragraph", "3"));
    private static readonly LegalReference QueryArticle48 = Address(Seg("Article", "48"));

    [Fact]
    public async Task Pipeline_runs_configured_stages_in_order()
    {
        var executionOrder = new List<string>();
        var pipeline = new LegalReferenceResolutionPipeline(
            new RecordingStage("first", executionOrder),
            new RecordingStage("second", executionOrder));
        var context = new LegalReferenceResolutionContext(
            new[] { QueryParagraph },
            Array.Empty<ResolutionResult>());

        var result = await pipeline.RunAsync(context);

        Assert.Equal(new[] { "first", "second" }, executionOrder);
        Assert.Equal(2, result.Results.Count);
        Assert.Empty(context.Results);
    }

    [Fact]
    public async Task Repository_stage_uses_stored_resolution_when_available()
    {
        var repository = new InMemoryLegalReferenceResolutionRepository();
        var resolved = Resolved(Fq("Legea 227/2015", Seg("Article", "47"), Seg("Paragraph", "3")));
        await repository.StoreAsync(QueryParagraph, resolved);
        var stage = new RepositoryLegalReferenceResolutionStage(repository);
        var context = new LegalReferenceResolutionContext(
            new[] { QueryParagraph },
            Array.Empty<ResolutionResult>());

        var result = await stage.ExecuteAsync(context);

        Assert.Equal("repository-resolution", stage.Name);
        Assert.Single(result.Results);
        Assert.Same(resolved, result.Results[0]);
        Assert.Empty(context.Results);
    }

    [Fact]
    public async Task Repository_stage_marks_missing_resolution_unresolved()
    {
        var stage = new RepositoryLegalReferenceResolutionStage(new InMemoryLegalReferenceResolutionRepository());
        var context = new LegalReferenceResolutionContext(
            new[] { QueryArticle48 },
            Array.Empty<ResolutionResult>());

        var result = await stage.ExecuteAsync(context);

        Assert.Single(result.Results);
        Assert.True(result.Results[0].IsUnresolved);
        var unresolved = result.Results[0].Unresolved;
        Assert.NotNull(unresolved);
        Assert.Equal(QueryArticle48, unresolved.Query);
        Assert.Equal("No stored resolution found.", unresolved.Reason);
    }

    [Fact]
    public async Task Engine_delegates_to_pipeline_and_returns_processed_results()
    {
        var resolved = Resolved(Fq("Legea 227/2015", Seg("Article", "47"), Seg("Paragraph", "3")));
        var pipeline = new StubResolutionPipeline(new[] { resolved });
        var engine = new LegalReferenceResolutionEngine(pipeline);

        var results = await engine.ResolveAsync(new[] { QueryParagraph });

        Assert.Single(results);
        Assert.Same(resolved, results[0]);
        Assert.NotNull(pipeline.ReceivedContext);
        Assert.Single(pipeline.ReceivedContext!.Queries);
        Assert.Empty(pipeline.ReceivedContext.Results);
    }
}
