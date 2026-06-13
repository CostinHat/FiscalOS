using System;
using FiscalOS.Domain.LegalReferences;
using FiscalOS.LegalCore;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Describe_Legal_Reference_Resolution_Evidence_Package_Model
{
    private static readonly DateTimeOffset At = new(2026, 6, 5, 12, 0, 0, TimeSpan.Zero);

    private static ReferenceSegment Seg(string kind, string value) => new(kind, value);

    private static LegalReference Address(params ReferenceSegment[] segments) => new(segments);

    private static FullyQualifiedLegalReference Fq(string document, params ReferenceSegment[] segments) =>
        new(new LegalDocumentReference(document), new LegalReference(segments));

    private static readonly LegalReference Query = Address(Seg("Article", "47"), Seg("Paragraph", "3"));
    private static readonly FullyQualifiedLegalReference Selected =
        Fq("Legea 227/2015", Seg("Article", "47"), Seg("Paragraph", "3"));

    private static ResolutionResult Result() =>
        new(ResolutionStatus.Resolved, new[] { new ResolutionCandidate(Selected) }, Unresolved: null);

    private static ResolutionAuditTrail Trail() =>
        new(new[]
        {
            new ResolutionAuditEntry(
                Query,
                new ResolutionDecision(ResolutionStatus.Resolved, Selected),
                new ResolutionEvidence("matched a single document"),
                At),
        });

    private static ResolutionProvenance Provenance() =>
        new(
            Query,
            ResolutionStatus.Resolved,
            new ProvenanceChain(new[] { new ProvenanceStep(new ProvenanceSource("Monitorul Oficial"), "extracted citation") }),
            Trail());

    [Fact]
    public void Package_composes_result_audit_trail_and_provenance()
    {
        var result = Result();
        var trail = Trail();
        var provenance = Provenance();

        var package = new ResolutionEvidencePackage(result, trail, provenance);

        Assert.Same(result, package.Result);
        Assert.Same(trail, package.AuditTrail);
        Assert.Same(provenance, package.Provenance);
    }

    [Fact]
    public void Package_preserves_separate_concerns()
    {
        var package = new ResolutionEvidencePackage(Result(), Trail(), Provenance());

        // Each concern remains independently accessible and unmerged.
        Assert.True(package.Result.IsResolved);
        Assert.Single(package.AuditTrail.Entries);
        Assert.Single(package.Provenance.Chain.Steps);
        Assert.Equal(ResolutionStatus.Resolved, package.Provenance.Status);
    }

    [Fact]
    public void Package_is_immutable_under_with_expression()
    {
        var result = Result();
        var trail = Trail();
        var provenance = Provenance();
        var package = new ResolutionEvidencePackage(result, trail, provenance);

        var emptyTrail = new ResolutionAuditTrail(Array.Empty<ResolutionAuditEntry>());
        var updated = package with { AuditTrail = emptyTrail };

        // Original package is unchanged; unaffected concerns are carried over.
        Assert.Same(trail, package.AuditTrail);
        Assert.Same(emptyTrail, updated.AuditTrail);
        Assert.Same(result, updated.Result);
        Assert.Same(provenance, updated.Provenance);
    }
}
