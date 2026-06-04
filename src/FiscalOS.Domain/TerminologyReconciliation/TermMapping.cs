namespace FiscalOS.Domain.TerminologyReconciliation;

public sealed record TermMapping(
    SourceTerm Source,
    CanonicalTerm Canonical);
