using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
using FiscalOS.Domain.LegalReferences;
using FiscalOS.LegalCore;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Define_Legal_Reference_Resolution_Pipeline_Contracts
{
    // Test-only doubles validating the contract shapes and semantics. Not
    // production implementations (no resolution engine, repository
    // implementation, search algorithm, or AI/NLP), per FOS-0042 scope.

    // A pure placeholder stage: records every query as unresolved. No search.
    private sealed class MarkUnresolvedStage : ILegalReferenceResolutionStage
    {
        public string Name => "mark-unresolved";

        public Task<LegalReferenceResolutionContext> ExecuteAsync(LegalReferenceResolutionContext context, CancellationToken cancellationToken = default)
        {
            var results = new List<ResolutionResult>(context.Results);
            foreach (var query in context.Queries)
            {
                results.Add(new ResolutionResult(
                    ResolutionStatus.Unresolved,
                    Array.Empty<ResolutionCandidate>(),
                    new UnresolvedReference(query, "pending resolution")));
            }

            return Task.FromResult(context with { Results = results });
        }
    }

    // A stage that reads previously stored outcomes from the FOS-0041 repository.
    private sealed class RepositoryLookupStage : ILegalReferenceResolutionStage
    {
        private readonly ILegalReferenceResolutionRepository _repository;

        public RepositoryLookupStage(ILegalReferenceResolutionRepository repository) => _repository = repository;

        public string Name => "repository-lookup";

        public async Task<LegalReferenceResolutionContext> ExecuteAsync(LegalReferenceResolutionContext context, CancellationToken cancellationToken = default)
        {
            var results = new List<ResolutionResult>(context.Results);
            foreach (var query in context.Queries)
            {
                var stored = await _repository.GetAsync(query, cancellationToken);
                if (stored is not null)
                {
                    results.Add(stored);
                }
            }

            return context with { Results = results };
        }
    }

    private sealed class SequentialPipeline : ILegalReferenceResolutionPipeline
    {
        private readonly IReadOnlyList<ILegalReferenceResolutionStage> _stages;

        public SequentialPipeline(params ILegalReferenceResolutionStage[] stages) => _stages = stages;

        public async Task<LegalReferenceResolutionContext> RunAsync(LegalReferenceResolutionContext context, CancellationToken cancellationToken = default)
        {
            var current = context;
            foreach (var stage in _stages)
            {
                current = await stage.ExecuteAsync(current, cancellationToken);
            }

            return current;
        }
    }

    // Minimal in-memory resolution repository double (FOS-0041), not production.
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
            => Task.FromResult<IReadOnlyList<ResolutionResult>>(_store.Values.Where(r => r.Status == status).ToList());

        public Task<IReadOnlyList<UnresolvedReference>> GetUnresolvedAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<UnresolvedReference>>(_store.Values.Where(r => r.Unresolved is not null).Select(r => r.Unresolved!).ToList());
    }

    private static ReferenceSegment Seg(string kind, string value) => new(kind, value);

    private static LegalReference Address(params ReferenceSegment[] segments) => new(segments);

    private static FullyQualifiedLegalReference Fq(string document, params ReferenceSegment[] segments) =>
        new(new LegalDocumentReference(document), new LegalReference(segments));

    private static ResolutionResult Resolved(FullyQualifiedLegalReference reference) =>
        new(ResolutionStatus.Resolved, new[] { new ResolutionCandidate(reference) }, Unresolved: null);

    private static LegalReferenceResolutionContext Context(params LegalReference[] queries) =>
        new(queries, Array.Empty<ResolutionResult>());

    [Fact]
    public async Task Stage_executes_and_returns_an_updated_context()
    {
        ILegalReferenceResolutionStage stage = new MarkUnresolvedStage();
        var query = Address(Seg("Article", "47"), Seg("Paragraph", "3"));
        var context = Context(query);

        var updated = await stage.ExecuteAsync(context);

        Assert.Equal("mark-unresolved", stage.Name);
        Assert.Single(updated.Results);
        Assert.True(updated.Results[0].IsUnresolved);
        // Original context is unchanged (immutable).
        Assert.Empty(context.Results);
    }

    [Fact]
    public async Task Pipeline_runs_stages_and_produces_a_context()
    {
        ILegalReferenceResolutionPipeline pipeline = new SequentialPipeline(
            new MarkUnresolvedStage(),
            new MarkUnresolvedStage());
        var query = Address(Seg("Article", "47"), Seg("Paragraph", "3"));

        var result = await pipeline.RunAsync(Context(query));

        // Each stage appends one outcome for the single query.
        Assert.Equal(2, result.Results.Count);
    }

    [Fact]
    public async Task Pipeline_can_resolve_queries_through_the_repository()
    {
        var repository = new InMemoryLegalReferenceResolutionRepository();
        var query = Address(Seg("Article", "47"), Seg("Paragraph", "3"));
        var resolved = Resolved(Fq("Legea 227/2015", Seg("Article", "47"), Seg("Paragraph", "3")));
        await repository.StoreAsync(query, resolved);

        ILegalReferenceResolutionPipeline pipeline = new SequentialPipeline(new RepositoryLookupStage(repository));

        var result = await pipeline.RunAsync(Context(query));

        Assert.Single(result.Results);
        Assert.True(result.Results[0].IsResolved);
        Assert.Same(resolved, result.Results[0]);
    }
}
