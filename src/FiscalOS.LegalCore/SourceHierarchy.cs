namespace FiscalOS.LegalCore;

public sealed record SourceHierarchy(SourceAuthorityLevel Level)
    : IComparable<SourceHierarchy>
{
    public bool IsHigherThan(SourceHierarchy other) => Level > other.Level;

    public bool IsLowerThan(SourceHierarchy other) => Level < other.Level;

    public bool IsEqualTo(SourceHierarchy other) => Level == other.Level;

    public int CompareTo(SourceHierarchy? other) =>
        other is null ? 1 : Level.CompareTo(other.Level);
}
