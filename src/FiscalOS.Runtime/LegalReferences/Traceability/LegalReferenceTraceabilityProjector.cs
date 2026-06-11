using FiscalOS.Domain.LegalReferences;

namespace FiscalOS.Runtime.LegalReferences.Traceability;

public sealed class LegalReferenceTraceabilityProjector
{
    private const string ResolvedLinkage = "This answer is linked to the requested reference and cites the resolved legal reference.";
    private const string AmbiguousLinkage = "The requested reference matched multiple possible legal references, so no single citation was selected.";
    private const string UnresolvedLinkage = "The requested reference could not be resolved from the available legal reference data.";

    private static readonly IReadOnlyList<string> DefaultLimitations = new[]
    {
        "This traceability summary is provided for decision support and does not replace professional judgment.",
        "It links the response to available reference data; it is not a compliance guarantee.",
    };

    public TraceabilitySummary Project(
        ResolutionEvidencePackage package,
        string correlationId,
        LegalReference requestedReference)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(requestedReference);

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            throw new ArgumentException("Correlation id cannot be empty.", nameof(correlationId));
        }

        var status = ToTraceabilityStatus(package.Result.Status);
        var citation = package.Result.IsResolved
            ? CitationFor(package.Result.ResolvedReference
                ?? throw new InvalidOperationException("A resolved result must contain exactly one selected candidate."))
            : null;
        var linkage = status switch
        {
            TraceabilityStatus.Resolved => ResolvedLinkage,
            TraceabilityStatus.Ambiguous => AmbiguousLinkage,
            TraceabilityStatus.Unresolved => UnresolvedLinkage,
            _ => throw new InvalidOperationException($"Unsupported traceability status: {status}."),
        };

        return new TraceabilitySummary(
            correlationId.Trim(),
            ReferenceFor(requestedReference),
            status,
            citation,
            SourceSummary: null,
            linkage,
            DefaultLimitations);
    }

    private static TraceabilityStatus ToTraceabilityStatus(ResolutionStatus status)
    {
        return status switch
        {
            ResolutionStatus.Resolved => TraceabilityStatus.Resolved,
            ResolutionStatus.Ambiguous => TraceabilityStatus.Ambiguous,
            ResolutionStatus.Unresolved => TraceabilityStatus.Unresolved,
            _ => throw new InvalidOperationException($"Unsupported resolution status: {status}."),
        };
    }

    private static PublicLegalCitation CitationFor(FullyQualifiedLegalReference reference)
    {
        return new PublicLegalCitation(
            $"{reference.Document}: {reference.Reference}",
            reference.Document.Value,
            ReferenceFor(reference.Reference));
    }

    private static PublicLegalReference ReferenceFor(LegalReference reference)
    {
        return new PublicLegalReference(
            reference.ToString(),
            reference.Segments
                .Select(segment => new PublicReferenceSegment(segment.Kind, segment.Value))
                .ToArray());
    }
}
