namespace FiscalOS.Domain.LegalReferences;

/// <summary>
/// An absolute structural legal address: a <see cref="LegalDocumentReference"/> together with a
/// <see cref="LegalReference"/>. Structural operations apply only within the same document.
/// </summary>
public sealed record FullyQualifiedLegalReference(
    LegalDocumentReference Document,
    LegalReference Reference)
{
    /// <summary>The enclosing address within the same document, or null when the reference has no parent.</summary>
    public FullyQualifiedLegalReference? Parent()
    {
        var parent = Reference.Parent();

        return parent is null
            ? null
            : new FullyQualifiedLegalReference(Document, parent);
    }

    /// <summary>True when, within the same document, this reference strictly encloses <paramref name="other"/>.</summary>
    public bool IsAncestorOf(FullyQualifiedLegalReference other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return Document.Equals(other.Document) && Reference.IsAncestorOf(other.Reference);
    }

    /// <summary>True when, within the same document, <paramref name="other"/> strictly encloses this reference.</summary>
    public bool IsDescendantOf(FullyQualifiedLegalReference other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return Document.Equals(other.Document) && Reference.IsDescendantOf(other.Reference);
    }

    /// <summary>Inclusive containment within the same document: equal to or an ancestor of <paramref name="other"/>.</summary>
    public bool Contains(FullyQualifiedLegalReference other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return Document.Equals(other.Document) && Reference.Contains(other.Reference);
    }
}
