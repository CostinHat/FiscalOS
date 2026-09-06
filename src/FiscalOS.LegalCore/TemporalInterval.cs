namespace FiscalOS.LegalCore;

/// <summary>Inclusive temporal interval with an optional open-ended finish.</summary>
public readonly record struct TemporalInterval
{
    public TemporalInterval(DateTimeOffset start, DateTimeOffset? end)
    {
        if (end is not null && end < start)
        {
            throw new ArgumentException("Temporal interval end cannot precede its start.", nameof(end));
        }

        Start = start;
        End = end;
    }

    public DateTimeOffset Start { get; }
    public DateTimeOffset? End { get; }

    public bool Contains(DateTimeOffset instant) => instant >= Start && (End is null || instant <= End);
}
