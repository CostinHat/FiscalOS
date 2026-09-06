namespace FiscalOS.LegalCore;

public readonly record struct BiTemporalValidity(ValidTime ValidTime, KnowledgeTime KnowledgeTime)
{
    public bool IsApplicableAt(DateTimeOffset instant) => ValidTime.Interval.Contains(instant);

    public bool WasKnownAt(DateTimeOffset instant) => KnowledgeTime.Interval.Contains(instant);
}
