namespace FiscalOS.Runtime.LegalReferences.Traceability;

public sealed record PublicLegalReference(
    string Display,
    IReadOnlyList<PublicReferenceSegment> Segments);
