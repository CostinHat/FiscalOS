namespace FiscalOS.LegalCore;

public readonly record struct KnowledgeTime(TemporalInterval Interval)
{
    public DateTimeOffset Start => Interval.Start;
    public DateTimeOffset? End => Interval.End;
}
