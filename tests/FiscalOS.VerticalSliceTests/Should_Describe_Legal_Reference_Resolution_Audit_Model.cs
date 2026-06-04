using System;
using FiscalOS.Domain.LegalReferences;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Describe_Legal_Reference_Resolution_Audit_Model
{
    private static ReferenceSegment Seg(string kind, string value) => new(kind, value);

    private static LegalReference Address(params ReferenceSegment[] segments) => new(segments);

    private static FullyQualifiedLegalReference Fq(string document, params ReferenceSegment[] segments) =>
        new(new LegalDocumentReference(document), new LegalReference(segments));

    private static readonly DateTimeOffset At = new(2026, 6, 4, 12, 0, 0, TimeSpan.Zero);

    private static readonly LegalReference Query = Address(Seg("Article", "47"), Seg("Paragraph", "3"));

    private static readonly FullyQualifiedLegalReference Selected =
        Fq("Legea 227/2015", Seg("Article", "47"), Seg("Paragraph", "3"));

    private static ResolutionAuditEntry Entry(ResolutionDecision decision, string evidence) =>
        new(Query, decision, new ResolutionEvidence(evidence), At);

    // -------- Validation --------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Resolution_evidence_rejects_empty_description(string description)
    {
        Assert.Throws<ArgumentException>(() => new ResolutionEvidence(description));
    }

    [Fact]
    public void Resolved_decision_requires_a_selected_reference()
    {
        Assert.Throws<ArgumentException>(() => new ResolutionDecision(ResolutionStatus.Resolved));
    }

    [Fact]
    public void Non_resolved_decision_rejects_a_selected_reference()
    {
        Assert.Throws<ArgumentException>(() => new ResolutionDecision(ResolutionStatus.Unresolved, Selected));
    }

    // -------- Value storage --------

    [Fact]
    public void Resolution_evidence_trims_and_exposes_description()
    {
        var evidence = new ResolutionEvidence("  matched a single document  ");

        Assert.Equal("matched a single document", evidence.Description);
        Assert.Equal("matched a single document", evidence.ToString());
    }

    [Fact]
    public void Resolved_decision_stores_status_and_selected_reference()
    {
        var decision = new ResolutionDecision(ResolutionStatus.Resolved, Selected);

        Assert.Equal(ResolutionStatus.Resolved, decision.Status);
        Assert.Equal(Selected, decision.SelectedReference);
    }

    [Fact]
    public void Unresolved_decision_has_no_selected_reference()
    {
        var decision = new ResolutionDecision(ResolutionStatus.Unresolved);

        Assert.Equal(ResolutionStatus.Unresolved, decision.Status);
        Assert.Null(decision.SelectedReference);
    }

    // -------- Decision / evidence modeling --------

    [Fact]
    public void Audit_entry_stores_query_decision_evidence_and_timestamp()
    {
        var decision = new ResolutionDecision(ResolutionStatus.Resolved, Selected);
        var evidence = new ResolutionEvidence("matched a single document");

        var entry = new ResolutionAuditEntry(Query, decision, evidence, At);

        Assert.Equal(Query, entry.Query);
        Assert.Equal(decision, entry.Decision);
        Assert.Equal(evidence, entry.Evidence);
        Assert.Equal(At, entry.Timestamp);
    }

    // -------- Audit trail composition --------

    [Fact]
    public void Audit_trail_composes_entries_in_order()
    {
        var resolved = Entry(new ResolutionDecision(ResolutionStatus.Resolved, Selected), "matched a single document");
        var deferred = Entry(new ResolutionDecision(ResolutionStatus.Ambiguous), "multiple candidate documents");

        var trail = new ResolutionAuditTrail(new[] { resolved, deferred });

        Assert.False(trail.IsEmpty);
        Assert.Equal(2, trail.Entries.Count);
        Assert.Same(resolved, trail.Entries[0]);
        Assert.Same(deferred, trail.Entries[1]);
    }

    [Fact]
    public void Empty_audit_trail_reports_empty()
    {
        var trail = new ResolutionAuditTrail(Array.Empty<ResolutionAuditEntry>());

        Assert.True(trail.IsEmpty);
        Assert.Empty(trail.Entries);
    }

    // -------- Immutability --------

    [Fact]
    public void Audit_trail_is_immutable_under_with_expression()
    {
        var first = Entry(new ResolutionDecision(ResolutionStatus.Resolved, Selected), "matched a single document");
        var second = Entry(new ResolutionDecision(ResolutionStatus.Unresolved), "no matching document");
        var trail = new ResolutionAuditTrail(new[] { first });

        var extended = trail with { Entries = new[] { first, second } };

        // Original trail is unchanged.
        Assert.Single(trail.Entries);
        Assert.Equal(2, extended.Entries.Count);
    }
}
