using System.Collections.Generic;
using System;
using FiscalOS.Domain.LegalReferences;
using FiscalOS.LegalCore;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Describe_Legal_Reference_Resolution_Model
{
    private static ReferenceSegment Seg(string kind, string value) => new(kind, value);

    private static LegalReference Address(params ReferenceSegment[] segments) => new(segments);

    private static FullyQualifiedLegalReference Fq(string document, params ReferenceSegment[] segments) =>
        new(new LegalDocumentReference(document), new LegalReference(segments));

    private static readonly LegalReference Article47Paragraph3 =
        Address(Seg("Article", "47"), Seg("Paragraph", "3"));

    // -------- Validation --------

    [Fact]
    public void Resolution_candidate_rejects_null_reference()
    {
        Assert.Throws<ArgumentNullException>(() => new ResolutionCandidate(null!));
    }

    [Fact]
    public void Unresolved_reference_rejects_null_query()
    {
        Assert.Throws<ArgumentNullException>(() => new UnresolvedReference(null!, "not found"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Unresolved_reference_rejects_empty_reason(string reason)
    {
        Assert.Throws<ArgumentException>(() => new UnresolvedReference(Article47Paragraph3, reason));
    }

    // -------- Value storage --------

    [Fact]
    public void Resolution_candidate_stores_reference()
    {
        var reference = Fq("Legea 227/2015", Seg("Article", "47"), Seg("Paragraph", "3"));

        var candidate = new ResolutionCandidate(reference);

        Assert.Equal(reference, candidate.Reference);
    }

    [Fact]
    public void Unresolved_reference_stores_query_and_trims_reason()
    {
        var unresolved = new UnresolvedReference(Article47Paragraph3, "  no matching document  ");

        Assert.Equal(Article47Paragraph3, unresolved.Query);
        Assert.Equal("no matching document", unresolved.Reason);
    }

    // -------- Status behavior: resolved --------

    [Fact]
    public void Resolved_result_exposes_the_single_resolved_reference()
    {
        var reference = Fq("Legea 227/2015", Seg("Article", "47"), Seg("Paragraph", "3"));
        var result = new ResolutionResult(
            ResolutionStatus.Resolved,
            new[] { new ResolutionCandidate(reference) },
            Unresolved: null);

        Assert.True(result.IsResolved);
        Assert.False(result.IsAmbiguous);
        Assert.False(result.IsUnresolved);
        Assert.Equal(reference, result.ResolvedReference);
    }

    // -------- Status behavior: ambiguous --------

    [Fact]
    public void Ambiguous_result_has_multiple_candidates_and_no_single_resolution()
    {
        var inCodFiscal = new ResolutionCandidate(
            Fq("Legea 227/2015", Seg("Article", "47"), Seg("Paragraph", "3")));
        var inOtherAct = new ResolutionCandidate(
            Fq("OUG 1/2020", Seg("Article", "47"), Seg("Paragraph", "3")));

        var result = new ResolutionResult(
            ResolutionStatus.Ambiguous,
            new[] { inCodFiscal, inOtherAct },
            Unresolved: null);

        Assert.True(result.IsAmbiguous);
        Assert.False(result.IsResolved);
        Assert.Equal(2, result.Candidates.Count);
        Assert.Null(result.ResolvedReference);
    }

    // -------- Status behavior: unresolved --------

    [Fact]
    public void Unresolved_result_carries_the_unresolved_query_and_no_candidates()
    {
        var unresolved = new UnresolvedReference(Article47Paragraph3, "no matching document");
        var result = new ResolutionResult(
            ResolutionStatus.Unresolved,
            Array.Empty<ResolutionCandidate>(),
            unresolved);

        Assert.True(result.IsUnresolved);
        Assert.False(result.IsResolved);
        Assert.Empty(result.Candidates);
        Assert.Null(result.ResolvedReference);
        Assert.NotNull(result.Unresolved);
        Assert.Equal("no matching document", result.Unresolved!.Reason);
    }
}
