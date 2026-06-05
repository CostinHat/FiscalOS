using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FiscalOS.Domain.LegalReferences;
using FiscalOS.Runtime.LegalReferences;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Run_Legal_Reference_Resolution_Provenance_Implementation
{
    private sealed class RecordingProvenanceStage : ILegalReferenceResolutionProvenanceStage
    {
        private readonly string _marker;
        private readonly List<string> _executionOrder;

        public RecordingProvenanceStage(string marker, List<string> executionOrder)
        {
            _marker = marker;
            _executionOrder = executionOrder;
        }

        public string Name => $"record-provenance-{_marker}";

        public Task<LegalReferenceResolutionProvenanceContext> ExecuteAsync(
            LegalReferenceResolutionProvenanceContext context,
            CancellationToken cancellationToken = default)
        {
            _executionOrder.Add(_marker);
            return Task.FromResult(context with
            {
                Provenances = context.Provenances.Concat(new[] { Provenance(Address(Seg("Marker", _marker)), Chain(Step("test", _marker))) }).ToList(),
            });
        }
    }

    private sealed class StubProvenancePipeline : ILegalReferenceResolutionProvenancePipeline
    {
        private readonly IReadOnlyList<ResolutionProvenance> _provenances;

        public StubProvenancePipeline(IReadOnlyList<ResolutionProvenance> provenances) => _provenances = provenances;

        public LegalReferenceResolutionProvenanceContext? ReceivedContext { get; private set; }

        public Task<LegalReferenceResolutionProvenanceContext> RunAsync(
            LegalReferenceResolutionProvenanceContext context,
            CancellationToken cancellationToken = default)
        {
            ReceivedContext = context;
            return Task.FromResult(context with { Provenances = _provenances });
        }
    }

    private sealed class InMemoryLegalReferenceResolutionProvenanceRepository : ILegalReferenceResolutionProvenanceRepository
    {
        private readonly Dictionary<LegalReference, ResolutionProvenance> _store = new();

        public Task StoreAsync(ResolutionProvenance provenance, CancellationToken cancellationToken = default)
        {
            _store[provenance.Query] = provenance;
            return Task.CompletedTask;
        }

        public Task<ResolutionProvenance?> GetAsync(LegalReference query, CancellationToken cancellationToken = default)
            => Task.FromResult<ResolutionProvenance?>(_store.TryGetValue(query, out var provenance) ? provenance : null);

        public Task<IReadOnlyList<ResolutionProvenance>> GetBySourceAsync(string source, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ResolutionProvenance>>(
                _store.Values.Where(provenance => provenance.Chain.Steps.Any(step => step.Source.Value == source)).ToList());

        public Task<IReadOnlyList<ProvenanceChain>> GetChainsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ProvenanceChain>>(_store.Values.Select(provenance => provenance.Chain).ToList());
    }

    private static ReferenceSegment Seg(string kind, string value) => new(kind, value);

    private static LegalReference Address(params ReferenceSegment[] segments) => new(segments);

    private static ProvenanceStep Step(string source, string description) =>
        new(new ProvenanceSource(source), description);

    private static ProvenanceChain Chain(params ProvenanceStep[] steps) => new(steps);

    private static ResolutionProvenance Provenance(LegalReference query, ProvenanceChain chain) =>
        new(query, ResolutionStatus.Resolved, chain, new ResolutionAuditTrail(Array.Empty<ResolutionAuditEntry>()));

    private static readonly LegalReference QueryParagraph = Address(Seg("Article", "47"), Seg("Paragraph", "3"));
    private static readonly LegalReference QueryArticle48 = Address(Seg("Article", "48"));

    [Fact]
    public async Task Provenance_pipeline_runs_configured_stages_in_order()
    {
        var executionOrder = new List<string>();
        var pipeline = new LegalReferenceResolutionProvenancePipeline(
            new RecordingProvenanceStage("first", executionOrder),
            new RecordingProvenanceStage("second", executionOrder));
        var context = new LegalReferenceResolutionProvenanceContext(Array.Empty<ResolutionProvenance>());

        var result = await pipeline.RunAsync(context);

        Assert.Equal(new[] { "first", "second" }, executionOrder);
        Assert.Equal(2, result.Provenances.Count);
        Assert.Empty(context.Provenances);
    }

    [Fact]
    public async Task Persist_stage_stores_provenances_without_changing_context()
    {
        var repository = new InMemoryLegalReferenceResolutionProvenanceRepository();
        var provenance = Provenance(QueryParagraph, Chain(Step("Monitorul Oficial", "extracted citation")));
        var stage = new PersistResolutionProvenanceStage(repository);
        var context = new LegalReferenceResolutionProvenanceContext(new[] { provenance });

        var result = await stage.ExecuteAsync(context);

        var stored = await repository.GetAsync(QueryParagraph);
        Assert.Equal("persist-resolution-provenance", stage.Name);
        Assert.Same(context, result);
        Assert.Same(provenance, stored);
    }

    [Fact]
    public async Task Provenance_engine_merges_processed_chain_steps_in_order()
    {
        var provenanceA = Provenance(QueryParagraph, Chain(Step("Monitorul Oficial", "extracted citation")));
        var provenanceB = Provenance(QueryArticle48, Chain(Step("EUR-Lex", "cross-referenced")));
        var engine = new LegalReferenceResolutionProvenanceEngine(new LegalReferenceResolutionProvenancePipeline());

        var chain = await engine.BuildAsync(new[] { provenanceA, provenanceB });

        Assert.Equal(2, chain.Steps.Count);
        Assert.Equal("Monitorul Oficial", chain.Steps[0].Source.Value);
        Assert.Equal("EUR-Lex", chain.Steps[1].Source.Value);
    }

    [Fact]
    public async Task Provenance_engine_delegates_to_pipeline_before_building_chain()
    {
        var processed = Provenance(QueryArticle48, Chain(Step("processed", "after pipeline")));
        var pipeline = new StubProvenancePipeline(new[] { processed });
        var engine = new LegalReferenceResolutionProvenanceEngine(pipeline);

        var chain = await engine.BuildAsync(new[] { Provenance(QueryParagraph, Chain(Step("input", "before pipeline"))) });

        Assert.Single(chain.Steps);
        Assert.Equal("processed", chain.Steps[0].Source.Value);
        Assert.NotNull(pipeline.ReceivedContext);
        Assert.Single(pipeline.ReceivedContext!.Provenances);
    }

    [Fact]
    public async Task Provenance_engine_returns_empty_chain_for_no_provenances()
    {
        var engine = new LegalReferenceResolutionProvenanceEngine(new LegalReferenceResolutionProvenancePipeline());

        var chain = await engine.BuildAsync(Array.Empty<ResolutionProvenance>());

        Assert.True(chain.IsEmpty);
    }
}
