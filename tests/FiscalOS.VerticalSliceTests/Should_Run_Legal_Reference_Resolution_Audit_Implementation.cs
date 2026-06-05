using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FiscalOS.Domain.LegalReferences;
using FiscalOS.Runtime.LegalReferences;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Run_Legal_Reference_Resolution_Audit_Implementation
{
    private sealed class RecordingAuditStage : ILegalReferenceResolutionAuditStage
    {
        private readonly string _marker;
        private readonly List<string> _executionOrder;

        public RecordingAuditStage(string marker, List<string> executionOrder)
        {
            _marker = marker;
            _executionOrder = executionOrder;
        }

        public string Name => $"record-audit-{_marker}";

        public Task<LegalReferenceResolutionAuditContext> ExecuteAsync(
            LegalReferenceResolutionAuditContext context,
            CancellationToken cancellationToken = default)
        {
            _executionOrder.Add(_marker);
            return Task.FromResult(context with
            {
                Entries = context.Entries.Concat(new[] { AuditEntry(Address(Seg("Marker", _marker))) }).ToList(),
            });
        }
    }

    private sealed class StubAuditPipeline : ILegalReferenceResolutionAuditPipeline
    {
        private readonly IReadOnlyList<ResolutionAuditEntry> _entries;

        public StubAuditPipeline(IReadOnlyList<ResolutionAuditEntry> entries) => _entries = entries;

        public LegalReferenceResolutionAuditContext? ReceivedContext { get; private set; }

        public Task<LegalReferenceResolutionAuditContext> RunAsync(
            LegalReferenceResolutionAuditContext context,
            CancellationToken cancellationToken = default)
        {
            ReceivedContext = context;
            return Task.FromResult(context with { Entries = _entries });
        }
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

    private static ResolutionAuditEntry AuditEntry(LegalReference query) =>
        new(query, new ResolutionDecision(ResolutionStatus.Unresolved), new ResolutionEvidence("test"), At);

    private static ResolutionResult Resolved(FullyQualifiedLegalReference reference) =>
        new(ResolutionStatus.Resolved, new[] { new ResolutionCandidate(reference) }, Unresolved: null);

    private static ResolutionResult Ambiguous(params FullyQualifiedLegalReference[] references) =>
        new(ResolutionStatus.Ambiguous, references.Select(reference => new ResolutionCandidate(reference)).ToList(), Unresolved: null);

    private static ResolutionResult Unresolved(LegalReference query, string reason) =>
        new(ResolutionStatus.Unresolved, Array.Empty<ResolutionCandidate>(), new UnresolvedReference(query, reason));

    private static readonly LegalReference QueryParagraph = Address(Seg("Article", "47"), Seg("Paragraph", "3"));
    private static readonly LegalReference QueryArticle48 = Address(Seg("Article", "48"));

    [Fact]
    public async Task Audit_pipeline_runs_configured_stages_in_order()
    {
        var executionOrder = new List<string>();
        var pipeline = new LegalReferenceResolutionAuditPipeline(
            new RecordingAuditStage("first", executionOrder),
            new RecordingAuditStage("second", executionOrder));
        var context = new LegalReferenceResolutionAuditContext(Array.Empty<ResolutionAuditEntry>());

        var result = await pipeline.RunAsync(context);

        Assert.Equal(new[] { "first", "second" }, executionOrder);
        Assert.Equal(2, result.Entries.Count);
        Assert.Empty(context.Entries);
    }

    [Fact]
    public async Task Persist_stage_stores_audit_trail_without_changing_context()
    {
        var repository = new InMemoryLegalReferenceResolutionAuditRepository();
        var entry = AuditEntry(QueryParagraph);
        var stage = new PersistResolutionAuditTrailStage(repository);
        var context = new LegalReferenceResolutionAuditContext(new[] { entry });

        var result = await stage.ExecuteAsync(context);

        var stored = await repository.GetAsync(QueryParagraph);
        Assert.Equal("persist-resolution-audit-trail", stage.Name);
        Assert.Same(context, result);
        Assert.NotNull(stored);
        Assert.Single(stored!.Entries);
        Assert.Same(entry, stored.Entries[0]);
    }

    [Fact]
    public async Task Audit_mapping_stage_maps_results_to_audit_entries()
    {
        var selected = Fq("Legea 227/2015", Seg("Article", "47"), Seg("Paragraph", "3"));
        var other = Fq("OUG 1/2020", Seg("Article", "48"));
        var stage = new ResolutionResultAuditEntryStage(
            new[]
            {
                Resolved(selected),
                Ambiguous(Fq("Legea 227/2015", Seg("Article", "48")), other),
                Unresolved(Address(Seg("Article", "99")), "No stored resolution found."),
            },
            () => At);
        var context = new LegalReferenceResolutionAuditContext(Array.Empty<ResolutionAuditEntry>());

        var result = await stage.ExecuteAsync(context);

        Assert.Equal("resolution-result-audit-entry", stage.Name);
        Assert.Equal(3, result.Entries.Count);
        Assert.Equal(ResolutionStatus.Resolved, result.Entries[0].Decision.Status);
        Assert.Equal(selected, result.Entries[0].Decision.SelectedReference);
        Assert.Equal(QueryArticle48, result.Entries[1].Query);
        Assert.Equal(ResolutionStatus.Ambiguous, result.Entries[1].Decision.Status);
        Assert.Equal("No stored resolution found.", result.Entries[2].Evidence.Description);
        Assert.All(result.Entries, entry => Assert.Equal(At, entry.Timestamp));
        Assert.Empty(context.Entries);
    }

    [Fact]
    public async Task Audit_mapping_stage_derives_ambiguous_query_from_the_first_candidate()
    {
        var first = Fq("Legea 227/2015", Seg("Article", "48"));
        var second = Fq("OUG 1/2020", Seg("Article", "49"));
        var stage = new ResolutionResultAuditEntryStage(
            new[] { Ambiguous(first, second) },
            () => At);

        var result = await stage.ExecuteAsync(new LegalReferenceResolutionAuditContext(Array.Empty<ResolutionAuditEntry>()));

        Assert.Single(result.Entries);
        Assert.Equal(first.Reference, result.Entries[0].Query);
        Assert.NotEqual(second.Reference, result.Entries[0].Query);
    }

    [Fact]
    public async Task Audit_engine_maps_results_to_audit_entries()
    {
        var selected = Fq("Legea 227/2015", Seg("Article", "47"), Seg("Paragraph", "3"));
        var other = Fq("OUG 1/2020", Seg("Article", "48"));
        var engine = new LegalReferenceResolutionAuditEngine(
            new LegalReferenceResolutionAuditPipeline(),
            () => At);

        var trail = await engine.AuditAsync(new[]
        {
            Resolved(selected),
            Ambiguous(Fq("Legea 227/2015", Seg("Article", "48")), other),
            Unresolved(Address(Seg("Article", "99")), "No stored resolution found."),
        });

        Assert.Equal(3, trail.Entries.Count);
        Assert.Equal(ResolutionStatus.Resolved, trail.Entries[0].Decision.Status);
        Assert.Equal(selected, trail.Entries[0].Decision.SelectedReference);
        Assert.Equal(QueryArticle48, trail.Entries[1].Query);
        Assert.Equal(ResolutionStatus.Ambiguous, trail.Entries[1].Decision.Status);
        Assert.Equal("No stored resolution found.", trail.Entries[2].Evidence.Description);
        Assert.All(trail.Entries, entry => Assert.Equal(At, entry.Timestamp));
    }

    [Fact]
    public async Task Audit_engine_delegates_to_pipeline_and_returns_processed_trail()
    {
        var processedEntry = AuditEntry(QueryArticle48);
        var pipeline = new StubAuditPipeline(new[] { processedEntry });
        var engine = new LegalReferenceResolutionAuditEngine(pipeline, () => At);

        var trail = await engine.AuditAsync(new[] { Resolved(Fq("Legea 227/2015", Seg("Article", "47"), Seg("Paragraph", "3"))) });

        Assert.Single(trail.Entries);
        Assert.Same(processedEntry, trail.Entries[0]);
        Assert.NotNull(pipeline.ReceivedContext);
        Assert.Single(pipeline.ReceivedContext!.Entries);
    }
}
