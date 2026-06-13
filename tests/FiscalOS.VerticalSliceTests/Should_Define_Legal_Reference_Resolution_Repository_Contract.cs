using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
using FiscalOS.Domain.LegalReferences;
using FiscalOS.LegalCore;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Define_Legal_Reference_Resolution_Repository_Contract
{
    // Minimal in-memory test double used only to validate the contract shape and
    // semantics. It is not a production implementation (no resolver engine, search
    // algorithm, database, or filesystem), per FOS-0041 scope.
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
            => Task.FromResult<IReadOnlyList<ResolutionResult>>(
                _store.Values.Where(result => result.Status == status).ToList());

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

    private static ResolutionResult Ambiguous(params FullyQualifiedLegalReference[] references) =>
        new(ResolutionStatus.Ambiguous, references.Select(r => new ResolutionCandidate(r)).ToList(), Unresolved: null);

    private static ResolutionResult Unresolved(LegalReference query, string reason) =>
        new(ResolutionStatus.Unresolved, Array.Empty<ResolutionCandidate>(), new UnresolvedReference(query, reason));

    private static readonly LegalReference QueryParagraph = Address(Seg("Article", "47"), Seg("Paragraph", "3"));
    private static readonly LegalReference QueryArticle48 = Address(Seg("Article", "48"));
    private static readonly LegalReference QueryArticle99 = Address(Seg("Article", "99"));

    private static async Task<ILegalReferenceResolutionRepository> SeededRepositoryAsync()
    {
        ILegalReferenceResolutionRepository repository = new InMemoryLegalReferenceResolutionRepository();
        await repository.StoreAsync(QueryParagraph, Resolved(Fq("Legea 227/2015", Seg("Article", "47"), Seg("Paragraph", "3"))));
        await repository.StoreAsync(QueryArticle48, Ambiguous(
            Fq("Legea 227/2015", Seg("Article", "48")),
            Fq("OUG 1/2020", Seg("Article", "48"))));
        await repository.StoreAsync(QueryArticle99, Unresolved(QueryArticle99, "no matching document"));
        return repository;
    }

    [Fact]
    public async Task Stored_result_can_be_retrieved_by_query()
    {
        ILegalReferenceResolutionRepository repository = new InMemoryLegalReferenceResolutionRepository();
        var result = Resolved(Fq("Legea 227/2015", Seg("Article", "47"), Seg("Paragraph", "3")));

        await repository.StoreAsync(QueryParagraph, result);
        var retrieved = await repository.GetAsync(QueryParagraph);

        Assert.Same(result, retrieved);
    }

    [Fact]
    public async Task Unknown_query_returns_null()
    {
        ILegalReferenceResolutionRepository repository = new InMemoryLegalReferenceResolutionRepository();

        var retrieved = await repository.GetAsync(QueryArticle99);

        Assert.Null(retrieved);
    }

    [Fact]
    public async Task Results_can_be_queried_by_status()
    {
        var repository = await SeededRepositoryAsync();

        var resolved = await repository.GetByStatusAsync(ResolutionStatus.Resolved);
        var ambiguous = await repository.GetByStatusAsync(ResolutionStatus.Ambiguous);
        var unresolved = await repository.GetByStatusAsync(ResolutionStatus.Unresolved);

        Assert.Single(resolved);
        Assert.True(resolved[0].IsResolved);
        Assert.Single(ambiguous);
        Assert.True(ambiguous[0].IsAmbiguous);
        Assert.Single(unresolved);
        Assert.True(unresolved[0].IsUnresolved);
    }

    [Fact]
    public async Task Unresolved_query_returns_unresolved_references()
    {
        var repository = await SeededRepositoryAsync();

        var unresolved = await repository.GetUnresolvedAsync();

        Assert.Single(unresolved);
        Assert.Equal(QueryArticle99, unresolved[0].Query);
        Assert.Equal("no matching document", unresolved[0].Reason);
    }
}
