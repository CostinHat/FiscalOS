using System;
using FiscalOS.Domain.LegalReferences;
using Xunit;

namespace FiscalOS.VerticalSliceTests;

public sealed class Should_Describe_Legal_Reference_Model
{
    private static ReferenceSegment Seg(string kind, string value) => new(kind, value);

    private static LegalReference Ref(params ReferenceSegment[] segments) => new(segments);

    // -------- Validation --------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Reference_segment_rejects_empty_kind(string kind)
    {
        Assert.Throws<ArgumentException>(() => new ReferenceSegment(kind, "47"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Reference_segment_rejects_empty_value(string value)
    {
        Assert.Throws<ArgumentException>(() => new ReferenceSegment("Article", value));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Legal_document_reference_rejects_empty(string value)
    {
        Assert.Throws<ArgumentException>(() => new LegalDocumentReference(value));
    }

    [Fact]
    public void Legal_reference_requires_at_least_one_segment()
    {
        Assert.Throws<ArgumentException>(() => new LegalReference(Array.Empty<ReferenceSegment>()));
    }

    // -------- Value storage --------

    [Fact]
    public void Reference_segment_trims_and_exposes_values()
    {
        var segment = new ReferenceSegment("  Article ", "  47 ");

        Assert.Equal("Article", segment.Kind);
        Assert.Equal("47", segment.Value);
        Assert.Equal("Article 47", segment.ToString());
    }

    [Fact]
    public void Legal_document_reference_trims_and_exposes_value()
    {
        var document = new LegalDocumentReference("  Legea 227/2015 ");

        Assert.Equal("Legea 227/2015", document.Value);
        Assert.Equal("Legea 227/2015", document.ToString());
    }

    [Fact]
    public void Legal_reference_stores_segments_in_order()
    {
        var reference = Ref(Seg("Article", "47"), Seg("Paragraph", "3"), Seg("Letter", "b"));

        Assert.Equal(3, reference.Segments.Count);
        Assert.Equal(Seg("Article", "47"), reference.Segments[0]);
        Assert.Equal(Seg("Letter", "b"), reference.Segments[2]);
        Assert.Equal("Article 47 / Paragraph 3 / Letter b", reference.ToString());
    }

    [Fact]
    public void Legal_references_with_same_segments_are_equal()
    {
        var a = Ref(Seg("Article", "47"), Seg("Paragraph", "3"));
        var b = Ref(Seg("Article", "47"), Seg("Paragraph", "3"));

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    // -------- Parent navigation --------

    [Fact]
    public void Parent_drops_the_last_segment()
    {
        var reference = Ref(Seg("Article", "47"), Seg("Paragraph", "3"), Seg("Letter", "b"));

        var parent = reference.Parent();

        Assert.NotNull(parent);
        Assert.Equal(Ref(Seg("Article", "47"), Seg("Paragraph", "3")), parent);
    }

    [Fact]
    public void Parent_of_a_single_segment_reference_is_null()
    {
        Assert.Null(Ref(Seg("Article", "47")).Parent());
    }

    // -------- Ancestor / descendant relationships --------

    [Fact]
    public void Ancestor_is_a_strict_prefix()
    {
        var article = Ref(Seg("Article", "47"));
        var letter = Ref(Seg("Article", "47"), Seg("Paragraph", "3"), Seg("Letter", "b"));

        Assert.True(article.IsAncestorOf(letter));
        Assert.True(letter.IsDescendantOf(article));
        Assert.False(letter.IsAncestorOf(article));
    }

    [Fact]
    public void A_reference_is_not_its_own_ancestor_or_descendant()
    {
        var reference = Ref(Seg("Article", "47"), Seg("Paragraph", "3"));

        Assert.False(reference.IsAncestorOf(reference));
        Assert.False(reference.IsDescendantOf(reference));
    }

    [Fact]
    public void Siblings_are_unrelated()
    {
        var a = Ref(Seg("Article", "47"));
        var b = Ref(Seg("Article", "48"));

        Assert.False(a.IsAncestorOf(b));
        Assert.False(a.IsDescendantOf(b));
    }

    // -------- Containment behavior --------

    [Fact]
    public void Contains_is_inclusive_of_self_and_descendants()
    {
        var paragraph = Ref(Seg("Article", "47"), Seg("Paragraph", "3"));
        var letter = Ref(Seg("Article", "47"), Seg("Paragraph", "3"), Seg("Letter", "b"));

        Assert.True(paragraph.Contains(letter));   // descendant
        Assert.True(paragraph.Contains(paragraph)); // self
        Assert.False(letter.Contains(paragraph));   // ancestor is not contained
    }

    [Fact]
    public void Contains_is_false_for_siblings()
    {
        var a = Ref(Seg("Article", "47"), Seg("Paragraph", "3"));
        var b = Ref(Seg("Article", "47"), Seg("Paragraph", "4"));

        Assert.False(a.Contains(b));
    }

    // -------- Fully qualified reference composition --------

    [Fact]
    public void Fully_qualified_reference_composes_document_and_reference()
    {
        var document = new LegalDocumentReference("Legea 227/2015");
        var reference = Ref(Seg("Article", "47"), Seg("Paragraph", "3"));

        var qualified = new FullyQualifiedLegalReference(document, reference);

        Assert.Equal(document, qualified.Document);
        Assert.Equal(reference, qualified.Reference);
    }

    [Fact]
    public void Fully_qualified_containment_holds_within_the_same_document()
    {
        var document = new LegalDocumentReference("Legea 227/2015");
        var article = new FullyQualifiedLegalReference(document, Ref(Seg("Article", "47")));
        var letter = new FullyQualifiedLegalReference(
            document,
            Ref(Seg("Article", "47"), Seg("Paragraph", "3"), Seg("Letter", "b")));

        Assert.True(article.IsAncestorOf(letter));
        Assert.True(letter.IsDescendantOf(article));
        Assert.True(article.Contains(letter));
    }

    [Fact]
    public void Fully_qualified_references_in_different_documents_are_unrelated()
    {
        var reference = Ref(Seg("Article", "47"));
        var letter = Ref(Seg("Article", "47"), Seg("Paragraph", "3"));

        var inCodulFiscal = new FullyQualifiedLegalReference(new LegalDocumentReference("Legea 227/2015"), reference);
        var inOtherAct = new FullyQualifiedLegalReference(new LegalDocumentReference("OUG 1/2020"), letter);

        Assert.False(inCodulFiscal.IsAncestorOf(inOtherAct));
        Assert.False(inCodulFiscal.Contains(inOtherAct));
    }

    [Fact]
    public void Fully_qualified_parent_keeps_the_document_and_drops_the_last_segment()
    {
        var document = new LegalDocumentReference("Legea 227/2015");
        var paragraph = new FullyQualifiedLegalReference(
            document,
            Ref(Seg("Article", "47"), Seg("Paragraph", "3")));

        var parent = paragraph.Parent();

        Assert.Equal(
            new FullyQualifiedLegalReference(document, Ref(Seg("Article", "47"))),
            parent);
    }
}
