using FiscalOS.Domain.LegalReferences;

namespace FiscalOS.Runtime.LegalReferences;

public sealed class ResolutionEvidencePackageComposer
{
    public IReadOnlyList<ResolutionEvidencePackage> Compose(
        IReadOnlyList<ResolutionResult> results,
        ResolutionAuditTrail auditTrail,
        IReadOnlyList<ResolutionProvenance> provenances)
    {
        ArgumentNullException.ThrowIfNull(results);
        ArgumentNullException.ThrowIfNull(auditTrail);
        ArgumentNullException.ThrowIfNull(provenances);

        return results.Select(result =>
        {
            var query = QueryFor(result);
            var trail = TrailFor(query, auditTrail);
            var provenance = ProvenanceFor(query, provenances);

            return new ResolutionEvidencePackage(result, trail, provenance);
        }).ToList();
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

    private static ResolutionAuditTrail TrailFor(LegalReference query, ResolutionAuditTrail auditTrail)
    {
        var entries = auditTrail.Entries
            .Where(entry => entry.Query.Equals(query))
            .ToList();

        if (entries.Count == 0)
        {
            throw new InvalidOperationException($"No audit entries were found for legal reference '{query}'.");
        }

        return new ResolutionAuditTrail(entries);
    }

    private static ResolutionProvenance ProvenanceFor(
        LegalReference query,
        IReadOnlyList<ResolutionProvenance> provenances)
    {
        return provenances.FirstOrDefault(provenance => provenance.Query.Equals(query))
            ?? throw new InvalidOperationException($"No provenance was found for legal reference '{query}'.");
    }
}
