namespace FiscalOS.Runtime.LegalReferences.IConta;

public sealed record IContaLegalCitation(
    string Display,
    string Document,
    IContaPublicLegalReference Reference);
