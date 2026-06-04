using System;
using FiscalOS.Domain.LegalReferences;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Describe_Legal_Reference_Resolution_Provenance_Model
{
    private static ReferenceSegment Seg(string kind, string value) => new(kind, value);

    private static LegalReference Address(params ReferenceSegment[] segments) => new(segments);

    private static FullyQualifiedLegalReference Fq(string document, params ReferenceSegment[] segments) =>
        new(new LegalDocumentReference(document), new LegalReference(segments));

    private static readonly DateTimeOffset At = new(2026, 6, 4, 12, 0, 0, TimeSpan.Zero);

    private static readonly LegalReference Query = Address(Seg("Article", "47"), Seg("Paragraph", "3"));

    private static ProvenanceStep Step(string source, string description) =>
        new(new ProvenanceSource(source), description);

    private static ResolutionAuditTrail Trail()
    {
        var entry = new ResolutionAuditEntry(
            Query,
            new ResolutionDecision(ResolutionStatus.Resolved, Fq("Legea 227/2015", Seg("Article", "47"), Seg("Paragraph", "3"))),
            new ResolutionEvidence("matched a single document"),
            At);
        return new ResolutionAuditTrail(new[] { entry });
    }

    // -------- Validation --------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Provenance_source_rejects_empty(string value)
    {
        Assert.Throws<ArgumentException>(() => new ProvenanceSource(value));
    }

    [Fact]
    public void Provenance_step_rejects_null_source()
    {
        Assert.Throws<ArgumentNullException>(() => new ProvenanceStep(null!, "extracted citation"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Provenance_step_rejects_empty_description(string description)
    {
        Assert.Throws<ArgumentException>(() => new ProvenanceStep(new ProvenanceSource("Monitorul Oficial"), description));
    }

    // -------- Value storage --------

    [Fact]
    public void Provenance_source_trims_and_exposes_value()
    {
        var source = new ProvenanceSource("  Monitorul Oficial  ");

        Assert.Equal("Monitorul Oficial", source.Value);
        Assert.Equal("Monitorul Oficial", source.ToString());
    }

    [Fact]
    public void Provenance_step_stores_source_and_trims_description()
    {
        var source = new ProvenanceSource("Monitorul Oficial");

        var step = new ProvenanceStep(source, "  extracted citation  ");

        Assert.Equal(source, step.Source);
        Assert.Equal("extracted citation", step.Description);
    }

    // -------- Source / step modeling + composition --------

    [Fact]
    public void Provenance_chain_composes_steps_in_order()
    {
        var first = Step("Monitorul Oficial", "extracted citation");
        var second = Step("ANAF dataset", "matched to consolidated act");

        var chain = new ProvenanceChain(new[] { first, second });

        Assert.False(chain.IsEmpty);
        Assert.Equal(2, chain.Steps.Count);
        Assert.Same(first, chain.Steps[0]);
        Assert.Same(second, chain.Steps[1]);
    }

    [Fact]
    public void Empty_provenance_chain_reports_empty()
    {
        var chain = new ProvenanceChain(Array.Empty<ProvenanceStep>());

        Assert.True(chain.IsEmpty);
        Assert.Empty(chain.Steps);
    }

    [Fact]
    public void Resolution_provenance_stores_query_status_chain_and_audit_trail()
    {
        var chain = new ProvenanceChain(new[] { Step("Monitorul Oficial", "extracted citation") });
        var trail = Trail();

        var provenance = new ResolutionProvenance(Query, ResolutionStatus.Resolved, chain, trail);

        Assert.Equal(Query, provenance.Query);
        Assert.Equal(ResolutionStatus.Resolved, provenance.Status);
        Assert.Single(provenance.Chain.Steps);
        Assert.Single(provenance.AuditTrail.Entries);
    }

    // -------- Immutability --------

    [Fact]
    public void Provenance_chain_is_immutable_under_with_expression()
    {
        var first = Step("Monitorul Oficial", "extracted citation");
        var second = Step("ANAF dataset", "matched to consolidated act");
        var chain = new ProvenanceChain(new[] { first });

        var extended = chain with { Steps = new[] { first, second } };

        // Original chain is unchanged.
        Assert.Single(chain.Steps);
        Assert.Equal(2, extended.Steps.Count);
    }
}
