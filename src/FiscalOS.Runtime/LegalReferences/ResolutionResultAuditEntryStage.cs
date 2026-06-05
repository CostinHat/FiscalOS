using FiscalOS.Domain.LegalReferences;

namespace FiscalOS.Runtime.LegalReferences;

public sealed class ResolutionResultAuditEntryStage : ILegalReferenceResolutionAuditStage
{
    private readonly IReadOnlyList<ResolutionResult> _results;
    private readonly Func<DateTimeOffset> _timestampProvider;

    public ResolutionResultAuditEntryStage(
        IReadOnlyList<ResolutionResult> results,
        Func<DateTimeOffset> timestampProvider)
    {
        ArgumentNullException.ThrowIfNull(results);
        ArgumentNullException.ThrowIfNull(timestampProvider);

        _results = results;
        _timestampProvider = timestampProvider;
    }

    public string Name => "resolution-result-audit-entry";

    public Task<LegalReferenceResolutionAuditContext> ExecuteAsync(
        LegalReferenceResolutionAuditContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var entries = new List<ResolutionAuditEntry>(context.Entries);
        entries.AddRange(_results.Select(ToEntry));

        return Task.FromResult(context with { Entries = entries });
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
