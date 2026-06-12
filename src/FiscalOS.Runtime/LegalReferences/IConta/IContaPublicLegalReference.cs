namespace FiscalOS.Runtime.LegalReferences.IConta;

public sealed record IContaPublicLegalReference(
    string Display,
    IReadOnlyList<IContaPublicReferenceSegment> Segments);
