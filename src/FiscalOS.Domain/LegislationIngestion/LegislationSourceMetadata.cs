namespace FiscalOS.Domain.LegislationIngestion;

public sealed record LegislationSourceMetadata
{
    public LegislationSourceId Id { get; }

    public string DisplayName { get; }

    public string SourceType { get; }

    public LegislationSourceMetadata(
        LegislationSourceId id,
        string displayName,
        string sourceType)
    {
        ArgumentNullException.ThrowIfNull(id);

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Legislation source display name cannot be empty.", nameof(displayName));
        }

        if (string.IsNullOrWhiteSpace(sourceType))
        {
            throw new ArgumentException("Legislation source type cannot be empty.", nameof(sourceType));
        }

        Id = id;
        DisplayName = displayName.Trim();
        SourceType = sourceType.Trim();
    }
}
