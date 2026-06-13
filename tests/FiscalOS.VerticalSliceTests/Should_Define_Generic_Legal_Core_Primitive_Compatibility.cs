using System;
using System.IO;
using FiscalOS.Domain.LegalReferences;
using FiscalOS.LegalCore;
using FiscalOS.LegalKnowledge;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Define_Generic_Legal_Core_Primitive_Compatibility
{
    private static ReferenceSegment Seg(string kind, string value) => new(kind, value);

    private static LegalReference Ref(params ReferenceSegment[] segments) => new(segments);

    [Fact]
    public void Structural_reference_primitives_preserve_current_value_behavior()
    {
        var document = new LegalDocumentReference(" Legea 227/2015 ");
        var article = Seg(" Article ", " 47 ");
        var paragraph = Seg("Paragraph", "3");
        var reference = Ref(article, paragraph);
        var qualified = new FullyQualifiedLegalReference(document, reference);

        Assert.Equal("Legea 227/2015", document.Value);
        Assert.Equal("FiscalOS.LegalCore", typeof(LegalDocumentReference).Namespace);
        Assert.Equal("FiscalOS.LegalCore", typeof(ReferenceSegment).Namespace);
        Assert.Equal("FiscalOS.LegalCore", typeof(LegalReference).Namespace);
        Assert.Equal("FiscalOS.LegalCore", typeof(FullyQualifiedLegalReference).Namespace);
        Assert.Equal("Article", article.Kind);
        Assert.Equal("47", article.Value);
        Assert.Equal("Article 47 / Paragraph 3", reference.ToString());
        Assert.Equal(document, qualified.Document);
        Assert.Equal(reference, qualified.Reference);
    }

    [Fact]
    public void Structural_reference_operations_remain_pure_prefix_comparisons()
    {
        var document = new LegalDocumentReference("Legea 227/2015");
        var article = new FullyQualifiedLegalReference(
            document,
            Ref(Seg("Article", "47")));
        var paragraph = new FullyQualifiedLegalReference(
            document,
            Ref(Seg("Article", "47"), Seg("Paragraph", "3")));
        var otherDocumentParagraph = new FullyQualifiedLegalReference(
            new LegalDocumentReference("OUG 1/2020"),
            Ref(Seg("Article", "47"), Seg("Paragraph", "3")));

        Assert.True(article.IsAncestorOf(paragraph));
        Assert.True(article.Contains(paragraph));
        Assert.False(article.Contains(otherDocumentParagraph));
        Assert.Equal(article, paragraph.Parent());
    }

    [Fact]
    public void Jurisdiction_identity_remains_a_stable_primitive_candidate()
    {
        var first = new JurisdictionId(" RO ");
        var second = new JurisdictionId("RO");
        var jurisdiction = new Jurisdiction(first, "Romania");

        Assert.Equal("RO", first.Value);
        Assert.Equal(first, second);
        Assert.Equal("FiscalOS.LegalCore", typeof(JurisdictionId).Namespace);
        Assert.Equal(first, jurisdiction.Id);
        Assert.Equal("Romania", jurisdiction.Name);
    }

    [Fact]
    public void Later_review_citation_and_authority_candidates_remain_compatible()
    {
        var citation = new LegalCitation(
            LegalSourceType.Law,
            "Legea 227/2015",
            "Art. 47",
            SpecificityLevel.Specific,
            new DateOnly(2024, 1, 1),
            new JurisdictionId("RO"));

        Assert.Equal(LegalSourceType.Law, citation.SourceType);
        Assert.Equal("FiscalOS.LegalCore", typeof(SourceAuthorityLevel).Namespace);
        Assert.Equal(SourceAuthorityLevel.Statutory, SourceAuthority.LevelFor(citation.SourceType));
        Assert.Equal(SourceAuthorityLevel.Statutory, CitationAuthority.LevelOf(citation));
        Assert.Equal(new SourceHierarchy(SourceAuthorityLevel.Statutory), CitationAuthority.HierarchyOf(citation));
        Assert.Equal(SpecificityLevel.Specific, CitationSpecificity.SpecificityOf(citation));
    }

    [Fact]
    public void Generic_legal_core_candidates_remain_distinct_from_ingestion_and_runtime_projects()
    {
        var root = RepositoryRoot();
        var domainProject = File.ReadAllText(Path.Combine(root, "src", "FiscalOS.Domain", "FiscalOS.Domain.csproj"));
        var legalKnowledgeProject = File.ReadAllText(Path.Combine(root, "src", "FiscalOS.LegalKnowledge", "FiscalOS.LegalKnowledge.csproj"));
        var runtimeProject = File.ReadAllText(Path.Combine(root, "src", "FiscalOS.Runtime", "FiscalOS.Runtime.csproj"));
        var legalCoreProject = File.ReadAllText(Path.Combine(root, "src", "FiscalOS.LegalCore", "FiscalOS.LegalCore.csproj"));

        Assert.Equal("FiscalOS.LegalCore", LegalCoreBoundary.Name);
        Assert.Contains("FiscalOS.LegalCore", domainProject);
        Assert.Contains("FiscalOS.LegalCore", legalKnowledgeProject);
        Assert.Contains("FiscalOS.LegalCore", runtimeProject);
        Assert.DoesNotContain("ProjectReference", legalCoreProject);
        Assert.DoesNotContain("PackageReference", legalCoreProject);
        Assert.DoesNotContain("FiscalOS.Runtime", domainProject);
        Assert.DoesNotContain("FiscalOS.Runtime", legalKnowledgeProject);
        Assert.DoesNotContain("FiscalOS.Api", domainProject);
        Assert.DoesNotContain("FiscalOS.Api", legalKnowledgeProject);
    }

    private static string RepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !File.Exists(Path.Combine(current.FullName, "FiscalOS.sln")))
        {
            current = current.Parent;
        }

        if (current is null)
        {
            throw new InvalidOperationException("Could not locate repository root.");
        }

        return current.FullName;
    }
}
