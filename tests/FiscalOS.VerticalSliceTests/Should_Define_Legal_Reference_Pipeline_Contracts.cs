using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FiscalOS.Domain.LegalReferences;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Define_Legal_Reference_Pipeline_Contracts
{
    // Test-only doubles validating the contract shapes and semantics. Not
    // production implementations (no orchestration runtime, repository
    // implementation, or extraction logic), per FOS-0039 scope.

    // A pure structural stage (FOS-0037): adds the parent of each reference.
    private sealed class AddParentsStage : ILegalReferenceStage
    {
        public string Name => "add-parents";

        public Task<LegalReferenceContext> ExecuteAsync(LegalReferenceContext context, CancellationToken cancellationToken = default)
        {
            var references = new List<FullyQualifiedLegalReference>(context.References);
            foreach (var reference in context.References)
            {
                var parent = reference.Parent();
                if (parent is not null && !references.Contains(parent))
                {
                    references.Add(parent);
                }
            }

            return Task.FromResult(context with { References = references });
        }
    }

    // A terminal stage that persists references through the FOS-0038 repository abstraction.
    private sealed class PersistStage : ILegalReferenceStage
    {
        private readonly ILegalReferenceRepository _repository;

        public PersistStage(ILegalReferenceRepository repository) => _repository = repository;

        public string Name => "persist";

        public async Task<LegalReferenceContext> ExecuteAsync(LegalReferenceContext context, CancellationToken cancellationToken = default)
        {
            foreach (var reference in context.References)
            {
                await _repository.StoreAsync(reference, cancellationToken);
            }

            return context;
        }
    }

    private sealed class SequentialPipeline : ILegalReferencePipeline
    {
        private readonly IReadOnlyList<ILegalReferenceStage> _stages;

        public SequentialPipeline(params ILegalReferenceStage[] stages) => _stages = stages;

        public async Task<LegalReferenceContext> RunAsync(LegalReferenceContext context, CancellationToken cancellationToken = default)
        {
            var current = context;
            foreach (var stage in _stages)
            {
                current = await stage.ExecuteAsync(current, cancellationToken);
            }

            return current;
        }
    }

    // Minimal in-memory repository double (FOS-0038), not a production implementation.
    private sealed class InMemoryLegalReferenceRepository : ILegalReferenceRepository
    {
        private readonly List<FullyQualifiedLegalReference> _store = new();

        public Task StoreAsync(FullyQualifiedLegalReference reference, CancellationToken cancellationToken = default)
        {
            if (!_store.Contains(reference))
            {
                _store.Add(reference);
            }

            return Task.CompletedTask;
        }

        public Task<FullyQualifiedLegalReference?> GetAsync(FullyQualifiedLegalReference reference, CancellationToken cancellationToken = default)
            => Task.FromResult<FullyQualifiedLegalReference?>(_store.FirstOrDefault(stored => stored.Equals(reference)));

        public Task<IReadOnlyList<FullyQualifiedLegalReference>> GetChildrenAsync(FullyQualifiedLegalReference parent, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<FullyQualifiedLegalReference>>(
                _store.Where(stored => stored.Parent() is { } storedParent && storedParent.Equals(parent)).ToList());

        public Task<IReadOnlyList<FullyQualifiedLegalReference>> GetDescendantsAsync(FullyQualifiedLegalReference ancestor, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<FullyQualifiedLegalReference>>(
                _store.Where(stored => ancestor.IsAncestorOf(stored)).ToList());
    }

    private static readonly LegalDocumentReference CodFiscal = new("Legea 227/2015");

    private static ReferenceSegment Seg(string kind, string value) => new(kind, value);

    private static FullyQualifiedLegalReference Fq(params ReferenceSegment[] segments) =>
        new(CodFiscal, new LegalReference(segments));

    private static LegalReferenceContext Context(params FullyQualifiedLegalReference[] references) =>
        new(references);

    [Fact]
    public async Task Stage_executes_and_returns_an_updated_context()
    {
        ILegalReferenceStage stage = new AddParentsStage();
        var letterB = Fq(Seg("Article", "47"), Seg("Paragraph", "3"), Seg("Letter", "b"));
        var context = Context(letterB);

        var updated = await stage.ExecuteAsync(context);

        Assert.Equal("add-parents", stage.Name);
        Assert.Equal(2, updated.References.Count);
        Assert.Contains(letterB.Parent()!, updated.References);
        // Original context is unchanged (immutable).
        Assert.Single(context.References);
    }

    [Fact]
    public async Task Pipeline_runs_stages_and_produces_a_context()
    {
        ILegalReferencePipeline pipeline = new SequentialPipeline(
            new AddParentsStage(),
            new AddParentsStage());
        var letterB = Fq(Seg("Article", "47"), Seg("Paragraph", "3"), Seg("Letter", "b"));

        var result = await pipeline.RunAsync(Context(letterB));

        // letterB -> +paragraph3 (stage 1) -> +article47 (stage 2)
        Assert.Equal(3, result.References.Count);
    }

    [Fact]
    public async Task Pipeline_can_persist_references_through_the_repository()
    {
        var repository = new InMemoryLegalReferenceRepository();
        ILegalReferencePipeline pipeline = new SequentialPipeline(
            new AddParentsStage(),
            new PersistStage(repository));
        var letterB = Fq(Seg("Article", "47"), Seg("Paragraph", "3"), Seg("Letter", "b"));

        await pipeline.RunAsync(Context(letterB));

        var article47 = Fq(Seg("Article", "47"));
        var descendants = await repository.GetDescendantsAsync(article47);

        Assert.Equal(2, descendants.Count); // paragraph3 and letterB were persisted
        Assert.NotNull(await repository.GetAsync(letterB));
    }
}
