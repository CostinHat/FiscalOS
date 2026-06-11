using FiscalOS.Domain.LegalReferences;
using FiscalOS.Runtime.LegalReferences.Traceability;

namespace FiscalOS.Runtime.LegalReferences.Embedded;

public sealed class EmbeddedLegalReferenceFeature
{
    private readonly LegalReferenceResolutionRuntime _runtime;
    private readonly LegalReferenceTraceabilityProjector _traceabilityProjector;

    public EmbeddedLegalReferenceFeature(
        LegalReferenceResolutionRuntime runtime,
        LegalReferenceTraceabilityProjector traceabilityProjector)
    {
        ArgumentNullException.ThrowIfNull(runtime);
        ArgumentNullException.ThrowIfNull(traceabilityProjector);

        _runtime = runtime;
        _traceabilityProjector = traceabilityProjector;
    }

    public async Task<EmbeddedLegalReferenceResponse> ResolveAsync(
        EmbeddedLegalReferenceRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Reference);

        var correlationId = NormalizeCorrelationId(request.CorrelationId);
        var packages = await _runtime.ResolveAsync(new[] { request.Reference }, cancellationToken);
        var package = SinglePackage(packages);
        var citation = package.Result.IsResolved
            ? CitationFor(package.Result.ResolvedReference
                ?? throw new InvalidOperationException("A resolved result must contain exactly one selected candidate."))
            : null;
        var traceabilitySummary = request.IncludeTraceability
            ? _traceabilityProjector.Project(package, correlationId, request.Reference)
            : null;

        return new EmbeddedLegalReferenceResponse(
            ToEmbeddedStatus(package.Result.Status),
            AnswerFor(package.Result, citation),
            citation,
            ExplanationFor(package.Result),
            correlationId,
            traceabilitySummary);
    }

    private static string NormalizeCorrelationId(string correlationId)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            throw new ArgumentException("Correlation id cannot be empty.", nameof(correlationId));
        }

        return correlationId.Trim();
    }

    private static ResolutionEvidencePackage SinglePackage(IReadOnlyList<ResolutionEvidencePackage> packages)
    {
        if (packages.Count != 1)
        {
            throw new InvalidOperationException("Embedded legal reference resolution expects exactly one evidence package.");
        }

        return packages[0];
    }

    private static EmbeddedLegalReferenceStatus ToEmbeddedStatus(ResolutionStatus status)
    {
        return status switch
        {
            ResolutionStatus.Resolved => EmbeddedLegalReferenceStatus.Resolved,
            ResolutionStatus.Ambiguous => EmbeddedLegalReferenceStatus.Ambiguous,
            ResolutionStatus.Unresolved => EmbeddedLegalReferenceStatus.Unresolved,
            _ => throw new InvalidOperationException($"Unsupported resolution status: {status}."),
        };
    }

    private static string? AnswerFor(ResolutionResult result, PublicLegalCitation? citation)
    {
        if (result.IsResolved)
        {
            return $"Resolved to {citation?.Display}.";
        }

        return null;
    }

    private static string ExplanationFor(ResolutionResult result)
    {
        return result.Status switch
        {
            ResolutionStatus.Resolved => "The requested legal reference was resolved to a citation.",
            ResolutionStatus.Ambiguous => "The requested legal reference matched multiple possible references; no single citation was selected.",
            ResolutionStatus.Unresolved => "The requested legal reference could not be resolved from the available legal reference data.",
            _ => throw new InvalidOperationException($"Unsupported resolution status: {result.Status}."),
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
