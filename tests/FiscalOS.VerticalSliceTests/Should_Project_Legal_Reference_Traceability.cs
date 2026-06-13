using System.Linq;
using System;
using FiscalOS.Domain.LegalReferences;
using FiscalOS.LegalCore;
using FiscalOS.Runtime.LegalReferences.Traceability;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Project_Legal_Reference_Traceability
{
    private static readonly LegalReference RequestedArticle47 = Address(Seg("Article", "47"));
    private static readonly LegalReference RequestedArticle48 = Address(Seg("Article", "48"));
    private static readonly FullyQualifiedLegalReference CitationArticle47 = Fq("Legea 227/2015", Seg("Article", "47"));
    private static readonly FullyQualifiedLegalReference CitationArticle48 = Fq("Legea 227/2015", Seg("Article", "48"));

    private static ReferenceSegment Seg(string kind, string value) => new(kind, value);

    private static LegalReference Address(params ReferenceSegment[] segments) => new(segments);

    private static FullyQualifiedLegalReference Fq(string document, params ReferenceSegment[] segments) =>
        new(new LegalDocumentReference(document), new LegalReference(segments));

    private static ResolutionResult Resolved(FullyQualifiedLegalReference reference) =>
        new(ResolutionStatus.Resolved, new[] { new ResolutionCandidate(reference) }, Unresolved: null);

    private static ResolutionResult Ambiguous(params FullyQualifiedLegalReference[] references) =>
        new(ResolutionStatus.Ambiguous, references.Select(reference => new ResolutionCandidate(reference)).ToArray(), Unresolved: null);

    private static ResolutionResult Unresolved(LegalReference query) =>
        new(ResolutionStatus.Unresolved, Array.Empty<ResolutionCandidate>(), new UnresolvedReference(query, "No stored resolution found."));

    private static ResolutionEvidencePackage Package(ResolutionResult result)
    {
        var query = result.ResolvedReference?.Reference
            ?? result.Candidates.FirstOrDefault()?.Reference.Reference
            ?? result.Unresolved!.Query;
        var trail = new ResolutionAuditTrail(new[]
        {
            new ResolutionAuditEntry(
                query,
                new ResolutionDecision(result.Status, result.ResolvedReference),
                new ResolutionEvidence("internal evidence"),
                DateTimeOffset.UnixEpoch),
        });
        var provenance = new ResolutionProvenance(
            query,
            result.Status,
            new ProvenanceChain(new[]
            {
                new ProvenanceStep(new ProvenanceSource("internal-provenance-source"), "internal provenance step"),
            }),
            trail);

        return new ResolutionEvidencePackage(result, trail, provenance);
    }

    [Fact]
    public void Resolved_projection_includes_correlation_requested_reference_and_citation()
    {
        var projector = new LegalReferenceTraceabilityProjector();

        var summary = projector.Project(Package(Resolved(CitationArticle47)), "corr-1", RequestedArticle47);

        Assert.Equal("corr-1", summary.CorrelationId);
        Assert.Equal(TraceabilityStatus.Resolved, summary.Status);
        Assert.Equal("Article 47", summary.RequestedReference.Display);
        Assert.NotNull(summary.Citation);
        Assert.Equal("Legea 227/2015", summary.Citation!.Document);
        Assert.Equal("Legea 227/2015: Article 47", summary.Citation.Display);
        Assert.Contains("requested reference", summary.Linkage);
        Assert.NotEmpty(summary.Limitations);
    }

    [Fact]
    public void Ambiguous_projection_has_no_selected_citation()
    {
        var projector = new LegalReferenceTraceabilityProjector();

        var summary = projector.Project(
            Package(Ambiguous(CitationArticle47, CitationArticle48)),
            "corr-2",
            RequestedArticle47);

        Assert.Equal(TraceabilityStatus.Ambiguous, summary.Status);
        Assert.Null(summary.Citation);
        Assert.Equal("Article 47", summary.RequestedReference.Display);
        Assert.Contains("no single citation was selected", summary.Linkage);
    }

    [Fact]
    public void Unresolved_projection_has_no_citation_and_keeps_requested_reference()
    {
        var projector = new LegalReferenceTraceabilityProjector();

        var summary = projector.Project(Package(Unresolved(RequestedArticle48)), "corr-3", RequestedArticle48);

        Assert.Equal(TraceabilityStatus.Unresolved, summary.Status);
        Assert.Null(summary.Citation);
        Assert.Equal("Article 48", summary.RequestedReference.Display);
        Assert.Contains("could not be resolved", summary.Linkage);
    }

    [Fact]
    public void Public_summary_does_not_expose_internal_resolution_artifacts()
    {
        var forbiddenTypeNames = new[]
        {
            nameof(ResolutionEvidencePackage),
            nameof(ResolutionAuditTrail),
            nameof(ResolutionAuditEntry),
            nameof(ResolutionEvidence),
            nameof(ResolutionProvenance),
            nameof(ProvenanceChain),
            nameof(ProvenanceStep),
        };

        var publicTypes = new[]
        {
            typeof(TraceabilitySummary),
            typeof(PublicLegalReference),
            typeof(PublicReferenceSegment),
            typeof(PublicLegalCitation),
        };

        var exposedPropertyTypeNames = publicTypes
            .SelectMany(type => type.GetProperties())
            .Select(property => property.PropertyType.Name)
            .ToArray();

        foreach (var forbiddenTypeName in forbiddenTypeNames)
        {
            Assert.DoesNotContain(forbiddenTypeName, exposedPropertyTypeNames);
        }
    }

    [Fact]
    public void Ambiguous_projection_does_not_use_first_candidate_as_public_requested_reference()
    {
        var projector = new LegalReferenceTraceabilityProjector();
        var requested = Address(Seg("Article", "999"));

        var summary = projector.Project(
            Package(Ambiguous(CitationArticle47, CitationArticle48)),
            "corr-4",
            requested);

        Assert.Equal("Article 999", summary.RequestedReference.Display);
        Assert.DoesNotContain("Article 47", summary.RequestedReference.Display);
        Assert.Null(summary.Citation);
    }

    [Fact]
    public void Citation_only_appears_for_resolved_outcomes()
    {
        var projector = new LegalReferenceTraceabilityProjector();

        var resolved = projector.Project(Package(Resolved(CitationArticle47)), "corr-5", RequestedArticle47);
        var ambiguous = projector.Project(Package(Ambiguous(CitationArticle47, CitationArticle48)), "corr-6", RequestedArticle47);
        var unresolved = projector.Project(Package(Unresolved(RequestedArticle48)), "corr-7", RequestedArticle48);

        Assert.NotNull(resolved.Citation);
        Assert.Null(ambiguous.Citation);
        Assert.Null(unresolved.Citation);
    }
}
