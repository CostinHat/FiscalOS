namespace FiscalOS.Core;

public sealed record FiscalPeriod
{
    public DateOnly StartDate { get; }

    public DateOnly EndDate { get; }
    
    public bool Contains(DateOnly date)
{
    return date >= StartDate && date <= EndDate;
}

    public FiscalPeriod(DateOnly startDate, DateOnly endDate)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException("End date cannot be before start date.");
        }

        StartDate = startDate;
        EndDate = endDate;
    }
}