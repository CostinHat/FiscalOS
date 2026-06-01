namespace FiscalOS.Core;

public sealed class FiscalSubject
{
    public Guid Id { get; init; }

    public FiscalCode FiscalCode { get; init; }
    = new("UNKNOWN");

    public string Name { get; init; } = string.Empty;

    public TaxIdentificationNumber TaxIdentificationNumber { get; init; }
    = new(string.Empty);

    public string RegistrationNumber { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}