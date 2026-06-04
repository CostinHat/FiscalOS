using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FiscalOS.Domain.LegalReferences;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Define_Legal_Reference_Resolution_Engine_Contract
{
    // Test-only doubles validating the contract shape and semantics. Not
    // production implementations (no resolution algorithm, search engine,
    // repository implementation, or AI/NLP), per FOS-0043 scope. The engine
    // double is composed over a FOS-0042 pipeline whose stage reads a FOS-0041
    // repository, to exercise the full layering.

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

    // FOS-0042 stage: produces one outcome per query, reading the repository and
    // falling back to unresolved. No search.
    private sealed class ResolveFromRepositoryStage : ILegalReferenceResolutionStage
    {
        private readonly ILegalReferenceResolutionRepository _repository;

        public ResolveFromRepositoryStage(ILegalReferenceResolutionRepository repository) => _repository = repository;

        public string Name => "resolve-from-repository";

        public async Task<LegalReferenceResolutionContext> ExecuteAsync(LegalReferenceResolutionContext context, CancellationToken cancellationToken = default)
        {
            var results = new List<ResolutionResult>(context.Results);
            foreach (var query in context.Queries)
            {
                var stored = await _repository.GetAsync(query, cancellationToken);
                results.Add(stored ?? new ResolutionResult(
                    ResolutionStatus.Unresolved,
                    Array.Empty<ResolutionCandidate>(),
                    new UnresolvedReference(query, "no stored resolution")));
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

    // FOS-0043 engine double: a facade over a FOS-0042 pipeline.
    private sealed class PipelineBackedResolutionEngine : ILegalReferenceResolutionEngine
    {
        private readonly ILegalReferenceResolutionPipeline _pipeline;

        public PipelineBackedResolutionEngine(ILegalReferenceResolutionPipeline pipeline) => _pipeline = pipeline;

        public async Task<IReadOnlyList<ResolutionResult>> ResolveAsync(IReadOnlyList<LegalReference> references, CancellationToken cancellationToken = default)
        {
            var context = new LegalReferenceResolutionContext(references, Array.Empty<ResolutionResult>());
            var resolved = await _pipeline.RunAsync(context, cancellationToken);
            return resolved.Results;
        }
    }

    private static ReferenceSegment Seg(string kind, string value) => new(kind, value);

    private static LegalReference Address(params ReferenceSegment[] segments) => new(segments);

    private static FullyQualifiedLegalReference Fq(string document, params ReferenceSegment[] segments) =>
        new(new LegalDocumentReference(document), new LegalReference(segments));

    private static ResolutionResult Resolved(FullyQualifiedLegalReference reference) =>
        new(ResolutionStatus.Resolved, new[] { new ResolutionCandidate(reference) }, Unresolved: null);

    private static ILegalReferenceResolutionEngine EngineWith(InMemoryLegalReferenceResolutionRepository repository) =>
        new PipelineBackedResolutionEngine(new SequentialPipeline(new ResolveFromRepositoryStage(repository)));

    private static readonly LegalReference QueryParagraph = Address(Seg("Article", "47"), Seg("Paragraph", "3"));
    private static readonly LegalReference QueryArticle48 = Address(Seg("Article", "48"));

    [Fact]
    public async Task Engine_resolves_a_known_reference()
    {
        var repository = new InMemoryLegalReferenceResolutionRepository();
        var resolved = Resolved(Fq("Legea 227/2015", Seg("Article", "47"), Seg("Paragraph", "3")));
        await repository.StoreAsync(QueryParagraph, resolved);
        var engine = EngineWith(repository);

        var results = await engine.ResolveAsync(new[] { QueryParagraph });

        Assert.Single(results);
        Assert.Same(resolved, results[0]);
        Assert.True(results[0].IsResolved);
    }

    [Fact]
    public async Task Engine_returns_one_result_per_reference()
    {
        var repository = new InMemoryLegalReferenceResolutionRepository();
        await repository.StoreAsync(QueryParagraph,
            Resolved(Fq("Legea 227/2015", Seg("Article", "47"), Seg("Paragraph", "3"))));
        var engine = EngineWith(repository);

        var results = await engine.ResolveAsync(new[] { QueryParagraph, QueryArticle48 });

        Assert.Equal(2, results.Count);
        Assert.True(results[0].IsResolved);
        Assert.True(results[1].IsUnresolved); // not stored
    }

    [Fact]
    public async Task Engine_returns_no_results_for_no_references()
    {
        var engine = EngineWith(new InMemoryLegalReferenceResolutionRepository());

        var results = await engine.ResolveAsync(Array.Empty<LegalReference>());

        Assert.Empty(results);
    }
}
