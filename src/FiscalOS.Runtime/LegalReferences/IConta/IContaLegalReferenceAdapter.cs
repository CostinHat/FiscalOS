using FiscalOS.Domain.LegalReferences;
using FiscalOS.Runtime.LegalReferences.Embedded;
using FiscalOS.Runtime.LegalReferences.Traceability;

namespace FiscalOS.Runtime.LegalReferences.IConta;

public sealed class IContaLegalReferenceAdapter
{
    private const string ValidationError = "validation_error";
    private const string AuthorizationDenied = "authorization_denied";
    private const string RuntimeError = "runtime_error";

    private readonly IEmbeddedLegalReferenceFeature _embeddedFeature;
    private readonly IIContaLegalReferenceOperationalSink _operationalSink;

    public IContaLegalReferenceAdapter(
        IEmbeddedLegalReferenceFeature embeddedFeature,
        IIContaLegalReferenceOperationalSink operationalSink)
    {
        ArgumentNullException.ThrowIfNull(embeddedFeature);
        ArgumentNullException.ThrowIfNull(operationalSink);

        _embeddedFeature = embeddedFeature;
        _operationalSink = operationalSink;
    }

    public async Task<IContaLegalReferenceAdapterResult> ResolveAsync(
        IContaLegalReferenceRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationError = Validate(request);
        if (validationError is not null)
        {
            Record("validation_failed", request, status: null, outcome: ValidationError);
            return IContaLegalReferenceAdapterResult.Failure(validationError);
        }

        if (!request.Authorization.IsAuthorized)
        {
            Record("authorization_denied", request, status: null, outcome: AuthorizationDenied);
            return IContaLegalReferenceAdapterResult.Failure(new IContaLegalReferenceApplicationError(
                AuthorizationDenied,
                "The current workflow is not authorized to resolve this legal reference.",
                request.CorrelationId.Trim()));
        }

        try
        {
            var embeddedRequest = new EmbeddedLegalReferenceRequest(
                ReferenceFor(request.Segments),
                request.CorrelationId.Trim(),
                request.IncludeTraceability);
            var embeddedResponse = await _embeddedFeature.ResolveAsync(embeddedRequest, cancellationToken);
            var response = ResponseFor(embeddedResponse);

            Record("request_completed", request, response.Status, "success");

            return IContaLegalReferenceAdapterResult.Success(response);
        }
        catch (Exception)
        {
            Record("unexpected_failure", request, status: null, outcome: RuntimeError);

            return IContaLegalReferenceAdapterResult.Failure(new IContaLegalReferenceApplicationError(
                RuntimeError,
                "The legal reference traceability request could not be completed.",
                request.CorrelationId.Trim()));
        }
    }

    private static IContaLegalReferenceApplicationError? Validate(IContaLegalReferenceRequest request)
    {
        if (request is null)
        {
            return new IContaLegalReferenceApplicationError(
                ValidationError,
                "Request is required.",
                CorrelationId: null);
        }

        if (string.IsNullOrWhiteSpace(request.CorrelationId))
        {
            return new IContaLegalReferenceApplicationError(
                ValidationError,
                "Correlation id is required.",
                CorrelationId: null);
        }

        if (request.Authorization is null)
        {
            return new IContaLegalReferenceApplicationError(
                ValidationError,
                "Authorization context is required.",
                request.CorrelationId.Trim());
        }

        if (request.OperationalMetadata is null)
        {
            return new IContaLegalReferenceApplicationError(
                ValidationError,
                "Operational metadata is required.",
                request.CorrelationId.Trim());
        }

        if (request.Segments is null || request.Segments.Count == 0)
        {
            return new IContaLegalReferenceApplicationError(
                ValidationError,
                "At least one legal reference segment is required.",
                request.CorrelationId.Trim());
        }

        if (request.Segments.Any(segment =>
                segment is null
                || string.IsNullOrWhiteSpace(segment.Kind)
                || string.IsNullOrWhiteSpace(segment.Value)))
        {
            return new IContaLegalReferenceApplicationError(
                ValidationError,
                "Legal reference segment kind and value are required.",
                request.CorrelationId.Trim());
        }

        return null;
    }

    private static LegalReference ReferenceFor(IReadOnlyList<IContaLegalReferenceSegment> segments)
    {
        return new LegalReference(segments
            .Select(segment => new ReferenceSegment(segment.Kind.Trim(), segment.Value.Trim()))
            .ToArray());
    }

    private static IContaLegalReferenceResponse ResponseFor(EmbeddedLegalReferenceResponse response)
    {
        return new IContaLegalReferenceResponse(
            ToIContaStatus(response.Status),
            response.Answer,
            response.Citation is null ? null : CitationFor(response.Citation),
            response.Explanation,
            response.CorrelationId,
            response.TraceabilitySummary is null ? null : TraceabilityFor(response.TraceabilitySummary));
    }

    private static IContaLegalReferenceStatus ToIContaStatus(EmbeddedLegalReferenceStatus status)
    {
        return status switch
        {
            EmbeddedLegalReferenceStatus.Resolved => IContaLegalReferenceStatus.Resolved,
            EmbeddedLegalReferenceStatus.Ambiguous => IContaLegalReferenceStatus.Ambiguous,
            EmbeddedLegalReferenceStatus.Unresolved => IContaLegalReferenceStatus.Unresolved,
            _ => throw new InvalidOperationException($"Unsupported embedded status: {status}."),
        };
    }

    private static IContaTraceabilitySummary TraceabilityFor(TraceabilitySummary summary)
    {
        return new IContaTraceabilitySummary(
            summary.CorrelationId,
            ReferenceFor(summary.RequestedReference),
            ToIContaStatus(summary.Status),
            summary.Citation is null ? null : CitationFor(summary.Citation),
            summary.SourceSummary,
            summary.Linkage,
            summary.Limitations.ToArray());
    }

    private static IContaLegalReferenceStatus ToIContaStatus(TraceabilityStatus status)
    {
        return status switch
        {
            TraceabilityStatus.Resolved => IContaLegalReferenceStatus.Resolved,
            TraceabilityStatus.Ambiguous => IContaLegalReferenceStatus.Ambiguous,
            TraceabilityStatus.Unresolved => IContaLegalReferenceStatus.Unresolved,
            _ => throw new InvalidOperationException($"Unsupported traceability status: {status}."),
        };
    }

    private static IContaLegalCitation CitationFor(PublicLegalCitation citation)
    {
        return new IContaLegalCitation(
            citation.Display,
            citation.Document,
            ReferenceFor(citation.Reference));
    }

    private static IContaPublicLegalReference ReferenceFor(PublicLegalReference reference)
    {
        return new IContaPublicLegalReference(
            reference.Display,
            reference.Segments
                .Select(segment => new IContaPublicReferenceSegment(segment.Kind, segment.Value))
                .ToArray());
    }

    private void Record(
        string name,
        IContaLegalReferenceRequest? request,
        IContaLegalReferenceStatus? status,
        string outcome)
    {
        _operationalSink.Record(new IContaLegalReferenceOperationalEvent(
            name,
            string.IsNullOrWhiteSpace(request?.CorrelationId) ? null : request.CorrelationId.Trim(),
            request?.OperationalMetadata?.TenantId,
            request?.OperationalMetadata?.AccountId,
            request?.OperationalMetadata?.WorkflowId,
            status,
            request?.IncludeTraceability,
            outcome));
    }
}
