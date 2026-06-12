namespace FiscalOS.Runtime.LegalReferences.IConta;

public sealed record IContaLegalReferenceApplicationError(
    string Code,
    string Message,
    string? CorrelationId);
