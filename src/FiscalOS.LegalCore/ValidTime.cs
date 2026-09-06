namespace FiscalOS.LegalCore;

public readonly record struct ValidTime(TemporalInterval Interval)
{
    public DateTimeOffset Start => Interval.Start;
    public DateTimeOffset? End => Interval.End;
}
