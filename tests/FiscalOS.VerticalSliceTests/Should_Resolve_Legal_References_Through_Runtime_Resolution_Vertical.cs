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

public sealed class Should_Resolve_Legal_References_Through_Runtime_Resolution_Vertical
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

    private static ReferenceSegment Seg(string kind, string value) => new(kind, value);

    private static LegalReference Address(params ReferenceSegment[] segments) => new(segments);

    private static FullyQualifiedLegalReference Fq(string document, params ReferenceSegment[] segments) =>
        new(new LegalDocumentReference(document), new LegalReference(segments));

    private static ResolutionResult Resolved(FullyQualifiedLegalReference reference) =>
        new(ResolutionStatus.Resolved, new[] { new ResolutionCandidate(reference) }, Unresolved: null);

    private static readonly LegalReference QueryParagraph = Address(Seg("Article", "47"), Seg("Paragraph", "3"));
    private static readonly LegalReference QueryArticle48 = Address(Seg("Article", "48"));

    [Fact]
    public async Task Runtime_resolution_engine_resolves_known_references_and_marks_unknown_references_unresolved()
    {
        var repository = new InMemoryLegalReferenceResolutionRepository();
        var selected = Fq("Legea 227/2015", Seg("Article", "47"), Seg("Paragraph", "3"));
        var resolved = Resolved(selected);
        await repository.StoreAsync(QueryParagraph, resolved);

        ILegalReferenceResolutionEngine engine = new LegalReferenceResolutionEngine(
            new LegalReferenceResolutionPipeline(
                new RepositoryLegalReferenceResolutionStage(repository)));

        var results = await engine.ResolveAsync(new[] { QueryParagraph, QueryArticle48 });

        Assert.Equal(2, results.Count);
        Assert.True(results[0].IsResolved);
        Assert.Same(resolved, results[0]);
        Assert.Equal(selected, results[0].ResolvedReference);
        Assert.True(results[1].IsUnresolved);
        Assert.Equal(QueryArticle48, results[1].Unresolved!.Query);
    }
}
