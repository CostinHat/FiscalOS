using FiscalOS.Domain.LegalReferences;

namespace FiscalOS.Runtime.LegalReferences;

public sealed class LegalReferenceResolutionAuditEngine : ILegalReferenceResolutionAuditEngine
{
    private readonly ILegalReferenceResolutionAuditPipeline _pipeline;
    private readonly Func<DateTimeOffset> _timestampProvider;

    public LegalReferenceResolutionAuditEngine(ILegalReferenceResolutionAuditPipeline pipeline)
        : this(pipeline, () => DateTimeOffset.UtcNow)
    {
    }

    public LegalReferenceResolutionAuditEngine(
        ILegalReferenceResolutionAuditPipeline pipeline,
        Func<DateTimeOffset> timestampProvider)
    {
        ArgumentNullException.ThrowIfNull(pipeline);
        ArgumentNullException.ThrowIfNull(timestampProvider);

        _pipeline = pipeline;
        _timestampProvider = timestampProvider;
    }

    public async Task<ResolutionAuditTrail> AuditAsync(
        IReadOnlyList<ResolutionResult> results,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(results);

        var entries = results.Select(ToEntry).ToList();
        var context = new LegalReferenceResolutionAuditContext(entries);
        var processed = await _pipeline.RunAsync(context, cancellationToken);

        return new ResolutionAuditTrail(processed.Entries);
    }

    private ResolutionAuditEntry ToEntry(ResolutionResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.Status switch
        {
            ResolutionStatus.Resolved => ResolvedEntry(result),
            ResolutionStatus.Ambiguous => AmbiguousEntry(result),
            ResolutionStatus.Unresolved => UnresolvedEntry(result),
            _ => throw new InvalidOperationException($"Unsupported resolution status: {result.Status}."),
        };
    }

    private ResolutionAuditEntry ResolvedEntry(ResolutionResult result)
    {
        var selected = result.ResolvedReference
            ?? throw new InvalidOperationException("A resolved result must contain exactly one selected candidate.");

        return new ResolutionAuditEntry(
            selected.Reference,
            new ResolutionDecision(ResolutionStatus.Resolved, selected),
            new ResolutionEvidence("Resolved legal reference."),
            _timestampProvider());
    }

    private ResolutionAuditEntry AmbiguousEntry(ResolutionResult result)
    {
        var firstCandidate = result.Candidates.FirstOrDefault()
            ?? throw new InvalidOperationException("An ambiguous result must contain at least one candidate.");

        return new ResolutionAuditEntry(
            firstCandidate.Reference.Reference,
            new ResolutionDecision(ResolutionStatus.Ambiguous),
            new ResolutionEvidence("Ambiguous legal reference resolution."),
            _timestampProvider());
    }

    private ResolutionAuditEntry UnresolvedEntry(ResolutionResult result)
    {
        var unresolved = result.Unresolved
            ?? throw new InvalidOperationException("An unresolved result must contain unresolved reference details.");

        return new ResolutionAuditEntry(
            unresolved.Query,
            new ResolutionDecision(ResolutionStatus.Unresolved),
            new ResolutionEvidence(unresolved.Reason),
            _timestampProvider());
    }
}
