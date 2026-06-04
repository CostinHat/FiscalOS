using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FiscalOS.Domain.LegalReferences;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Define_Legal_Reference_Resolution_Audit_Engine_Contract
{
    // Test-only doubles validating the contract shape and semantics. Not
    // production implementations (no audit algorithm, pipeline implementation,
    // repository implementation, or AI/NLP), per FOS-0047 scope. The engine double
    // is composed over a FOS-0046 pipeline whose stage persists via a FOS-0045
    // repository, to exercise the full layering.

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

    // FOS-0047 engine double: a facade that maps results to audit entries and runs
    // them through a FOS-0046 pipeline. The mapping is trivial test logic, not a
    // production audit algorithm.
    private sealed class PipelineBackedAuditEngine : ILegalReferenceResolutionAuditEngine
    {
        private readonly ILegalReferenceResolutionAuditPipeline _pipeline;

        public PipelineBackedAuditEngine(ILegalReferenceResolutionAuditPipeline pipeline) => _pipeline = pipeline;

        public async Task<ResolutionAuditTrail> AuditAsync(IReadOnlyList<ResolutionResult> results, CancellationToken cancellationToken = default)
        {
            var entries = results.Select(ToEntry).ToList();
            var context = new LegalReferenceResolutionAuditContext(entries);
            var processed = await _pipeline.RunAsync(context, cancellationToken);
            return new ResolutionAuditTrail(processed.Entries);
        }

        private static ResolutionAuditEntry ToEntry(ResolutionResult result)
        {
            LegalReference query;
            ResolutionDecision decision;

            switch (result.Status)
            {
                case ResolutionStatus.Resolved:
                    var selected = result.Candidates[0].Reference;
                    query = selected.Reference;
                    decision = new ResolutionDecision(ResolutionStatus.Resolved, selected);
                    break;
                case ResolutionStatus.Ambiguous:
                    query = result.Candidates[0].Reference.Reference;
                    decision = new ResolutionDecision(ResolutionStatus.Ambiguous);
                    break;
                default:
                    query = result.Unresolved!.Query;
                    decision = new ResolutionDecision(ResolutionStatus.Unresolved);
                    break;
            }

            return new ResolutionAuditEntry(query, decision, new ResolutionEvidence($"audited status {result.Status}"), default);
        }
    }

    private static ReferenceSegment Seg(string kind, string value) => new(kind, value);

    private static LegalReference Address(params ReferenceSegment[] segments) => new(segments);

    private static FullyQualifiedLegalReference Fq(string document, params ReferenceSegment[] segments) =>
        new(new LegalDocumentReference(document), new LegalReference(segments));

    private static ResolutionResult Resolved(FullyQualifiedLegalReference reference) =>
        new(ResolutionStatus.Resolved, new[] { new ResolutionCandidate(reference) }, Unresolved: null);

    private static ResolutionResult Ambiguous(params FullyQualifiedLegalReference[] references) =>
        new(ResolutionStatus.Ambiguous, references.Select(r => new ResolutionCandidate(r)).ToList(), Unresolved: null);

    private static ResolutionResult Unresolved(LegalReference query, string reason) =>
        new(ResolutionStatus.Unresolved, Array.Empty<ResolutionCandidate>(), new UnresolvedReference(query, reason));

    private static ILegalReferenceResolutionAuditEngine EngineWith(InMemoryLegalReferenceResolutionAuditRepository repository) =>
        new PipelineBackedAuditEngine(new SequentialPipeline(new PersistTrailStage(repository)));

    [Fact]
    public async Task Engine_produces_an_audit_trail_for_results()
    {
        var engine = EngineWith(new InMemoryLegalReferenceResolutionAuditRepository());
        var resolved = Resolved(Fq("Legea 227/2015", Seg("Article", "47"), Seg("Paragraph", "3")));

        var trail = await engine.AuditAsync(new[] { resolved });

        Assert.Single(trail.Entries);
        Assert.Equal(ResolutionStatus.Resolved, trail.Entries[0].Decision.Status);
    }

    [Fact]
    public async Task Engine_produces_one_entry_per_result()
    {
        var engine = EngineWith(new InMemoryLegalReferenceResolutionAuditRepository());
        var results = new[]
        {
            Resolved(Fq("Legea 227/2015", Seg("Article", "47"), Seg("Paragraph", "3"))),
            Ambiguous(Fq("Legea 227/2015", Seg("Article", "48")), Fq("OUG 1/2020", Seg("Article", "48"))),
            Unresolved(Address(Seg("Article", "99")), "no matching document"),
        };

        var trail = await engine.AuditAsync(results);

        Assert.Equal(3, trail.Entries.Count);
    }

    [Fact]
    public async Task Engine_returns_an_empty_trail_for_no_results()
    {
        var engine = EngineWith(new InMemoryLegalReferenceResolutionAuditRepository());

        var trail = await engine.AuditAsync(Array.Empty<ResolutionResult>());

        Assert.True(trail.IsEmpty);
    }

    [Fact]
    public async Task Engine_persists_the_audit_trail_through_the_repository()
    {
        var repository = new InMemoryLegalReferenceResolutionAuditRepository();
        var engine = EngineWith(repository);

        await engine.AuditAsync(new[] { Resolved(Fq("Legea 227/2015", Seg("Article", "47"), Seg("Paragraph", "3"))) });

        var stored = await repository.GetByDecisionStatusAsync(ResolutionStatus.Resolved);
        Assert.Single(stored);
    }
}
