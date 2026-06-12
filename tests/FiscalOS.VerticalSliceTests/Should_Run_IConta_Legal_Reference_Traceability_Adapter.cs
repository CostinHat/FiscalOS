using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FiscalOS.Domain.LegalReferences;
using FiscalOS.Runtime.LegalReferences;
using FiscalOS.Runtime.LegalReferences.Embedded;
using FiscalOS.Runtime.LegalReferences.IConta;
using FiscalOS.Runtime.LegalReferences.Traceability;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Run_IConta_Legal_Reference_Traceability_Adapter
{
    private static readonly IContaOperationalMetadata Metadata = new("tenant-1", "account-1", "workflow-1");
    private static readonly IContaAuthorizationContext Authorized = new(
        IsAuthorized: true,
        DenialReason: null,
        UserId: "user-1",
        TenantId: "tenant-1",
        AccountId: "account-1",
        WorkflowId: "workflow-1");

    private static readonly IContaAuthorizationContext Denied = Authorized with
    {
        IsAuthorized = false,
        DenialReason = "missing permission",
    };

    private static IContaLegalReferenceRequest Request(
        bool includeTraceability = true,
        IContaAuthorizationContext? authorization = null,
        string correlationId = " corr-1 ",
        params IContaLegalReferenceSegment[] segments)
    {
        return new IContaLegalReferenceRequest(
            segments.Length == 0 ? new[] { new IContaLegalReferenceSegment(" Article ", " 47 ") } : segments,
            correlationId,
            includeTraceability,
            authorization ?? Authorized,
            Metadata);
    }

    [Fact]
    public async Task Authorized_request_invokes_embedded_feature()
    {
        var embedded = new StubEmbeddedFeature(Response(includeTraceability: true));
        var adapter = AdapterWith(embedded);

        var result = await adapter.ResolveAsync(Request());

        Assert.True(result.IsSuccess);
        Assert.Equal(1, embedded.CallCount);
    }

    [Fact]
    public async Task Unauthorized_request_does_not_invoke_embedded_feature()
    {
        var embedded = new StubEmbeddedFeature(Response(includeTraceability: true));
        var adapter = AdapterWith(embedded);

        var result = await adapter.ResolveAsync(Request(authorization: Denied));

        Assert.False(result.IsSuccess);
        Assert.Equal("authorization_denied", result.Error!.Code);
        Assert.Equal(0, embedded.CallCount);
    }

    [Fact]
    public async Task Invalid_request_does_not_invoke_embedded_feature()
    {
        var embedded = new StubEmbeddedFeature(Response(includeTraceability: true));
        var adapter = AdapterWith(embedded);

        var result = await adapter.ResolveAsync(Request(correlationId: " "));

        Assert.False(result.IsSuccess);
        Assert.Equal("validation_error", result.Error!.Code);
        Assert.Equal(0, embedded.CallCount);
    }

    [Fact]
    public async Task Input_mapping_preserves_requested_reference_identity()
    {
        var embedded = new StubEmbeddedFeature(Response(includeTraceability: false));
        var adapter = AdapterWith(embedded);

        await adapter.ResolveAsync(Request(segments: new IContaLegalReferenceSegment("Article", "999")));

        Assert.NotNull(embedded.LastRequest);
        Assert.Equal("Article 999", embedded.LastRequest!.Reference.ToString());
    }

    [Fact]
    public async Task Output_mapping_preserves_response_fields()
    {
        var adapter = AdapterWith(new StubEmbeddedFeature(Response(includeTraceability: true)));

        var result = await adapter.ResolveAsync(Request());

        var response = result.Response!;
        Assert.Equal(IContaLegalReferenceStatus.Resolved, response.Status);
        Assert.Equal("Resolved to Legea 227/2015: Article 47.", response.Answer);
        Assert.Equal("Legea 227/2015: Article 47", response.Citation!.Display);
        Assert.Equal("resolved explanation", response.Explanation);
        Assert.Equal("corr-1", response.CorrelationId);
        Assert.NotNull(response.TraceabilitySummary);
        Assert.Equal("corr-1", response.TraceabilitySummary!.CorrelationId);
    }

    [Fact]
    public async Task Include_traceability_false_omits_traceability_summary()
    {
        var adapter = AdapterWith(new StubEmbeddedFeature(Response(includeTraceability: false)));

        var result = await adapter.ResolveAsync(Request(includeTraceability: false));

        Assert.True(result.IsSuccess);
        Assert.Null(result.Response!.TraceabilitySummary);
    }

    [Fact]
    public async Task Include_traceability_true_includes_traceability_summary()
    {
        var adapter = AdapterWith(new StubEmbeddedFeature(Response(includeTraceability: true)));

        var result = await adapter.ResolveAsync(Request(includeTraceability: true));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Response!.TraceabilitySummary);
    }

    [Fact]
    public async Task Operational_events_contain_only_public_safe_metadata()
    {
        var sink = new CapturingOperationalSink();
        var adapter = AdapterWith(new StubEmbeddedFeature(Response(includeTraceability: true)), sink);

        await adapter.ResolveAsync(Request());

        var operationalEvent = Assert.Single(sink.Events);
        Assert.Equal("request_completed", operationalEvent.Name);
        Assert.Equal("corr-1", operationalEvent.CorrelationId);
        Assert.Equal("tenant-1", operationalEvent.TenantId);
        Assert.Equal("account-1", operationalEvent.AccountId);
        Assert.Equal("workflow-1", operationalEvent.WorkflowId);
        Assert.Equal(IContaLegalReferenceStatus.Resolved, operationalEvent.Status);
        Assert.True(operationalEvent.IncludeTraceability);
        Assert.Equal("success", operationalEvent.Outcome);
    }

    [Fact]
    public async Task Runtime_failure_maps_to_public_safe_application_error()
    {
        var sink = new CapturingOperationalSink();
        var adapter = AdapterWith(new ThrowingEmbeddedFeature(), sink);

        var result = await adapter.ResolveAsync(Request());

        Assert.False(result.IsSuccess);
        Assert.Equal("runtime_error", result.Error!.Code);
        Assert.Equal("The legal reference traceability request could not be completed.", result.Error.Message);
        Assert.Equal("corr-1", result.Error.CorrelationId);
        var operationalEvent = Assert.Single(sink.Events);
        Assert.Equal("unexpected_failure", operationalEvent.Name);
    }

    [Fact]
    public void IConta_adapter_models_do_not_expose_internal_architecture_terms()
    {
        var publicTypes = new[]
        {
            typeof(IContaLegalReferenceRequest),
            typeof(IContaLegalReferenceResponse),
            typeof(IContaTraceabilitySummary),
            typeof(IContaLegalReferenceApplicationError),
            typeof(IContaLegalReferenceOperationalEvent),
        };
        var forbiddenTerms = new[]
        {
            "ResolutionEvidencePackage",
            "ResolutionAuditTrail",
            "ResolutionAuditEntry",
            "ResolutionProvenance",
            "ProvenanceChain",
            "Repository",
            "Pipeline",
            "Graph",
        };
        var exposedNames = publicTypes
            .SelectMany(type => type.GetProperties())
            .Select(property => property.PropertyType.Name)
            .Concat(publicTypes.Select(type => type.Name))
            .ToArray();

        foreach (var forbiddenTerm in forbiddenTerms)
        {
            Assert.DoesNotContain(exposedNames, name => name.Contains(forbiddenTerm, StringComparison.Ordinal));
        }
    }

    private static IContaLegalReferenceAdapter AdapterWith(
        IEmbeddedLegalReferenceFeature embeddedFeature,
        IIContaLegalReferenceOperationalSink? sink = null)
    {
        return new IContaLegalReferenceAdapter(
            embeddedFeature,
            sink ?? new NoopIContaLegalReferenceOperationalSink());
    }

    private static EmbeddedLegalReferenceResponse Response(bool includeTraceability)
    {
        var requested = PublicReference("Article 47");
        var citation = new PublicLegalCitation(
            "Legea 227/2015: Article 47",
            "Legea 227/2015",
            requested);
        var traceability = includeTraceability
            ? new TraceabilitySummary(
                "corr-1",
                requested,
                TraceabilityStatus.Resolved,
                citation,
                SourceSummary: null,
                "traceability linkage",
                new[] { "decision support" })
            : null;

        return new EmbeddedLegalReferenceResponse(
            EmbeddedLegalReferenceStatus.Resolved,
            "Resolved to Legea 227/2015: Article 47.",
            citation,
            "resolved explanation",
            "corr-1",
            traceability);
    }

    private static PublicLegalReference PublicReference(string display)
    {
        return new PublicLegalReference(
            display,
            new[] { new PublicReferenceSegment("Article", display.Split(' ').Last()) });
    }

    private sealed class StubEmbeddedFeature : IEmbeddedLegalReferenceFeature
    {
        private readonly EmbeddedLegalReferenceResponse _response;

        public StubEmbeddedFeature(EmbeddedLegalReferenceResponse response)
        {
            _response = response;
        }

        public int CallCount { get; private set; }

        public EmbeddedLegalReferenceRequest? LastRequest { get; private set; }

        public Task<EmbeddedLegalReferenceResponse> ResolveAsync(
            EmbeddedLegalReferenceRequest request,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastRequest = request;

            return Task.FromResult(_response);
        }
    }

    private sealed class ThrowingEmbeddedFeature : IEmbeddedLegalReferenceFeature
    {
        public Task<EmbeddedLegalReferenceResponse> ResolveAsync(
            EmbeddedLegalReferenceRequest request,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("internal runtime details");
        }
    }

    private sealed class CapturingOperationalSink : IIContaLegalReferenceOperationalSink
    {
        public List<IContaLegalReferenceOperationalEvent> Events { get; } = new();

        public void Record(IContaLegalReferenceOperationalEvent operationalEvent)
        {
            Events.Add(operationalEvent);
        }
    }
}
