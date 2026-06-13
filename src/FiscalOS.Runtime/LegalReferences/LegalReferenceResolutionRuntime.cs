using FiscalOS.Domain.LegalReferences;
using FiscalOS.LegalCore;

namespace FiscalOS.Runtime.LegalReferences;

public sealed class LegalReferenceResolutionRuntime
{
    private readonly ILegalReferenceResolutionEngine _resolutionEngine;
    private readonly ILegalReferenceResolutionAuditEngine _auditEngine;
    private readonly ILegalReferenceResolutionProvenanceEngine _provenanceEngine;
    private readonly ILegalReferenceResolutionEvidencePackageEngine _evidencePackageEngine;
    private readonly ResolutionEvidencePackageComposer _composer;

    public LegalReferenceResolutionRuntime(
        ILegalReferenceResolutionEngine resolutionEngine,
        ILegalReferenceResolutionAuditEngine auditEngine,
        ILegalReferenceResolutionProvenanceEngine provenanceEngine,
        ILegalReferenceResolutionEvidencePackageEngine evidencePackageEngine,
        ResolutionEvidencePackageComposer composer)
    {
        ArgumentNullException.ThrowIfNull(resolutionEngine);
        ArgumentNullException.ThrowIfNull(auditEngine);
        ArgumentNullException.ThrowIfNull(provenanceEngine);
        ArgumentNullException.ThrowIfNull(evidencePackageEngine);
        ArgumentNullException.ThrowIfNull(composer);

        _resolutionEngine = resolutionEngine;
        _auditEngine = auditEngine;
        _provenanceEngine = provenanceEngine;
        _evidencePackageEngine = evidencePackageEngine;
        _composer = composer;
    }

    public async Task<IReadOnlyList<ResolutionEvidencePackage>> ResolveAsync(
        IReadOnlyList<LegalReference> references,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(references);

        var results = await _resolutionEngine.ResolveAsync(references, cancellationToken);
        var auditTrail = await _auditEngine.AuditAsync(results, cancellationToken);
        var provenances = results.Select(result => ProvenanceFor(result, auditTrail)).ToList();

        await _provenanceEngine.BuildAsync(provenances, cancellationToken);
        var packages = _composer.Compose(results, auditTrail, provenances);

        return await _evidencePackageEngine.ProcessAsync(packages, cancellationToken);
    }

    private static ResolutionProvenance ProvenanceFor(
        ResolutionResult result,
        ResolutionAuditTrail auditTrail)
    {
        var query = QueryFor(result);
        var step = result.Status switch
        {
            ResolutionStatus.Resolved => Step("resolved from runtime resolution result"),
            ResolutionStatus.Ambiguous => Step("recorded ambiguous runtime resolution result"),
            ResolutionStatus.Unresolved => Step("recorded unresolved runtime resolution result"),
            _ => throw new InvalidOperationException($"Unsupported resolution status: {result.Status}."),
        };

        return new ResolutionProvenance(
            query,
            result.Status,
            new ProvenanceChain(new[] { step }),
            auditTrail);
    }

    private static LegalReference QueryFor(ResolutionResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsResolved)
        {
            return result.ResolvedReference?.Reference
                ?? throw new InvalidOperationException("A resolved result must contain exactly one selected candidate.");
        }

        if (result.IsAmbiguous)
        {
            return result.Candidates.FirstOrDefault()?.Reference.Reference
                ?? throw new InvalidOperationException("An ambiguous result must contain at least one candidate.");
        }

        return result.Unresolved?.Query
            ?? throw new InvalidOperationException("An unresolved result must contain unresolved reference details.");
    }

    private static ProvenanceStep Step(string description)
    {
        return new ProvenanceStep(
            new ProvenanceSource("resolution-runtime"),
            description);
    }
}
