using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FiscalOS.Domain.LegalReferences;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Define_Legal_Reference_Resolution_Provenance_Engine_Contract
{
    // Test-only doubles validating the contract shape and semantics. Not
    // production implementations (no provenance algorithm, pipeline implementation,
    // repository implementation, or AI/NLP), per FOS-0051 scope. The engine double
    // is composed over a FOS-0050 pipeline whose stage persists via a FOS-0049
    // repository, to exercise the full layering.

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

    // FOS-0051 engine double: a facade that runs provenances through a FOS-0050
    // pipeline and merges their chains. The merge is trivial test logic, not a
    // production provenance algorithm.
    private sealed class PipelineBackedProvenanceEngine : ILegalReferenceResolutionProvenanceEngine
    {
        private readonly ILegalReferenceResolutionProvenancePipeline _pipeline;

        public PipelineBackedProvenanceEngine(ILegalReferenceResolutionProvenancePipeline pipeline) => _pipeline = pipeline;

        public async Task<ProvenanceChain> BuildAsync(IReadOnlyList<ResolutionProvenance> provenances, CancellationToken cancellationToken = default)
        {
            var context = new LegalReferenceResolutionProvenanceContext(provenances);
            var processed = await _pipeline.RunAsync(context, cancellationToken);
            var steps = processed.Provenances.SelectMany(provenance => provenance.Chain.Steps).ToList();
            return new ProvenanceChain(steps);
        }
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

    private static ILegalReferenceResolutionProvenanceEngine EngineWith(InMemoryLegalReferenceResolutionProvenanceRepository repository) =>
        new PipelineBackedProvenanceEngine(new SequentialPipeline(new PersistStage(repository)));

    [Fact]
    public async Task Engine_builds_a_chain_from_a_single_provenance()
    {
        var engine = EngineWith(new InMemoryLegalReferenceResolutionProvenanceRepository());
        var provenance = Provenance(QueryParagraph, Chain(Step("Monitorul Oficial", "extracted citation")));

        var chain = await engine.BuildAsync(new[] { provenance });

        Assert.Single(chain.Steps);
        Assert.Equal("Monitorul Oficial", chain.Steps[0].Source.Value);
    }

    [Fact]
    public async Task Engine_merges_steps_from_all_provenances()
    {
        var engine = EngineWith(new InMemoryLegalReferenceResolutionProvenanceRepository());
        var provenances = new[]
        {
            Provenance(QueryParagraph, Chain(Step("Monitorul Oficial", "extracted citation"))),
            Provenance(QueryArticle48, Chain(Step("EUR-Lex", "cross-referenced"))),
        };

        var chain = await engine.BuildAsync(provenances);

        Assert.Equal(2, chain.Steps.Count);
    }

    [Fact]
    public async Task Engine_returns_an_empty_chain_for_no_provenances()
    {
        var engine = EngineWith(new InMemoryLegalReferenceResolutionProvenanceRepository());

        var chain = await engine.BuildAsync(Array.Empty<ResolutionProvenance>());

        Assert.True(chain.IsEmpty);
    }

    [Fact]
    public async Task Engine_persists_provenances_through_the_repository()
    {
        var repository = new InMemoryLegalReferenceResolutionProvenanceRepository();
        var engine = EngineWith(repository);
        var provenance = Provenance(QueryParagraph, Chain(Step("Monitorul Oficial", "extracted citation")));

        await engine.BuildAsync(new[] { provenance });

        var stored = await repository.GetAsync(QueryParagraph);
        Assert.Same(provenance, stored);
    }
}
