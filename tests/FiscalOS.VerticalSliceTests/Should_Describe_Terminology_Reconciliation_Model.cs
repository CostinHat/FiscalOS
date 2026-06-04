using System;
using FiscalOS.Domain.TerminologyReconciliation;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Describe_Terminology_Reconciliation_Model
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Canonical_term_rejects_empty(string value)
    {
        Assert.Throws<ArgumentException>(() => new CanonicalTerm(value));
    }

    [Fact]
    public void Canonical_term_trims_and_exposes_value()
    {
        var term = new CanonicalTerm("  LegalCitation ");

        Assert.Equal("LegalCitation", term.Value);
        Assert.Equal("LegalCitation", term.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Source_term_rejects_empty(string value)
    {
        Assert.Throws<ArgumentException>(() => new SourceTerm(value));
    }

    [Fact]
    public void Source_term_trims_and_exposes_value()
    {
        var term = new SourceTerm("  LegalAtom ");

        Assert.Equal("LegalAtom", term.Value);
        Assert.Equal("LegalAtom", term.ToString());
    }

    [Fact]
    public void Term_mapping_stores_source_and_canonical()
    {
        var mapping = new TermMapping(new SourceTerm("LegalAtom"), new CanonicalTerm("LegalCitation"));

        Assert.Equal("LegalAtom", mapping.Source.Value);
        Assert.Equal("LegalCitation", mapping.Canonical.Value);
    }

    [Fact]
    public void Reconciliation_result_stores_values_and_reports_full_reconciliation()
    {
        var mapping = new TermMapping(new SourceTerm("LegalAtom"), new CanonicalTerm("LegalCitation"));

        var result = new ReconciliationResult(
            new[] { mapping },
            Array.Empty<SourceTerm>());

        Assert.Single(result.Mappings);
        Assert.Empty(result.UnmappedSourceTerms);
        Assert.True(result.IsFullyReconciled);
    }

    [Fact]
    public void Reconciliation_result_reports_incomplete_when_terms_are_unmapped()
    {
        var result = new ReconciliationResult(
            Array.Empty<TermMapping>(),
            new[] { new SourceTerm("PracticeGraph") });

        Assert.False(result.IsFullyReconciled);
        Assert.Single(result.UnmappedSourceTerms);
    }
}
