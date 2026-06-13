using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
using FiscalOS.Domain.LegalReferences;
using FiscalOS.LegalCore;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Define_Legal_Reference_Resolution_Provenance_Pipeline_Contracts
{
    // Test-only doubles validating the contract shapes and semantics. Not
    // production implementations (no provenance engine, repository implementation,
    // resolution engine, or AI/NLP), per FOS-0050 scope.

    private sealed class AppendProvenanceStage : ILegalReferenceResolutionProvenanceStage
    {
        private readonly ResolutionProvenance _provenance;

        public AppendProvenanceStage(ResolutionProvenance provenance) => _provenance = provenance;

        public string Name => "append-provenance";

        public Task<LegalReferenceResolutionProvenanceContext> ExecuteAsync(LegalReferenceResolutionProvenanceContext context, CancellationToken cancellationToken = default)
        {
            var provenances = new List<ResolutionProvenance>(context.Provenances) { _provenance };
            return Task.FromResult(context with { Provenances = provenances });
        }
    }

    // A terminal stage that persists the accumulated provenances through the
    // FOS-0049 repository abstraction.
    private sealed class PersistStage : ILegalReferenceResolutionProvenanceStage
    {
        private readonly ILegalReferenceResolutionProvenanceRepository _repository;

        public PersistStage(ILegalReferenceResolutionProvenanceRepository repository) => _repository = repository;

        public string Name => "persist";

        public async Task<LegalReferenceResolutionProvenanceContext> ExecuteAsync(LegalReferenceResolutionProvenanceContext context, CancellationToken cancellationToken = default)
        {
            foreach (var provenance in context.Provenances)
            {
                await _repository.StoreAsync(provenance, cancellationToken);
            }

            return context;
        }
    }

    private sealed class SequentialPipeline : ILegalReferenceResolutionProvenancePipeline
    {
        private readonly IReadOnlyList<ILegalReferenceResolutionProvenanceStage> _stages;

        public SequentialPipeline(params ILegalReferenceResolutionProvenanceStage[] stages) => _stages = stages;

        public async Task<LegalReferenceResolutionProvenanceContext> RunAsync(LegalReferenceResolutionProvenanceContext context, CancellationToken cancellationToken = default)
        {
            var current = context;
            foreach (var stage in _stages)
            {
                current = await stage.ExecuteAsync(current, cancellationToken);
            }

            return current;
        }
    }

    // Minimal in-memory provenance repository double (FOS-0049), not production.
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
                _store.Values.Where(p => p.Chain.Steps.Any(step => step.Source.Value == source)).ToList());

        public Task<IReadOnlyList<ProvenanceChain>> GetChainsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ProvenanceChain>>(_store.Values.Select(p => p.Chain).ToList());
    }

    private static ReferenceSegment Seg(string kind, string value) => new(kind, value);

    private static LegalReference Address(params ReferenceSegment[] segments) => new(segments);

    private static ProvenanceChain Chain(params ProvenanceStep[] steps) => new(steps);

    private static ProvenanceStep Step(string source, string description) =>
        new(new ProvenanceSource(source), description);

    private static ResolutionProvenance Provenance(LegalReference query, ProvenanceChain chain) =>
        new(query, ResolutionStatus.Resolved, chain, new ResolutionAuditTrail(Array.Empty<ResolutionAuditEntry>()));

    private static readonly LegalReference QueryParagraph = Address(Seg("Article", "47"), Seg("Paragraph", "3"));
    private static readonly LegalReference QueryArticle48 = Address(Seg("Article", "48"));

    private static ResolutionProvenance ProvenanceA() =>
        Provenance(QueryParagraph, Chain(Step("Monitorul Oficial", "extracted citation")));

    private static ResolutionProvenance ProvenanceB() =>
        Provenance(QueryArticle48, Chain(Step("EUR-Lex", "cross-referenced")));

    private static LegalReferenceResolutionProvenanceContext Context() =>
        new(Array.Empty<ResolutionProvenance>());

    [Fact]
    public async Task Stage_executes_and_returns_an_updated_context()
    {
        var provenance = ProvenanceA();
        ILegalReferenceResolutionProvenanceStage stage = new AppendProvenanceStage(provenance);
        var context = Context();

        var updated = await stage.ExecuteAsync(context);

        Assert.Equal("append-provenance", stage.Name);
        Assert.Single(updated.Provenances);
        Assert.Same(provenance, updated.Provenances[0]);
        // Original context is unchanged (immutable).
        Assert.Empty(context.Provenances);
    }

    [Fact]
    public async Task Pipeline_runs_stages_and_produces_a_context()
    {
        ILegalReferenceResolutionProvenancePipeline pipeline = new SequentialPipeline(
            new AppendProvenanceStage(ProvenanceA()),
            new AppendProvenanceStage(ProvenanceB()));

        var result = await pipeline.RunAsync(Context());

        Assert.Equal(2, result.Provenances.Count);
    }

    [Fact]
    public async Task Pipeline_can_persist_provenance_through_the_repository()
    {
        var repository = new InMemoryLegalReferenceResolutionProvenanceRepository();
        var provenance = ProvenanceA();
        ILegalReferenceResolutionProvenancePipeline pipeline = new SequentialPipeline(
            new AppendProvenanceStage(provenance),
            new PersistStage(repository));

        await pipeline.RunAsync(Context());

        var stored = await repository.GetAsync(QueryParagraph);
        Assert.Same(provenance, stored);
    }
}
