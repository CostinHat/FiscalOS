using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FiscalOS.Domain.LegalReferences;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Define_Legal_Reference_Repository_Contract
{
    // Minimal in-memory test double used only to validate the contract shape and
    // structural semantics. It is not a production implementation (no database,
    // filesystem, graph store, or DI), per FOS-0038 scope.
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
        {
            FullyQualifiedLegalReference? found = _store.FirstOrDefault(stored => stored.Equals(reference));
            return Task.FromResult(found);
        }

        public Task<IReadOnlyList<FullyQualifiedLegalReference>> GetChildrenAsync(FullyQualifiedLegalReference parent, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<FullyQualifiedLegalReference> children = _store
                .Where(stored => stored.Parent() is { } storedParent && storedParent.Equals(parent))
                .ToList();
            return Task.FromResult(children);
        }

        public Task<IReadOnlyList<FullyQualifiedLegalReference>> GetDescendantsAsync(FullyQualifiedLegalReference ancestor, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<FullyQualifiedLegalReference> descendants = _store
                .Where(stored => ancestor.IsAncestorOf(stored))
                .ToList();
            return Task.FromResult(descendants);
        }
    }

    private static readonly LegalDocumentReference CodFiscal = new("Legea 227/2015");
    private static readonly LegalDocumentReference OtherAct = new("OUG 1/2020");

    private static ReferenceSegment Seg(string kind, string value) => new(kind, value);

    private static FullyQualifiedLegalReference Fq(LegalDocumentReference document, params ReferenceSegment[] segments) =>
        new(document, new LegalReference(segments));

    private static readonly FullyQualifiedLegalReference Article47 =
        Fq(CodFiscal, Seg("Article", "47"));

    private static readonly FullyQualifiedLegalReference Paragraph3 =
        Fq(CodFiscal, Seg("Article", "47"), Seg("Paragraph", "3"));

    private static readonly FullyQualifiedLegalReference LetterB =
        Fq(CodFiscal, Seg("Article", "47"), Seg("Paragraph", "3"), Seg("Letter", "b"));

    private static readonly FullyQualifiedLegalReference Article48 =
        Fq(CodFiscal, Seg("Article", "48"));

    [Fact]
    public async Task Stored_reference_can_be_retrieved()
    {
        ILegalReferenceRepository repository = new InMemoryLegalReferenceRepository();

        await repository.StoreAsync(Paragraph3);
        var retrieved = await repository.GetAsync(Paragraph3);

        Assert.Equal(Paragraph3, retrieved);
    }

    [Fact]
    public async Task Unknown_reference_returns_null()
    {
        ILegalReferenceRepository repository = new InMemoryLegalReferenceRepository();

        var retrieved = await repository.GetAsync(LetterB);

        Assert.Null(retrieved);
    }

    [Fact]
    public async Task Children_query_returns_immediate_children_only()
    {
        ILegalReferenceRepository repository = new InMemoryLegalReferenceRepository();
        await repository.StoreAsync(Article47);
        await repository.StoreAsync(Paragraph3);
        await repository.StoreAsync(LetterB);
        await repository.StoreAsync(Article48);

        var children = await repository.GetChildrenAsync(Article47);

        Assert.Single(children);
        Assert.Contains(Paragraph3, children); // immediate child
        Assert.DoesNotContain(LetterB, children); // grandchild
        Assert.DoesNotContain(Article47, children); // self
        Assert.DoesNotContain(Article48, children); // sibling
    }

    [Fact]
    public async Task Descendants_query_returns_all_descendants()
    {
        ILegalReferenceRepository repository = new InMemoryLegalReferenceRepository();
        await repository.StoreAsync(Article47);
        await repository.StoreAsync(Paragraph3);
        await repository.StoreAsync(LetterB);
        await repository.StoreAsync(Article48);

        var descendants = await repository.GetDescendantsAsync(Article47);

        Assert.Equal(2, descendants.Count);
        Assert.Contains(Paragraph3, descendants);
        Assert.Contains(LetterB, descendants);
        Assert.DoesNotContain(Article47, descendants); // self is not a descendant
        Assert.DoesNotContain(Article48, descendants); // sibling
    }

    [Fact]
    public async Task Hierarchy_queries_are_scoped_to_the_same_document()
    {
        ILegalReferenceRepository repository = new InMemoryLegalReferenceRepository();
        var paragraphInOtherDocument = Fq(OtherAct, Seg("Article", "47"), Seg("Paragraph", "3"));
        await repository.StoreAsync(Paragraph3);
        await repository.StoreAsync(paragraphInOtherDocument);

        var children = await repository.GetChildrenAsync(Article47);
        var descendants = await repository.GetDescendantsAsync(Article47);

        Assert.DoesNotContain(paragraphInOtherDocument, children);
        Assert.DoesNotContain(paragraphInOtherDocument, descendants);
        Assert.Contains(Paragraph3, children);
        Assert.Contains(Paragraph3, descendants);
    }
}
