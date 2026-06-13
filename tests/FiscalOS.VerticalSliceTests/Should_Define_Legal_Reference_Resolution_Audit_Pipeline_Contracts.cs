using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
using FiscalOS.Domain.LegalReferences;
using FiscalOS.LegalCore;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Define_Legal_Reference_Resolution_Audit_Pipeline_Contracts
{
    // Test-only doubles validating the contract shapes and semantics. Not
    // production implementations (no audit engine, repository implementation,
    // resolution engine, or AI/NLP), per FOS-0046 scope.

    private sealed class AppendEntryStage : ILegalReferenceResolutionAuditStage
    {
        private readonly ResolutionAuditEntry _entry;

        public AppendEntryStage(ResolutionAuditEntry entry) => _entry = entry;

        public string Name => "append-entry";

        public Task<LegalReferenceResolutionAuditContext> ExecuteAsync(LegalReferenceResolutionAuditContext context, CancellationToken cancellationToken = default)
        {
            var entries = new List<ResolutionAuditEntry>(context.Entries) { _entry };
            return Task.FromResult(context with { Entries = entries });
        }
    }

    // A terminal stage that persists the accumulated entries as a trail through
    // the FOS-0045 audit repository abstraction.
    private sealed class PersistTrailStage : ILegalReferenceResolutionAuditStage
    {
        private readonly ILegalReferenceResolutionAuditRepository _repository;

        public PersistTrailStage(ILegalReferenceResolutionAuditRepository repository) => _repository = repository;

        public string Name => "persist-trail";

        public async Task<LegalReferenceResolutionAuditContext> ExecuteAsync(LegalReferenceResolutionAuditContext context, CancellationToken cancellationToken = default)
        {
            await _repository.StoreAsync(new ResolutionAuditTrail(context.Entries), cancellationToken);
            return context;
        }
    }

    private sealed class SequentialPipeline : ILegalReferenceResolutionAuditPipeline
    {
        private readonly IReadOnlyList<ILegalReferenceResolutionAuditStage> _stages;

        public SequentialPipeline(params ILegalReferenceResolutionAuditStage[] stages) => _stages = stages;

        public async Task<LegalReferenceResolutionAuditContext> RunAsync(LegalReferenceResolutionAuditContext context, CancellationToken cancellationToken = default)
        {
            var current = context;
            foreach (var stage in _stages)
            {
                current = await stage.ExecuteAsync(current, cancellationToken);
            }

            return current;
        }
    }

    // Minimal in-memory audit repository double (FOS-0045), not production.
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

    private static readonly DateTimeOffset At = new(2026, 6, 4, 12, 0, 0, TimeSpan.Zero);

    private static ReferenceSegment Seg(string kind, string value) => new(kind, value);

    private static LegalReference Address(params ReferenceSegment[] segments) => new(segments);

    private static FullyQualifiedLegalReference Fq(string document, params ReferenceSegment[] segments) =>
        new(new LegalDocumentReference(document), new LegalReference(segments));

    private static readonly LegalReference QueryParagraph = Address(Seg("Article", "47"), Seg("Paragraph", "3"));

    private static ResolutionAuditEntry ResolvedEntry() =>
        new(QueryParagraph,
            new ResolutionDecision(ResolutionStatus.Resolved, Fq("Legea 227/2015", Seg("Article", "47"), Seg("Paragraph", "3"))),
            new ResolutionEvidence("matched a single document"),
            At);

    private static ResolutionAuditEntry UnresolvedEntry() =>
        new(Address(Seg("Article", "99")),
            new ResolutionDecision(ResolutionStatus.Unresolved),
            new ResolutionEvidence("no matching document"),
            At);

    private static LegalReferenceResolutionAuditContext Context() =>
        new(Array.Empty<ResolutionAuditEntry>());

    [Fact]
    public async Task Stage_executes_and_returns_an_updated_context()
    {
        var entry = ResolvedEntry();
        ILegalReferenceResolutionAuditStage stage = new AppendEntryStage(entry);
        var context = Context();

        var updated = await stage.ExecuteAsync(context);

        Assert.Equal("append-entry", stage.Name);
        Assert.Single(updated.Entries);
        Assert.Same(entry, updated.Entries[0]);
        // Original context is unchanged (immutable).
        Assert.Empty(context.Entries);
    }

    [Fact]
    public async Task Pipeline_runs_stages_and_produces_a_context()
    {
        ILegalReferenceResolutionAuditPipeline pipeline = new SequentialPipeline(
            new AppendEntryStage(ResolvedEntry()),
            new AppendEntryStage(UnresolvedEntry()));

        var result = await pipeline.RunAsync(Context());

        Assert.Equal(2, result.Entries.Count);
    }

    [Fact]
    public async Task Pipeline_can_persist_the_audit_trail_through_the_repository()
    {
        var repository = new InMemoryLegalReferenceResolutionAuditRepository();
        ILegalReferenceResolutionAuditPipeline pipeline = new SequentialPipeline(
            new AppendEntryStage(ResolvedEntry()),
            new PersistTrailStage(repository));

        await pipeline.RunAsync(Context());

        var trail = await repository.GetAsync(QueryParagraph);
        var resolved = await repository.GetByDecisionStatusAsync(ResolutionStatus.Resolved);

        Assert.NotNull(trail);
        Assert.Single(trail!.Entries);
        Assert.Single(resolved);
        Assert.Equal(ResolutionStatus.Resolved, resolved[0].Decision.Status);
    }
}
