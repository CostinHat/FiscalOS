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

public sealed class Should_Build_Provenance_Through_Runtime_Provenance_Vertical
{
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

    private static readonly DateTimeOffset At = new(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);

    private static ReferenceSegment Seg(string kind, string value) => new(kind, value);

    private static LegalReference Address(params ReferenceSegment[] segments) => new(segments);

    private static FullyQualifiedLegalReference Fq(string document, params ReferenceSegment[] segments) =>
        new(new LegalDocumentReference(document), new LegalReference(segments));

    private static ResolutionResult Resolved(FullyQualifiedLegalReference reference) =>
        new(ResolutionStatus.Resolved, new[] { new ResolutionCandidate(reference) }, Unresolved: null);

    private static ProvenanceStep Step(string source, string description) =>
        new(new ProvenanceSource(source), description);

    private static ResolutionProvenance ProvenanceFor(ResolutionResult result, ResolutionAuditTrail trail)
    {
        var query = result.ResolvedReference?.Reference ?? result.Unresolved!.Query;
        var step = result.IsResolved
            ? Step("resolution-runtime", "resolved from repository result")
            : Step("resolution-runtime", "recorded unresolved repository miss");

        return new ResolutionProvenance(
            query,
            result.Status,
            new ProvenanceChain(new[] { step }),
            trail);
    }

    private static readonly LegalReference QueryParagraph = Address(Seg("Article", "47"), Seg("Paragraph", "3"));
    private static readonly LegalReference QueryArticle48 = Address(Seg("Article", "48"));

    [Fact]
    public async Task Runtime_provenance_engine_builds_chain_from_resolution_and_audit_outputs()
    {
        var resolutionRepository = new InMemoryLegalReferenceResolutionRepository();
        var selected = Fq("Legea 227/2015", Seg("Article", "47"), Seg("Paragraph", "3"));
        await resolutionRepository.StoreAsync(QueryParagraph, Resolved(selected));

        ILegalReferenceResolutionEngine resolutionEngine = new LegalReferenceResolutionEngine(
            new LegalReferenceResolutionPipeline(
                new RepositoryLegalReferenceResolutionStage(resolutionRepository)));
        ILegalReferenceResolutionAuditEngine auditEngine = new LegalReferenceResolutionAuditEngine(
            new LegalReferenceResolutionAuditPipeline(),
            () => At);

        var provenanceRepository = new InMemoryLegalReferenceResolutionProvenanceRepository();
        ILegalReferenceResolutionProvenanceEngine provenanceEngine = new LegalReferenceResolutionProvenanceEngine(
            new LegalReferenceResolutionProvenancePipeline(
                new PersistResolutionProvenanceStage(provenanceRepository)));

        var results = await resolutionEngine.ResolveAsync(new[] { QueryParagraph, QueryArticle48 });
        var auditTrail = await auditEngine.AuditAsync(results);
        var provenances = results.Select(result => ProvenanceFor(result, auditTrail)).ToList();

        var chain = await provenanceEngine.BuildAsync(provenances);

        Assert.Equal(2, chain.Steps.Count);
        Assert.All(chain.Steps, step => Assert.Equal("resolution-runtime", step.Source.Value));
        Assert.Contains(chain.Steps, step => step.Description == "resolved from repository result");
        Assert.Contains(chain.Steps, step => step.Description == "recorded unresolved repository miss");

        var storedResolved = await provenanceRepository.GetAsync(QueryParagraph);
        var storedUnresolved = await provenanceRepository.GetAsync(QueryArticle48);
        var chains = await provenanceRepository.GetChainsAsync();

        Assert.NotNull(storedResolved);
        Assert.NotNull(storedUnresolved);
        Assert.Equal(ResolutionStatus.Resolved, storedResolved!.Status);
        Assert.Equal(ResolutionStatus.Unresolved, storedUnresolved!.Status);
        Assert.Equal(2, chains.Count);
    }
}
