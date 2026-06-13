using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
using FiscalOS.Domain.LegalReferences;
using FiscalOS.LegalCore;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Define_Legal_Reference_Resolution_Provenance_Repository_Contract
{
    // Minimal in-memory test double used only to validate the contract shape and
    // semantics. It is not a production implementation (no provenance engine,
    // resolution engine, database, or filesystem), per FOS-0049 scope.
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

    private static ProvenanceStep Step(string source, string description) =>
        new(new ProvenanceSource(source), description);

    private static ProvenanceChain Chain(params ProvenanceStep[] steps) => new(steps);

    private static ResolutionAuditTrail EmptyTrail() => new(Array.Empty<ResolutionAuditEntry>());

    private static ResolutionProvenance Provenance(LegalReference query, ResolutionStatus status, ProvenanceChain chain) =>
        new(query, status, chain, EmptyTrail());

    private static readonly LegalReference QueryParagraph = Address(Seg("Article", "47"), Seg("Paragraph", "3"));
    private static readonly LegalReference QueryArticle48 = Address(Seg("Article", "48"));

    private static readonly ProvenanceChain ChainA =
        Chain(Step("Monitorul Oficial", "extracted citation"), Step("ANAF dataset", "matched to consolidated act"));

    private static readonly ProvenanceChain ChainB =
        Chain(Step("EUR-Lex", "cross-referenced"));

    private static readonly ResolutionProvenance ProvenanceA =
        Provenance(QueryParagraph, ResolutionStatus.Resolved, ChainA);

    private static readonly ResolutionProvenance ProvenanceB =
        Provenance(QueryArticle48, ResolutionStatus.Ambiguous, ChainB);

    private static async Task<ILegalReferenceResolutionProvenanceRepository> SeededRepositoryAsync()
    {
        ILegalReferenceResolutionProvenanceRepository repository = new InMemoryLegalReferenceResolutionProvenanceRepository();
        await repository.StoreAsync(ProvenanceA);
        await repository.StoreAsync(ProvenanceB);
        return repository;
    }

    [Fact]
    public async Task Stored_provenance_can_be_retrieved_by_query()
    {
        var repository = await SeededRepositoryAsync();

        var provenance = await repository.GetAsync(QueryParagraph);

        Assert.Same(ProvenanceA, provenance);
    }

    [Fact]
    public async Task Unknown_query_returns_null()
    {
        var repository = await SeededRepositoryAsync();

        var provenance = await repository.GetAsync(Address(Seg("Article", "1")));

        Assert.Null(provenance);
    }

    [Fact]
    public async Task Provenance_can_be_queried_by_source()
    {
        var repository = await SeededRepositoryAsync();

        var fromMonitorul = await repository.GetBySourceAsync("Monitorul Oficial");
        var fromEurLex = await repository.GetBySourceAsync("EUR-Lex");

        Assert.Single(fromMonitorul);
        Assert.Same(ProvenanceA, fromMonitorul[0]);
        Assert.Single(fromEurLex);
        Assert.Same(ProvenanceB, fromEurLex[0]);
    }

    [Fact]
    public async Task All_chains_can_be_retrieved()
    {
        var repository = await SeededRepositoryAsync();

        var chains = await repository.GetChainsAsync();

        Assert.Equal(2, chains.Count);
        Assert.Contains(ChainA, chains);
        Assert.Contains(ChainB, chains);
    }
}
