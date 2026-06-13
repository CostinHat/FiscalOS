namespace FiscalOS.Domain.LegislationIngestion;

public sealed record RawDocumentFingerprint
{
    public RawDocumentContentHash ContentHash { get; }

    public RawDocumentHashAlgorithm Algorithm { get; }

    public RawDocumentFingerprint(
        RawDocumentContentHash contentHash,
        RawDocumentHashAlgorithm algorithm)
    {
        ArgumentNullException.ThrowIfNull(contentHash);
        ArgumentNullException.ThrowIfNull(algorithm);

        ContentHash = contentHash;
        Algorithm = algorithm;
    }
}
