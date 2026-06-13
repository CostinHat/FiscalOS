using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using FiscalOS.Domain.LegalReferences;
using FiscalOS.LegalCore;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Define_Legal_Reference_Resolution_Evidence_Package_Repository_Contract
{
    // Minimal in-memory test double used only to validate the contract shape and
    // semantics. It is not a production implementation (no database, filesystem,
    // engine, or pipeline), per FOS-0053 scope.
    private sealed class InMemoryLegalReferenceResolutionEvidencePackageRepository : ILegalReferenceResolutionEvidencePackageRepository
    {
        private readonly Dictionary<LegalReference, ResolutionEvidencePackage> _store = new();

        public Task StoreAsync(LegalReference query, ResolutionEvidencePackage package, CancellationToken cancellationToken = default)
        {
            _store[query] = package;
            return Task.CompletedTask;
        }

        public Task<ResolutionEvidencePackage?> GetAsync(LegalReference query, CancellationToken cancellationToken = default)
            => Task.FromResult<ResolutionEvidencePackage?>(_store.TryGetValue(query, out var package) ? package : null);
    }

    private static readonly DateTimeOffset At = new(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);

    private static ReferenceSegment Seg(string kind, string value) => new(kind, value);

    private static LegalReference Address(params ReferenceSegment[] segments) => new(segments);

    private static FullyQualifiedLegalReference Fq(string document, params ReferenceSegment[] segments) =>
        new(new LegalDocumentReference(document), new LegalReference(segments));

    private static readonly LegalReference QueryParagraph = Address(Seg("Article", "47"), Seg("Paragraph", "3"));
    private static readonly FullyQualifiedLegalReference Selected =
        Fq("Legea 227/2015", Seg("Article", "47"), Seg("Paragraph", "3"));

    private static ResolutionAuditTrail Trail() =>
        new(new[]
        {
            new ResolutionAuditEntry(
                QueryParagraph,
                new ResolutionDecision(ResolutionStatus.Resolved, Selected),
                new ResolutionEvidence("matched a single document"),
                At),
        });

    private static ResolutionEvidencePackage Package()
    {
        var result = new ResolutionResult(ResolutionStatus.Resolved, new[] { new ResolutionCandidate(Selected) }, Unresolved: null);
        var provenance = new ResolutionProvenance(
            QueryParagraph,
            ResolutionStatus.Resolved,
            new ProvenanceChain(new[] { new ProvenanceStep(new ProvenanceSource("Monitorul Oficial"), "extracted citation") }),
            Trail());
        return new ResolutionEvidencePackage(result, Trail(), provenance);
    }

    [Fact]
    public async Task Stored_package_can_be_retrieved_by_query()
    {
        ILegalReferenceResolutionEvidencePackageRepository repository = new InMemoryLegalReferenceResolutionEvidencePackageRepository();
        var package = Package();

        await repository.StoreAsync(QueryParagraph, package);
        var retrieved = await repository.GetAsync(QueryParagraph);

        Assert.Same(package, retrieved);
    }

    [Fact]
    public async Task Unknown_query_returns_null()
    {
        ILegalReferenceResolutionEvidencePackageRepository repository = new InMemoryLegalReferenceResolutionEvidencePackageRepository();

        var retrieved = await repository.GetAsync(Address(Seg("Article", "1")));

        Assert.Null(retrieved);
    }

    [Fact]
    public async Task Retrieved_package_preserves_separate_concerns()
    {
        ILegalReferenceResolutionEvidencePackageRepository repository = new InMemoryLegalReferenceResolutionEvidencePackageRepository();
        var package = Package();
        await repository.StoreAsync(QueryParagraph, package);

        var retrieved = await repository.GetAsync(QueryParagraph);

        Assert.NotNull(retrieved);
        Assert.Same(package.Result, retrieved!.Result);
        Assert.Same(package.AuditTrail, retrieved.AuditTrail);
        Assert.Same(package.Provenance, retrieved.Provenance);
    }
}
