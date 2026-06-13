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

public sealed class Should_Audit_Legal_Reference_Resolution_Through_Runtime_Audit_Vertical
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
            => Task.FromResult<IReadOnlyList<ResolutionAuditEntry>>(_entries.Where(entry => entry.Decision.Status == status).ToList());

        public Task<IReadOnlyList<ResolutionAuditEntry>> GetUnresolvedAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ResolutionAuditEntry>>(_entries.Where(entry => entry.Decision.Status == ResolutionStatus.Unresolved).ToList());
    }

    private static readonly DateTimeOffset At = new(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);

    private static ReferenceSegment Seg(string kind, string value) => new(kind, value);

    private static LegalReference Address(params ReferenceSegment[] segments) => new(segments);

    private static FullyQualifiedLegalReference Fq(string document, params ReferenceSegment[] segments) =>
        new(new LegalDocumentReference(document), new LegalReference(segments));

    private static ResolutionResult Resolved(FullyQualifiedLegalReference reference) =>
        new(ResolutionStatus.Resolved, new[] { new ResolutionCandidate(reference) }, Unresolved: null);

    private static readonly LegalReference QueryParagraph = Address(Seg("Article", "47"), Seg("Paragraph", "3"));
    private static readonly LegalReference QueryArticle48 = Address(Seg("Article", "48"));

    [Fact]
    public async Task Runtime_audit_engine_audits_results_from_runtime_resolution_engine()
    {
        var resolutionRepository = new InMemoryLegalReferenceResolutionRepository();
        var selected = Fq("Legea 227/2015", Seg("Article", "47"), Seg("Paragraph", "3"));
        await resolutionRepository.StoreAsync(QueryParagraph, Resolved(selected));

        ILegalReferenceResolutionEngine resolutionEngine = new LegalReferenceResolutionEngine(
            new LegalReferenceResolutionPipeline(
                new RepositoryLegalReferenceResolutionStage(resolutionRepository)));

        var auditRepository = new InMemoryLegalReferenceResolutionAuditRepository();
        ILegalReferenceResolutionAuditEngine auditEngine = new LegalReferenceResolutionAuditEngine(
            new LegalReferenceResolutionAuditPipeline(
                new PersistResolutionAuditTrailStage(auditRepository)),
            () => At);

        var results = await resolutionEngine.ResolveAsync(new[] { QueryParagraph, QueryArticle48 });
        var trail = await auditEngine.AuditAsync(results);

        Assert.Equal(2, trail.Entries.Count);
        Assert.Equal(ResolutionStatus.Resolved, trail.Entries[0].Decision.Status);
        Assert.Equal(selected, trail.Entries[0].Decision.SelectedReference);
        Assert.Equal(ResolutionStatus.Unresolved, trail.Entries[1].Decision.Status);
        Assert.Equal(QueryArticle48, trail.Entries[1].Query);

        var storedResolved = await auditRepository.GetByDecisionStatusAsync(ResolutionStatus.Resolved);
        var storedUnresolved = await auditRepository.GetUnresolvedAsync();

        Assert.Single(storedResolved);
        Assert.Single(storedUnresolved);
        Assert.All(trail.Entries, entry => Assert.Equal(At, entry.Timestamp));
    }
}
