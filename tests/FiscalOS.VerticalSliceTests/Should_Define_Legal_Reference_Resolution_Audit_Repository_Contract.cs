using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FiscalOS.Domain.LegalReferences;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Define_Legal_Reference_Resolution_Audit_Repository_Contract
{
    // Minimal in-memory test double used only to validate the contract shape and
    // semantics. It is not a production implementation (no audit engine, resolution
    // engine, database, or filesystem), per FOS-0045 scope.
    private sealed class InMemoryLegalReferenceResolutionAuditRepository : ILegalReferenceResolutionAuditRepository
    {
        private readonly List<ResolutionAuditEntry> _entries = new();

        public Task StoreAsync(ResolutionAuditTrail trail, CancellationToken cancellationToken = default)
        {
            _entries.AddRange(trail.Entries);
            return Task.CompletedTask;
        }

        public Task<ResolutionAuditTrail?> GetAsync(LegalReference query, CancellationToken cancellationToken = default)
        {
            var matching = _entries.Where(entry => entry.Query.Equals(query)).ToList();
            return Task.FromResult<ResolutionAuditTrail?>(matching.Count == 0 ? null : new ResolutionAuditTrail(matching));
        }

        public Task<IReadOnlyList<ResolutionAuditEntry>> GetByDecisionStatusAsync(ResolutionStatus status, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ResolutionAuditEntry>>(
                _entries.Where(entry => entry.Decision.Status == status).ToList());

        public Task<IReadOnlyList<ResolutionAuditEntry>> GetUnresolvedAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ResolutionAuditEntry>>(
                _entries.Where(entry => entry.Decision.Status == ResolutionStatus.Unresolved).ToList());
    }

    private static readonly DateTimeOffset At = new(2026, 6, 4, 12, 0, 0, TimeSpan.Zero);

    private static ReferenceSegment Seg(string kind, string value) => new(kind, value);

    private static LegalReference Address(params ReferenceSegment[] segments) => new(segments);

    private static FullyQualifiedLegalReference Fq(string document, params ReferenceSegment[] segments) =>
        new(new LegalDocumentReference(document), new LegalReference(segments));

    private static ResolutionAuditEntry Entry(LegalReference query, ResolutionDecision decision, string evidence) =>
        new(query, decision, new ResolutionEvidence(evidence), At);

    private static readonly LegalReference QueryParagraph = Address(Seg("Article", "47"), Seg("Paragraph", "3"));
    private static readonly LegalReference QueryArticle48 = Address(Seg("Article", "48"));
    private static readonly LegalReference QueryArticle99 = Address(Seg("Article", "99"));

    private static readonly ResolutionAuditEntry ResolvedEntry =
        Entry(QueryParagraph,
            new ResolutionDecision(ResolutionStatus.Resolved, Fq("Legea 227/2015", Seg("Article", "47"), Seg("Paragraph", "3"))),
            "matched a single document");

    private static readonly ResolutionAuditEntry AmbiguousEntry =
        Entry(QueryArticle48, new ResolutionDecision(ResolutionStatus.Ambiguous), "multiple candidate documents");

    private static readonly ResolutionAuditEntry UnresolvedEntry =
        Entry(QueryArticle99, new ResolutionDecision(ResolutionStatus.Unresolved), "no matching document");

    private static async Task<ILegalReferenceResolutionAuditRepository> SeededRepositoryAsync()
    {
        ILegalReferenceResolutionAuditRepository repository = new InMemoryLegalReferenceResolutionAuditRepository();
        await repository.StoreAsync(new ResolutionAuditTrail(new[] { ResolvedEntry, AmbiguousEntry, UnresolvedEntry }));
        return repository;
    }

    [Fact]
    public async Task Stored_trail_can_be_retrieved_by_query()
    {
        var repository = await SeededRepositoryAsync();

        var trail = await repository.GetAsync(QueryParagraph);

        Assert.NotNull(trail);
        Assert.Single(trail!.Entries);
        Assert.Equal(QueryParagraph, trail.Entries[0].Query);
        Assert.Equal(ResolutionStatus.Resolved, trail.Entries[0].Decision.Status);
    }

    [Fact]
    public async Task Unknown_query_returns_null()
    {
        var repository = await SeededRepositoryAsync();

        var trail = await repository.GetAsync(Address(Seg("Article", "1")));

        Assert.Null(trail);
    }

    [Fact]
    public async Task Entries_can_be_queried_by_decision_status()
    {
        var repository = await SeededRepositoryAsync();

        var resolved = await repository.GetByDecisionStatusAsync(ResolutionStatus.Resolved);
        var ambiguous = await repository.GetByDecisionStatusAsync(ResolutionStatus.Ambiguous);

        Assert.Single(resolved);
        Assert.Same(ResolvedEntry, resolved[0]);
        Assert.Single(ambiguous);
        Assert.Same(AmbiguousEntry, ambiguous[0]);
    }

    [Fact]
    public async Task Unresolved_query_returns_unresolved_entries()
    {
        var repository = await SeededRepositoryAsync();

        var unresolved = await repository.GetUnresolvedAsync();

        Assert.Single(unresolved);
        Assert.Same(UnresolvedEntry, unresolved[0]);
        Assert.Equal(QueryArticle99, unresolved[0].Query);
    }
}
